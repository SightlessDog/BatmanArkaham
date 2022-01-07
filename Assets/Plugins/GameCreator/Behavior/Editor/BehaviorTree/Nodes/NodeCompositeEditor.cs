namespace GameCreator.Behavior
{
	using UnityEngine;
	using UnityEditor;

	[CustomEditor(typeof(NodeComposite))]
	public class NodeCompositeEditor : NodeEditor
	{
        private const float ICON_PADDING_LEFT = 10f;
        private const float ICON_PADDING_TOP = 4f;

        private BehaviorResources.Name[] ICON = new BehaviorResources.Name[]
        {
            BehaviorResources.Name.CompositeSelector,
            BehaviorResources.Name.CompositeSequence,
            BehaviorResources.Name.CompositeRandomSelector,
            BehaviorResources.Name.CompositeRandomSequence,
            BehaviorResources.Name.CompositeParallel
        };

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spComposite;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected override void OnEnable()
        {
            // if (target == null) return;

            base.OnEnable();
            this.spComposite = serializedObject.FindProperty("composite");
        }

        // VIRTUAL METHODS: -----------------------------------------------------------------------

        public override string GetName()
		{
            int index = this.spComposite.enumValueIndex;
            return this.spComposite.enumDisplayNames[index];
        }

		protected override OutputType GetOutputType()
		{
			return OutputType.Multiple;
		}

        protected override Rect PaintHead()
        {
            Rect rect = base.PaintHead();

            float size = this.GetHeadHeight() - (ICON_PADDING_TOP * 2f);
            Rect iconRect = new Rect(
                rect.x + ICON_PADDING_LEFT,
                rect.y + (rect.height / 2f - size / 2f),
                size,
                size
            );

            Texture2D texture = BehaviorResources.GetTexture(
                ICON[this.spComposite.enumValueIndex],
                BehaviorResources.Format.Auto
            );

            GUI.DrawTexture(iconRect, texture);
            
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
            this.PaintInspectorComposite();

            this.PaintInspectorConditions();
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private void PaintInspectorComposite()
        {
            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
            EditorGUILayout.PropertyField(this.spComposite);
            EditorGUILayout.EndVertical();
        }
    }
}