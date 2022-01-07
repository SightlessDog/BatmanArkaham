namespace GameCreator.Melee
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class Combo
    {
        public CharacterMelee.ActionKey[] combo;
        public ComboSystem.Condition condition;

        public MeleeClip meleeClip;
        public bool isEnabled = true;
        public bool shouldRepeat = false;

        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public Combo()
        {
            this.combo = new CharacterMelee.ActionKey[] { CharacterMelee.ActionKey.A };
            this.condition = ComboSystem.Condition.None;
            this.meleeClip = null;
            this.isEnabled = true;
            this.shouldRepeat = false;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------


    }
}