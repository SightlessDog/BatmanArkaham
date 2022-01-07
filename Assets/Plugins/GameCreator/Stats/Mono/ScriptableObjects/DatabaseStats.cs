namespace GameCreator.Stats
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using GameCreator.Core;

    #if UNITY_EDITOR
    using UnityEditor;
    #endif

	public class DatabaseStats : IDatabase
	{
        public StatsAsset statsAsset;
        public AttrsAsset attrsAsset;
        public StatusEffectsAsset statusEffectsAsset;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public StatAsset[] GetStatAssets()
        {
            return this.statsAsset.stats;
        }

        public AttrAsset[] GetAttrAssets()
        {
            return this.attrsAsset.attributes;
        }

        public StatusEffectAsset[] GetStatusEffectAssets()
        {
            return this.statusEffectsAsset.statusEffects;
        }

        public StatusEffectAsset GetStatusEffect(string uniqueID)
        {
            if (this.statusEffectsAsset == null) return null;
            for (int i = 0; i < this.statusEffectsAsset.statusEffects.Length; ++i)
            {
                if (this.statusEffectsAsset.statusEffects[i].uniqueID == uniqueID)
                {
                    return this.statusEffectsAsset.statusEffects[i];
                }
            }

            return null;
        }

        // PUBLIC STATIC METHODS: -----------------------------------------------------------------

        public static DatabaseStats Load()
        {
            return IDatabase.LoadDatabase<DatabaseStats>();
        }

        // OVERRIDE METHODS: ----------------------------------------------------------------------

        #if UNITY_EDITOR

        protected override string GetProjectPath()
        {
            return "Assets/Plugins/GameCreatorData/Stats/Resources";
        }

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            IDatabase.Setup<DatabaseStats>();
        }

        #endif
	}
}