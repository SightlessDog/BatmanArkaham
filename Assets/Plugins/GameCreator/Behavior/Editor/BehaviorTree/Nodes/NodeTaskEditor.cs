namespace GameCreator.Behavior
{
	using UnityEngine;
	using UnityEditor;

	[CustomEditor(typeof(NodeTask))]
	public class NodeTaskEditor : NodeEditor
	{
        public override string GetName()
		{
            string customName = this.spEditorName.stringValue;
            return (string.IsNullOrEmpty(customName) ? "Task" : customName);
		}

		protected override bool HasOutput()
		{
			return false;
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

            this.PaintInspectorConditions();
            EditorGUILayout.Space();

            this.PaintInspectorActions();
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }
    }
}