namespace GameCreator.Stats
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using GameCreator.Core;

    [CustomPropertyDrawer(typeof(StatSelectorAttribute))]
    public class StatSelectorAttributePD : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect rectLabel = new Rect(
                position.x,
                position.y,
                EditorGUIUtility.labelWidth,
                position.height
            );

            Rect rectField = new Rect(
                rectLabel.x + rectLabel.width,
                rectLabel.y,
                position.width - rectLabel.width,
                rectLabel.height
            );

            GUIContent gcStat = new GUIContent((property.objectReferenceValue == null
                ? "(none)"
                : ((StatAsset)property.objectReferenceValue).stat.uniqueName
            ));

            EditorGUI.PrefixLabel(rectLabel, label);
            if (EditorGUI.DropdownButton(rectField, gcStat, FocusType.Keyboard))
            {
                PopupWindow.Show(
                    rectField,
                    new StatWindowPD(rectField, property)
                );
            }
        }
    }
}