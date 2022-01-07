namespace GameCreator.Stats
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;

    [Serializable]
    public class StatusEffectAsset : ScriptableObject
    {
        public string uniqueID = "";
        public StatusEffect statusEffect = new StatusEffect();

        #if UNITY_EDITOR
        public string GetNodeTitle()
        {
            return this.statusEffect.uniqueName;
        }
        #endif
    }
}