namespace GameCreator.Melee
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;
	using GameCreator.Characters;

    [AddComponentMenu("")]
	public class ConditionMeleeIsAttacking : ICondition
	{
        public enum Attacking
        {
            IsAttacking,
            IsNotAttacking
        }

        public TargetCharacter character = new TargetCharacter(TargetCharacter.Target.Player);
		public Attacking attacking = Attacking.IsAttacking;

		public override bool Check(GameObject target)
		{
			Character _character = this.character.GetCharacter(target);
			if (character == null) return attacking == Attacking.IsAttacking ? false : true;

			CharacterMelee melee = _character.GetComponent<CharacterMelee>();
			if (melee == null) return attacking == Attacking.IsAttacking ? false : true;

            switch (this.attacking)
            {
				case Attacking.IsAttacking: return melee.IsAttacking;
				case Attacking.IsNotAttacking: return !melee.IsAttacking;
			}

			return false;
		}
        
		#if UNITY_EDITOR

        public static new string NAME = "Melee/Is Character Attacking";
		private const string NODE_TITLE = "Character {0} {1}";

        public override string GetNodeTitle()
        {
			return string.Format(
                NODE_TITLE,
                this.character,
                this.attacking
            );
        }

        #endif
    }
}
