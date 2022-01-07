namespace GameCreator.Stats
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Localization;

    [Serializable]
    public class StatAsset : ScriptableObject
    {
        public string uniqueID = "";
        public Stat stat = new Stat();
        public bool isHidden = false;

        #if UNITY_EDITOR
        public string GetNodeTitle()
        {
            return this.stat.uniqueName;
        }
        #endif
    }
}