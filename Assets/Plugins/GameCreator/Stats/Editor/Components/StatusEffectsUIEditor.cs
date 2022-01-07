namespace GameCreator.Stats
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using GameCreator.Core;

    [CustomEditor(typeof(StatusEffectsUI))]
    public class StatusEffectsUIEditor : Editor
    {
        private const string PROP_TARGET = "target";
        private const string PROP_SHOW = "show";

        private const string PROP_PREFAB = "prefabStatusEffect";
        private const string PROP_CONTAINER = "container";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spTarget;
        private SerializedProperty spShow;

        private SerializedProperty spPrefabStatusEffect;
        private SerializedProperty spContainer;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void OnEnable()
        {
            this.spTarget = serializedObject.FindProperty(PROP_TARGET);
            this.spShow = serializedObject.FindProperty(PROP_SHOW);

            this.spPrefabStatusEffect = serializedObject.FindProperty(PROP_PREFAB);
            this.spContainer = serializedObject.FindProperty(PROP_CONTAINER);
        }

        // PAINT METHODS: -------------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            if (target == null || serializedObject == null) return;
            serializedObject.Update();

            EditorGUILayout.PropertyField(this.spTarget);
            EditorGUILayout.PropertyField(this.spShow);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spContainer);
            EditorGUILayout.PropertyField(this.spPrefabStatusEffect);

            serializedObject.ApplyModifiedProperties();
        }
    }
}