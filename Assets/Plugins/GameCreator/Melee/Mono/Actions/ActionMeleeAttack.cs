namespace GameCreator.Melee
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;
    using GameCreator.Characters;

    [AddComponentMenu("")]
	public class ActionMeleeAttack : IAction
	{
		public TargetCharacter character = new TargetCharacter(TargetCharacter.Target.Player);
		public CharacterMelee.ActionKey key = CharacterMelee.ActionKey.A;

		public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
			Character _character = this.character.GetCharacter(target);
			if (character == null) return true;

			CharacterMelee melee = _character.GetComponent<CharacterMelee>();
			if (melee != null) melee.Execute(this.key);

			return true;
        }

		#if UNITY_EDITOR

        public static new string NAME = "Melee/Input Melee Attack";
		private const string NODE_TITLE = "Input Melee {0} on {1}";

        public override string GetNodeTitle()
        {
			return string.Format(NODE_TITLE, this.key, this.character);
        }

        #endif
    }
}
