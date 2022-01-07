namespace GameCreator.Behavior
{
	using UnityEngine;
	using UnityEditor;

	[CustomEditor(typeof(NodeRoot))]
	public class NodeRootEditor : NodeEditor
	{
		public override string GetName()
		{
			return "Entry";
		}

		public override float GetNodeWidth()
		{
			return 99f;
		}

		protected override float GetBodyHeight()
		{
			return 0f;
		}

		protected override float GetBottomPadding()
		{
			return 0f;
		}

		protected override bool HasInput()
		{
			return false;
		}

		protected override OutputType GetOutputType()
		{
			return OutputType.Single;
		}
		
		protected override Rect PaintBody()
		{
			return Rect.zero;
		}

        // INSPECTOR METHODS: ---------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Root Node", MessageType.Info);
        }
    }
}