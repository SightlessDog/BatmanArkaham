namespace GameCreator.Stats
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEditorInternal;
    using GameCreator.Variables;

    [CustomPropertyDrawer(typeof(StatusEffect))]
    public class StatusEffectPD : PropertyDrawer
    {
        public const string PROP_UNIQUE_NAME = "uniqueName";
        public const string PROP_SHORT = "shortName";
        public const string PROP_TITLE = "title";
        public const string PROP_DESCR = "description";
        public const string PROP_COLOR = "color";
        public const string PROP_ICON = "icon";

        public const string PROP_TYPE = "type";
        public const string PROP_HAS_DURATION = "hasDuration";
        public const string PROP_DURATION = "duration";
        public const string PROP_MAX_STACKS = "maxStack";

        public const string PROP_ACT_ONSTART = "actionOnStart";
        public const string PROP_ACT_WHILEACTIVE = "actionWhileActive";
        public const string PROP_MAX_ONEND = "actionOnEnd";

        private const float ELEM_HEIGHT = 16f;
        private const float ELEM_PADDING = 2f;
        private static readonly Vector2 RECT_SPRITE = new Vector2(70f, 70f);
        private static readonly Vector2 RECT_COLOR = new Vector2(15f, 70f);

        private static readonly GUIContent GC_NAME = new GUIContent("Name (ID)");

        // PAINT METHODS: -------------------------------------------------------------------------

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = this.PaintHead(position, property);
            position = this.PaintBody(position, property);
        }

        private Rect PaintHead(Rect position, SerializedProperty property)
        {
            Rect rectTest = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label);
            float availableWidth = rectTest.width;

            Rect rectSprite = new Rect(
                position.x,
                position.y,
                RECT_SPRITE.x,
                RECT_SPRITE.y
            );

            Rect rectColor = new Rect(
                rectSprite.x + availableWidth - RECT_COLOR.x,
                rectSprite.y,
                RECT_COLOR.x,
                RECT_COLOR.y
            );

            Rect rectUniqueName = new Rect(
                rectSprite.x + rectSprite.width + ELEM_PADDING,
                rectSprite.y,
                availableWidth - (rectSprite.width + ELEM_PADDING) - (rectColor.width + ELEM_PADDING),
                ELEM_HEIGHT
            );

            Rect rectTitle = new Rect(
                rectUniqueName.x,
                rectUniqueName.y + rectUniqueName.height + ELEM_PADDING,
                rectUniqueName.width,
                rectUniqueName.height
            );

            Rect rectDescription = new Rect(
                rectTitle.x,
                rectTitle.y + rectTitle.height + ELEM_PADDING,
                rectTitle.width,
                rectTitle.height
            );

            Rect rectShort = new Rect(
                rectDescription.x,
                rectDescription.y + rectDescription.height + ELEM_PADDING,
                rectDescription.width,
                rectDescription.height
            );

            EditorGUI.ObjectField(rectSprite, property.FindPropertyRelative(PROP_ICON), typeof(Sprite), GUIContent.none);

            SerializedProperty spColor = property.FindPropertyRelative(PROP_COLOR);
            #if UNITY_2018_1_OR_NEWER
            spColor.colorValue = EditorGUI.ColorField(rectColor, GUIContent.none, spColor.colorValue, false, false, false);
            #else
            spColor.colorValue = EditorGUI.ColorField(rectColor, GUIContent.none, spColor.colorValue, false, false, false, null);
            #endif

            SerializedProperty spUniqueName = property.FindPropertyRelative(PROP_UNIQUE_NAME);
            EditorGUI.PropertyField(rectUniqueName, spUniqueName, GC_NAME);
            spUniqueName.stringValue = ProcessName(spUniqueName.stringValue);
                
            EditorGUI.PropertyField(rectTitle, property.FindPropertyRelative(PROP_TITLE));
            EditorGUI.PropertyField(rectDescription, property.FindPropertyRelative(PROP_DESCR));
            EditorGUI.PropertyField(rectShort, property.FindPropertyRelative(PROP_SHORT));

            return new Rect(position.x, position.y + rectSprite.height, availableWidth, 0f);
        }

        private Rect PaintBody(Rect rect, SerializedProperty property)
        {
            Rect rectType = new Rect(
                rect.x,
                rect.y + ELEM_HEIGHT + ELEM_PADDING,
                rect.width,
                ELEM_HEIGHT
            );

            Rect rectHasDuration = new Rect(
                rectType.x,
                rectType.y + rectType.height + ELEM_PADDING,
                rectType.width,
                ELEM_HEIGHT
            );

            Rect rectDuration = new Rect(
                rectHasDuration.x,
                rectHasDuration.y + rectHasDuration.height + ELEM_PADDING,
                rectHasDuration.width,
                ELEM_HEIGHT
            );

            Rect rectStack = new Rect(
                rectDuration.x,
                rectDuration.y + rectDuration.height + ELEM_PADDING,
                rectDuration.width,
                ELEM_HEIGHT
            );

            EditorGUI.PropertyField(rectType, property.FindPropertyRelative(PROP_TYPE));

            SerializedProperty spHasDuration = property.FindPropertyRelative(PROP_HAS_DURATION);
            EditorGUI.PropertyField(rectHasDuration, spHasDuration);
            EditorGUI.BeginDisabledGroup(!spHasDuration.boolValue);
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(rectDuration, property.FindPropertyRelative(PROP_DURATION));
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();
            EditorGUI.PropertyField(rectStack, property.FindPropertyRelative(PROP_MAX_STACKS));

            return new Rect(rectStack.x, rectStack.y + rectStack.height, rectStack.width, 0f);
        }

        // HEIGHT: --------------------------------------------------------------------------------

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float header = RECT_SPRITE.y;
            float body = (ELEM_HEIGHT * 4f) + (ELEM_PADDING * 4f);
            return header + body;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static string ProcessName(string name)
        {
            return name.Trim().Replace(' ', '-');
        }
    }
}