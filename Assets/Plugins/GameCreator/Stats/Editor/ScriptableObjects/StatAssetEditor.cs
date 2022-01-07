namespace GameCreator.Stats
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using GameCreator.Core;

    [CustomEditor(typeof(StatAsset))]
    public class StatAssetEditor : Editor
    {
        public const string PROP_UNIQUE_ID = "uniqueID";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spUniqueID;
        private SerializedProperty spStat;

        public SerializedProperty spIsHidden;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void OnEnable()
        {
            if (target == null || serializedObject == null) return;

            this.spUniqueID = serializedObject.FindProperty(PROP_UNIQUE_ID);
            this.spStat = serializedObject.FindProperty("stat");
            this.spIsHidden = serializedObject.FindProperty("isHidden");

            if (string.IsNullOrEmpty(this.spUniqueID.stringValue))
            {
                this.spUniqueID.stringValue = Guid.NewGuid().ToString("N");
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                serializedObject.Update();
            }
        }

        // PAINT METHODS: -------------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            if (target == null || serializedObject == null) return;

            serializedObject.Update();
            EditorGUILayout.PropertyField(this.spStat);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}