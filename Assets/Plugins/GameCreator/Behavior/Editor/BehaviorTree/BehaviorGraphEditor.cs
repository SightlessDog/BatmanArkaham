using UnityEditor.Experimental;

namespace GameCreator.Behavior
{
	using System;
	using System.IO;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
    using GameCreator.Core;

	[CustomEditor(typeof(BehaviorGraph))]
	public class BehaviorGraphEditor : Editor
	{
        private static readonly GUIContent GC_ADD_ROOT = new GUIContent("Root");
		private static readonly GUIContent GC_ADD_COMPOSITE = new GUIContent("Composite");
        private static readonly GUIContent GC_ADD_BEHAVIOR_GRAPH = new GUIContent("Behavior Graph");
        private static readonly GUIContent GC_ADD_DECORATOR = new GUIContent("Decorator");
        private static readonly GUIContent GC_ADD_TASK = new GUIContent("Task");

        private static readonly GUIContent GC_DELETE_NODE = new GUIContent("Delete Node");

        // PROPERTIES: ----------------------------------------------------------------------------

        private BehaviorGraph behaviorGraph;

		public SerializedProperty spRoot { private set; get; }
		public SerializedProperty spNodes { private set; get; }
		
		public SerializedProperty spBlackboard { private set; get; }
		public SerializedProperty spBlackboardList { private set; get; }

        public SerializedProperty spPosition { private set; get; }
        public SerializedProperty spZoom { private set; get; }

        public Dictionary<int, NodeEditor> nodeEditors { private set; get; }

    // INITIALIZERS: --------------------------------------------------------------------------

    private void OnEnable()
		{
			this.behaviorGraph = this.target as BehaviorGraph;
			if (this.behaviorGraph == null) return;

			this.spRoot = serializedObject.FindProperty("root");
			this.spNodes = serializedObject.FindProperty("nodes");
			
			this.spBlackboard = serializedObject.FindProperty("blackboard");
			this.spBlackboardList = this.spBlackboard.FindPropertyRelative("list");

            this.spPosition = serializedObject.FindProperty("position");
            this.spZoom = serializedObject.FindProperty("zoom");

            this.nodeEditors = new Dictionary<int, NodeEditor>();
			for (int i = 0; i < this.behaviorGraph.nodes.Count; ++i)
			{
				int nodeID = this.behaviorGraph.nodes[i].GetInstanceID();
				NodeEditor editor = (NodeEditor)Editor.CreateEditor(this.behaviorGraph.nodes[i]); 
				
				this.nodeEditors.Add(nodeID, editor);
			}
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void CreateConnection(Node nodeOutput, Node nodeInput)
		{
			if (nodeOutput == null) return;
			if (nodeInput == null) return;
            if (!this.IsValidConnection(nodeOutput, nodeInput)) return;

			int nodeOutputID = nodeOutput.GetInstanceID();
			int nodeInputID = nodeInput.GetInstanceID();

			if (nodeInput.input != null)
			{
				int otherInputID = nodeInput.input.GetInstanceID();
				this.nodeEditors[otherInputID].RemoveOutput(nodeInput);
				this.nodeEditors[nodeInputID].SetInput(null);
			}
			
			this.nodeEditors[nodeOutputID].SetOutput(nodeInput);
			this.nodeEditors[nodeInputID].SetInput(nodeOutput);
		}

		public void RearrangeExecutionIndexes(Node node)
		{
			if (node == null) return;
			List<Node> outputs = node.outputs;
			outputs.Sort((a, b) => a.position.x.CompareTo(b.position.x));

		}

		public void DisconnectInput(Node node)
		{
			Node other = node.input;
			if (other != null)
			{
				int otherID = other.GetInstanceID();
				this.nodeEditors[otherID].RemoveOutput(node);
			}

			int nodeID = node.GetInstanceID();
			this.nodeEditors[nodeID].SetInput(null);
		}

		public void DisconnectOutput(Node node)
		{
			for (int i = 0; i < node.outputs.Count; ++i)
			{
				if (node.outputs[i] != null)
				{
					int otherID = node.outputs[i].GetInstanceID();
					this.nodeEditors[otherID].RemoveInput();
				}
			}

			int nodeID = node.GetInstanceID();
			this.nodeEditors[nodeID].RemoveOutputs();
		}

        public void UpdateGraph(Vector2 position, float zoom)
        {
            this.spPosition.vector2Value = position;
            this.spZoom.floatValue = zoom;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        // CREATE NODE METHODS: -------------------------------------------------------------------

        public void ShowCreateMenu(Node parent = null)
		{
			GenericMenu menu = new GenericMenu();

			if (this.behaviorGraph.root != null) menu.AddDisabledItem(GC_ADD_ROOT);
			else menu.AddItem(GC_ADD_ROOT, false, this.CreateRoot, parent);
				
			menu.AddItem(GC_ADD_COMPOSITE, false, this.CreateComposite, parent);
			menu.AddItem(GC_ADD_TASK, false, this.CreateTask, parent);
            menu.AddSeparator(string.Empty);
            menu.AddItem(GC_ADD_DECORATOR, false, this.CreateDecorator, parent);
            menu.AddItem(GC_ADD_BEHAVIOR_GRAPH, false, this.CreateBehaviorGraph, parent);
				
			menu.ShowAsContext();
		}

        public void ShowNodeMenu(Node node)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(GC_DELETE_NODE, false, this.RemoveNode, node);

            menu.ShowAsContext();
        }

        public void CreateRoot(object parent = null)
		{
			NodeRoot node = this.CreateNode<NodeRoot>(parent as Node);
			this.spRoot.objectReferenceValue = node;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            this.serializedObject.Update();
		}

		public void CreateComposite(object parent = null)
		{
			this.CreateNode<NodeComposite>(parent as Node);
		}

		public void CreateTask(object parent = null)
		{
			this.CreateNode<NodeTask>(parent as Node);
		}

        public void CreateDecorator(object parent = null)
        {
            this.CreateNode<NodeDecorator>(parent as Node);
        }

        public void CreateBehaviorGraph(object parent = null)
        {
            this.CreateNode<NodeBehaviorGraph>(parent as Node);
        }

        public void RemoveNode(object parameter)
		{
            Node node = (Node)parameter;
			int nodeID = node.GetInstanceID();

			if (node.input != null)
			{
				Node parent = node.input;
				this.nodeEditors[parent.GetInstanceID()].RemoveOutput(node);
			}
			
			this.nodeEditors[nodeID].RemoveInput();
			this.nodeEditors[nodeID].RemoveOutputs();
            this.nodeEditors[nodeID].OnDestroyNode();


            int nodeIndex = this.behaviorGraph.nodes.FindIndex(item => item == node);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            this.serializedObject.Update();

			this.spNodes.GetArrayElementAtIndex(nodeIndex).objectReferenceValue = null;
			this.spNodes.DeleteArrayElementAtIndex(nodeIndex);

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            this.serializedObject.Update();
			
			this.nodeEditors.Remove(nodeID);
			DestroyImmediate(node, true);
			
			string graphPath = AssetDatabase.GetAssetPath(this.behaviorGraph);
			AssetDatabase.ImportAsset(graphPath);
		}
		
		// PRIVATE METHODS: -----------------------------------------------------------------------

		private T CreateNode<T>(Node parent = null) where T : Node
		{
			T node = ScriptableObject.CreateInstance<T>();

			NodeEditor editor = (NodeEditor)Editor.CreateEditor(node);

            node.name = editor.GetName();
            node.hideFlags = HideFlags.HideInHierarchy;
            node.graph = this.behaviorGraph;
			node.position = (
				BehaviorWindowEvents.MOUSE_POSITION -
				new Vector2(editor.GetNodeWidth() / 2.0f, 0.0f)
			);
			
			Debug.Log("Parent: " + this.serializedObject.targetObject.name);
			Debug.Log("Child: " + node.name);

			string assetPath = AssetDatabase.GetAssetPath(this.behaviorGraph);
			Debug.Log("Path: " + assetPath);
			
			AssetDatabase.AddObjectToAsset(node, assetPath);
			AssetDatabase.ImportAsset(assetPath);
			// AssetDatabase.SaveAssets();
			
			this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            this.serializedObject.Update();
			
			this.spNodes.AddToObjectArray(node);
			this.nodeEditors.Add(node.GetInstanceID(), editor);
			if (parent != null)
			{
				this.CreateConnection(parent, node);
				this.RearrangeExecutionIndexes(parent);
			}

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            this.serializedObject.Update();

            Selection.activeObject = node;

            return node;
		}

		private bool IsValidConnection(Node nodeOutput, Node nodeInput)
        {
            for (int i = 0; i < nodeInput.outputs.Count; ++i)
            {
                bool valid = this.IsValidConnection(nodeInput.outputs[i], nodeOutput);
                if (!valid) return false;
            }

            while (nodeOutput.input != null)
            {
                if (nodeOutput == nodeInput) return false;
                nodeOutput = nodeOutput.input;
            }

            return true;
        }

        // INSPECTOR PAINT METHOD: ----------------------------------------------------------------

        public override void OnInspectorGUI()
		{
            if (GUILayout.Button("Open"))
            {
                if (this.target == null) return;

                int instanceID = this.target.GetInstanceID();
                BehaviorWindow.OnOpenAsset(instanceID, 0);
            }
		}
	}
}