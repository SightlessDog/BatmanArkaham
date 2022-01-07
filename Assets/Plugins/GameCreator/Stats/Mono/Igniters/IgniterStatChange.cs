namespace GameCreator.Stats
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using GameCreator.Core;

    [AddComponentMenu("")]
    public class IgniterStatChange : Igniter
	{
        public enum Detection
        {
            OnChange,
            OnIncrease,
            OnDecrease
        }

		#if UNITY_EDITOR
        public new static string NAME = "Stats/On Stat Change";
        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Stats/Icons/Igniters/";
        public new static string ICON_PATH = "Assets/Plugins/GameCreator/Stats/Icons/Igniters/";
        #endif

        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);

        [StatSelector]
        public StatAsset stat;
        public Detection detect = Detection.OnChange;

        private Stats component;
        private float value;

        private void Start()
        {
            GameObject targetGO = this.target.GetGameObject(gameObject);
            if (!targetGO)
            {
                Debug.LogError("Trigger Stat Change: No target defined", null);
                return;
            }

            this.component = targetGO.GetComponentInChildren<Stats>();
            if (!this.component)
            {
                Debug.LogError("Trigger Stat Change: Could not get Stats component in target", null);
                return;
            }

            this.value = this.component.GetStat(this.stat.stat.uniqueName);
            this.component.AddOnChangeStat(this.OnChangeStat);
        }

        private void OnDestroy()
        {
            if (this.isExitingApplication || !this.component) return;
            this.component.RemoveOnChangeStat(this.OnChangeStat);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnChangeStat(Stats.EventArgs args)
        {
            float current = this.component.GetStat(this.stat.stat.uniqueName);
            switch (this.detect)
            {
                case Detection.OnChange:
                    if (!Mathf.Approximately(current, this.value))
                    {
                        this.ExecuteTrigger(component.gameObject);
                    }
                    break;

                case Detection.OnIncrease:
                    if (current > this.value)
                    {
                        this.ExecuteTrigger(component.gameObject);
                    }
                    break;

                case Detection.OnDecrease:
                    if (current < this.value)
                    {
                        this.ExecuteTrigger(component.gameObject);
                    }
                    break;
            }

            this.value = current;
        }
    }
}
