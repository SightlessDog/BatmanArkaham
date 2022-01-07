namespace GameCreator.Stats
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;

    [Serializable]
    public class StatsAsset : ScriptableObject
    {
        public StatAsset[] stats = new StatAsset[0];
    }
}