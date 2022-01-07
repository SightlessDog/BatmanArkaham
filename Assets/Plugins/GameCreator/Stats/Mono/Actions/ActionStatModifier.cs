namespace GameCreator.Stats
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;

	#if UNITY_EDITOR
	using UnityEditor;
	#endif

	[AddComponentMenu("")]
    public class ActionStatModifier : IAction
	{
        public enum Operation
        {
            Add,
            Remove
        }

        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Invoker);

        public Operation operation = Operation.Add;
        public StatModifier statModifier = new StatModifier();

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            GameObject targetGO = this.target.GetGameObject(target);
            if (targetGO == null)
            {
                Debug.LogError("Action Stats Modifier: No target defined");
                return true;
            }

            Stats stats = targetGO.GetComponentInChildren<Stats>();
            if (stats == null)
            {
                Debug.LogError("Action Stats Modifier: Could not get Stats component in target");
                return true;
            }

            switch (this.operation)
            {
                case Operation.Add: stats.AddStatModifier(this.statModifier); break;
                case Operation.Remove: stats.RemoveStatModifier(this.statModifier); break;
            }

            return true;
        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

        #if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Stats/Icons/Actions/";

        public static new string NAME = "Stats/Stat Modifier";
        private const string NODE_TITLE = "{0} stat modifier {1} {2}";

		// PROPERTIES: ----------------------------------------------------------------------------

		private SerializedProperty spTarget;
        private SerializedProperty spOperation;
        private SerializedProperty spStatModifier;

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle()
		{
            string value = "";
            switch (this.statModifier.type)
            {
                case StatModifier.EffectType.Contant:
                    value = this.statModifier.value.ToString("+#;-#;0");
                    break;

                case StatModifier.EffectType.Percent:
                    value = string.Format("{0:0}%", this.statModifier.value);
                    break;
            }

            string stat = (this.statModifier.stat == null 
                ? "(none)" 
                : this.statModifier.stat.stat.uniqueName
            );

            return string.Format(
                NODE_TITLE, 
                this.operation,
                value,
                stat
            );
		}

		protected override void OnEnableEditorChild ()
		{
            this.spTarget = this.serializedObject.FindProperty("target");
            this.spOperation = this.serializedObject.FindProperty("operation");
            this.spStatModifier = this.serializedObject.FindProperty("statModifier");
		}

		protected override void OnDisableEditorChild ()
		{
            this.spTarget = null;
            this.spStatModifier = null;
            this.spOperation = null;
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spTarget);
            EditorGUILayout.PropertyField(this.spOperation);
            EditorGUILayout.PropertyField(this.spStatModifier, true);

			this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}
