namespace GameCreator.Stats
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using GameCreator.Core;

    [CustomEditor(typeof(StatUI))]
    public class StatUIEditor : Editor
    {
        private const string PROP_TARGET = "target";
        private const string PROP_STAT = "stat";

        private const string PROP_ICON = "icon";
        private const string PROP_COLOR = "color";
        private const string PROP_TITLE = "title";
        private const string PROP_DESCR = "description";
        private const string PROP_SHORT = "shortName";

        private const string PROP_VALUE = "value";
        private const string PROP_IMGFI = "imageFill";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spTarget;
        private SerializedProperty spStat;

        private SerializedProperty spTitle;
        private SerializedProperty spDescription;
        private SerializedProperty spShortName;

        private SerializedProperty spIcon;
        private SerializedProperty spColor;
        private SerializedProperty spValue;
        private SerializedProperty spImageFill;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void OnEnable()
        {
            this.spTarget = serializedObject.FindProperty(PROP_TARGET);
            this.spStat = serializedObject.FindProperty(PROP_STAT);

            this.spIcon = serializedObject.FindProperty(PROP_ICON);
            this.spColor = serializedObject.FindProperty(PROP_COLOR);
            this.spTitle = serializedObject.FindProperty(PROP_TITLE);
            this.spDescription = serializedObject.FindProperty(PROP_DESCR);
            this.spShortName = serializedObject.FindProperty(PROP_SHORT);

            this.spValue = serializedObject.FindProperty(PROP_VALUE);
            this.spImageFill = serializedObject.FindProperty(PROP_IMGFI);
        }

        // PAINT METHODS: -------------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            if (target == null || serializedObject == null) return;
            serializedObject.Update();

            EditorGUILayout.PropertyField(this.spTarget);
            EditorGUILayout.PropertyField(this.spStat);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spIcon);
            EditorGUILayout.PropertyField(this.spColor);
            EditorGUILayout.PropertyField(this.spTitle);
            EditorGUILayout.PropertyField(this.spDescription);
            EditorGUILayout.PropertyField(this.spShortName);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spValue);
            EditorGUILayout.PropertyField(this.spImageFill);

            serializedObject.ApplyModifiedProperties();
        }
    }
}