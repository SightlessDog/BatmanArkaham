namespace GameCreator.Melee
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;
    using GameCreator.Characters;
    using GameCreator.Variables;

    [AddComponentMenu("")]
	public class ActionMeleeAddDefense : IAction
	{
		public TargetCharacter character = new TargetCharacter(TargetCharacter.Target.Player);
		public NumberProperty amount = new NumberProperty(10f);

		public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
			Character _character = this.character.GetCharacter(target);
			if (character == null) return true;

			CharacterMelee melee = _character.GetComponent<CharacterMelee>();
			if (melee != null) melee.AddDefense(this.amount.GetValue(target));

			return true;
        }

		#if UNITY_EDITOR

        public static new string NAME = "Melee/Add Character Defense";
		private const string NODE_TITLE = "Add {0} Defense to {1}";

        public override string GetNodeTitle()
        {
			return string.Format(NODE_TITLE, this.amount, this.character);
        }

        #endif
    }
}
