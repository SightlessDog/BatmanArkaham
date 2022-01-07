namespace GameCreator.Melee
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;
	using GameCreator.Characters;
    using GameCreator.Variables;

    #if UNITY_EDITOR
	using UnityEditor;
    #endif

	[AddComponentMenu("")]
	public class ConditionMeleeComparePoise : ICondition
	{
		public enum Comparison
		{
			Equal,
			EqualInteger,
			Less,
			LessOrEqual,
			Greater,
			GreaterOrEqual
		}

		public TargetCharacter character = new TargetCharacter(TargetCharacter.Target.Player);

		[Space]
		public Comparison comparison = Comparison.GreaterOrEqual;
		public NumberProperty compareTo = new NumberProperty();

		// OVERRIDERS: ----------------------------------------------------------------------------

		public override bool Check(GameObject target)
		{
			Character _character = this.character.GetCharacter(target);
			if (character == null) return false;

			CharacterMelee melee = _character.GetComponent<CharacterMelee>();
			if (melee == null) return false;


			float var1 = melee.Poise;
			float var2 = this.compareTo.GetValue(target);

			switch (this.comparison)
			{
				case Comparison.Equal: return Mathf.Approximately(var1, var2);
				case Comparison.EqualInteger: return Mathf.RoundToInt(var1) == Mathf.RoundToInt(var2);
				case Comparison.Less: return var1 < var2;
				case Comparison.LessOrEqual: return var1 <= var2;
				case Comparison.Greater: return var1 > var2;
				case Comparison.GreaterOrEqual: return var1 >= var2;
			}

			return false;
		}
        
		#if UNITY_EDITOR

        public static new string NAME = "Melee/Compare Poise";
		private const string NODE_TITLE = "Character {0} poise is {1} than {2}";

        public override string GetNodeTitle()
        {
			return string.Format(
                NODE_TITLE,
                this.character,
                ObjectNames.NicifyVariableName(this.comparison.ToString()),
                this.compareTo
            );
        }

        #endif
    }
}
