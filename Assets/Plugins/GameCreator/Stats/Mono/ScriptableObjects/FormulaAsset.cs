namespace GameCreator.Stats
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;

    [CreateAssetMenu(fileName ="Custom Formula", menuName = "Game Creator/Stats/Formula")]
    public class FormulaAsset : ScriptableObject
    {
        public Formula formula = new Formula();
    }
}