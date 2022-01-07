namespace GameCreator.Stats
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;

    [Serializable]
    public class StatusEffectsAsset : ScriptableObject
    {
        public StatusEffectAsset[] statusEffects = new StatusEffectAsset[0];
    }
}