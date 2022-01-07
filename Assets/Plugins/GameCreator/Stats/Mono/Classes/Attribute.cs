namespace GameCreator.Stats
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Localization;

    [Serializable]
    public class Attribute
    {
        public string uniqueName = "";
        public string shortName = "";

        [LocStringNoPostProcess] public LocString title = new LocString();
        [LocStringNoPostProcess] public LocString description = new LocString();

        public Sprite icon = null;
        public Color color = Color.grey;

        public float minValue = 0f;

        [StatSelector]
        [Tooltip("Maximum value of the Attribute")]
        public StatAsset stat;

        [Range(0.0f, 1.0f)]
        [Tooltip("Current percent value of the Attribute")]
        public float percent = 1f;
    }

    public class AttributeSelectorAttribute : PropertyAttribute
    {
        public AttributeSelectorAttribute() { }
    }
}