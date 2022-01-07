namespace GameCreator.Melee
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;
	using GameCreator.Characters;

    [AddComponentMenu("")]
	public class ConditionMeleeIsBlocking : ICondition
	{
        public enum Blocking
        {
            IsBlocking,
            IsNotBlocking
        }

        public TargetCharacter character = new TargetCharacter(TargetCharacter.Target.Player);
		public Blocking blocking = Blocking.IsBlocking;

		public override bool Check(GameObject target)
		{
			Character _character = this.character.GetCharacter(target);
			if (character == null) return blocking == Blocking.IsBlocking ? false : true;

			CharacterMelee melee = _character.GetComponent<CharacterMelee>();
			if (melee == null) return blocking == Blocking.IsBlocking ? false : true;

            switch (this.blocking)
            {
				case Blocking.IsBlocking: return melee.IsBlocking;
				case Blocking.IsNotBlocking: return !melee.IsBlocking;
			}

			return false;
		}
        
		#if UNITY_EDITOR

        public static new string NAME = "Melee/Is Character Blocking";
		private const string NODE_TITLE = "Character {0} {1}";

        public override string GetNodeTitle()
        {
			return string.Format(
                NODE_TITLE,
                this.character,
                this.blocking
            );
        }

        #endif
    }
}
