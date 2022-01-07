namespace GameCreator.Stats
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;
    using GameCreator.Variables;

	#if UNITY_EDITOR
	using UnityEditor;
	#endif

	[AddComponentMenu("")]
    public class ConditionAttr : ICondition
	{
        public enum Comparison
        {
            ValueIsGreaterOrEqual,
            ValueIsLessOrEqual,
            PercentIsGreaterOrEqual,
            PercentIsLessOrEqual,
        }

        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);

        [AttributeSelector]
        public AttrAsset attribute;

        public Comparison compare = Comparison.ValueIsGreaterOrEqual;

        public NumberProperty value = new NumberProperty(1f);
        [Range(0.0f, 1.0f)] public float percent = 0.5f;

		// EXECUTABLE: ----------------------------------------------------------------------------

		public override bool Check(GameObject target)
		{
            GameObject targetGO = this.target.GetGameObject(target);
            if (!targetGO)
            {
                Debug.LogError("Condition Attribute: No target defined");
                return false;
            }

            Stats stats = targetGO.GetComponentInChildren<Stats>();
            if (!stats)
            {
                Debug.LogError("Condition Attribute: Could not get Stats component in target", targetGO);
                return false;
            }

            switch (this.compare)
            {
                case Comparison.ValueIsGreaterOrEqual:
                    return stats.GetAttrValue(this.attribute.attribute.uniqueName) >= this.value.GetValue(target);
                case Comparison.ValueIsLessOrEqual:
                    return stats.GetAttrValue(this.attribute.attribute.uniqueName) <= this.value.GetValue(target);
                case Comparison.PercentIsGreaterOrEqual:
                    return stats.GetAttrValuePercent(this.attribute.attribute.uniqueName) >= this.percent;
                case Comparison.PercentIsLessOrEqual:
                    return stats.GetAttrValuePercent(this.attribute.attribute.uniqueName) <= this.percent;
            }

            return false;
		}

		// +--------------------------------------------------------------------------------------+
		// | EDITOR                                                                               |
		// +--------------------------------------------------------------------------------------+

		#if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Stats/Icons/Conditions/";

		public static new string NAME = "Stats/Attribute Value";
        private const string NODE_TITLE = "Attribute {0} of {1} {2} than {3}";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spTarget;
        private SerializedProperty spAttr;

        private SerializedProperty spCompare;
        private SerializedProperty spValue;
        private SerializedProperty spPercent;

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle()
		{
            string statName = (!this.attribute ? "(none)" : this.attribute.attribute.uniqueName);
            string compareName = this.spCompare.enumDisplayNames[(int)this.compare];

            string valueName = ((this.compare == Comparison.ValueIsGreaterOrEqual || this.compare == Comparison.ValueIsLessOrEqual)
                ? this.value.ToString()
                : this.percent.ToString("0%")
            );

            return string.Format(
                NODE_TITLE,
                statName,
                this.target.ToString(),
                compareName,
                valueName
            );
		}

		protected override void OnEnableEditorChild ()
		{
            this.spTarget = this.serializedObject.FindProperty("target");
            this.spAttr = this.serializedObject.FindProperty("attribute");
            this.spCompare = this.serializedObject.FindProperty("compare");
            this.spValue = this.serializedObject.FindProperty("value");
            this.spPercent = this.serializedObject.FindProperty("percent");
		}

		protected override void OnDisableEditorChild ()
		{
            this.spTarget = null;
            this.spAttr = null;
            this.spCompare = null;
            this.spValue = null;
            this.spPercent = null;
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spTarget);
            EditorGUILayout.PropertyField(this.spAttr);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spCompare);

            switch (this.spCompare.intValue)
            {
                case (int)Comparison.ValueIsGreaterOrEqual:
                case (int)Comparison.ValueIsLessOrEqual:
                    EditorGUILayout.PropertyField(this.spValue);
                    break;

                case (int)Comparison.PercentIsGreaterOrEqual:
                case (int)Comparison.PercentIsLessOrEqual:
                    EditorGUILayout.PropertyField(this.spPercent);
                    break;
            }

			this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}
