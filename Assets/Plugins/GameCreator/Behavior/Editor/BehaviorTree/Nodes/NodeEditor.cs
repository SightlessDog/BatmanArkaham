namespace GameCreator.Behavior
{
    using System;
    using System.IO;
    using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using GameCreator.Core;

    public class NodeEditor : Editor
	{
        protected static GUIContent GC_NAME = new GUIContent("Name");
        protected static GUIContent GC_USE_CONDITIONS = new GUIContent("Use Conditions");
        protected static GUIContent GC_USE_ACTIONS = new GUIContent("Use Actions");
        protected static GUIContent GC_CONDITIONS = new GUIContent("Conditions");
        protected static GUIContent GC_ACTIONS = new GUIContent("Actions");

        private static GUIContent GC_ERROR = new GUIContent(" UNDEFINED");

        protected static Color COLOR_SECTION_BACKGR = new Color(0, 0, 0, 0.2f);
        protected static Color COLOR_SECTION_BORDER = new Color(0, 0, 0, 0.4f);

        private const string PREFAB_NAME = "{0}.prefab";
        private const string PATH_ACTIONS = "Assets/Plugins/GameCreatorData/Behavior/Actions/";
        private const string PATH_CONDITIONS = "Assets/Plugins/GameCreatorData/Behavior/Conditions/";
        private const string PATH_ERROR_IMG = "Assets/Plugins/GameCreator/Behavior/Icons/Other/Error.png";

        private static readonly Color COLOR_NODE = new Color(0.25f, 0.25f, 0.25f);
        private static readonly Color COLOR_NODE_BORDER = new Color(1f, 1f, 1f, 0.3f);

        private static readonly Color COLOR_BODY = new Color(0.20f, 0.20f, 0.20f);
        private static readonly Color COLOR_BODY_BORDER = new Color(0.15f, 0.15f, 0.15f);

        private static readonly Color SELECT_COLOR = new Color(0.8f, 0.8f, 0.8f);
        private const float SELECT_BORDER_OFFSET = 2f;
        private const float SELECT_BORDER_RADIUS = 7f;

        private static readonly Color COLOR_NOODLE = new Color(0.27f, 0.75f, 1.0f);
        private static readonly Color COLOR_N_DRAG = new Color(0.27f, 0.75f, 1.0f);
		
		private static readonly Color COLOR_INPUT = new Color(0.27f, 0.75f, 1.0f);
		private static readonly Color COLOR_OUTPUT = new Color(0.27f, 0.75f, 1.0f);

        private static readonly Color COLOR_NONE = new Color(0.50f, 0.50f, 0.50f);
        private static readonly Color COLOR_RUNNING = new Color(0.27f, 0.75f, 1.0f);
        private static readonly Color COLOR_SUCCESS = new Color(0.51f, 0.82f, 0.21f);
        private static readonly Color COLOR_FAIL = new Color(0.99f, 0.20f, 0.20f);

        private const float UI_BORDER_WIDTH = 1.5f;
		private const float UI_BORDER_RADIUS = 5f;
		
		private const float IO_SIZE = 16f;
		private const float IO_PADDING = 2f;

        private const float BODY_ITEM_HEIGHT = 22f;
        private const float BODY_ITEM_SEPARATOR = 8f;

        protected enum OutputType
		{
			Single,
			Multiple
		}
		
		// PROPERTIES: ----------------------------------------------------------------------------

		public Node node { private set; get; }

        protected SerializedProperty spEditorName;
        protected SerializedProperty spEditorPosition;

		private SerializedProperty spInput;
		private SerializedProperty spOutputs;
        private SerializedProperty spMode;

        protected SerializedProperty spUseConditionsList;
        protected SerializedProperty spPrefabConditionsList;

        protected IConditionsListEditor editorConditionsList;
        protected IActionsListEditor editorActionsList;

        protected SerializedProperty spUseActionsList;
        protected SerializedProperty spPrefabActionsList;
		
		// INITIALIZERS: --------------------------------------------------------------------------

		protected virtual void OnEnable()
		{
            if (target == null || serializedObject == null) return;
			this.node = (Node) this.target;

            this.spEditorName = serializedObject.FindProperty("editorName");
			this.spEditorPosition = serializedObject.FindProperty("position");

			this.spInput = serializedObject.FindProperty("input");
			this.spOutputs = serializedObject.FindProperty("outputs");
            this.spMode = serializedObject.FindProperty("mode");

            bool updateSerializedObject = false;
			for (int i = this.spOutputs.arraySize - 1; i >= 0; --i)
			{
				SerializedProperty item = this.spOutputs.GetArrayElementAtIndex(i);
				if (item.objectReferenceValue == null)
				{
					this.spOutputs.DeleteArrayElementAtIndex(i);
					updateSerializedObject = true;
				}
			}

            this.spUseConditionsList = serializedObject.FindProperty("useConditionsList");
            this.spPrefabConditionsList = serializedObject.FindProperty("prefabConditionsList");

            this.spUseActionsList = serializedObject.FindProperty("useActionsList");
            this.spPrefabActionsList = serializedObject.FindProperty("prefabActionsList");

            if (updateSerializedObject)
			{
				this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
				this.serializedObject.Update();
			}

            if (GC_ERROR.image == null) GC_ERROR.image = AssetDatabase.LoadAssetAtPath<Texture>(
                PATH_ERROR_IMG
            );
        }

        protected virtual void OnDisable()
		{
			if (target == null || serializedObject == null) return;
		}

        private void UpdateInitConditions()
        {
            if (!this.spUseConditionsList.boolValue) return;
            if (this.spPrefabConditionsList.objectReferenceValue == null)
            {
                string conditionsName = Guid.NewGuid().ToString("N");
                GameCreatorUtilities.CreateFolderStructure(PATH_CONDITIONS);
                string path = Path.Combine(PATH_CONDITIONS, string.Format(PREFAB_NAME, conditionsName));
                path = AssetDatabase.GenerateUniqueAssetPath(path);

                GameObject sceneInstance = new GameObject(conditionsName);
                sceneInstance.AddComponent<IConditionsList>();

                GameObject prefabInstance = PrefabUtility.SaveAsPrefabAsset(sceneInstance, path);
                DestroyImmediate(sceneInstance);

                IConditionsList prefabConditions = prefabInstance.GetComponent<IConditionsList>();
                this.spPrefabConditionsList.objectReferenceValue = prefabConditions;

                this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                this.serializedObject.Update();
            }

            if (this.editorConditionsList == null)
            {
                this.editorConditionsList = Editor.CreateEditor(
                    this.spPrefabConditionsList.objectReferenceValue
                ) as IConditionsListEditor;
            }
        }

        private void UpdateInitActions()
        {
            if (!this.spUseActionsList.boolValue) return;
            if (this.spPrefabActionsList.objectReferenceValue == null)
            {
                string actionsName = Guid.NewGuid().ToString("N");
                GameCreatorUtilities.CreateFolderStructure(PATH_ACTIONS);
                string path = Path.Combine(PATH_ACTIONS, string.Format(PREFAB_NAME, actionsName));
                path = AssetDatabase.GenerateUniqueAssetPath(path);

                GameObject sceneInstance = new GameObject(actionsName);
                sceneInstance.AddComponent<Actions>();

                GameObject prefabInstance = PrefabUtility.SaveAsPrefabAsset(sceneInstance, path);
                DestroyImmediate(sceneInstance);

                Actions prefabActions = prefabInstance.GetComponent<Actions>();
                prefabActions.destroyAfterFinishing = true;
                this.spPrefabActionsList.objectReferenceValue = prefabActions.actionsList;

                this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                this.serializedObject.Update();
            }

            if (this.editorActionsList == null)
            {
                this.editorActionsList = Editor.CreateEditor(
                    this.spPrefabActionsList.objectReferenceValue
                ) as IActionsListEditor;
            }
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public Vector2 GetEditorPosition()
		{
			return this.spEditorPosition.vector2Value;
		}

		public void SetEditorPosition(Vector2 position)
		{
			serializedObject.Update();
			this.spEditorPosition.vector2Value = new Vector2(
                Mathf.Round(position.x),
                Mathf.Round(position.y)
            );
			serializedObject.ApplyModifiedPropertiesWithoutUndo();
		}

        // INSPECTOR METHODS: ---------------------------------------------------------------------

        protected void PaintInspectorHead()
        {
            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
            EditorGUILayout.PropertyField(this.spEditorName, GC_NAME);
            EditorGUILayout.EndVertical();
        }

        protected void PaintInspectorConditions()
        {
            this.PaintSection(GC_CONDITIONS);
            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
            EditorGUILayout.PropertyField(this.spUseConditionsList, GC_USE_CONDITIONS);

            EditorGUI.BeginDisabledGroup(!this.spUseConditionsList.boolValue);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(this.spMode);
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();

            if (this.spUseConditionsList.boolValue)
            {
                this.UpdateInitConditions();
                EditorGUILayout.BeginVertical();
                this.editorConditionsList.OnInspectorGUI();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        protected void PaintInspectorActions()
        {
            this.PaintSection(GC_ACTIONS);
            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
            EditorGUILayout.PropertyField(this.spUseActionsList, GC_USE_ACTIONS);
            if (this.spUseActionsList.boolValue)
            {
                this.UpdateInitActions();
                this.editorActionsList.OnInspectorGUI();
            }
            EditorGUILayout.EndVertical();
        }

        // PRIVATE INSPECTOR METHODS: -------------------------------------------------------------

        private void PaintSection(GUIContent content)
        {
            Rect rect = GUILayoutUtility.GetRect(content, BehaviorStyles.GetInspectorSection());
            rect.xMin -= 4f;
            rect.xMax += 4f;

            GUI.DrawTexture(
                rect, Texture2D.whiteTexture, ScaleMode.StretchToFill,
                false, 1f, COLOR_SECTION_BACKGR, 0f, 0f
            );

            GUI.DrawTexture(
                rect, Texture2D.whiteTexture, ScaleMode.StretchToFill,
                false, 1f, COLOR_SECTION_BORDER, new Vector4(0, 1, 0, 1), 0f
            );

            EditorGUI.LabelField(rect, content, BehaviorStyles.GetInspectorSection());
        }

        // VIRTUAL METHODS: -----------------------------------------------------------------------

        public virtual string GetName()
		{
			return "Node";
		}
		
		public virtual float GetNodeWidth()
		{
			return 200;
		}

		protected virtual float GetHeadHeight()
		{
			return 30f;
		}
		
		protected virtual float GetBodyHeight()
		{
            float height = 0f;
            bool conditions = false;
            bool actions = false;

            if (this.ConditionsAvailable())
            {
                int count = this.editorConditionsList.subEditors.Length;
                height += count * BODY_ITEM_HEIGHT;
                conditions = true;
            }

            if (this.ActionsAvailable())
            {
                int count = this.editorActionsList.subEditors.Length;
                height += count * BODY_ITEM_HEIGHT;
                actions = true;
            }

            if (conditions && actions) height += BODY_ITEM_SEPARATOR;

            return height;
        }

		protected virtual float GetBottomPadding()
		{
			return 8f;
		}

		protected virtual bool HasInput()
		{
			return true;
		}
		
		protected virtual bool HasOutput()
		{
			return true;
		}

		protected virtual OutputType GetOutputType()
		{
			return OutputType.Multiple;
		}

		protected virtual Rect PaintHead()
		{
			float height = this.GetHeadHeight();
			Rect rect = GUILayoutUtility.GetRect(
				0f, 4000f,
				height, height	
			);
			
			EditorGUI.LabelField(rect, this.GetName(), BehaviorStyles.GetNodeHead());
			return rect;
		}
		
		protected virtual Rect PaintBody()
		{
			float height = this.GetBodyHeight();
			Rect rect = GUILayoutUtility.GetRect(
				0f, 4000f,
				height, height
			);

            Rect rectBorderTop = new Rect(rect.x, rect.y, rect.width, 1f);
            Rect rectBorderBot = new Rect(rect.x, rect.y + rect.height - 1f, rect.width, 1f);

            GUI.DrawTexture(
				rect, Texture2D.whiteTexture, 
				ScaleMode.StretchToFill, false, 1f, 
				COLOR_BODY, 0f, 0f
			);

            GUI.DrawTexture(
                rectBorderTop, Texture2D.whiteTexture, ScaleMode.StretchToFill, 
                false, 1f, COLOR_BODY_BORDER, 0f, 0f
            );

            GUI.DrawTexture(
                rectBorderBot, Texture2D.whiteTexture, ScaleMode.StretchToFill,
                false, 1f, COLOR_BODY_BORDER, 0f, 0f
            );

            Rect rectContent = new Rect(rect);
            rectContent = this.PaintBodyConditions(rectContent);
            rectContent = this.PaintSeparator(rectContent);
            rectContent = this.PaintBodyActions(rectContent);

            return rect;
		}

        public virtual void OnDestroyNode()
        {
            return;
        }

        // PAINT NODE METHODS: --------------------------------------------------------------------

        public Rect PaintNode(BehaviorWindow window)
		{
			if (target == null || serializedObject == null) return Rect.zero;
			serializedObject.Update();

            Rect nodeRect = this.GetNodeRect(window, this.node);
			GUI.DrawTexture(
				nodeRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 1f,
				COLOR_NODE, 0f, UI_BORDER_RADIUS
			);

            GUILayout.BeginArea(nodeRect);
			
			this.PaintHead();
			this.PaintBody();
			
			GUILayout.EndArea();

            if (Selection.Contains(this.node))
            {
                Rect nodeSelectRect = new Rect(
                    nodeRect.x - SELECT_BORDER_OFFSET,
                    nodeRect.y - SELECT_BORDER_OFFSET,
                    nodeRect.width + (SELECT_BORDER_OFFSET * 2),
                    nodeRect.height + (SELECT_BORDER_OFFSET * 2)
                );

                this.PaintNodeHighlight(nodeSelectRect, SELECT_COLOR, SELECT_BORDER_RADIUS);
            }

            if (EditorApplication.isPlaying)
            {
                this.PaintRuntimeState(nodeRect);
            }

			this.PaintNodeOutputConnections(window);

            GUI.DrawTexture(
                nodeRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 1f,
                COLOR_NODE_BORDER, 1f, UI_BORDER_RADIUS
            );

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
			return nodeRect;
		}
		
		public Rect PaintNodeInput(BehaviorWindow window)
		{
			if (!this.HasInput()) return Rect.zero;
			Rect inputRect = this.GetNodeInputRect(window, this.node);
			
			Texture2D texture = BehaviorResources.GetTexture(
				BehaviorResources.Name.InputEmpty,
				BehaviorResources.Format.Auto
			);
			
			if (this.node.input != null)
			{
				texture = BehaviorResources.GetTexture(
					BehaviorResources.Name.InputLink,
					BehaviorResources.Format.Auto
				);
			}

            Color color = this.GetCurrentStateColor(
                (this.node.input == null ? Color.white : COLOR_INPUT)
            );

            GUI.DrawTexture(
                inputRect, texture, ScaleMode.StretchToFill, 
                true, 1.0f, color, 0f, 0f
            );

			if (this.node.input != null)
			{
				int value = this.node.input.outputs.FindIndex(node => node == this.node) + 1;
				EditorGUI.LabelField(inputRect, value.ToString(), BehaviorStyles.GetNodeInput());
			}
			
			return inputRect;
		}
		
		public Rect PaintNodeOutput(BehaviorWindow window)
		{
			if (!this.HasOutput()) return Rect.zero;
			Rect outputRect = this.GetNodeOutputRect(window, this.node);
			
			Texture2D texture = BehaviorResources.GetTexture(
				BehaviorResources.Name.OutputEmpty,
				BehaviorResources.Format.Auto
			);

			if (this.node.outputs.Count > 0)
			{
				texture = BehaviorResources.GetTexture(
					BehaviorResources.Name.OutputLink,
					BehaviorResources.Format.Auto
				);
			}

            Color color = this.GetCurrentStateColor(
                (this.node.outputs.Count == 0 ? Color.white : COLOR_OUTPUT)
            );

			GUI.DrawTexture(
                outputRect, texture, ScaleMode.StretchToFill, 
                true, 1.0f, color, 0f, 0f
            );

			return outputRect;
		}

		public void PaintDragConnection(BehaviorWindow window)
		{
			Rect rectA = this.GetNodeOutputRect(window, this.node);
			Vector2 pointA = rectA.position + new Vector2(rectA.width / 2f, rectA.height / 2f);
			Vector2 pointB = UnityEngine.Event.current.mousePosition;
			
            Texture2D texture = BehaviorResources.GetTexture(
                BehaviorResources.Name.Noodle, 
                BehaviorResources.Format.LowRes
            );

            Handles.color = COLOR_N_DRAG * new Color(1,1,1, 0.5f);
            Handles.DrawAAPolyLine(texture, 2, pointA, pointB);
		}

		public void SetInput(Node node)
		{
			serializedObject.ApplyModifiedPropertiesWithoutUndo();
			serializedObject.Update();
			
			this.spInput.objectReferenceValue = node;
			
			serializedObject.ApplyModifiedPropertiesWithoutUndo();
			serializedObject.Update();
		}

		public void SetOutput(Node node)
		{
			serializedObject.ApplyModifiedPropertiesWithoutUndo();
			serializedObject.Update();

			switch (this.GetOutputType())
			{
				case OutputType.Single:
					this.RemoveOutputs();
					this.spOutputs.AddToObjectArray(node);
					break;
				
				case OutputType.Multiple:
					this.spOutputs.AddToObjectArray(node);
					break;
			}
			
			serializedObject.ApplyModifiedPropertiesWithoutUndo();
			serializedObject.Update();
		}

		public void RemoveInput()
		{
			serializedObject.ApplyModifiedPropertiesWithoutUndo();
			serializedObject.Update();
			
			this.spInput.objectReferenceValue = null;
			
			serializedObject.ApplyModifiedPropertiesWithoutUndo();
			serializedObject.Update();
		}

		public void RemoveOutput(Node node)
		{
			serializedObject.ApplyModifiedPropertiesWithoutUndo();
			serializedObject.Update();

			for (int i = this.spOutputs.arraySize - 1; i >= 0; --i)
			{
				SerializedProperty spOutput = this.spOutputs.GetArrayElementAtIndex(i);
				if (spOutput.objectReferenceValue == null ||
				    spOutput.objectReferenceValue.GetInstanceID() == node.GetInstanceID())
				{
					spOutput.objectReferenceValue = null;
					spOutputs.DeleteArrayElementAtIndex(i);
					break;
				}
			}
			
			serializedObject.ApplyModifiedPropertiesWithoutUndo();
			serializedObject.Update();
		}

		public void RemoveOutputs()
		{
			serializedObject.ApplyModifiedPropertiesWithoutUndo();
			serializedObject.Update();
			
			this.spOutputs.ClearArray();
			
			serializedObject.ApplyModifiedPropertiesWithoutUndo();
			serializedObject.Update();
		}
		
		public float GetNodeHeight()
		{
			return (
				this.GetHeadHeight() +
				this.GetBodyHeight() +
				this.GetBottomPadding()
			);
		}
		
		// PRIVATE METHODS: -----------------------------------------------------------------------

		private void PaintNodeHighlight(Rect rect, Color color, float radius = UI_BORDER_RADIUS)
		{
			GUI.DrawTexture(
				rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, 
				false, 1.0f, color, UI_BORDER_WIDTH, radius
            );
		}

        private void PaintRuntimeState(Rect rect)
        {
            if (BehaviorWindow.WINDOW == null) return;
            if (BehaviorWindow.WINDOW.behaviorGraphEditor == null) return;
            if (BehaviorWindow.WINDOW.runtimeBehavior == null) return;

            int instanceID = this.node.GetInstanceID();
            Node.Return state = BehaviorWindow.WINDOW.runtimeBehavior.GetState(instanceID);
            if (state != Node.Return.None)
            {
                this.PaintNodeHighlight(rect, this.GetCurrentStateColor(COLOR_NONE));
            }
        }

        private void PaintNodeOutputConnections(BehaviorWindow window)
		{
			Rect rectA = this.GetNodeOutputRect(window, this.node);
			Vector2 pointA = rectA.position + new Vector2(rectA.width / 2f, rectA.height / 2f);
			
			for (int i = 0; i < this.node.outputs.Count; ++i)
			{
				if (this.node.outputs[i] == null) continue;
				
				Rect rectB = this.GetNodeInputRect(window, this.node.outputs[i]);
				Vector2 pointB = rectB.position + new Vector2(rectB.width / 2f, rectB.height / 2f);

                Texture2D texture = BehaviorResources.GetTexture(
                    BehaviorResources.Name.Noodle,
                    BehaviorResources.Format.LowRes
                );

                int outputID = this.node.outputs[i].GetInstanceID();
                Handles.color = this.GetCurrentStateColor(COLOR_NOODLE, outputID);
				Handles.DrawAAPolyLine(texture, 2, pointA, pointB);
			}
        }

		private Rect GetNodeRect(BehaviorWindow window, Node node)
		{
			Vector2 position = window.GridToWindowPositionNoClipped(node.position);
			int nodeID = node.GetInstanceID();
			
			return new Rect(
				position.x,
				position.y,
				window.behaviorGraphEditor.nodeEditors[nodeID].GetNodeWidth(),
				window.behaviorGraphEditor.nodeEditors[nodeID].GetNodeHeight()
			);
		}
		
		private Rect GetNodeInputRect(BehaviorWindow window, Node node)
		{
			Rect nodeRect = this.GetNodeRect(window, node);
			return new Rect(
				nodeRect.x + nodeRect.width/2.0f - IO_SIZE/2f,
				nodeRect.y - (IO_SIZE + IO_PADDING),
				IO_SIZE,
				IO_SIZE
			);
		}

		private Rect GetNodeOutputRect(BehaviorWindow window, Node node)
		{
			Rect nodeRect = this.GetNodeRect(window, node);
			return new Rect(
				nodeRect.x + nodeRect.width / 2.0f - IO_SIZE / 2f,
				nodeRect.y + nodeRect.height + IO_PADDING,
				IO_SIZE,
				IO_SIZE
			);
		}

        protected bool ActionsAvailable()
        {
            if (!this.spUseActionsList.boolValue) return false;
            if (this.editorActionsList == null) return false;

            this.editorActionsList.UpdateSubEditors(this.node.prefabActionsList.actions);
            if (this.editorActionsList.subEditors.Length == 0) return false;

            return true;
        }

        protected bool ConditionsAvailable()
        {
            if (!this.spUseConditionsList.boolValue) return false;
            if (this.editorConditionsList == null) return false;
            if (this.editorConditionsList.subEditors.Length == 0) return false;

            return true;
        }

        protected Rect PaintBodyConditions(Rect rect)
        {
            this.UpdateInitConditions();
            if (this.editorConditionsList != null)
            {
                this.editorConditionsList.serializedObject.Update();
                this.editorConditionsList.UpdateSubEditors(this.node.prefabConditionsList.conditions);
            }

            if (!this.ConditionsAvailable()) return rect;

            Rect nextRect = new Rect(rect.x, rect.y, rect.width, BODY_ITEM_HEIGHT);
            int size = this.editorConditionsList.subEditors.Length;

            for (int i = 0; i < size; ++i)
            {
                GUIContent content = GC_ERROR;
                if (this.editorConditionsList.subEditors[i] != null &&
                    this.editorConditionsList.subEditors[i].condition != null)
                {
                    content = new GUIContent(
                        " " + this.editorConditionsList.subEditors[i].condition.GetNodeTitle(),
                        this.editorConditionsList.subEditors[i].GetIcon()
                    );
                }
                
                EditorGUI.LabelField(nextRect, content, BehaviorStyles.GetNodeAction());
                nextRect.position = new Vector2(nextRect.x, nextRect.y + nextRect.height);
            }

            return nextRect;
        }

        protected Rect PaintBodyActions(Rect rect)
        {
            this.UpdateInitActions();
            if (this.editorActionsList != null)
            {
                this.editorActionsList.serializedObject.Update();
                this.editorActionsList.UpdateSubEditors(this.node.prefabActionsList.actions);
            }

            if (!this.ActionsAvailable()) return rect;

            Rect nextRect = new Rect(rect.x, rect.y, rect.width, BODY_ITEM_HEIGHT);
            int size = this.editorActionsList.subEditors.Length;

            for (int i = 0; i < size; ++i)
            {
                GUIContent content = GC_ERROR;
                if (this.editorActionsList.subEditors[i] != null &&
                    this.editorActionsList.subEditors[i].action != null)
                {
                    content = new GUIContent(
                        " " + this.editorActionsList.subEditors[i].action.GetNodeTitle(),
                        this.editorActionsList.subEditors[i].GetIcon()
                    );
                }

                EditorGUI.LabelField(nextRect, content, BehaviorStyles.GetNodeAction());
                nextRect.position = new Vector2(nextRect.x, nextRect.y + nextRect.height);
            }

            return nextRect;
        }

        protected Rect PaintSeparator(Rect rect)
        {
            if (!this.ConditionsAvailable() || !this.ActionsAvailable()) return rect;
            Rect separator = new Rect(
                rect.x,
                rect.y,
                rect.width,
                BODY_ITEM_SEPARATOR
            );

            Rect borderTop = new Rect(separator.x, separator.y, separator.width, 1f);
            Rect borderLow = new Rect(separator.x, separator.y + separator.height - 1f, separator.width, 1f);

            GUI.DrawTexture(
                separator, Texture2D.whiteTexture, ScaleMode.StretchToFill,
                false, 1f, COLOR_NODE, 0f, 0f
            );

            GUI.DrawTexture(
                borderTop, Texture2D.whiteTexture, ScaleMode.StretchToFill,
                false, 1f, COLOR_BODY_BORDER, 0f, 0f
            );

            GUI.DrawTexture(
                borderLow, Texture2D.whiteTexture, ScaleMode.StretchToFill,
                false, 1f, COLOR_BODY_BORDER, 0f, 0f
            );

            separator.position = new Vector2(separator.x, separator.y + separator.height);
            return separator;
        }

        protected Color GetCurrentStateColor(Color fallback, int instanceID = 0)
        {
            if (!EditorApplication.isPlaying) return fallback;

            if (BehaviorWindow.WINDOW == null) return COLOR_NONE;
            if (BehaviorWindow.WINDOW.behaviorGraphEditor == null) return COLOR_NONE;
            if (BehaviorWindow.WINDOW.runtimeBehavior == null) return COLOR_NONE;

            instanceID = (instanceID == 0 ? this.node.GetInstanceID() : instanceID);
            Node.Return state = BehaviorWindow.WINDOW.runtimeBehavior.GetState(instanceID);

            switch (state)
            {
                case Node.Return.Running: return COLOR_RUNNING;
                case Node.Return.Success: return COLOR_SUCCESS;
                case Node.Return.Fail: return COLOR_FAIL;
                case Node.Return.None: return COLOR_NONE;
                default: return fallback;
            }
        }
    }
}