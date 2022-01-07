namespace GameCreator.Stats
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using GameCreator.Variables;

    [CustomPropertyDrawer(typeof(Stat))]
    public class StatPD : PropertyDrawer
    {
        public const string PROP_UNIQUE_NAME = "uniqueName";
        public const string PROP_SHORT = "shortName";
        public const string PROP_TITLE = "title";
        public const string PROP_DESCR = "description";
        public const string PROP_COLOR = "color";
        public const string PROP_ICON = "icon";
        public const string PROP_VALUE = "baseValue";
        public const string PROP_FORMULA = "formula";

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
            Rect rectValue = new Rect(
                rect.x,
                rect.y + ELEM_HEIGHT,
                rect.width,
                ELEM_HEIGHT
            );

            Rect rectFormula = new Rect(
                rectValue.x,
                rectValue.y + rectValue.height + ELEM_PADDING,
                rectValue.width,
                ELEM_HEIGHT
            );

            EditorGUI.PropertyField(rectValue, property.FindPropertyRelative(PROP_VALUE));
            EditorGUI.PropertyField(rectFormula, property.FindPropertyRelative(PROP_FORMULA));

            return new Rect(rect.x, rectFormula.y + rectFormula.height, rect.width, 0.0f);
        }

        // HEIGHT: --------------------------------------------------------------------------------

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float header = RECT_SPRITE.y;
            float body = ELEM_HEIGHT + ELEM_HEIGHT + ELEM_PADDING;
            return header + body;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static string ProcessName(string name)
        {
            return name.Trim().Replace(' ', '-');
        }
    }
}