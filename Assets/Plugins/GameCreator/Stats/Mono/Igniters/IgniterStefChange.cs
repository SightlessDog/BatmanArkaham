namespace GameCreator.Stats
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using GameCreator.Core;

    [AddComponentMenu("")]
    public class IgniterStefChange : Igniter 
	{
        public enum Detection
        {
            OnChange,
            OnIncrease,
            OnDecrease
        }

        #if UNITY_EDITOR
        public new static string NAME = "Stats/On Status Effect Change";
        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Stats/Icons/Igniters/";
        public new static string ICON_PATH = "Assets/Plugins/GameCreator/Stats/Icons/Igniters/";
        #endif

        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);

        [StatusEffectSelector]
        public StatusEffectAsset statusEffect;
        public Detection detect = Detection.OnChange;

        private Stats component;

        private void Start()
        {
            GameObject targetGO = this.target.GetGameObject(gameObject);
            if (!targetGO)
            {
                Debug.LogError("Trigger Status Effect Change: No target defined", null);
                return;
            }

            this.component = targetGO.GetComponentInChildren<Stats>();
            if (!this.component)
            {
                Debug.LogError("Trigger Status Effect Change: Could not get Stats component in target", null);
                return;
            }

            this.component.AddOnChangeStef(this.OnChangeStatusEffect);
        }

        private void OnDestroy()
        {
            if (this.isExitingApplication || !this.component) return;
            this.component.RemoveOnChangeStef(this.OnChangeStatusEffect);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnChangeStatusEffect(Stats.EventArgs args)
        {
            switch (this.detect)
            {
                case Detection.OnChange:
                    if (args.operation == Stats.EventArgs.Operation.Change)
                    {
                        this.ExecuteTrigger(component.gameObject);
                    }
                    break;

                case Detection.OnIncrease:
                    if (args.operation == Stats.EventArgs.Operation.Add)
                    {
                        this.ExecuteTrigger(component.gameObject);
                    }
                    break;

                case Detection.OnDecrease:
                    if (args.operation == Stats.EventArgs.Operation.Remove)
                    {
                        this.ExecuteTrigger(component.gameObject);
                    }
                    break;
            }
        }
    }
}