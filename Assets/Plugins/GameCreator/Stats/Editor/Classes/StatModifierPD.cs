namespace GameCreator.Stats
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using GameCreator.Variables;

    [CustomPropertyDrawer(typeof(StatModifier))]
    public class StatModifierPD : PropertyDrawer
    {
        public const string PROP_STAT = "stat";
        public const string PROP_TYPE = "type";
        public const string PROP_VALUE = "value";

        private static readonly GUIContent GC_STAT_MODIFIER = new GUIContent("Stat Modifier");

        // PAINT METHODS: -------------------------------------------------------------------------

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect rectTitle = new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight
            );

            Rect rectStat = new Rect(
                rectTitle.x,
                rectTitle.y + rectTitle.height + EditorGUIUtility.standardVerticalSpacing,
                rectTitle.width,
                EditorGUIUtility.singleLineHeight
            );

            Rect rectType = new Rect(
                rectStat.x,
                rectStat.y + rectStat.height + EditorGUIUtility.standardVerticalSpacing,
                rectStat.width,
                EditorGUIUtility.singleLineHeight
            );

            Rect rectValue = new Rect(
                rectType.x,
                rectType.y + rectType.height + EditorGUIUtility.standardVerticalSpacing,
                rectType.width,
                EditorGUIUtility.singleLineHeight
            );

            EditorGUI.LabelField(rectTitle, GC_STAT_MODIFIER);

            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(rectStat, property.FindPropertyRelative(PROP_STAT));
            EditorGUI.PropertyField(rectType, property.FindPropertyRelative(PROP_TYPE));
            EditorGUI.PropertyField(rectValue, property.FindPropertyRelative(PROP_VALUE));
            EditorGUI.indentLevel--;
        }


        // HEIGHT: --------------------------------------------------------------------------------

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +
                EditorGUIUtility.singleLineHeight
            );
        }
    }
}