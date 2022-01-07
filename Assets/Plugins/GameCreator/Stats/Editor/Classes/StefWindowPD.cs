namespace GameCreator.Stats
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using GameCreator.Core;

    public class StefWindowPD : PopupWindowContent
    {
        private const float WIN_HEIGHT = 300f;

        private Rect windowRect = Rect.zero;
        private Vector2 scroll = Vector2.zero;
        private int stefIndex = -1;

        private GUIStyle suggestionHeaderStyle;
        private GUIStyle stefStyle;

        private SerializedProperty property;
        private StatusEffectAsset[] stefsCollection;

        private bool keyPressedAny = false;
        private bool keyPressedUp = false;
        private bool keyPressedDown = false;
        private bool keyPressedEnter = false;
        private bool keyFlagVerticalMoved = false;
        private Rect stefSelectedRect = Rect.zero;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public StefWindowPD(Rect activatorRect, SerializedProperty property)
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
            this.stefStyle = new GUIStyle(GUI.skin.FindStyle("MenuItem"));

            this.stefsCollection = DatabaseStats.LoadDatabase<DatabaseStats>().statusEffectsAsset.statusEffects;
        }

        // GUI METHODS: ---------------------------------------------------------------------------

        public override void OnGUI(Rect windowRect)
        {
            if (this.property == null) { this.editorWindow.Close(); return; }
            this.property.serializedObject.Update();

            this.HandleKeyboardInput();

            this.PaintStefs();

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

        private void PaintStefs()
        {
            EditorGUILayout.BeginHorizontal(this.suggestionHeaderStyle);
            EditorGUILayout.LabelField("StatusEffects", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            this.scroll = EditorGUILayout.BeginScrollView(this.scroll);
            int stefsCount = this.stefsCollection.Length;

            if (stefsCount > 0)
            {
                for (int i = 0; i < stefsCount; ++i)
                {
                    if (this.stefsCollection == null) continue;

                    string stefName = this.stefsCollection[i].statusEffect.uniqueName;
                    if (string.IsNullOrEmpty(stefName)) stefName = "No Name";
                    GUIContent stefContent = new GUIContent(stefName);

                    Rect stefRect = GUILayoutUtility.GetRect(stefContent, this.stefStyle);
                    bool stefHasFocus = (i == this.stefIndex);
                    bool mouseEnter = stefHasFocus && UnityEngine.Event.current.type == EventType.MouseDown;

                    if (UnityEngine.Event.current.type == EventType.Repaint)
                    {
                        this.stefStyle.Draw(
                            stefRect,
                            stefContent,
                            stefHasFocus,
                            stefHasFocus,
                            false,
                            false
                        );
                    }

                    if (this.stefIndex == i) this.stefSelectedRect = stefRect;

                    if (stefHasFocus)
                    {
                        if (mouseEnter || this.keyPressedEnter)
                        {
                            if (this.keyPressedEnter) UnityEngine.Event.current.Use();
                            this.property.objectReferenceValue = this.stefsCollection[i];
                            this.property.serializedObject.ApplyModifiedProperties();
                            this.property.serializedObject.Update();

                            this.editorWindow.Close();
                        }
                    }

                    if (UnityEngine.Event.current.type == EventType.MouseMove &&
                        GUILayoutUtility.GetLastRect().Contains(UnityEngine.Event.current.mousePosition))
                    {
                        this.stefIndex = i;
                    }
                }

                if (this.keyPressedDown && this.stefIndex < stefsCount - 1)
                {
                    this.stefIndex++;
                    UnityEngine.Event.current.Use();
                }
                else if (this.keyPressedUp && this.stefIndex > 0)
                {
                    this.stefIndex--;
                    UnityEngine.Event.current.Use();
                }
            }

            EditorGUILayout.EndScrollView();
            float scrollHeight = GUILayoutUtility.GetLastRect().height;

            if (UnityEngine.Event.current.type == EventType.Repaint && this.keyFlagVerticalMoved)
            {
                this.keyFlagVerticalMoved = false;
                if (this.stefSelectedRect != Rect.zero)
                {
                    bool isUpperLimit = this.scroll.y > this.stefSelectedRect.y;
                    bool isLowerLimit = (this.scroll.y + scrollHeight <
                        this.stefSelectedRect.position.y + this.stefSelectedRect.size.y
                    );

                    if (isUpperLimit)
                    {
                        this.scroll = Vector2.up * (this.stefSelectedRect.position.y);
                        this.editorWindow.Repaint();
                    }
                    else if (isLowerLimit)
                    {
                        float positionY = this.stefSelectedRect.y + this.stefSelectedRect.height - scrollHeight;
                        this.scroll = Vector2.up * positionY;
                        this.editorWindow.Repaint();
                    }
                }
            }
        }
    }
}