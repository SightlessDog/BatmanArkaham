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
	public class ConditionStat : ICondition
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

        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);

        [StatSelector]
        public StatAsset stat;

        public Comparison compare = Comparison.Less;
        public NumberProperty value = new NumberProperty(1f);

		// EXECUTABLE: ----------------------------------------------------------------------------

		public override bool Check(GameObject target)
		{
            GameObject targetGO = this.target.GetGameObject(target);
            if (!targetGO)
            {
                Debug.LogError("Condition Stat: No target defined");
                return false;
            }

            Stats stats = targetGO.GetComponentInChildren<Stats>();
            if (!stats)
            {
                Debug.LogError("Condition Stat: Could not get Stats component in target");
                return false;
            }

            float current = stats.GetStat(this.stat.stat.uniqueName);
            switch (this.compare)
            {
                case Comparison.Greater:        return current > value.GetValue(target);
                case Comparison.Less:           return current < value.GetValue(target);
                case Comparison.Equal:          return Mathf.Approximately(current, value.GetValue(target));
                case Comparison.Different:      return !Mathf.Approximately(current, value.GetValue(target));
                case Comparison.GreaterOrEqual: return current >= value.GetValue(target);
                case Comparison.LessOrEqual:    return current <= value.GetValue(target);
            }

            return false;
		}

		// +--------------------------------------------------------------------------------------+
		// | EDITOR                                                                               |
		// +--------------------------------------------------------------------------------------+

		#if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Stats/Icons/Conditions/";

		public static new string NAME = "Stats/Stat Value";
        private const string NODE_TITLE = "Stat {0} of {1} is {2} than {3}";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spTarget;
		private SerializedProperty spStat;

        private SerializedProperty spCompare;
        private SerializedProperty spValue;

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle()
		{
            string statName = (!this.stat ? "(none)" : this.stat.stat.uniqueName);
            string compareName = this.spCompare.enumDisplayNames[(int)this.compare];

            return string.Format(
                NODE_TITLE,
                statName,
                this.target.ToString(),
                compareName,
                this.value.ToString()
            );
		}

		protected override void OnEnableEditorChild ()
		{
            this.spTarget = this.serializedObject.FindProperty("target");
            this.spStat = this.serializedObject.FindProperty("stat");
            this.spCompare = this.serializedObject.FindProperty("compare");
            this.spValue = this.serializedObject.FindProperty("value");
		}

		protected override void OnDisableEditorChild ()
		{
            this.spTarget = null;
            this.spStat = null;
            this.spCompare = null;
            this.spValue = null;
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spTarget);
            EditorGUILayout.PropertyField(this.spStat);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spCompare);
            EditorGUILayout.PropertyField(this.spValue);

			this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}
