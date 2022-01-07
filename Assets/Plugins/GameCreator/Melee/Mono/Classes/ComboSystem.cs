namespace GameCreator.Melee
{
    using System;
    using System.Collections;
	using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Core.Hooks;

    public class ComboSystem
	{
        public enum Condition
        {
            None,
            OnAir,
            AfterBlock,
            AfterPerfectBlock,
            RunningForward,
            RunningBackwards,
            InputForward,
            InputBackwards
        }

        private static readonly Vector3 PLANE = new Vector3(1, 0, 1);
        private const float BLOCK_WINDOW = 0.75f;

        private const string AXIS_H = "Horizontal";
        private const string AXIS_V = "Vertical";

        // PROPERTIES: ----------------------------------------------------------------------------

        private readonly TreeCombo<CharacterMelee.ActionKey, Combo> root;
        private TreeCombo<CharacterMelee.ActionKey, Combo> current;

        private readonly CharacterMelee melee;

        private float startAttackTime = -100f;
        private Combo currentCombo;

        private bool isBlock;
        private bool isPerfectBlock;
        private float blockTime;
        private float perfectBlockTime;

        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public ComboSystem(CharacterMelee melee, List<Combo> combos)
        {
            this.melee = melee;
            this.startAttackTime = -100;

            this.root = new TreeCombo<CharacterMelee.ActionKey, Combo>(CharacterMelee.ActionKey.A);

            foreach (Combo combo in combos)
            {
                CharacterMelee.ActionKey[] actions = combo.combo;
                TreeCombo<CharacterMelee.ActionKey, Combo> node = this.root;

                for (int i = 0; i < actions.Length; ++i)
                {
                    if (node.HasChild(actions[i]))
                    {
                        node = node.GetChild(actions[i]);
                    }
                    else
                    {
                        node = node.AddChild(new TreeCombo<CharacterMelee.ActionKey, Combo>(
                            actions[i]
                        ));
                    }

                    if (i == actions.Length - 1)
                    {
                        node.SetData(combo);
                    }
                }
            }
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public int GetCurrentPhase()
        {
            float prevTime = Time.time - this.startAttackTime;
            float currentPhase = -1f;

            if (this.currentCombo == null) return Mathf.RoundToInt(currentPhase);
            if (this.currentCombo.meleeClip == null) return Mathf.RoundToInt(currentPhase);

            if (prevTime < this.currentCombo.meleeClip.Length)
            {
                currentPhase = this.currentCombo.meleeClip.attackPhase.Evaluate(prevTime);
            }

            return Mathf.RoundToInt(currentPhase);
        }

        public MeleeClip GetCurrentClip()
        {
            if (this.currentCombo == null) return null;
            return this.currentCombo.meleeClip;
        }

        public MeleeClip Select(CharacterMelee.ActionKey actionkey)
        {
            int currentPhase = this.GetCurrentPhase();

            if (currentPhase == -1 && this.melee.Character.characterLocomotion.isBusy) return null;

            if (currentPhase == 0) return null;
            if (currentPhase == 1) return null;

            TreeCombo<CharacterMelee.ActionKey, Combo> next = this.current;
            if (next == null || this.currentCombo == null) next = this.root;

            if (!next.HasChild(actionkey)) return null;
            next = next.GetChild(actionkey);

            this.startAttackTime = Time.time;
            this.current = next;

            this.currentCombo = this.SelectMeleeClip(); ;

            this.isBlock = false;
            this.isPerfectBlock = false;

            if (this.currentCombo == null) return null;
            return this.currentCombo.meleeClip;
        }

        public void Stop()
        {
            this.startAttackTime = -100f;

            this.current = this.root;
            this.currentCombo = null;
        }

        public void Update()
        {
            if (this.currentCombo == null) return;

            if (Time.time - this.startAttackTime > this.currentCombo.meleeClip.Length)
            {
                this.Stop();
            }
        }

        // CALLBACKS: -----------------------------------------------------------------------------

        public void OnPerfectBlock()
        {
            this.isPerfectBlock = true;
            this.perfectBlockTime = Time.time;
        }

        public void OnBlock()
        {
            this.isBlock = true;
            this.blockTime = Time.time;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private float GetRunningAngle()
        {
            float runSpeed = this.melee.Character.characterLocomotion.runSpeed - 0.15f;
            if (this.melee.Character.GetCharacterState().forwardSpeed.sqrMagnitude <= runSpeed)
            {
                return 1000;
            }

            Vector3 characterForward = this.melee.transform.TransformDirection(Vector3.forward);
            Vector3 moveDirection = this.melee.Character.characterLocomotion.GetMovementDirection();
            Vector3 charDirection = Vector3.Scale(characterForward, PLANE);

            return Vector3.SignedAngle(moveDirection, charDirection, Vector3.up);
        }

        private float GetInputAngle()
        {
            float inputH = Input.GetAxisRaw(AXIS_H);
            float inputV = Input.GetAxisRaw(AXIS_V);

            Vector3 input = new Vector3(inputH, 0.0f, inputV);
            Vector3 direction = HookCamera.Instance.transform.TransformDirection(input);

            direction.Scale(PLANE);
            if (direction.sqrMagnitude <= 0.1f) return 1000;

            return Vector3.SignedAngle(
                this.melee.transform.TransformDirection(Vector3.forward),
                direction,
                Vector3.up
            );
        }

        private Combo SelectMeleeClip()
        {
            Combo[] candidates = this.current.GetData();
            float runAngle = this.GetRunningAngle();
            float inputAngle = this.GetInputAngle();

            for (int i = 0; i < candidates.Length; ++i)
            {
                if (!candidates[i].isEnabled) continue;
                switch (candidates[i].condition)
                {
                    case Condition.None:
                        return candidates[i];

                    case Condition.OnAir:
                        if (!this.melee.Character.IsGrounded())
                        {
                            return candidates[i];
                        }
                        break;

                    case Condition.RunningForward:
                        if (runAngle <= 20f && runAngle >= -20f)
                        {
                            return candidates[i];
                        }
                        break;

                    case Condition.RunningBackwards:
                        if (runAngle <= 500f && (runAngle <= -160f || runAngle >= 160f))
                        {
                            return candidates[i];
                        }
                        break;

                    case Condition.InputForward:
                        if (inputAngle <= 20f && inputAngle >= -20f)
                        {
                            return candidates[i];
                        }
                        break;

                    case Condition.InputBackwards:
                        if (inputAngle <= 500f && (inputAngle <= -160f || inputAngle >= 160f))
                        {
                            return candidates[i];
                        }
                        break;

                    case Condition.AfterBlock:
                        if (this.isBlock && Time.time < this.blockTime + BLOCK_WINDOW)
                        {
                            return candidates[i];
                        }
                        break;

                    case Condition.AfterPerfectBlock:
                        if (this.isPerfectBlock && Time.time < this.perfectBlockTime + BLOCK_WINDOW)
                        {
                            return candidates[i];
                        }
                        break;
                }
            }

            return null;
        }
	}
}