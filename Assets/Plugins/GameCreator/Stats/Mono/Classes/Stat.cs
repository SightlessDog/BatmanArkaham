namespace GameCreator.Stats
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Localization;

    [Serializable]
    public class Stat
    {
        public string uniqueName = "";
        public string shortName = "";

        [LocStringNoPostProcess] public LocString title = new LocString();
        [LocStringNoPostProcess] public LocString description = new LocString();

        public Sprite icon = null;
        public Color color = Color.grey;

        [Tooltip("Initial value of the Stat")]
        public float baseValue = 0.0f;

        [Tooltip("Formula applied when getting the value of the Stat")]
        public FormulaAsset formula;
    }

    public class StatSelectorAttribute : PropertyAttribute
    {
        public StatSelectorAttribute() { }
    }
}