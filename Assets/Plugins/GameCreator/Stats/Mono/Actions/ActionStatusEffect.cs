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
    public class ActionStatusEffect : IAction
	{
        public enum Operation
        {
            Add,
            Remove,
            RemoveAll,
            RemovePositives,
            RemoveNegatives,
            RemoveOthers,
        }

        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Invoker);

        public Operation operation = Operation.Add;

        [StatusEffectSelector]
        public StatusEffectAsset statusEffect;

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            GameObject targetGO = this.target.GetGameObject(target);
            if (targetGO == null)
            {
                Debug.LogError("Action Status Effect: No target defined");
                return true;
            }

            Stats stats = targetGO.GetComponentInChildren<Stats>();
            if (stats == null)
            {
                Debug.LogError("Action Status Effect: Could not get Stats component in target");
                return true;
            }

            switch (this.operation)
            {
                case Operation.Add: stats.AddStatusEffect(this.statusEffect); break;
                case Operation.Remove: stats.RemoveStatusEffect(this.statusEffect); break;
                
                case Operation.RemoveAll :
                    stats.RemoveStatusEffect(StatusEffect.Type.Positive);
                    stats.RemoveStatusEffect(StatusEffect.Type.Negative);
                    stats.RemoveStatusEffect(StatusEffect.Type.Other);
                    break;

                case Operation.RemovePositives :
                    stats.RemoveStatusEffect(StatusEffect.Type.Positive);
                    break;

                case Operation.RemoveNegatives:
                    stats.RemoveStatusEffect(StatusEffect.Type.Negative);
                    break;

                case Operation.RemoveOthers:
                    stats.RemoveStatusEffect(StatusEffect.Type.Other);
                    break;
            }

            return true;
        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

        #if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Stats/Icons/Actions/";

        public static new string NAME = "Stats/Status Effect";
        private const string NODE_TITLE = "{0} status effect {1}";

		// PROPERTIES: ----------------------------------------------------------------------------

		private SerializedProperty spTarget;
        private SerializedProperty spOperation;
        private SerializedProperty spStatusEffect;

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle()
		{
            string statusEffectName = "";
            if (this.operation == Operation.Add || this.operation == Operation.Remove)
            {
                statusEffectName = (this.statusEffect == null
                    ? "(none)"
                    : this.statusEffect.statusEffect.uniqueName
                );
            }

            string opName = this.spOperation.enumDisplayNames[(int)this.operation];
            return string.Format(
                NODE_TITLE, 
                opName,
                statusEffectName
            );
		}

		protected override void OnEnableEditorChild ()
		{
            this.spTarget = this.serializedObject.FindProperty("target");
            this.spOperation = this.serializedObject.FindProperty("operation");
            this.spStatusEffect = this.serializedObject.FindProperty("statusEffect");
		}

		protected override void OnDisableEditorChild ()
		{
            this.spTarget = null;
            this.spOperation = null;
            this.spStatusEffect = null;
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spTarget);
            EditorGUILayout.PropertyField(this.spOperation);

            Operation op = (Operation)this.spOperation.intValue;
            if (op == Operation.Add || op == Operation.Remove)
            {
                EditorGUILayout.PropertyField(this.spStatusEffect, true);
            }

			this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}
