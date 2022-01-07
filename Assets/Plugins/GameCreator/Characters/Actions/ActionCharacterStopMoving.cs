namespace GameCreator.Characters
{
    using System.Collections;
    using System.Collections.Generic;
    using GameCreator.Core;
    using GameCreator.Variables;
    using UnityEngine;
    using UnityEngine.Events;

    [AddComponentMenu("")]
    public class ActionCharacterStopMoving : IAction
    {
        public TargetCharacter character = new TargetCharacter(TargetCharacter.Target.Player);

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            Character instance = this.character.GetCharacter(target);
            if (instance == null) return true;

            instance.characterLocomotion.SetDirectionalDirection(Vector3.zero);
            return true;
        }

        #if UNITY_EDITOR

        public new static string NAME = "Character/Stop Movement";
        private const string NODE_TITLE = "Character {0} stop movement";

        public override string GetNodeTitle()
        {
            return string.Format(NODE_TITLE, this.character);
        }

        #endif
    }
}
