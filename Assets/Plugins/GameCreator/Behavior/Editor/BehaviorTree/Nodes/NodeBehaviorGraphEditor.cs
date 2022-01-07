namespace GameCreator.Behavior
{
	using UnityEngine;
	using UnityEditor;

	[CustomEditor(typeof(NodeBehaviorGraph))]
	public class NodeBehaviorGraphEditor : NodeEditor
	{
        private const string NAME_EMPTY = "Behavior Graph (none)";

        private const float BTN_EDIT_WIDTH = 40f;
        private const float BTN_EDIT_HEIGHT = 20f;

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spBehaviorGraph;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected override void OnEnable()
        {
            if (target == null) return;

            base.OnEnable();
            this.spBehaviorGraph = serializedObject.FindProperty("behaviorGraph");
        }

        // VIRTUAL METHODS: -----------------------------------------------------------------------

        public override string GetName()
		{
            if (this.spBehaviorGraph.objectReferenceValue == null) return NAME_EMPTY;
            string graphName = this.spBehaviorGraph.objectReferenceValue.name;
            return ObjectNames.NicifyVariableName(graphName);
        }

        protected override bool HasOutput()
        {
            return false;
        }

        protected override Rect PaintHead()
        {
            Rect rect = base.PaintHead();

            float vOffset = rect.height / 2.0f - BTN_EDIT_HEIGHT / 2.0f;
            Rect rectButton = new Rect(
                rect.x + rect.width - (BTN_EDIT_WIDTH + vOffset),
                rect.y + vOffset,
                BTN_EDIT_WIDTH,
                BTN_EDIT_HEIGHT
            );

            BehaviorGraph graph = this.spBehaviorGraph.objectReferenceValue as BehaviorGraph;

            EditorGUI.BeginDisabledGroup(graph == null);
            if (GUI.Button(rectButton, "Enter")) BehaviorWindow.WINDOW.OpenBehaviorGraphNode(graph);
            EditorGUI.EndDisabledGroup();

            return rect;
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

            this.PaintInspectorHead();
            this.PaintInspector();

            this.PaintInspectorConditions();
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }

        private void PaintInspector()
        {
            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
            EditorGUILayout.PropertyField(this.spBehaviorGraph);
            EditorGUILayout.EndVertical();
        }
    }
}