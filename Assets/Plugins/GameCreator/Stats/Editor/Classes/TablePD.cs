namespace GameCreator.Stats
{
    using System;
    using System.Text;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using GameCreator.Variables;

    [CustomPropertyDrawer(typeof(Table))]
    public class TablePD : PropertyDrawer
    {
        private const string PROP_MAXRESULT = "maxTier";
        private const string PROP_THRESHOLD = "threshold";

        private static readonly Color PLOT_COLOR = new Color(0, 0, 0, 0.5f);
        private const float PLOT_HEIGHT = 100f;

        private static readonly Color BAR_HIGH_COLOR = new Color(256f, 256f, 256f, 0.2f);
        private static readonly Color BAR_NORM_COLOR = new Color(0f, 256f, 256f, 0.75f);
        private const float BAR_WIDTH = 5f;
        private const float BAR_SPACE = 2f;

        private const string SELECT_FMT = "Tier: {0:0}\tProgress: {1:0}";

        // PROPERTIES: ----------------------------------------------------------------------------

        private Vector2 scroll = Vector2.zero;
        private int selectIndex = 0;
        private string selectText = "";

        // PAINT METHODS: -------------------------------------------------------------------------

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect rectThreshold = new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight
            );

            Rect rectMaxResult = new Rect(
                rectThreshold.x,
                rectThreshold.y + rectThreshold.height + EditorGUIUtility.standardVerticalSpacing,
                rectThreshold.width,
                EditorGUIUtility.singleLineHeight
            );

            SerializedProperty spMaxResult = property.FindPropertyRelative(PROP_MAXRESULT);
            SerializedProperty spThreshold = property.FindPropertyRelative(PROP_THRESHOLD);

            float prevMaxResult = spMaxResult.floatValue;
            float prevThreshold = spThreshold.floatValue;

            EditorGUI.PropertyField(rectThreshold, spThreshold);
            EditorGUI.PropertyField(rectMaxResult, spMaxResult);

            Rect rectPlot = new Rect(
                position.x,
                rectMaxResult.y + rectMaxResult.height + EditorGUIUtility.standardVerticalSpacing,
                position.width,
                PLOT_HEIGHT + EditorGUIUtility.singleLineHeight
            );

            this.PaintPlot(
                rectPlot,
                Mathf.Floor(spMaxResult.floatValue),
                spThreshold.floatValue
            );
        }

        private void PaintPlot(Rect rectPlot, float maxResult, float threshold)
        {
            float plotWidth = Mathf.Max(
                (maxResult) * (BAR_WIDTH + BAR_SPACE),
                rectPlot.width
            );

            Rect rectView = new Rect(
                rectPlot.x,
                rectPlot.y,
                plotWidth,
                PLOT_HEIGHT
            );

            this.scroll = GUI.BeginScrollView(rectPlot, this.scroll, rectView);
            EditorGUI.DrawRect(rectView, PLOT_COLOR);

            float maxValue = Table.Progress(maxResult, threshold);

            for (float i = 1f; i <= maxResult; ++i)
            {
                float value = Table.Progress(i, threshold);
                float height = (value / maxValue) * (PLOT_HEIGHT - 10f);

                float x = rectView.x + ((i - 1f) * (BAR_WIDTH + BAR_SPACE));
                Rect rectBar = new Rect(
                    x,
                    rectView.y + (PLOT_HEIGHT - height),
                    BAR_WIDTH,
                    height
                );

                Rect rectHighlight = new Rect(
                    x,
                    rectView.y,
                    BAR_WIDTH,
                    PLOT_HEIGHT
                );

                if (this.selectIndex == (int)i)
                {
                    EditorGUI.DrawRect(rectHighlight, BAR_HIGH_COLOR);
                    this.selectText = string.Format(SELECT_FMT, i, value);
                }

                EditorGUI.DrawRect(rectBar, BAR_NORM_COLOR);
            }

            if (UnityEngine.Event.current.type == EventType.MouseUp &&
                rectView.Contains(UnityEngine.Event.current.mousePosition))
            {
                this.selectIndex = Mathf.FloorToInt(
                    (UnityEngine.Event.current.mousePosition.x - rectPlot.x) / 
                    (BAR_WIDTH + BAR_SPACE)
                ) + 1;

                UnityEngine.Event.current.Use();
            }

            GUI.EndScrollView();

            if (!string.IsNullOrEmpty(selectText))
            {
                EditorGUI.LabelField(rectPlot, this.selectText, EditorStyles.whiteLargeLabel);
            }
        }

        // HEIGHT: --------------------------------------------------------------------------------

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + 
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + 
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +
                PLOT_HEIGHT
            );
        }
    }
}