namespace GameCreator.Stats
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;

    [Serializable]
    public class AttrsAsset : ScriptableObject
    {
        public AttrAsset[] attributes = new AttrAsset[0];
    }
}