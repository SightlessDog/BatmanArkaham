namespace GameCreator.Melee
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;
	using GameCreator.Characters;

	[AddComponentMenu("")]
	public class ConditionMeleeArmed : ICondition
	{
        public enum Armed
        {
            IsArmed,
            IsUnarmed
        }

		public TargetCharacter character = new TargetCharacter(TargetCharacter.Target.Player);
		public Armed condition = Armed.IsArmed;

		public override bool Check(GameObject target)
		{
			Character _character = this.character.GetCharacter(target);
			if (character == null) return condition == Armed.IsArmed ? false : true;

			CharacterMelee melee = _character.GetComponent<CharacterMelee>();
			if (melee == null) return condition == Armed.IsArmed ? false : true;

			if (melee.currentWeapon != null && this.condition == Armed.IsArmed) return true;
			if (melee.currentWeapon == null && this.condition == Armed.IsUnarmed) return true;
			return false;
		}
        
		#if UNITY_EDITOR

        public static new string NAME = "Melee/Is Character Armed";
		private const string NODE_TITLE = "Character {0} {1}";

        public override string GetNodeTitle()
        {
			return string.Format(NODE_TITLE, this.character, this.condition);
        }

        #endif
    }
}
