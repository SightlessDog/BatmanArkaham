namespace GameCreator.Behavior
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
	using UnityEditor;
    using GameCreator.Core;
    using System.Reflection;
    using System.Linq;

    [CustomEditor(typeof(NodeDecorator))]
	public class NodeDecoratorEditor : NodeEditor
	{
        private const string NAME_EMPTY = "No Decorator";
        private const string LABEL_DECORATOR = "Decorator";

        private const BindingFlags BINDING_FLAGS = (
            BindingFlags.Public |
            BindingFlags.Static |
            BindingFlags.FlattenHierarchy
        );

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spDecorator;
        private Editor editorDecorator;

        private List<Type> decoratorTypes = new List<Type>();
        private string[] decoratorTypesNames = new string[0];

        // INITIALIZERS: --------------------------------------------------------------------------

        protected override void OnEnable()
        {
            if (target == null) return;
            base.OnEnable();

            this.spDecorator = serializedObject.FindProperty("decorator");
            this.decoratorTypes = this.GetAllClassTypesOf(typeof(NodeIDecorator));

            this.decoratorTypesNames = new string[this.decoratorTypes.Count + 1];
            this.UpdateFirstDecoratorTypeName();
            this.UpdateDecoratorEditor();

            for (int i = 0; i < this.decoratorTypes.Count; ++i)
            {
                string title = (string)this.decoratorTypes[i]
                    .GetField("NAME", BINDING_FLAGS)
                    .GetValue(null);
                this.decoratorTypesNames[i + 1] = ObjectNames.NicifyVariableName(title);
            }
        }

        // VIRTUAL METHODS: -----------------------------------------------------------------------

        public override string GetName()
		{
            if (this.spDecorator.objectReferenceValue == null) return NAME_EMPTY;
            return ((NodeIDecorator)this.spDecorator.objectReferenceValue).GetName();
        }

        protected override OutputType GetOutputType()
        {
            return OutputType.Single;
        }

        public override float GetNodeWidth()
        {
            return 150f;
        }

        protected override float GetBodyHeight()
        {
            return 0f;
        }

        protected override float GetBottomPadding()
        {
            return 0f;
        }

        public override void OnDestroyNode()
        {
            base.OnDestroyNode();
            if (this.spDecorator.objectReferenceValue != null)
            {
                DestroyImmediate(this.spDecorator.objectReferenceValue, true);
            }
        }

        // INSPECTOR METHODS: ---------------------------------------------------------------------

        public override bool UseDefaultMargins()
        {
            return false;
        }

        public override void OnInspectorGUI()
        {
            if (target == null) return;
            serializedObject.Update();

            this.PaintInspector();

            serializedObject.ApplyModifiedProperties();
        }

        private void PaintInspector()
        {
            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);

            int index = EditorGUILayout.Popup(LABEL_DECORATOR, 0, this.decoratorTypesNames);
            if (index != 0)
            {
                Type type = this.decoratorTypes[index - 1];
                this.AssignDecorator(type);
                this.UpdateFirstDecoratorTypeName();
            }

            if (this.editorDecorator != null)
            {
                EditorGUI.indentLevel++;
                this.editorDecorator.OnInspectorGUI();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void AssignDecorator(Type type)
        {
            if (this.spDecorator.objectReferenceValue != null)
            {
                DestroyImmediate(this.spDecorator.objectReferenceValue, true);
            }

            NodeIDecorator decorator = (NodeIDecorator) ScriptableObject.CreateInstance(type);
            decorator.hideFlags = HideFlags.HideInHierarchy;

            string targetPath = AssetDatabase.GetAssetPath(target);
            AssetDatabase.AddObjectToAsset(decorator, targetPath);
            AssetDatabase.ImportAsset(targetPath);

            this.spDecorator.objectReferenceValue = decorator;

            this.serializedObject.ApplyModifiedProperties();
            this.serializedObject.Update();

            this.UpdateDecoratorEditor();
        }

        private void UpdateFirstDecoratorTypeName()
        {
            if (this.spDecorator.objectReferenceValue == null)
            {
                this.decoratorTypesNames[0] = "(none)";
            }
            else
            {
                NodeIDecorator dectorator = (NodeIDecorator)this.spDecorator.objectReferenceValue;
                this.decoratorTypesNames[0] = dectorator.GetName();
            }
        }

        private void UpdateDecoratorEditor()
        {
            if (this.spDecorator.objectReferenceValue == null)
            {
                this.editorDecorator = null;
                return;
            }

            this.editorDecorator = Editor.CreateEditor(this.spDecorator.objectReferenceValue);
        }

        private List<Type> GetAllClassTypesOf(Type parentType)
        {
            List<Type> result = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; ++i)
            {
                Type[] types = assemblies[i].GetTypes();
                for (int j = 0; j < types.Length; ++j)
                {
                    if (types[j].BaseType == parentType)
                    {
                        result.Add(types[j]);
                    }
                }
            }

            return result;
        }
    }
}