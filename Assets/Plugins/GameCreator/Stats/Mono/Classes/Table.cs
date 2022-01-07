namespace GameCreator.Stats
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;

    [Serializable]
    public class Table
    {
        public float maxTier = 99f;
        public float threshold = 50f;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public float Tier(float progress)
        {
            return Table.Tier(progress, this.maxTier, this.threshold);
        }

        public float PercentNextTier(float progress)
        {
            float currentTier = this.Tier(progress);
            float a = Table.Progress(currentTier, this.threshold);
            float b = Table.Progress(Math.Min(currentTier + 1, this.maxTier), this.threshold);
            return (progress - a)/(b - a);
        }

        // STATIC METHODS: ------------------------------------------------------------------------

        public static float Tier(float progress, float maxTier, float threshold)
        {
            float result = (1 + Mathf.Sqrt(1f + (8f * progress / threshold))) / 2.0f;
            return Mathf.Min(Mathf.Floor(result), maxTier);
        }

        public static float Progress(float tier, float threshold)
        {
            float result = ((Mathf.Pow(tier, 2.0f) - tier) * threshold) / 2.0f;
            return Mathf.Floor(result);
        }
    }
}