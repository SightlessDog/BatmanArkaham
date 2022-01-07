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
	public class ActionMeleeDraw : IAction
	{
		public TargetCharacter character = new TargetCharacter(TargetCharacter.Target.Player);

        [Space]
        public bool drawPreviousWeapon = false;

        public MeleeWeapon meleeWeapon;
        public MeleeShield meleeShield;

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            Character _character = this.character.GetCharacter(target);
            if (character == null) return true;

            CharacterMelee melee = _character.GetComponent<CharacterMelee>();
            if (melee != null)
            {
                CoroutinesManager.Instance.StartCoroutine(melee.Draw(
                    this.drawPreviousWeapon ? melee.previousWeapon : this.meleeWeapon,
                    this.drawPreviousWeapon ? melee.previousShield : this.meleeShield
                ));
            }

            return true;
        }

		#if UNITY_EDITOR

        public static new string NAME = "Melee/Draw Weapon";
        private const string NODE_TITLE = "Character {0} draw {1} {2}";

        public override string GetNodeTitle()
        {
            return string.Format(
                NODE_TITLE,
                this.character,
                this.meleeWeapon ? this.meleeWeapon.name : "(none)",
                this.meleeShield ? "and " + this.meleeShield.name : string.Empty
            );
        }

        private SerializedProperty spCharacter;
        private SerializedProperty spDrawPreviousWeapon;
        private SerializedProperty spMeleeWeapon;
        private SerializedProperty spMeleeShield;

        protected override void OnEnableEditorChild()
        {
            base.OnEnableEditorChild();

            this.spCharacter = this.serializedObject.FindProperty("character");
            this.spDrawPreviousWeapon = this.serializedObject.FindProperty("drawPreviousWeapon");
            this.spMeleeWeapon = this.serializedObject.FindProperty("meleeWeapon");
            this.spMeleeShield = this.serializedObject.FindProperty("meleeShield");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spCharacter);
            EditorGUILayout.PropertyField(this.spDrawPreviousWeapon);

            EditorGUI.BeginDisabledGroup(this.spDrawPreviousWeapon.boolValue);
            EditorGUILayout.PropertyField(this.spMeleeWeapon);
            EditorGUILayout.PropertyField(this.spMeleeShield);
            EditorGUI.EndDisabledGroup();

            this.serializedObject.ApplyModifiedProperties();
        }

        #endif
    }
}
