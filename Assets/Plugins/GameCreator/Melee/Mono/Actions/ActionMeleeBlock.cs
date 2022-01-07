namespace GameCreator.Melee
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;
    using GameCreator.Characters;

    #if UNITY_EDITOR
    using UnityEditor;
    #endif

    [AddComponentMenu("")]
	public class ActionMeleeBlock : IAction
	{
        public enum Block
        {
            StartBlocking,
            StopBlocking
        }

		public TargetCharacter character = new TargetCharacter(TargetCharacter.Target.Player);

        [Space] public Block blocking = Block.StartBlocking;

		public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
			Character _character = this.character.GetCharacter(target);
			if (character == null) return true;

			CharacterMelee melee = _character.GetComponent<CharacterMelee>();
			if (melee != null)
            {
                switch (this.blocking)
                {
                    case Block.StartBlocking: melee.StartBlocking(); break;
                    case Block.StopBlocking: melee.StopBlocking(); break;
                }
            }

			return true;
        }

		#if UNITY_EDITOR

        public static new string NAME = "Melee/Melee Blocking";
		private const string NODE_TITLE = "Melee {0} {1}";

        public override string GetNodeTitle()
        {
			return string.Format(
                NODE_TITLE,
                this.character,
                ObjectNames.NicifyVariableName(this.blocking.ToString())
            );
        }

        #endif
    }
}
