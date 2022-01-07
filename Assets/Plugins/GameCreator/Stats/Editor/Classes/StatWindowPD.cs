namespace GameCreator.Stats
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using GameCreator.Core;

    public class StatWindowPD : PopupWindowContent
    {
        private const float WIN_HEIGHT = 300f;

        private Rect windowRect = Rect.zero;
        private Vector2 scroll = Vector2.zero;
        private int statIndex = -1;

        private GUIStyle suggestionHeaderStyle;
        private GUIStyle statStyle;

        private SerializedProperty property;
        private StatAsset[] statsCollection;

        private bool keyPressedAny = false;
        private bool keyPressedUp = false;
        private bool keyPressedDown = false;
        private bool keyPressedEnter = false;
        private bool keyFlagVerticalMoved = false;
        private Rect statSelectedRect = Rect.zero;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public StatWindowPD(Rect activatorRect, SerializedProperty property)
        {
            this.windowRect = new Rect(
                activatorRect.x,
                activatorRect.y + activatorRect.height,
                activatorRect.width,
                WIN_HEIGHT
            );

            this.scroll = Vector2.zero;
            this.property = property;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(this.windowRect.width, WIN_HEIGHT);
        }

        public override void OnOpen()
        {
            this.suggestionHeaderStyle = new GUIStyle(GUI.skin.FindStyle("IN BigTitle"));
            this.suggestionHeaderStyle.margin = new RectOffset(0, 0, 0, 0);
            this.statStyle = new GUIStyle(GUI.skin.FindStyle("MenuItem"));

            this.statsCollection = DatabaseStats.LoadDatabase<DatabaseStats>().statsAsset.stats;
        }

        // GUI METHODS: ---------------------------------------------------------------------------

        public override void OnGUI(Rect windowRect)
        {
            if (this.property == null) { this.editorWindow.Close(); return; }
            this.property.serializedObject.Update();

            this.HandleKeyboardInput();

            this.PaintStats();

            this.property.serializedObject.ApplyModifiedProperties();

            if (this.keyPressedEnter)
            {
                this.editorWindow.Close();
                UnityEngine.Event.current.Use();
            }

            bool repaintEvent = false;
            repaintEvent = repaintEvent || UnityEngine.Event.current.type == EventType.MouseMove;
            repaintEvent = repaintEvent || UnityEngine.Event.current.type == EventType.MouseDown;
            repaintEvent = repaintEvent || this.keyPressedAny;
            if (repaintEvent) this.editorWindow.Repaint();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void HandleKeyboardInput()
        {
            this.keyPressedAny = false;
            this.keyPressedUp = false;
            this.keyPressedDown = false;
            this.keyPressedEnter = false;

            if (UnityEngine.Event.current.type != EventType.KeyDown) return;

            this.keyPressedAny = true;
            this.keyPressedUp = (UnityEngine.Event.current.keyCode == KeyCode.UpArrow);
            this.keyPressedDown = (UnityEngine.Event.current.keyCode == KeyCode.DownArrow);

            this.keyPressedEnter = (
                UnityEngine.Event.current.keyCode == KeyCode.KeypadEnter ||
                UnityEngine.Event.current.keyCode == KeyCode.Return
            );

            this.keyFlagVerticalMoved = (
                this.keyPressedUp ||
                this.keyPressedDown
            );
        }

        private void PaintStats()
        {
            EditorGUILayout.BeginHorizontal(this.suggestionHeaderStyle);
            EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            this.scroll = EditorGUILayout.BeginScrollView(this.scroll);
            int statsCount = this.statsCollection.Length;

            if (statsCount > 0)
            {
                for (int i = 0; i < statsCount; ++i)
                {
                    if (this.statsCollection == null) continue;

                    string statName = this.statsCollection[i].stat.uniqueName;
                    if (string.IsNullOrEmpty(statName)) statName = "No Name";
                    GUIContent statContent = new GUIContent(statName);

                    Rect statRect = GUILayoutUtility.GetRect(statContent, this.statStyle);
                    bool statHasFocus = (i == this.statIndex);
                    bool mouseEnter = statHasFocus && UnityEngine.Event.current.type == EventType.MouseDown;

                    if (UnityEngine.Event.current.type == EventType.Repaint)
                    {
                        this.statStyle.Draw(
                            statRect,
                            statContent,
                            statHasFocus,
                            statHasFocus,
                            false,
                            false
                        );
                    }

                    if (this.statIndex == i) this.statSelectedRect = statRect;

                    if (statHasFocus)
                    {
                        if (mouseEnter || this.keyPressedEnter)
                        {
                            if (this.keyPressedEnter) UnityEngine.Event.current.Use();
                            this.property.objectReferenceValue = this.statsCollection[i];
                            this.property.serializedObject.ApplyModifiedProperties();
                            this.property.serializedObject.Update();

                            this.editorWindow.Close();
                        }
                    }

                    if (UnityEngine.Event.current.type == EventType.MouseMove &&
                        GUILayoutUtility.GetLastRect().Contains(UnityEngine.Event.current.mousePosition))
                    {
                        this.statIndex = i;
                    }
                }

                if (this.keyPressedDown && this.statIndex < statsCount - 1)
                {
                    this.statIndex++;
                    UnityEngine.Event.current.Use();
                }
                else if (this.keyPressedUp && this.statIndex > 0)
                {
                    this.statIndex--;
                    UnityEngine.Event.current.Use();
                }
            }

            EditorGUILayout.EndScrollView();
            float scrollHeight = GUILayoutUtility.GetLastRect().height;

            if (UnityEngine.Event.current.type == EventType.Repaint && this.keyFlagVerticalMoved)
            {
                this.keyFlagVerticalMoved = false;
                if (this.statSelectedRect != Rect.zero)
                {
                    bool isUpperLimit = this.scroll.y > this.statSelectedRect.y;
                    bool isLowerLimit = (this.scroll.y + scrollHeight <
                        this.statSelectedRect.position.y + this.statSelectedRect.size.y
                    );

                    if (isUpperLimit)
                    {
                        this.scroll = Vector2.up * (this.statSelectedRect.position.y);
                        this.editorWindow.Repaint();
                    }
                    else if (isLowerLimit)
                    {
                        float positionY = this.statSelectedRect.y + this.statSelectedRect.height - scrollHeight;
                        this.scroll = Vector2.up * positionY;
                        this.editorWindow.Repaint();
                    }
                }
            }
        }
    }
}