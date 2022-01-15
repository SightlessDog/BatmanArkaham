namespace GameCreator.Melee
{
    using System;
    using System.Collections;
	using System.Collections.Generic;
    using GameCreator.Core;
    using GameCreator.Characters;
    using UnityEngine;
    using UnityEngine.Audio;
    using GameCreator.Variables;
    using GameCreator.Pool;

    [RequireComponent(typeof(Character))]
    [AddComponentMenu("Game Creator/Melee/Character Melee")]
    public class CharacterMelee : TargetMelee
    {
        public enum ActionKey
        {
            A, B, C,
            D, E, F
        }

        public enum HitResult
        {
            Ignore,
            ReceiveDamage,
            AttackBlock,
            PerfectBlock
        }

        private const float MIN_RAND_PITCH = 0.8f;
        private const float MAX_RAND_PITCH = 1.2f;

        private const float TRANSITION = 0.15f;
        private const float INPUT_BUFFER_TIME = 0.35f;

        private const CharacterAnimation.Layer LAYER_DEFEND = CharacterAnimation.Layer.Layer3;

        // PROPERTIES: ----------------------------------------------------------------------------

        public MeleeWeapon currentWeapon;
        public MeleeShield currentShield;

        public MeleeWeapon previousWeapon;
        public MeleeShield previousShield;

        private ComboSystem comboSystem;
        private InputBuffer inputBuffer;

        public float Poise { get; protected set; }
        private float poiseDelayCooldown;

        public NumberProperty delayPoise = new NumberProperty(1f);
        public NumberProperty maxPoise = new NumberProperty(5f);
        public NumberProperty poiseRecoveryRate = new NumberProperty(1f);

        public float Defense { get; protected set; }
        private float defenseDelayCooldown;

        public bool IsDrawing { get; private set; }
        public bool IsSheathing { get; private set; }

        public bool IsAttacking { get; private set; }
        public bool IsBlocking  { get; private set; }
        public bool HasFocusTarget { get; private set; }

        public bool IsStaggered => this.isStaggered && Time.time <= this.staggerEndtime;
        public bool IsInvincible => this.isInvincible && Time.time <= this.invincibilityEndTime;
        public bool IsUninterruptable => this.isUninterruptable && Time.time <= this.uninterruptableEndTime;

        public event Action<MeleeWeapon> EventDrawWeapon;
        public event Action<MeleeWeapon> EventSheatheWeapon;
        public event Action<MeleeClip> EventAttack;
        public event Action EventStagger;
        public event Action EventBreakDefense;
        public event Action<bool> EventBlock;
        public event Action<bool> EventFocus;

        // PRIVATE PROPERTIES: --------------------------------------------------------------------

        private GameObject modelWeapon;
        private GameObject modelShield;

        private MeleeClip currentMeleeClip;
        private HashSet<int> targetsEvaluated;

        private float startBlockingTime = -100f;

        private bool isStaggered;
        private float staggerEndtime;

        private bool isInvincible;
        private float invincibilityEndTime;

        private bool isUninterruptable;
        private float uninterruptableEndTime;

        // ACCESSORS: -----------------------------------------------------------------------------

        public Character Character { get; private set; }
        public CharacterAnimator CharacterAnimator { get; private set; }
        public BladeComponent Blade { get; private set; }

        // INITIALIZERS: --------------------------------------------------------------------------

        public void Awake()
        {
            this.Character = GetComponent<Character>();
            this.CharacterAnimator = GetComponent<CharacterAnimator>();
            this.inputBuffer = new InputBuffer(INPUT_BUFFER_TIME);
        }

        // UPDATE: --------------------------------------------------------------------------------

        private void Update()
        {
            this.UpdatePoise();
            this.UpdateDefense();

            if (this.comboSystem != null)
            {
                this.comboSystem.Update();

                if (this.CanAttack() && this.inputBuffer.HasInput())
                {
                    ActionKey key = this.inputBuffer.GetInput();
                    MeleeClip meleeClip = this.comboSystem.Select(key);

                    if (meleeClip)
                    {
                        this.inputBuffer.ConsumeInput();

                        this.currentMeleeClip = meleeClip;
                        this.targetsEvaluated = new HashSet<int>();

                        this.currentMeleeClip.Play(this);
                        if (this.EventAttack != null) this.EventAttack.Invoke(meleeClip);
                    }
                }
            }
        }

        private void LateUpdate()
        {
            this.IsAttacking = false;

            if (this.comboSystem != null)
            {
                int phase = this.comboSystem.GetCurrentPhase();
                this.IsAttacking = phase >= 0f;

                if (this.Blade != null && phase == 1)
                {
                    GameObject[] hits = this.Blade.CaptureHits();
                    for (int i = 0; i < hits.Length; ++i)
                    {
                        int hitInstanceID = hits[i].GetInstanceID();

                        if (this.targetsEvaluated.Contains(hitInstanceID)) continue;
                        if (hits[i].transform.IsChildOf(this.transform)) continue;

                        HitResult hitResult = HitResult.ReceiveDamage;

                        CharacterMelee targetMelee = hits[i].GetComponent<CharacterMelee>();
                        MeleeClip attack = this.comboSystem.GetCurrentClip();

                        if (targetMelee != null)
                        {
                            hitResult = targetMelee.OnReceiveAttack(this, attack);
                        }

                        IgniterMeleeOnReceiveAttack[] triggers = (
                            hits[i].GetComponentsInChildren<IgniterMeleeOnReceiveAttack>()
                        );

                        bool hitSuccess = triggers.Length > 0;
                        if (hitSuccess)
                        {
                            for (int j = 0; j < triggers.Length; ++j)
                            {
                                triggers[j].OnReceiveAttack(this, attack, hitResult);
                            }
                        }

                        if (hitSuccess || targetMelee != null)
                        {
                            Vector3 position = this.Blade.GetImpactPosition();
                            attack.ExecuteActionsOnHit(position, hits[i].gameObject);
                        }

                        if (attack.pushForce > float.Epsilon)
                        {
                            Rigidbody[] rigidbodies = hits[i].GetComponents<Rigidbody>();
                            for (int j = 0; j < rigidbodies.Length; ++j)
                            {
                                Vector3 direction = rigidbodies[j].transform.position - transform.position;
                                rigidbodies[j].AddForce(direction.normalized * attack.pushForce, ForceMode.Impulse);
                            }
                        }

                        this.targetsEvaluated.Add(hitInstanceID);
                    }
                }
            }
        }

        private void UpdatePoise()
        {
            this.poiseDelayCooldown = Mathf.Max(0f, poiseDelayCooldown - Time.deltaTime);
            if (this.poiseDelayCooldown > float.Epsilon) return;

            this.Poise += this.poiseRecoveryRate.GetValue(gameObject) * Time.deltaTime;
            this.Poise = Mathf.Min(this.Poise, this.maxPoise.GetValue(gameObject));
        }

        private void UpdateDefense()
        {
            if (!this.currentShield) return;

            this.defenseDelayCooldown = Mathf.Max(0f, defenseDelayCooldown - Time.deltaTime);
            if (this.defenseDelayCooldown > float.Epsilon) return;

            this.Defense += this.currentShield.defenseRecoveryRate.GetValue(gameObject) * Time.deltaTime;
            this.Defense = Mathf.Min(this.Defense, this.currentShield.maxDefense.GetValue(gameObject));
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IEnumerator Sheathe()
        {
            if (this.Character.characterLocomotion.isBusy) yield break;
            if (!this.CanAttack()) yield break;
            if (this.IsAttacking) yield break;

            this.ReleaseTargetFocus();

            WaitForSeconds wait = new WaitForSeconds(0f);
            if (this.currentWeapon != null)
            {
                if (this.currentWeapon.characterState != null)
                {
                    CharacterState currentState = this.CharacterAnimator.GetState(MeleeWeapon.LAYER_STANCE);
                    if (currentState != null)
                    {
                        float time = this.ResetState(currentState, MeleeWeapon.LAYER_STANCE);
                        wait = new WaitForSeconds(time);
                    }
                }

                this.PlayAudio(this.currentWeapon.audioSheathe);
            }

            this.Character.characterLocomotion.isBusy = true;
            this.IsSheathing = true;

            yield return wait;

            if (this.EventSheatheWeapon != null) this.EventSheatheWeapon.Invoke(this.currentWeapon);
            if (this.modelWeapon != null) Destroy(this.modelWeapon);
            if (this.modelShield != null) Destroy(this.modelShield);

            this.OnSheatheWeapon();

            yield return wait;

            this.IsSheathing = false;

            this.previousWeapon = this.currentWeapon;
            this.previousShield = this.currentShield;

            this.currentWeapon = null;
            this.currentShield = null;

            this.comboSystem = null;

            this.Character.characterLocomotion.isBusy = false;
        }

        public IEnumerator Draw(MeleeWeapon weapon, MeleeShield shield = null)
        {
            if (this.Character.characterLocomotion.isBusy) yield break;
            if (this.IsAttacking) yield break;
            if (!this.CanAttack()) yield break;

            yield return this.Sheathe();

            if (weapon != null)
            {
                this.currentWeapon = weapon;
                this.EquipShield(shield != null ? shield : weapon.defaultShield);

                this.comboSystem = new ComboSystem(this, weapon.combos);

                WaitForSeconds wait = new WaitForSeconds(0f);

                if (this.currentWeapon.characterState != null)
                {
                    CharacterState state = this.currentWeapon.characterState;
                    float time = this.ChangeState(
                        this.currentWeapon.characterState,
                        this.currentWeapon.characterMask,
                        MeleeWeapon.LAYER_STANCE
                    );

                    if (state.enterClip != null) wait = new WaitForSeconds(time);
                }

                this.PlayAudio(this.currentWeapon.audioDraw);

                this.Character.characterLocomotion.isBusy = true;
                this.IsDrawing = true;

                yield return wait;

                if (this.EventDrawWeapon != null) this.EventDrawWeapon.Invoke(this.currentWeapon);

                this.modelWeapon = this.currentWeapon.EquipWeapon(this.CharacterAnimator);
                this.Blade = this.modelWeapon.GetComponentInChildren<BladeComponent>();
                if (this.Blade != null) this.Blade.Setup(this);

                this.OnDrawWeapon();

                yield return wait;

                this.IsDrawing = false;
                this.Character.characterLocomotion.isBusy = false;
            }
        }

        public void EquipShield(MeleeShield shield)
        {
            if (shield == null) return;

            if (this.modelShield != null) Destroy(this.modelShield);

            this.modelShield = shield.EquipShield(this.CharacterAnimator);
            this.currentShield = shield;
        }

        public void StartBlocking()
        {
            if (this.Character.characterLocomotion.isBusy) return;

            if (this.IsDrawing) return;
            if (this.IsSheathing) return;
            if (this.IsStaggered) return;
            if (this.IsAttacking) return;

            if (this.currentShield == null) return;
            if (this.currentShield.defendState != null)
            {
                this.CharacterAnimator.SetState(
                    this.currentShield.defendState,
                    this.currentShield.defendMask,
                    1f, 0.15f, 1f,
                    LAYER_DEFEND
                );
            }

            if (!this.IsBlocking && this.EventBlock != null)
            {
                this.EventBlock.Invoke(true);
            }

            this.startBlockingTime = Time.time;
            this.IsBlocking = true;
        }

        public void StopBlocking()
        {
            if (!this.IsBlocking) return;

            if (this.EventBlock != null) this.EventBlock.Invoke(false);
            this.CharacterAnimator.ResetState(0.25f, LAYER_DEFEND);
            this.IsBlocking = false;
        }

        public void Execute(ActionKey actionKey)
        {
            if (!this.currentWeapon) return;
            if (!this.CanAttack()) return;

            this.StopBlocking();
            this.inputBuffer.AddInput(actionKey);
        }

        public void StopAttack()
        {
            this.comboSystem.Stop();
        }

        public int GetCurrentPhase()
        {
            if (this.comboSystem == null) return -1;
            return this.comboSystem.GetCurrentPhase();
        }

        public void PlayAudio(AudioClip audioClip)
        {
            if (audioClip == null) return;

            Vector3 position = transform.position;
            if (this.Blade != null) position = this.Blade.transform.position;

            float pitch = UnityEngine.Random.Range(MIN_RAND_PITCH, MAX_RAND_PITCH);
            AudioMixerGroup soundMixer = DatabaseGeneral.Load().soundAudioMixer;

            AudioManager.Instance.PlaySound3D(
                audioClip, 0f, position, 1f, pitch,
                1.0f, soundMixer
            );
        }

        public void SetPosture(MeleeClip.Posture posture, float duration)
        {
            if (!this.IsStaggered && posture == MeleeClip.Posture.Stagger)
            {
                this.comboSystem.Stop();
                if (EventStagger != null) EventStagger.Invoke();
            }

            this.isStaggered = posture == MeleeClip.Posture.Stagger;
            this.staggerEndtime = Time.time + duration;
        }

        public void SetInvincibility(float duration)
        {
            this.isInvincible = true;
            this.invincibilityEndTime = Time.time + duration;
        }

        public void SetUninterruptable(float duration)
        {
            this.isUninterruptable = true;
            this.uninterruptableEndTime = Time.time + duration;
        }

        public void SetPoise(float value)
        {
            this.poiseDelayCooldown = this.delayPoise.GetValue(gameObject);
            this.Poise = Mathf.Clamp(value, 0f, this.maxPoise.GetValue(gameObject));
        }

        public void AddPoise(float value)
        {
            this.SetPoise(this.Poise + value);
        }

        public void SetDefense(float value)
        {
            if (!this.currentShield) return;

            this.defenseDelayCooldown = this.currentShield.delayDefense.GetValue(gameObject);
            this.Defense = Mathf.Clamp(value, 0f, this.currentShield.maxDefense.GetValue(gameObject));
        }

        public void AddDefense(float value)
        {
            this.SetDefense(this.Defense + value);
        }

        public void SetTargetFocus(TargetMelee target)
        {
            if (target == null) return;

            var direction = CharacterLocomotion.OVERRIDE_FACE_DIRECTION.Target;
            var position = new TargetPosition(TargetPosition.Target.Transform)
            {
                targetTransform = target.transform
            };

            this.Character.characterLocomotion.overrideFaceDirection = direction;
            this.Character.characterLocomotion.overrideFaceDirectionTarget = position;

            target.SetTracker(this);

            this.HasFocusTarget = true;
            if (this.EventFocus != null) this.EventFocus.Invoke(true);
        }

        public void ReleaseTargetFocus()
        {
            if (!this.HasFocusTarget) return;

            var direction = CharacterLocomotion.OVERRIDE_FACE_DIRECTION.None;
            this.Character.characterLocomotion.overrideFaceDirection = direction;

            this.HasFocusTarget = false;
            if (this.EventFocus != null) this.EventFocus.Invoke(false);
        }

        // VIRTUAL METHODS: -----------------------------------------------------------------------

        protected virtual void OnSheatheWeapon()
        { }

        protected virtual void OnDrawWeapon()
        { }

        // CALLBACK METHODS: ----------------------------------------------------------------------

        public HitResult OnReceiveAttack(CharacterMelee attacker, MeleeClip attack)
        {
            if (this.currentWeapon == null) return HitResult.ReceiveDamage;
            if (this.IsInvincible) return HitResult.Ignore;

            if (this.Blade == null)
            {
                Debug.LogError("No BladeComponent found. Add one in your Weapon Asset", this);
                return HitResult.Ignore;
            }

            float attackAngle = Vector3.Angle(
                attacker.transform.TransformDirection(Vector3.forward),
                this.transform.TransformDirection(Vector3.forward)
            );

            float defenseAngle = this.currentShield != null
                ? this.currentShield.defenseAngle.GetValue(gameObject)
                : 0f;

            if (this.currentShield != null &&
                attack.isBlockable && this.IsBlocking &&
                180f - attackAngle < defenseAngle/2f)
            {
                this.AddDefense(-attack.defenseDamage);
                if (this.Defense > 0)
                {
                    if (Time.time < this.startBlockingTime + this.currentShield.perfectBlockWindow)
                    {
                        if (attacker != null)
                        {
                            MeleeClip attackerReaction = this.Character.IsGrounded()
                                ? this.currentShield.groundPerfectBlockReaction
                                : this.currentShield.airbornPerfectBlockReaction;

                            attackerReaction.Play(attacker);
                        }

                        if (this.currentShield.perfectBlockClip != null)
                        {
                            this.currentShield.perfectBlockClip.Play(this);
                        }

                        this.ExecuteEffects(
                            this.Blade.GetImpactPosition(),
                            this.currentShield.audioPerfectBlock,
                            this.currentShield.prefabImpactPerfectBlock
                        );

                        this.comboSystem.OnPerfectBlock();
                        return HitResult.PerfectBlock;
                    }

                    MeleeClip blockReaction = this.currentShield.GetBlockReaction();
                    if (blockReaction != null) blockReaction.Play(this);

                    this.ExecuteEffects(
                        this.Blade.GetImpactPosition(),
                        this.currentShield.audioBlock,
                        this.currentShield.prefabImpactBlock
                    );

                    this.comboSystem.OnBlock();
                    return HitResult.AttackBlock;
                }
                else
                {
                    this.Defense = 0f;
                    this.StopBlocking();

                    if (this.EventBreakDefense != null) this.EventBreakDefense.Invoke();
                }
            }

            this.AddPoise(-attack.poiseDamage);
            bool isFrontalAttack = attackAngle >= 90f;
            bool isKnockback = this.Poise <= float.Epsilon;

            MeleeClip hitReaction = this.currentWeapon.GetHitReaction(
                this.Character.IsGrounded(),
                isFrontalAttack,
                isKnockback
            );

            this.ExecuteEffects(
                attacker.Blade.GetImpactPosition(),
                isKnockback
                    ? this.currentWeapon.audioImpactKnockback
                    : this.currentWeapon.audioImpactNormal,
                isKnockback
                    ? this.currentWeapon.prefabImpactKnockback
                    : this.currentWeapon.prefabImpactNormal
            );

            attack.ExecuteHitPause();
            if (!this.IsUninterruptable)
            {
                hitReaction.Play(this);
            }

            return HitResult.ReceiveDamage;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ExecuteEffects(Vector3 position, AudioClip audio, GameObject prefab)
        {
            this.PlayAudio(audio);
            if (prefab != null)
            {
                GameObject impact = PoolManager.Instance.Pick(prefab);
                impact.transform.position = position;
            }
        }

        private bool CanAttack()
        {
            if (this.IsSheathing) return false;
            if (this.IsDrawing) return false;
            if (this.IsStaggered) return false;
            return true;
        }

        private float ResetState(CharacterState state, CharacterAnimation.Layer layer)
        {
            float time = TRANSITION;
            if (state != null)
            {
                if (state.exitClip != null)
                {
                    time = state.exitClip.length;
                }

                time = Mathf.Max(TRANSITION, time) * 0.5f;
                this.CharacterAnimator.ResetState(time, layer);
            }

            return time;
        }

        private float ChangeState(CharacterState state, AvatarMask mask, CharacterAnimation.Layer layer)
        {
            float time = TRANSITION;
            if (state != null)
            {
                if (state.enterClip != null)
                {
                    time = state.enterClip.length;
                }

                time = Mathf.Max(TRANSITION, time) * 0.5f;
                this.CharacterAnimator.SetState(state, mask, 1f, time, 1f, layer);
            }

            return time;
        }
    }
}
