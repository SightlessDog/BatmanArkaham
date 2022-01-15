namespace GameCreator.Melee
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(CharacterMelee))]
    public class CharacterMeleeEditor : Editor
    {
        private SerializedProperty spCurrentWeapon;
        private SerializedProperty spCurrentShield;

        private SerializedProperty spPoiseDelay;
        private SerializedProperty spPoiseMax;
        private SerializedProperty spPoiseRecovery;

        // INITIALIZER: ---------------------------------------------------------------------------

        private void OnEnable()
        {
            this.spCurrentWeapon = this.serializedObject.FindProperty("currentWeapon");
            this.spCurrentShield = this.serializedObject.FindProperty("currentShield");

            this.spPoiseDelay = this.serializedObject.FindProperty("delayPoise");
            this.spPoiseMax = this.serializedObject.FindProperty("maxPoise");
            this.spPoiseRecovery = this.serializedObject.FindProperty("poiseRecoveryRate");
        }

        // PAINT METHODS: -------------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(this.spCurrentWeapon);
            EditorGUILayout.PropertyField(this.spCurrentShield);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spPoiseDelay);
            EditorGUILayout.PropertyField(this.spPoiseMax);
            EditorGUILayout.PropertyField(this.spPoiseRecovery);

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
 