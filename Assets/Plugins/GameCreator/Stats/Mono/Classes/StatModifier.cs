namespace GameCreator.Stats
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Localization;

    [Serializable]
    public class StatModifier
    {
        public enum EffectType
        {
            Contant = 100,
            Percent = 200
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        [StatSelector]
        public StatAsset stat;

        public EffectType type = EffectType.Contant;
        public float value = 0.0f;
    }
}