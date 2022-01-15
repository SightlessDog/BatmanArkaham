namespace GameCreator.Melee
{
    using System.Collections;
    using GameCreator.Core;
    using UnityEngine;

    [CreateAssetMenu(
        fileName = "Melee Clip",
        menuName = "Game Creator/Melee/Melee Clip"
    )]
    public class MeleeClip : ScriptableObject
    {
        public enum Interrupt
        {
            Interruptible,
            Uninterruptible,
        }

        public enum Vulnerable
        {
            Vulnerable,
            Invincible,
        }

        public enum Posture
        {
            Steady,
            Stagger,
        }

        // STATIC & CONSTS: -----------------------------------------------------------------------

        private const int HITPAUSE_TIME_LAYER = 80;

        private static readonly Keyframe[] DEFAULT_KEY_MOVEMENT = {
            new Keyframe(0f, 0f),
            new Keyframe(1f, 0f)
        };

        // PROPERTIES: ----------------------------------------------------------------------------

        public AnimationClip animationClip;
        public AvatarMask avatarMask;
        public float transitionIn = 0.25f;
        public float transitionOut = 0.25f;

        // movement:
        public AnimationCurve movementForward = new AnimationCurve(DEFAULT_KEY_MOVEMENT);
        public AnimationCurve movementSides = new AnimationCurve(DEFAULT_KEY_MOVEMENT);
        public AnimationCurve movementVertical = new AnimationCurve(DEFAULT_KEY_MOVEMENT);

        [Range(0f, 1f)] public float gravityInfluence = 1f;
        public float movementMultiplier = 1.0f;

        // audio:
        public AudioClip soundEffect;

        // hit pause:
        public bool hitPause = true;
        [Range(0f, 1f)]
        public float hitPauseAmount = 0.05f;
        public float hitPauseDuration = 0.05f;

        // attack:
        public bool isAttack = true;
        public bool isBlockable = true;
        public float pushForce = 50f;

        public float poiseDamage = 2f;
        public float defenseDamage = 1f;

        // properties:
        public Interrupt interruptible = Interrupt.Interruptible;
        public Vulnerable vulnerability = Vulnerable.Vulnerable;
        public Posture posture = Posture.Steady;

        public AnimationCurve attackPhase = new AnimationCurve(
            new Keyframe(0.00f, 0f),
            new Keyframe(0.25f, 1f),
            new Keyframe(0.50f, 2f),
            new Keyframe(1.00f, 2f)
        );

        public float Length { get => this.animationClip == null ? 0f : this.animationClip.length; }

        private WaitForSecondsRealtime hitPauseCoroutine;

        public IActionsList actionsOnExecute;
        public IActionsList actionsOnHit;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Play(CharacterMelee melee)
        {
            if (this.interruptible == Interrupt.Uninterruptible) melee.SetUninterruptable(this.Length);
            if (this.vulnerability == Vulnerable.Invincible) melee.SetInvincibility(this.Length);

            melee.SetPosture(this.posture, this.Length);
            melee.PlayAudio(this.soundEffect);

            melee.Character.GetCharacterAnimator().StopGesture(0.1f);
            melee.Character.GetCharacterAnimator().CrossFadeGesture(
                this.animationClip, 1.0f, this.avatarMask,
                this.transitionIn, this.transitionOut
            );

            float duration = Mathf.Max(0, this.animationClip.length - this.transitionOut);

            melee.Character.RootMovement(
                this.movementMultiplier,
                duration,
                this.gravityInfluence,
                this.movementForward,
                this.movementSides,
                this.movementVertical
            );

            this.ExecuteActionsOnStart(melee.Blade.GetImpactPosition(), melee.gameObject);
        }

        public void ExecuteHitPause()
        {
            if (!this.hitPause) return;

            TimeManager.Instance.SetTimeScale(this.hitPauseAmount, HITPAUSE_TIME_LAYER);
            CoroutinesManager.Instance.StartCoroutine(this.ExecuteHitPause(
                this.hitPauseDuration
            ));
        }

        public void ExecuteActionsOnStart(Vector3 position, GameObject target)
        {
            if (this.actionsOnExecute)
            {
                GameObject actionsInstance = Instantiate<GameObject>(
                    this.actionsOnExecute.gameObject,
                    position,
                    Quaternion.identity
                );

                actionsInstance.hideFlags = HideFlags.HideInHierarchy;
                Actions actions = actionsInstance.GetComponent<Actions>();

                if (!actions) return;
                actions.Execute(target, null);
            }
        }

        public void ExecuteActionsOnHit(Vector3 position, GameObject target)
        {
            if (this.actionsOnHit)
            {
                GameObject actionsInstance = Instantiate<GameObject>(
                    this.actionsOnHit.gameObject,
                    position,
                    Quaternion.identity
                );

                actionsInstance.hideFlags = HideFlags.HideInHierarchy;
                Actions actions = actionsInstance.GetComponent<Actions>();

                if (!actions) return;
                actions.Execute(target, null);
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private IEnumerator ExecuteHitPause(float duration)
        {
            if (this.hitPauseCoroutine != null)
            {
                CoroutinesManager.Instance.StopCoroutine(this.hitPauseCoroutine);
            }

            this.hitPauseCoroutine = new WaitForSecondsRealtime(duration);
            yield return this.hitPauseCoroutine;

            TimeManager.Instance.SetTimeScale(1f, HITPAUSE_TIME_LAYER);
            this.hitPauseCoroutine = null;
        }
    }
}