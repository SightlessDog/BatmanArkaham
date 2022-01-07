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
    public class ConditionStatusEffect : ICondition
	{
        public enum Operation
        {
            HasStatusEffect,
            DoesNotHave
        }

        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);

        public Operation condition = Operation.HasStatusEffect;
        [StatusEffectSelector]
        public StatusEffectAsset statusEffect;

        [Indent] public int minAmount = 1;

		// EXECUTABLE: ----------------------------------------------------------------------------

		public override bool Check(GameObject target)
		{
            GameObject targetGO = this.target.GetGameObject(target);
            if (!targetGO)
            {
                Debug.LogError("Condition Status Effect: No target defined");
                return false;
            }

            Stats stats = targetGO.GetComponentInChildren<Stats>();
            if (!stats)
            {
                Debug.LogError("Condition Status Effect: Could not get Stats component in target");
                return false;
            }

            bool hasStatusEffect = stats.HasStatusEffect(this.statusEffect, this.minAmount);

            if (this.condition == Operation.HasStatusEffect && hasStatusEffect) return true;
            if (this.condition == Operation.DoesNotHave && !hasStatusEffect) return true;

            return false;
		}

		// +--------------------------------------------------------------------------------------+
		// | EDITOR                                                                               |
		// +--------------------------------------------------------------------------------------+

		#if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Stats/Icons/Conditions/";

		public static new string NAME = "Stats/Status Effect";
        private const string NODE_TITLE = "{0} {1} status effect {2}";

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle()
		{
            string statName = (!this.statusEffect
                ? "(none)" 
                : this.statusEffect.statusEffect.uniqueName
            );

            string conditionName = (this.condition == Operation.HasStatusEffect
                ? "Has"
                : "Does not have"
            );

            return string.Format(
                NODE_TITLE,
                conditionName,
                this.target,
                statName
            );
		}

		#endif
	}
}
