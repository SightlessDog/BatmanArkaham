namespace GameCreator.Melee
{ 
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;
    using GameCreator.Variables;
    using GameCreator.Characters;

    [AddComponentMenu("")]
	public class ActionMeleeSetInvincible : IAction
	{
		public TargetCharacter character = new TargetCharacter(TargetCharacter.Target.Player);

        [Space]
        public NumberProperty duration = new NumberProperty(1.0f);

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            Character _character = this.character.GetCharacter(target);
            if (character == null) return true;

            CharacterMelee melee = _character.GetComponent<CharacterMelee>();
            if (melee != null) melee.SetInvincibility(this.duration.GetValue(target));

            return true;
        }

        #if UNITY_EDITOR

        public static new string NAME = "Melee/Set Invincibility";
        private const string NODE_TITLE = "Set {0} invicible for {1} seconds";

        public override string GetNodeTitle()
        {
            return string.Format(NODE_TITLE, this.character, this.duration);
        }

        #endif
    }
}
