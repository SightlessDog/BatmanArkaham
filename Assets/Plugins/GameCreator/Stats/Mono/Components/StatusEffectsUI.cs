namespace GameCreator.Stats
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using GameCreator.Core;

    [AddComponentMenu("UI/Game Creator/Status Effects UI", 0)]
    public class StatusEffectsUI : MonoBehaviour
    {
        public enum StatusEffectType
        {
            All = -1,
            Positive = StatusEffect.Type.Positive,
            Negative = StatusEffect.Type.Negative,
            Other = StatusEffect.Type.Other,
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);

        public bool showAll = true;
        public StatusEffectType show = StatusEffectType.All;

        public GameObject prefabStatusEffect;
        public RectTransform container;

        private bool exitingApplication = false;
        private Stats stats = null;
        private Dictionary<string, StatusEffectUI> statusEffects = new Dictionary<string, StatusEffectUI>();

        // INITIALIZERS: --------------------------------------------------------------------------

        private void Start()
        {
            this.stats = this.GetStatsTarget();
            if (!this.stats) return;

            this.stats.AddOnChangeStef(this.UpdateStatusEffectsList);
            this.UpdateStatusEffectsList(null);
        }

        private void OnDestroy()
        {
            if (this.exitingApplication) return;
            if (!this.stats) return;

            this.stats.RemoveOnChangeStef(this.UpdateStatusEffectsList);
        }

        private void OnApplicationQuit()
        {
            this.exitingApplication = true;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private Stats GetStatsTarget()
        {
            GameObject targetGO = this.target.GetGameObject(gameObject);
            if (!targetGO) return null;

            Stats statsComponent = targetGO.GetComponentInChildren<Stats>(true);
            if (!statsComponent) return null;

            return statsComponent;
        }

        private void UpdateStatusEffectsList(Stats.EventArgs args)
        {
            if (!this.stats) return;

            List<string> statusEffectsList = (this.show == StatusEffectType.All
                ? this.stats.GetStatusEffects()
                : this.stats.GetStatusEffects((StatusEffect.Type)this.show)
            );
            
            List<string> removeCandidates = new List<string>(this.statusEffects.Keys);
            foreach (string statusEffectItem in statusEffectsList)
            {
                if (this.statusEffects.ContainsKey(statusEffectItem))
                {
                    removeCandidates.Remove(statusEffectItem);
                    this.statusEffects[statusEffectItem].UpdateStatusEffect();
                }
                else
                {
                    GameObject instance = Instantiate(this.prefabStatusEffect, this.container);
                    StatusEffectUI statusEffectUI = instance.GetComponentInChildren<StatusEffectUI>();

                    this.statusEffects.Add(statusEffectItem, statusEffectUI);
                    statusEffectUI.Setup(this.stats, statusEffectItem);
                }
            }

            for (int i = removeCandidates.Count - 1; i >= 0; --i)
            {
                string removeKey = removeCandidates[i];
                StatusEffectUI statusEffectUI = this.statusEffects[removeKey];

                Destroy(statusEffectUI.gameObject);
                this.statusEffects.Remove(removeKey);
            }
        }
    }
}