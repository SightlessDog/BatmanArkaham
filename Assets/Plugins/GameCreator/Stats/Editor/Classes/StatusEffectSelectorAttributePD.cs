namespace GameCreator.Stats
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using GameCreator.Core;

    [CustomPropertyDrawer(typeof(StatusEffectSelectorAttribute))]
    public class StatusEffectSelectorAttributePD : PropertyDrawer
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

            GUIContent gcStatusEffect = new GUIContent((property.objectReferenceValue == null
                ? "(none)"
                : ((StatusEffectAsset)property.objectReferenceValue).statusEffect.uniqueName
            ));

            EditorGUI.PrefixLabel(rectLabel, label);
            if (EditorGUI.DropdownButton(rectField, gcStatusEffect, FocusType.Keyboard))
            {
                PopupWindow.Show(
                    rectField,
                    new StefWindowPD(rectField, property)
                );
            }
        }
    }
}