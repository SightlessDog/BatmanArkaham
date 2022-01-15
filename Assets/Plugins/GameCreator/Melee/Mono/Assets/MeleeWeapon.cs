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
        public enum WeaponBone
        {
            Root = -1,
            RightHand = HumanBodyBones.RightHand,
            LeftHand = HumanBodyBones.LeftHand,
            RightArm = HumanBodyBones.RightLowerArm,
            LeftArm = HumanBodyBones.LeftLowerArm,
            RightFoot = HumanBodyBones.RightFoot,
            LeftFoot = HumanBodyBones.LeftFoot,
            Camera = 100,
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
        public GameObject prefab;
        public WeaponBone attachment = WeaponBone.RightHand;
        public Vector3 positionOffset;
        public Vector3 rotationOffset;

        // audio:
        public AudioClip audioSheathe;
        public AudioClip audioDraw;
        public AudioClip audioImpactNormal;
        public AudioClip audioImpactKnockback;

        // reactions:
        public List<MeleeClip> groundHitReactionsFront = new List<MeleeClip>();
        public List<MeleeClip> groundHitReactionsBehind = new List<MeleeClip>();

        public List<MeleeClip> airborneHitReactionsFront = new List<MeleeClip>();
        public List<MeleeClip> airborneHitReactionsBehind = new List<MeleeClip>();

        public List<MeleeClip> knockbackReaction = new List<MeleeClip>();

        // combo system:
        public List<Combo> combos = new List<Combo>();

        // impacts:
        public GameObject prefabImpactNormal;
        public GameObject prefabImpactKnockback;

        // PRIVATE PROPERTIES: --------------------------------------------------------------------

        private int prevRandomHit = -1;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public GameObject EquipWeapon(CharacterAnimator character)
        {
            if (this.prefab == null) return null;
            if (character == null) return null;

            Transform bone = null;
            switch (this.attachment)
            {
                case WeaponBone.Root:
                    bone = character.transform;
                    break;

                case WeaponBone.Camera:
                    bone = HookCamera.Instance.transform;
                    break;

                default:
                    bone = character.animator.GetBoneTransform((HumanBodyBones)this.attachment);
                    break;

            }

            if (!bone) return null;

            GameObject instance = Instantiate(this.prefab);
            instance.transform.localScale = this.prefab.transform.localScale;

            instance.transform.SetParent(bone);

            instance.transform.localPosition = this.positionOffset;
            instance.transform.localRotation = Quaternion.Euler(this.rotationOffset);

            return instance;
        }

        public MeleeClip GetHitReaction(bool isGrounded, bool frontalAttack, bool isKnockback)
        {
            int index;
            MeleeClip meleeClip = null;

            if (isKnockback)
            {
                index = UnityEngine.Random.Range(0, this.knockbackReaction.Count - 1);
                if (this.knockbackReaction.Count != 1 && index == this.prevRandomHit) index++;
                this.prevRandomHit = index;

                return this.knockbackReaction[index];
            }

            switch (isGrounded)
            {
                case true:
                    switch (frontalAttack)
                    {
                        case true:
                            index = UnityEngine.Random.Range(0, this.groundHitReactionsFront.Count - 1);
                            if (this.groundHitReactionsFront.Count != 1 && index == this.prevRandomHit) index++;
                            this.prevRandomHit = index;

                            meleeClip = this.groundHitReactionsFront[index];
                            break;

                        case false:
                            index = UnityEngine.Random.Range(0, this.groundHitReactionsBehind.Count);
                            if (this.groundHitReactionsBehind.Count != 1 && index == this.prevRandomHit) index++;
                            this.prevRandomHit = index;

                            meleeClip = this.groundHitReactionsBehind[index];
                            break;
                    }
                    break;

                case false:
                    switch (frontalAttack)
                    {
                        case true:
                            index = UnityEngine.Random.Range(0, this.airborneHitReactionsFront.Count);
                            if (this.airborneHitReactionsFront.Count != 1 && index == this.prevRandomHit) index++;
                            this.prevRandomHit = index;

                            meleeClip = this.airborneHitReactionsFront[index];
                            break;

                        case false:
                            index = UnityEngine.Random.Range(0, this.airborneHitReactionsBehind.Count);
                            if (this.airborneHitReactionsBehind.Count != 1 && index == this.prevRandomHit) index++;
                            this.prevRandomHit = index;

                            meleeClip = this.airborneHitReactionsBehind[index];
                            break;
                    }
                    break;
            }

            return meleeClip;
        }
    }
}
