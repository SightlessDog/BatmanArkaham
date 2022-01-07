namespace GameCreator.Stats
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Localization;

    [Serializable]
    public class AttrAsset : ScriptableObject
    {
        public string uniqueID = "";
        public Attribute attribute = new Attribute();

        #if UNITY_EDITOR
        public string GetNodeTitle()
        {
            return this.attribute.uniqueName;
        }
        #endif
    }
}