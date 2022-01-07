namespace GameCreator.Stats
{
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.AnimatedValues;
    using GameCreator.Core;

    [CustomPropertyDrawer(typeof(Formula))]
    public class FormulaPD : PropertyDrawer
    {
        private const float ELEM_PADDING = 2f;
        private const string PROP_FORMULA = "formula";
        private const string PROP_TABLE = "table";

        private static readonly GUIContent GC_DOCUMENTATION = new GUIContent("Reference");
        private static readonly GUIContent GC_TABLE = new GUIContent("Progression Table");
        private static GUIContent VALIDATE_BOX_TEXT = new GUIContent("");

        private static string VALIDATED_TEXT = "Nf1MwmlLTo";

        private const float TITLE_HEIGHT = 20f;
        private const float FORMULA_HEIGHT = 25f;
        private const float VALIDATE_HEIGHT = 18f;

        private const float SEPARATOR_HEIGHT = 10f;

        private const float DOC_TITLE_HEIGHT = 18f;
        private const float DOC_TEXT_HEIGHT = 18f;

        private static GUIStyle STYLE_TITLE;
        private static GUIStyle STYLE_FORMULA;
        private static GUIStyle STYLE_VALIDATE;

        private static GUIStyle STYLE_DOC_TITLE;
        private static GUIStyle STYLE_DOC_TEXT;

        private Texture2D TEXTURE_VALID;
        private Texture2D TEXTURE_ERROR;

        // DOCUMENTATION: -------------------------------------------------------------------------

        private class Doc
        {
            public string title;
            public string text;
            public string hint;

            public Doc(string title, string text, string hint = "")
            {
                this.title = title;
                this.text = text;
                this.hint = hint;
            }
        }

        private static readonly Doc[] DOCUMENTATION = new Doc[] {
            new Doc("<b>this</b>[<color=#666666>value</color>]", "Returns the current stat value (without modifiers)"),
            new Doc("<b>table</b>[<color=#666666>input</color>]", "Returns the corresponding value of the table given a numeric input"),
            new Doc("<b>table</b>:rise[<color=#666666>input</color>]", "Returns the current percentage filled towards the next tier"),
            new Doc("<b>rand</b>[<color=#666666>min</color>, <color=#666666>max</color>]", "Returns a random integer between min (inclusive) and max (exclusive)"),
            new Doc("<b>dice</b>[<color=#666666>rolls</color>, <color=#666666>sides</color>]", "Returns the result of rolling a dice with a defined set of parameters"),
            new Doc("<b>chance</b>[<color=#666666>input</color>]", "Returns 1 if a random value (from 0 to 1) is < or = than the input, and 0 otherwise"),
            new Doc("<b>stat</b>[<color=#666666>name</color>]", "Returns the value of a stat identified by 'name'"),
            new Doc("<b>attr</b>[<color=#666666>name</color>]", "Returns the value of an attribute identified by 'name'"),
            new Doc("<b>stat</b>:other[<color=#666666>name</color>]", "Returns the opponent (if any) stat value identified by 'name'"),
            new Doc("<b>attr</b>:other[<color=#666666>name</color>]", "Returns the opponent (if any) attribute value identified by 'name'"),
            new Doc("<b>local</b>[<color=#666666>name</color>]", "Returns the value of a local number variable identified by 'name'"),
            new Doc("<b>local</b>:other[<color=#666666>name</color>]", "Returns the opponent (if any) value of a local number variable identified by 'name'"),
            new Doc("<b>global</b>[<color=#666666>name</color>]", "Returns the value of a global variable identified by 'name'"),
            new Doc("<b>min</b>[<color=#666666>a</color>, <color=#666666>b</color>]", "Returns the smallest value"),
            new Doc("<b>max</b>[<color=#666666>a</color>, <color=#666666>b</color>]", "Returns the largest value"),
            new Doc("<b>round</b>[<color=#666666>value</color>]", "Rounds the value to the nearest integer"),
            new Doc("<b>floor</b>[<color=#666666>value</color>]", "Returns the largest integer smaller or equal"),
            new Doc("<b>ceil</b>[<color=#666666>value</color>]", "Returns the smallest integer greater or equal to X"),
        };

        // PAINT METHODS: -------------------------------------------------------------------------

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            this.InitializeStyles();

            Rect rectFormula = new Rect(
                position.x,
                position.y,
                position.width,
                FORMULA_HEIGHT
            );

            bool preventFormulaSelectAll = (UnityEngine.Event.current.type == EventType.MouseDown);
            Color formulaCursorColor = GUI.skin.settings.cursorColor;

            if (preventFormulaSelectAll)
                GUI.skin.settings.cursorColor = new Color(0, 0, 0, 0);

            SerializedProperty spFormula = property.FindPropertyRelative(PROP_FORMULA);
            spFormula.stringValue = EditorGUI.TextField(
                rectFormula,
                GUIContent.none,
                spFormula.stringValue, 
                STYLE_FORMULA
            );

            if (preventFormulaSelectAll)
            {
                GUI.skin.settings.cursorColor = formulaCursorColor;
            }

            Rect rectValidate = new Rect(
                rectFormula.x,
                rectFormula.y + rectFormula.height + SEPARATOR_HEIGHT,
                rectFormula.width,
                VALIDATE_HEIGHT
            );

            if (spFormula.stringValue != VALIDATED_TEXT)
            {
                VALIDATED_TEXT = spFormula.stringValue;
                this.ValidateFormula(VALIDATED_TEXT);
            }

            EditorGUI.LabelField(rectValidate, VALIDATE_BOX_TEXT, STYLE_VALIDATE);

            float positionY = this.PaintDocumentation(new Rect(
                position.x, 
                rectValidate.y + rectValidate.height,
                rectValidate.width,
                0.0f
            ));

            SerializedProperty spTable = property.FindPropertyRelative(PROP_TABLE);

            this.PaintTable(new Rect(
                position.x,
                positionY + ELEM_PADDING,
                rectValidate.width,
                0.0f
            ), spTable);
        }

        private float PaintDocumentation(Rect position)
        {
            Rect rectDocTitle = new Rect(
                position.x,
                position.y + SEPARATOR_HEIGHT,
                position.width,
                TITLE_HEIGHT
            );

            EditorGUI.LabelField(rectDocTitle, GC_DOCUMENTATION, STYLE_TITLE);

            Rect rectDoc = new Rect(
                rectDocTitle.x,
                rectDocTitle.y + rectDocTitle.height + SEPARATOR_HEIGHT,
                rectDocTitle.width,
                DOC_TITLE_HEIGHT + ELEM_PADDING + DOC_TEXT_HEIGHT + ELEM_PADDING
            );

            float height = rectDoc.y + rectDoc.height;
            for (int i = 0; i < DOCUMENTATION.Length; ++i)
            {
                Rect rectTitle = new Rect(
                    rectDoc.x,
                    rectDoc.y + (i * rectDoc.height),
                    rectDoc.width,
                    DOC_TITLE_HEIGHT
                );

                Rect rectText = new Rect(
                    rectTitle.x,
                    rectTitle.y + rectTitle.height + ELEM_PADDING,
                    rectDoc.width,
                    DOC_TEXT_HEIGHT
                );

                EditorGUI.LabelField(
                    rectTitle,
                    DOCUMENTATION[i].title,
                    STYLE_DOC_TITLE
                );

                EditorGUI.LabelField(
                    rectText,
                    DOCUMENTATION[i].text,
                    STYLE_DOC_TEXT
                );

                height = rectText.y + rectText.height;
            }

            return height;
        }

        private void PaintTable(Rect position, SerializedProperty property)
        {
            Rect rectTableTitle = new Rect(
                position.x,
                position.y + SEPARATOR_HEIGHT,
                position.width,
                TITLE_HEIGHT
            );

            EditorGUI.LabelField(rectTableTitle, GC_TABLE, STYLE_TITLE);

            Rect rectTable = new Rect(
                rectTableTitle.x,
                rectTableTitle.y + rectTableTitle.height + EditorGUIUtility.singleLineHeight,
                rectTableTitle.width,
                EditorGUI.GetPropertyHeight(property)
            );

            EditorGUI.PropertyField(rectTable, property, true);
        }

        // HEIGHT: --------------------------------------------------------------------------------

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (
                90f +
                DOCUMENTATION.Length * (DOC_TITLE_HEIGHT + DOC_TEXT_HEIGHT + ELEM_PADDING) +
                3 * (EditorGUIUtility.singleLineHeight) + 
                EditorGUI.GetPropertyHeight(property.FindPropertyRelative(PROP_TABLE)) + 
                EditorGUIUtility.singleLineHeight
            );
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static bool INIT_STYLES = false;

        private void InitializeStyles()
        {

            if (TEXTURE_VALID == null || TEXTURE_ERROR == null)
            {
                TEXTURE_VALID = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/GameCreator/Stats/Icons/Valid.png");
                TEXTURE_ERROR = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/GameCreator/Stats/Icons/Error.png");
            }

            if (INIT_STYLES) return;
            INIT_STYLES = true;

            STYLE_TITLE = new GUIStyle(EditorStyles.helpBox);
            STYLE_TITLE.alignment = TextAnchor.MiddleLeft;
            STYLE_TITLE.fixedHeight = TITLE_HEIGHT;
            STYLE_TITLE.fontStyle = FontStyle.Bold;
            STYLE_TITLE.fontSize = 11;

            STYLE_FORMULA = new GUIStyle(EditorStyles.textField);
            STYLE_FORMULA.alignment = TextAnchor.MiddleLeft;
            STYLE_FORMULA.padding = new RectOffset(5, 5, 0, 0);
            STYLE_FORMULA.margin = new RectOffset(0, 0, 0, 0);
            STYLE_FORMULA.fixedHeight = FORMULA_HEIGHT;

            STYLE_VALIDATE = new GUIStyle(CoreGUIStyles.GetButtonRight());
            STYLE_VALIDATE.fixedHeight = FORMULA_HEIGHT;

            STYLE_VALIDATE = new GUIStyle(EditorStyles.label);
            STYLE_VALIDATE.alignment = TextAnchor.MiddleLeft;
            STYLE_VALIDATE.fixedHeight = VALIDATE_HEIGHT;
            STYLE_VALIDATE.fontSize = 10;
            STYLE_VALIDATE.richText = true;
            STYLE_VALIDATE.padding = new RectOffset(0, 0, 0, 0);
            STYLE_VALIDATE.margin = new RectOffset(0, 0, 0, 0);
            STYLE_VALIDATE.imagePosition = ImagePosition.ImageLeft;

            STYLE_DOC_TITLE = new GUIStyle(EditorStyles.label);
            STYLE_DOC_TITLE.fontSize = 12;
            STYLE_DOC_TITLE.richText = true;
            STYLE_DOC_TITLE.alignment = TextAnchor.MiddleLeft;

            STYLE_DOC_TEXT = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
            STYLE_DOC_TITLE.richText = true;
            STYLE_DOC_TEXT.alignment = TextAnchor.UpperLeft;
        }

        private void ValidateFormula(string formula)
        {
            bool checkDelimiters = CheckDelimiters(formula);

            if (checkDelimiters)
            {
                VALIDATE_BOX_TEXT = new GUIContent(" Expression seems valid", TEXTURE_VALID);
            }
            else
            {
                VALIDATE_BOX_TEXT = new GUIContent(" Oops! There's something wrong", TEXTURE_ERROR);
            }
        }

        private static readonly Dictionary<char, char> DELIMITERS = new Dictionary<char, char>()
        {
            { ')', '(' },
            { ']', '[' },
        };

        private static bool CheckDelimiters(string content)
        {
            Stack<char> delimiters = new Stack<char>();

            for (int i = 0; i < content.Length; ++i)
            {
                switch (content[i])
                {
                    case '[':
                    case '(':
                        delimiters.Push(content[i]);
                        break;

                    case ']':
                    case ')':
                        if (delimiters.Count > 0 && delimiters.Peek() == DELIMITERS[content[i]]) delimiters.Pop();
                        else return false;
                        break;
                }
            }

            if (delimiters.Count > 0) return false;
            return true;
        }
    }
}