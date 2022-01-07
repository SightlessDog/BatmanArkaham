namespace GameCreator.Stats
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.AnimatedValues;
    using GameCreator.Core;

    [CustomEditor(typeof(FormulaAsset))]
    public class FormulaAssetEditor : Editor
    {
        private SerializedProperty spFormula;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void OnEnable()
        {
            this.spFormula = serializedObject.FindProperty("formula");
        }

        // PAINT METHODS: -------------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(this.spFormula, true);
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}