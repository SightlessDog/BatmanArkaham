namespace GameCreator.Stats
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using GameCreator.Core;

    [CustomEditor(typeof(AttributeUI))]
    public class AttributeUIEditor : Editor
    {
        private const string PROP_TARGET = "target";
        private const string PROP_ATTRIBUTE = "attribute";

        private const string PROP_ICON = "icon";
        private const string PROP_COLOR = "color";
        private const string PROP_TITLE = "title";
        private const string PROP_DESCR = "description";
        private const string PROP_SHORT = "shortName";

        private const string PROP_VALFORMAT = "valueFormat";
        private const string PROP_VALUE = "value";

        private const string PROP_FILL = "valueFillImage";
        private const string PROP_SCALEX = "valueScaleX";
        private const string PROP_SCALEY = "valueScaleY";

        private const string PROP_TRANS_UP = "smoothTransitionUp";
        private const string PROP_TRANS_DN = "smoothTransitionDown";
        private const string PROP_TRANS_SPEED = "transitionSpeed";
        private const string PROP_TRANS_DELAY = "transitionDelay";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spTarget;
        private SerializedProperty spAttribute;

        private SerializedProperty spIcon;
        private SerializedProperty spColor;
        private SerializedProperty spTitle;
        private SerializedProperty spDescription;
        private SerializedProperty spShortName;

        private SerializedProperty spValueFormat;
        private SerializedProperty spValue;

        private SerializedProperty spValueFillImage;
        private SerializedProperty spValueScaleX;
        private SerializedProperty spValueScaleY;

        private SerializedProperty spTransitionUp;
        private SerializedProperty spTransitionDn;
        private SerializedProperty spTransitionSpeed;
        private SerializedProperty spTransitionDelay;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void OnEnable()
        {
            this.spTarget = serializedObject.FindProperty(PROP_TARGET);
            this.spAttribute = serializedObject.FindProperty(PROP_ATTRIBUTE);

            this.spIcon = serializedObject.FindProperty(PROP_ICON);
            this.spColor = serializedObject.FindProperty(PROP_COLOR);
            this.spTitle = serializedObject.FindProperty(PROP_TITLE);
            this.spDescription = serializedObject.FindProperty(PROP_DESCR);
            this.spShortName = serializedObject.FindProperty(PROP_SHORT);

            this.spValueFormat = serializedObject.FindProperty(PROP_VALFORMAT);
            this.spValue = serializedObject.FindProperty(PROP_VALUE);

            this.spValueFillImage = serializedObject.FindProperty(PROP_FILL);
            this.spValueScaleX = serializedObject.FindProperty(PROP_SCALEX);
            this.spValueScaleY = serializedObject.FindProperty(PROP_SCALEY);

            this.spTransitionUp = serializedObject.FindProperty(PROP_TRANS_UP);
            this.spTransitionDn = serializedObject.FindProperty(PROP_TRANS_DN);
            this.spTransitionSpeed = serializedObject.FindProperty(PROP_TRANS_SPEED);
            this.spTransitionDelay = serializedObject.FindProperty(PROP_TRANS_DELAY);
        }

        // PAINT METHODS: -------------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            if (target == null || serializedObject == null) return;
            serializedObject.Update();

            EditorGUILayout.PropertyField(this.spTarget);
            EditorGUILayout.PropertyField(this.spAttribute);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spIcon);
            EditorGUILayout.PropertyField(this.spColor);
            EditorGUILayout.PropertyField(this.spTitle);
            EditorGUILayout.PropertyField(this.spDescription);
            EditorGUILayout.PropertyField(this.spShortName);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spValueFormat);
            EditorGUILayout.PropertyField(this.spValue);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spValueFillImage);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spValueScaleX);
            EditorGUILayout.PropertyField(this.spValueScaleY);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spTransitionUp);
            EditorGUILayout.PropertyField(this.spTransitionDn);
            EditorGUILayout.PropertyField(this.spTransitionSpeed);
            EditorGUILayout.PropertyField(this.spTransitionDelay);

            serializedObject.ApplyModifiedProperties();
        }
    }
}