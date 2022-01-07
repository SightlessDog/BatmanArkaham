namespace GameCreator.Melee
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using GameCreator.Core;
    using GameCreator.Characters;

    [AddComponentMenu("")]
    public class IgniterMeleeOnAttack : Igniter 
	{
		#if UNITY_EDITOR
        public new static string NAME = "Melee/On Attack";
        public new static bool REQUIRES_COLLIDER = false;
        #endif

        public TargetCharacter character = new TargetCharacter(TargetCharacter.Target.Player);

        private void Start()
        {
            Character _character = this.character.GetCharacter(gameObject);
            if (_character == null) return;

            CharacterMelee melee = _character.GetComponent<CharacterMelee>();
            if (melee == null) return;

            melee.EventAttack += this.OnAttack;
        }

        private void OnAttack(MeleeClip attack)
		{
            Character _character = this.character.GetCharacter(gameObject);
            if (_character == null) return;
            this.ExecuteTrigger(_character.gameObject);
		}
	}
}