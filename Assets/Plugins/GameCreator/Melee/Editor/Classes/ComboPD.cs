namespace GameCreator.Melee
{
    using System;
    using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using UnityEditor;
    using GameCreator.Core;
    using System.Linq;
    using System.Linq.Expressions;

    [CustomPropertyDrawer(typeof(Combo))]
	public class ComboPD : PropertyDrawer
	{
        private const float COMBO_KEY_WIDTH = 30f;
        private const float COMBO_BTN_WIDTH = 25f;

        private const int MIN_NUM_COMBOS = 1;
        private const int MAX_NUM_COMBOS = 6;

        private SerializedProperty spComboItemSelected;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty spCombo = property.FindPropertyRelative("combo");
            SerializedProperty spCondition = property.FindPropertyRelative("condition");
            SerializedProperty spMeleeClip = property.FindPropertyRelative("meleeClip");
            SerializedProperty spIsEnabled = property.FindPropertyRelative("isEnabled");

            float comboLabelWidth = position.width - (
                (spCombo.arraySize * COMBO_KEY_WIDTH) +
                (2 * COMBO_BTN_WIDTH) +
                EditorGUIUtility.standardVerticalSpacing
            );

            Rect rectComboLabel = new Rect(
                position.x,
                position.y,
                comboLabelWidth,
                EditorGUIUtility.singleLineHeight
            );

            EditorGUI.LabelField(rectComboLabel, spCombo.displayName);

            Rect rectComboItem = new Rect(rectComboLabel);

            for (int i = 0; i < spCombo.arraySize; ++i)
            {
                rectComboItem = new Rect(
                    rectComboItem.x + rectComboItem.width,
                    rectComboItem.y,
                    COMBO_KEY_WIDTH,
                    EditorGUIUtility.singleLineHeight
                );

                SerializedProperty spComboItem = spCombo.GetArrayElementAtIndex(i);
                CharacterMelee.ActionKey item = (CharacterMelee.ActionKey)spComboItem.enumValueIndex;

                GUIStyle comboItemStyle = GUI.skin.button;
                if (spCombo.arraySize > 1)
                {
                    if (i == spCombo.arraySize - 1) comboItemStyle = CoreGUIStyles.GetButtonRight();
                    else if (i == 0) comboItemStyle = CoreGUIStyles.GetButtonLeft();
                    else comboItemStyle = CoreGUIStyles.GetButtonMid();
                }

                if (GUI.Button(rectComboItem, item.ToString(), comboItemStyle))
                {
                    this.spComboItemSelected = spComboItem;
                    GenericMenu comboMenu = new GenericMenu();
                    CharacterMelee.ActionKey[] options = Enum.GetValues(typeof(CharacterMelee.ActionKey))
                        .Cast<CharacterMelee.ActionKey>()
                        .Select(x => x)
                        .ToArray();

                    for (int j = 0; j < options.Length; ++j)
                    {
                        comboMenu.AddItem(
                            new GUIContent(options[j].ToString()),
                            item == options[j],
                            this.SelectOption,
                            j
                        );
                    }

                    comboMenu.ShowAsContext();
                }
            }

            Rect rectComboButtonRmv = new Rect(
                rectComboItem.x + rectComboItem.width + EditorGUIUtility.standardVerticalSpacing,
                rectComboItem.y,
                COMBO_BTN_WIDTH,
                EditorGUIUtility.singleLineHeight
            );

            Rect rectComboButtonAdd = new Rect(
                rectComboButtonRmv.x + rectComboButtonRmv.width,
                rectComboButtonRmv.y,
                COMBO_BTN_WIDTH,
                EditorGUIUtility.singleLineHeight
            );

            EditorGUI.BeginDisabledGroup(spCombo.arraySize <= MIN_NUM_COMBOS);
            if (GUI.Button(rectComboButtonRmv, "-", CoreGUIStyles.GetButtonLeft()))
            {
                spCombo.DeleteArrayElementAtIndex(spCombo.arraySize - 1);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(spCombo.arraySize >= MAX_NUM_COMBOS);
            if (GUI.Button(rectComboButtonAdd, "+", CoreGUIStyles.GetButtonRight()))
            {
                spCombo.InsertArrayElementAtIndex(spCombo.arraySize);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.indentLevel += 1;

            Rect rectCondition = new Rect(
                position.x,
                rectComboLabel.y + rectComboLabel.height + EditorGUIUtility.standardVerticalSpacing,
                position.width,
                EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(rectCondition, spCondition);

            Rect rectMeleeClip = new Rect(
                rectCondition.x,
                rectCondition.y + rectCondition.height + EditorGUIUtility.standardVerticalSpacing,
                rectCondition.width,
                EditorGUIUtility.singleLineHeight
            );

            Rect rectIsEnabled = new Rect(
                rectMeleeClip.x,
                rectMeleeClip.y + rectMeleeClip.height + EditorGUIUtility.standardVerticalSpacing,
                rectMeleeClip.width,
                EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(rectMeleeClip, spMeleeClip);
            EditorGUI.PropertyField(rectIsEnabled, spIsEnabled);

            EditorGUI.indentLevel -= 1;
        }

        private void SelectOption(object value)
        {
            this.spComboItemSelected.intValue = (int)value;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetHeight();
        }

        public static float GetHeight()
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