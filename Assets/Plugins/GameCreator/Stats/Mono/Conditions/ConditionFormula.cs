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
    public class ConditionFormula : ICondition
	{
        public enum Comparison
        {
            Greater,
            Less,
            Equal,
            Different,
            GreaterOrEqual,
            LessOrEqual
        }

        public FormulaAsset formula;

        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);
        public TargetGameObject other = new TargetGameObject(TargetGameObject.Target.Invoker);

        public Comparison compare = Comparison.Less;
        public NumberProperty value = new NumberProperty(1f);

		// EXECUTABLE: ----------------------------------------------------------------------------

		public override bool Check(GameObject target)
		{
            Stats statsTarget = null;
            Stats statsOther = null;

            GameObject targetGO = this.target.GetGameObject(target);
            GameObject otherGO = this.other.GetGameObject(target);

            if (targetGO) statsTarget = targetGO.GetComponentInChildren<Stats>(true);
            if (otherGO) statsOther = otherGO.GetComponentInChildren<Stats>(true);

            float formulaValue = this.formula.formula.Calculate(0.0f, statsTarget, statsOther);

            switch (this.compare)
            {
                case Comparison.Greater:        return formulaValue > value.GetValue(target);
                case Comparison.Less:           return formulaValue < value.GetValue(target);
                case Comparison.Equal:          return Mathf.Approximately(formulaValue, value.GetValue(target));
                case Comparison.Different:      return !Mathf.Approximately(formulaValue, value.GetValue(target));
                case Comparison.GreaterOrEqual: return formulaValue >= value.GetValue(target);
                case Comparison.LessOrEqual:    return formulaValue <= value.GetValue(target);
            }

            return false;
		}

		// +--------------------------------------------------------------------------------------+
		// | EDITOR                                                                               |
		// +--------------------------------------------------------------------------------------+

		#if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Stats/Icons/Conditions/";

		public static new string NAME = "Stats/Formula";
        private const string NODE_TITLE = "Formula {0} is {1} than {2}";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spFormula;

        private SerializedProperty spTarget;
		private SerializedProperty spOther;

        private SerializedProperty spCompare;
        private SerializedProperty spValue;

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle()
		{
            string formulaName = (!this.formula ? "(none)" : this.formula.name);
            string compareName = this.spCompare.enumDisplayNames[(int)this.compare];

            return string.Format(
                NODE_TITLE,
                formulaName,
                compareName,
                this.value.ToString()
            );
		}

		protected override void OnEnableEditorChild ()
		{
            this.spFormula = this.serializedObject.FindProperty("formula");
            this.spTarget = this.serializedObject.FindProperty("target");
            this.spOther = this.serializedObject.FindProperty("other");
            this.spCompare = this.serializedObject.FindProperty("compare");
            this.spValue = this.serializedObject.FindProperty("value");
		}

		protected override void OnDisableEditorChild ()
		{
            this.spFormula = null;
            this.spTarget = null;
            this.spOther = null;
            this.spCompare = null;
            this.spValue = null;
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spFormula);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.spTarget);
            EditorGUILayout.PropertyField(this.spOther);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spCompare);
            EditorGUILayout.PropertyField(this.spValue);

			this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}
