namespace GameCreator.Stats
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using GameCreator.Core;

    [CustomPropertyDrawer(typeof(AttributeSelectorAttribute))]
    public class AttrSelectorAttributePD : PropertyDrawer
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

            GUIContent gcAttribute = new GUIContent((property.objectReferenceValue == null
                ? "(none)"
                : ((AttrAsset)property.objectReferenceValue).attribute.uniqueName
            ));

            EditorGUI.PrefixLabel(rectLabel, label);
            if (EditorGUI.DropdownButton(rectField, gcAttribute, FocusType.Keyboard))
            {
                PopupWindow.Show(
                    rectField,
                    new AttrWindowPD(rectField, property)
                );
            }
        }
    }
}