namespace GameCreator.Melee
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;
    using GameCreator.Characters;

    [AddComponentMenu("")]
	public class ActionMeleeChangeShield : IAction
	{
		public TargetCharacter character = new TargetCharacter(TargetCharacter.Target.Player);
		public MeleeShield shield;

		public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
			Character _character = this.character.GetCharacter(target);
			if (character == null) return true;

			CharacterMelee melee = _character.GetComponent<CharacterMelee>();
			if (melee != null) melee.EquipShield(this.shield);

			return true;
        }

		#if UNITY_EDITOR

        public static new string NAME = "Melee/Change Shield";
		private const string NODE_TITLE = "Change shield {0} on {1}";

        public override string GetNodeTitle()
        {
			return string.Format(
                NODE_TITLE,
                this.shield != null ? this.shield.name : "(none)",
                this.character
            );
        }

        #endif
    }
}
