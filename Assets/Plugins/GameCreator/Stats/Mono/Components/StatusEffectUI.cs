namespace GameCreator.Stats
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using GameCreator.Core;

    [AddComponentMenu("UI/Game Creator/Status Effect UI", 0)]
    public class StatusEffectUI : MonoBehaviour
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        public Text title;
        public Text description;
        public Image icon;
        public Graphic color;
        public Text stack;

        private Stats stats;
        private string statusEffectID;

        // INITIALIZERS: --------------------------------------------------------------------------

        public void Setup(Stats stats, string statusEffectID)
        {
            this.stats = stats;
            this.statusEffectID = statusEffectID;
            this.UpdateStatusEffect();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void UpdateStatusEffect()
        {
            if (this.title) this.title.text = this.stats.GetStatusEffectTitle(this.statusEffectID);
            if (this.description) this.description.text = this.stats.GetStatusEffectDescription(this.statusEffectID);
            if (this.icon) this.icon.sprite = this.stats.GetStatusEffectIcon(this.statusEffectID);
            if (this.color) this.color.color = this.stats.GetStatusEffectColor(this.statusEffectID);
            if (this.stack) this.stack.text = this.stats.GetStatusEffectStack(this.statusEffectID).ToString();
        }
    }
}