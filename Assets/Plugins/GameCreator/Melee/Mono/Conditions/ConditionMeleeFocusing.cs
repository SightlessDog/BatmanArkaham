namespace GameCreator.Melee
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;
	using GameCreator.Characters;

	[AddComponentMenu("")]
	public class ConditionMeleeFocusing : ICondition
	{
        public enum Focus
        {
            IsFocused,
            IsNotFocused
        }

		public TargetCharacter character = new TargetCharacter(TargetCharacter.Target.Player);
		public Focus condition = Focus.IsFocused;

		public override bool Check(GameObject target)
		{
			Character _character = this.character.GetCharacter(target);
			if (character == null) return condition == Focus.IsFocused ? false : true;

			CharacterMelee melee = _character.GetComponent<CharacterMelee>();
			if (melee == null) return condition == Focus.IsFocused ? false : true;

			if (melee.HasFocusTarget && this.condition == Focus.IsFocused) return true;
			if (!melee.HasFocusTarget && this.condition == Focus.IsNotFocused) return true;
			return false;
		}
        
		#if UNITY_EDITOR

        public static new string NAME = "Melee/Has Character Focus";
		private const string NODE_TITLE = "Character {0} {1}";

        public override string GetNodeTitle()
        {
			return string.Format(NODE_TITLE, this.character, this.condition);
        }

        #endif
    }
}
