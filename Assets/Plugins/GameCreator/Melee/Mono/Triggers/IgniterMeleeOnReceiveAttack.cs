namespace GameCreator.Melee
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Variables;

    [AddComponentMenu("")]
    public class IgniterMeleeOnReceiveAttack : Igniter
    {
#if UNITY_EDITOR
        public new static string NAME = "Melee/On Receive Attack";
        public new static bool REQUIRES_COLLIDER = true;
#endif

        public CharacterMelee.HitResult type = CharacterMelee.HitResult.ReceiveDamage;

        [Space] public VariableProperty storeAttacker = new VariableProperty();

        public void OnReceiveAttack(CharacterMelee attacker, MeleeClip attack,
            CharacterMelee.HitResult hitResult)
        {
            if (this.type == hitResult)
            {
                this.storeAttacker.Set(attacker.gameObject, gameObject);
                this.ExecuteTrigger(attacker.gameObject);
            }
        }
    }
}