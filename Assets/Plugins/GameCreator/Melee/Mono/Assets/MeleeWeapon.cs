namespace GameCreator.Melee
{
    using System;
    using System.Collections;
	using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Characters;
    using GameCreator.Core;
    using GameCreator.Localization;
    using GameCreator.Core.Hooks;

    [CreateAssetMenu(fileName = "Melee Weapon", menuName = "Game Creator/Melee/Melee Weapon")]
    public class MeleeWeapon : ScriptableObject
	{
        public enum HitLocation
        {
            FrontUpper, FrontMiddle, FrontLower,
            BackUpper, BackMiddle, BackLower

        }

        public const CharacterAnimation.Layer LAYER_STANCE = CharacterAnimation.Layer.Layer1;

        // PROPERTIES: ----------------------------------------------------------------------------

        // general:
        [LocStringNoPostProcess] public LocString weaponName = new LocString("Weapon Name");
        [LocStringNoPostProcess] public LocString weaponDescription = new LocString("Weapon Description");

        public MeleeShield defaultShield;
        public CharacterState characterState;
        public AvatarMask characterMask;

        // 3d model:
        public List<GameObject> prefabs = new List<GameObject>();

        // audio:
        public AudioClip audioSheathe;
        public AudioClip audioDraw;
        public AudioClip audioImpactNormal;
        public AudioClip audioImpactKnockback;

        // reactions:
        public List<MeleeClip> groundHitReactionsFrontUpper = new List<MeleeClip>();
        public List<MeleeClip> groundHitReactionsFrontMiddle = new List<MeleeClip>();
        public List<MeleeClip> groundHitReactionsFrontLower = new List<MeleeClip>();

        public List<MeleeClip> groundHitReactionsBackUpper = new List<MeleeClip>();
        public List<MeleeClip> groundHitReactionsBackMiddle = new List<MeleeClip>();
        public List<MeleeClip> groundHitReactionsBackLower = new List<MeleeClip>();

        public List<MeleeClip> airborneHitReactionsFrontUpper  = new List<MeleeClip>();
        public List<MeleeClip> airborneHitReactionsFrontMiddle = new List<MeleeClip>();
        public List<MeleeClip> airborneHitReactionsFrontLower = new List<MeleeClip>();

        public List<MeleeClip> airborneHitReactionsBackUpper  = new List<MeleeClip>();
        public List<MeleeClip> airborneHitReactionsBackMiddle = new List<MeleeClip>();
        public List<MeleeClip> airborneHitReactionsBackLower = new List<MeleeClip>();

        public List<MeleeClip> knockbackReaction = new List<MeleeClip>();

        // combo system:
        public List<Combo> combos = new List<Combo>();

        // impacts:
        public GameObject prefabImpactNormal;
        public GameObject prefabImpactKnockback;

        // PRIVATE PROPERTIES: --------------------------------------------------------------------

        private int prevRandomHit = -1;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public List<GameObject> EquipWeapon(CharacterAnimator character)
        {
            if (this.prefabs == null || this.prefabs.Count == 0) return null;
            if (character == null) return null;
            var instances = new List<GameObject>();
            foreach (var prefab in prefabs)
            {
                GameObject instance = Instantiate(prefab);
                instances.Add(instance);
                instance.transform.localScale = prefab.transform.localScale;

                var blade = instance.GetComponentInChildren<BladeComponent>();
                Transform bone = null;
                switch (blade.bone)
                {
                    case BladeComponent.WeaponBone.Root:
                        bone = character.transform;
                        break;

                    case BladeComponent.WeaponBone.Camera:
                        bone = HookCamera.Instance.transform;
                        break;

                    default:
                        bone = character.animator.GetBoneTransform((HumanBodyBones)blade.bone);
                        break;
                }
                if (!bone) continue;
                blade.transform.SetParent(bone);

                blade.transform.localPosition = prefab.transform.position;
                blade.transform.localRotation = prefab.transform.rotation;

            }
            return instances;
        }

        public MeleeClip GetHitReaction(bool isGrounded, HitLocation location, bool isKnockback)
        {
            int index;
            MeleeClip meleeClip = null;
            List<MeleeClip> hitReactionList;

            if (isKnockback) hitReactionList = knockbackReaction;
            else
                switch (location)
                {
                    case HitLocation.FrontUpper:
                        hitReactionList = isGrounded ? this.groundHitReactionsFrontUpper : this.airborneHitReactionsFrontUpper;
                        break;
                    case HitLocation.FrontMiddle:
                        hitReactionList = isGrounded ? this.groundHitReactionsFrontMiddle : this.airborneHitReactionsFrontMiddle;
                        break;
                    case HitLocation.FrontLower:
                        hitReactionList = isGrounded ? this.groundHitReactionsFrontLower : this.airborneHitReactionsFrontLower;
                        break;
                    case HitLocation.BackUpper:
                        hitReactionList = isGrounded ? this.groundHitReactionsBackUpper : this.airborneHitReactionsBackUpper;
                        break;
                    case HitLocation.BackMiddle:
                        hitReactionList = isGrounded ? this.groundHitReactionsBackMiddle : this.airborneHitReactionsBackMiddle;
                        break;
                    case HitLocation.BackLower:
                        hitReactionList = isGrounded ? this.groundHitReactionsBackLower : this.airborneHitReactionsBackLower;
                        break;
                    default: hitReactionList = knockbackReaction; break;
                }
            index = UnityEngine.Random.Range(0, hitReactionList.Count - 1);
            if (hitReactionList.Count != 1 && index == this.prevRandomHit) index++;
            this.prevRandomHit = index;

            meleeClip = hitReactionList[index];

            return meleeClip;
        }
    }
}
