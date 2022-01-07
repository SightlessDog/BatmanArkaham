namespace GameCreator.Melee
{
    using System;
    using System.Collections;
	using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Characters;
    using GameCreator.Core;
    using GameCreator.Localization;
    using GameCreator.Variables;
    using GameCreator.Core.Hooks;

    [CreateAssetMenu(fileName = "Melee Shield", menuName = "Game Creator/Melee/Melee Shield")]
    public class MeleeShield : ScriptableObject
	{
        public enum ShieldBone
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

        // PROPERTIES: ----------------------------------------------------------------------------

        // general:
        [LocStringNoPostProcess] public LocString shieldName = new LocString("Shield Name");
        [LocStringNoPostProcess] public LocString shieldDescription = new LocString("Shield Description");

        // 3d model:
        public GameObject prefab;
        public ShieldBone attachment = ShieldBone.RightHand;
        public Vector3 positionOffset;
        public Vector3 rotationOffset;

        // reactions:
        public MeleeClip perfectBlockClip;

        public List<MeleeClip> blockingHitReaction = new List<MeleeClip>();
        public MeleeClip groundPerfectBlockReaction;
        public MeleeClip airbornPerfectBlockReaction;

        // defense system:
        public CharacterState defendState;
        public AvatarMask defendMask;

        [Range(0f, 180f)] public float lowerBodyRotation;
        public NumberProperty defenseAngle = new NumberProperty(120f);

        public NumberProperty defenseRecoveryRate = new NumberProperty(0.5f);
        public NumberProperty maxDefense = new NumberProperty(3f);
        public NumberProperty delayDefense = new NumberProperty(0.75f);

        public float perfectBlockWindow = 0.20f;

        public AudioClip audioBlock;
        public AudioClip audioPerfectBlock;

        public GameObject prefabImpactBlock;
        public GameObject prefabImpactPerfectBlock;

        // PRIVATE PROPERTIES: --------------------------------------------------------------------

        private int prevRandomBlock = -1;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public GameObject EquipShield(CharacterAnimator character)
        {
            if (this.prefab == null) return null;
            if (character == null) return null;

            Transform bone = null;
            switch (this.attachment)
            {
                case ShieldBone.Root:
                    bone = character.transform;
                    break;

                case ShieldBone.Camera:
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

        public MeleeClip GetBlockReaction()
        {
            if (this.blockingHitReaction.Count == 0) return null;

            int index = UnityEngine.Random.Range(0, this.blockingHitReaction.Count - 1);

            if (this.blockingHitReaction.Count != 1 && index == this.prevRandomBlock) index++;
            this.prevRandomBlock = index;

            return this.blockingHitReaction[index];
        }
    }
}
