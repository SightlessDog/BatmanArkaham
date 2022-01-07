namespace GameCreator.Stats
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using GameCreator.Core;

    [CustomEditor(typeof(AttrAsset))]
    public class AttrAssetEditor : Editor
    {
        public const string PROP_UNIQUE_ID = "uniqueID";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spUniqueID;
        private SerializedProperty spAttribute;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void OnEnable()
        {
            if (target == null || serializedObject == null) return;

            this.spUniqueID = serializedObject.FindProperty(PROP_UNIQUE_ID);
            this.spAttribute = serializedObject.FindProperty("attribute");

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
            EditorGUILayout.PropertyField(this.spAttribute);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}