namespace GameCreator.Stats
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.AnimatedValues;
    using GameCreator.Core;

    [CustomEditor(typeof(Stats))]
    public class StatsEditor : Editor
    {
        public class StatOverride
        {
            public SerializedProperty property;

            public StatOverride(SerializedProperty property)
            {
                this.property = property;
            }
        }

        public class EditorData
        {
            public AnimBool isExpanded;

            public EditorData(StatsEditor statsEditor)
            {
                this.isExpanded = new AnimBool(statsEditor.Repaint);
                this.isExpanded.speed = 3.0f;
            }
        }

        // CONSTANT PROPERTIES: -------------------------------------------------------------------

        private static Texture2D ATTR_BAR_BACK_BRD;
        private static Texture2D ATTR_BAR_BACK_BCK;
        private static Texture2D ATTR_BAR_PROGRESS;

        private const string PR_FMT_ED = "<b>{0}</b>: {1:0}%";
        private const string PR_FMT_RN = "<b>{0}</b>: {1}/{2}";
        private const string STEF_FMT = " <b>{0}</b> ({1})";

        private static readonly GUIContent GC_VALUE = new GUIContent("Base Value (override)");
        private static readonly GUIContent GC_FORMULA = new GUIContent("Formula (override)");
        private const string GC_DEFAULT = "{0}";
        private static GUIStyle STYLE_LABEL = null;

        // PROPERTIES: ----------------------------------------------------------------------------

        private Stats instance;
        private SerializedProperty spStatsOverridesList;
        private SerializedProperty spSaveStats;

        private StatsAsset statsAsset;
        private AttrsAsset attrsAsset;

        private int statsAssetHash = 0;
        private int attrsAssetHash = 0;

        private List<EditorData> statsEditorData;

        private Dictionary<string, StatOverride> spStatsOverrides;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void OnEnable()
        {
            if (target == null || serializedObject == null) return;
            this.instance = (Stats)this.target;

            this.spStatsOverridesList = serializedObject.FindProperty("statsOverrides");
            this.spSaveStats = serializedObject.FindProperty("saveStats");
            this.spStatsOverrides = new Dictionary<string, StatOverride>();

            int overrideCount = this.spStatsOverridesList.arraySize;
            for (int i = 0; i < overrideCount; ++i)
            {
                SerializedProperty spElement = this.spStatsOverridesList.GetArrayElementAtIndex(i);
                string statUniqueID = spElement.FindPropertyRelative("statUniqueID").stringValue;
                if (string.IsNullOrEmpty(statUniqueID)) continue;

                this.spStatsOverrides.Add(
                    statUniqueID,
                    new StatOverride(spElement)
                );
            }

            this.statsAsset = DatabaseStatsEditor.GetStatsAsset();
            this.attrsAsset = DatabaseStatsEditor.GetAttrsAsset();

            this.statsAssetHash = this.statsAsset.GetHashCode();
            this.attrsAssetHash = this.attrsAsset.GetHashCode();
            this.statsEditorData = new List<EditorData>();

            bool statsListAdded = false;
            int statsSize = this.statsAsset.stats.Length;
            for (int i = 0; i < statsSize; ++i)
            {
                this.statsEditorData.Add(new EditorData(this));
                string statUniqueID = this.statsAsset.stats[i].uniqueID;

                if (!this.spStatsOverrides.ContainsKey(statUniqueID))
                {
                    int insertIndex = this.spStatsOverridesList.arraySize;
                    this.spStatsOverridesList.InsertArrayElementAtIndex(insertIndex);

                    SerializedProperty spItem = this.spStatsOverridesList.GetArrayElementAtIndex(insertIndex);

                    spItem.FindPropertyRelative("statUniqueID").stringValue = statUniqueID;
                    spItem.FindPropertyRelative("overrideValue").boolValue = false;
                    spItem.FindPropertyRelative("overrideFormula").boolValue = false;
                    spItem.FindPropertyRelative("baseValue").floatValue = 0.0f;
                    spItem.FindPropertyRelative("formula").objectReferenceValue = null;

                    this.spStatsOverrides.Add(statUniqueID, new StatOverride(spItem));
                    statsListAdded = true;
                }
            }

            if (statsListAdded)
            {
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }
        }

        public override bool RequiresConstantRepaint()
        {
            return EditorApplication.isPlaying;
        }

        // PAINT METHODS: -------------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            if (target == null || serializedObject == null) return;
            if (STYLE_LABEL == null)
            {
                STYLE_LABEL = new GUIStyle(EditorStyles.label);
                STYLE_LABEL.richText = true;
            }

            if (this.statsAssetHash != this.statsAsset.GetHashCode() ||
                this.attrsAssetHash != this.attrsAsset.GetHashCode() ||
                this.statsEditorData.Count != this.statsAsset.stats.Length)
            {
                this.OnEnable();
                serializedObject.ApplyModifiedProperties();
            }

            serializedObject.Update();

            this.PaintAttributes();

            int statsSize = this.statsAsset.stats.Length;
            for (int i = 0; i < statsSize; ++i)
            {
                StatAsset statAsset = this.statsAsset.stats[i];
                if (statAsset.isHidden) continue;

                string uname = statAsset.uniqueID;

                this.PaintStatHeader(statAsset, i);

                using (var group = new EditorGUILayout.FadeGroupScope(this.statsEditorData[i].isExpanded.faded))
                {
                    if (group.visible)
                    {
                        EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());
                        EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
                        this.PaintStatBody(statAsset, this.spStatsOverrides[uname]);
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.EndVertical();
                    }
                }
            }

            this.PaintStatusEffects();

            EditorGUILayout.Space();

            this.spSaveStats.boolValue = EditorGUILayout.ToggleLeft(
                this.spSaveStats.displayName,
                this.spSaveStats.boolValue
            );

            GlobalEditorID.Paint(this.instance);
            serializedObject.ApplyModifiedProperties();
        }

        // ATTRIBUTES PAINTER: --------------------------------------------------------------------

        private void PaintAttributes()
        {
            if (this.attrsAsset.attributes.Length == 0) return;
            EditorGUILayout.Space();

            for (int i = 0; i < this.attrsAsset.attributes.Length; ++i)
            {
                AttrAsset attrAsset = this.attrsAsset.attributes[i];
                this.PaintAttribute(attrAsset);

                GUILayout.Space(5f);
            }
        }

        private void PaintAttribute(AttrAsset attrAsset)
        {
            if (ATTR_BAR_BACK_BCK == null || ATTR_BAR_BACK_BRD == null)
            {
                ATTR_BAR_BACK_BCK = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                ATTR_BAR_BACK_BCK.SetPixel(0, 0, new Color(0,0,0, 0.1f));
                ATTR_BAR_BACK_BCK.Apply();

                ATTR_BAR_BACK_BRD = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                ATTR_BAR_BACK_BRD.SetPixel(0, 0, new Color(0, 0, 0, 0.65f));
                ATTR_BAR_BACK_BRD.Apply();
            }

            if (ATTR_BAR_PROGRESS == null)
            {
                ATTR_BAR_PROGRESS = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                ATTR_BAR_PROGRESS.SetPixel(0, 0, Color.white);
                ATTR_BAR_PROGRESS.Apply();
            }

            Rect rect = GUILayoutUtility.GetRect(
                EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth,
                EditorGUIUtility.singleLineHeight
            );

            Rect rectLabel = new Rect(
                rect.x,
                rect.y,
                EditorGUIUtility.labelWidth,
                rect.height
            );

            Rect rectBg = new Rect(
                rect.x + EditorGUIUtility.labelWidth,
                rect.y,
                rect.width - EditorGUIUtility.labelWidth,
                rect.height
            );

            Rect rectPr = rectBg;

            GUIContent gcLabel = GUIContent.none;
            string attrName = attrAsset.attribute.shortName;

            switch (EditorApplication.isPlaying)
            {
                case false : 
                    rectPr = new Rect(
                        rectBg.x + 1f,
                        rect.y + 1f,
                        (rectBg.width - 2f) * attrAsset.attribute.percent,
                        rect.height - 2f
                    );

                    gcLabel = new GUIContent(string.Format(
                        PR_FMT_ED, 
                        attrName, 
                        attrAsset.attribute.percent * 100f
                    )); 
                    break;

                case true :
                    float curValue = instance.GetAttrValue(attrAsset.attribute.uniqueName);
                    float maxValue = instance.GetAttrMaxValue(attrAsset.attribute.uniqueName);

                    rectPr = new Rect(
                        rectBg.x + 1f,
                        rect.y + 1f,
                        (rectBg.width - 2f) * (curValue/maxValue),
                        rect.height - 2f
                    );

                    gcLabel = new GUIContent(string.Format(
                        PR_FMT_RN,
                        attrName,
                        curValue,
                        maxValue
                    )); 
                    break;
            }

            EditorGUI.LabelField(rectLabel, gcLabel, STYLE_LABEL);

            Color progrColor = attrAsset.attribute.color;
            GUI.DrawTexture(rectBg, ATTR_BAR_BACK_BCK, ScaleMode.StretchToFill, true, 0, Color.white, 0f, 0f);
            GUI.DrawTexture(rectBg, ATTR_BAR_BACK_BRD, ScaleMode.StretchToFill, true, 0, Color.white, 1f, 0f);
            GUI.DrawTexture(rectPr, ATTR_BAR_PROGRESS, ScaleMode.StretchToFill, true, 0, progrColor,  0f, 0f);
        }

        // STATUS EFFECTS PAINTER: ----------------------------------------------------------------

        private void PaintStatusEffects()
        {
            if (!EditorApplication.isPlaying) return;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Status Effects:", EditorStyles.boldLabel);

            foreach (KeyValuePair<string, Stats.RuntimeStefData> item in this.instance.runtimeStefsData)
            {
                Stats.RuntimeStefData data = item.Value;
                int count = data.listStatus.Count;
                if (count > 0)
                {
                    GUIContent content = new GUIContent(
                        string.Format(STEF_FMT, data.statusEffect.statusEffect.uniqueName, count),
                        (data.statusEffect.statusEffect.icon == null 
                            ? null 
                            : data.statusEffect.statusEffect.icon.texture
                        )
                    );

                    EditorGUILayout.LabelField(content, STYLE_LABEL);
                }
            }

            EditorGUILayout.EndVertical();
        }

        // STATS PAINTER: -------------------------------------------------------------------------

        private void PaintStatHeader(StatAsset statAsset, int index)
        {
            EditorData statEditorData = this.statsEditorData[index];

            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, CoreGUIStyles.GetToggleButtonOff());
            Rect rectColor = new Rect(rect.x, rect.y, 5f, rect.height);
            Rect rectButton = new Rect(rect.x + rectColor.width, rect.y, rect.width - rectColor.width, rect.height);

            Color tmpColor = GUI.backgroundColor;
            GUI.backgroundColor = statAsset.stat.color;
            GUI.Button(rectColor, GUIContent.none, CoreGUIStyles.GetToggleButtonLeftOff());
            GUI.backgroundColor = tmpColor;

            string title = statAsset.stat.uniqueName;
            if (Application.isPlaying)
            {
                title = string.Format(
                    "<b>{0}</b>: {1}",
                    title,
                    this.instance.GetStat(title)
                );
            }

            GUIStyle style = (statEditorData.isExpanded.target
                ? CoreGUIStyles.GetToggleButtonRightOn()
                : CoreGUIStyles.GetToggleButtonRightOff()
            );
            
            if (GUI.Button(rectButton, title, style))
            {
                statEditorData.isExpanded.target = !statEditorData.isExpanded.target;
            }
        }

        private void PaintStatBody(StatAsset statAsset, StatOverride statOverride)
        {
            this.PaintOverrideStat(
                statOverride.property.FindPropertyRelative("overrideValue"), 
                statOverride.property.FindPropertyRelative("baseValue"),
                GC_VALUE,
                statAsset.stat.baseValue.ToString()
            );

            this.PaintOverrideStat(
                statOverride.property.FindPropertyRelative("overrideFormula"),
                statOverride.property.FindPropertyRelative("formula"),
                GC_FORMULA,
                (statAsset.stat.formula == null ? "(none)" : statAsset.stat.formula.name)
            );
        }

        private void PaintOverrideStat(SerializedProperty spEnable, SerializedProperty spValue, 
            GUIContent content, string defaultValue)
        {
            Rect rect = GUILayoutUtility.GetRect(content, EditorStyles.label);
            Rect rectEnable = new Rect(
                rect.x,
                rect.y,
                EditorGUIUtility.labelWidth + 20f,
                rect.height
            );
            Rect rectValue = new Rect(
                rectEnable.x + rectEnable.width,
                rect.y,
                rect.width - rectEnable.width,
                rect.height
            );

            EditorGUI.PropertyField(rectEnable, spEnable, content);

            if (spEnable.boolValue)
            {
                EditorGUI.PropertyField(rectValue, spValue, GUIContent.none);
            }
            else
            {
                GUIContent @default = new GUIContent(string.Format(
                    GC_DEFAULT,
                    defaultValue
                ));

                EditorGUI.LabelField(rectValue, @default, STYLE_LABEL);
            }
        }
    }
}