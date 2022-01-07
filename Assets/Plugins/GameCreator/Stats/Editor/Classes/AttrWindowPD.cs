namespace GameCreator.Stats
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using GameCreator.Core;

    public class AttrWindowPD : PopupWindowContent
    {
        private const float WIN_HEIGHT = 300f;

        private Rect windowRect = Rect.zero;
        private Vector2 scroll = Vector2.zero;
        private int attrIndex = -1;

        private GUIStyle suggestionHeaderStyle;
        private GUIStyle attrStyle;

        private SerializedProperty property;
        private AttrAsset[] attrsCollection;

        private bool keyPressedAny = false;
        private bool keyPressedUp = false;
        private bool keyPressedDown = false;
        private bool keyPressedEnter = false;
        private bool keyFlagVerticalMoved = false;
        private Rect attrSelectedRect = Rect.zero;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public AttrWindowPD(Rect activatorRect, SerializedProperty property)
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
            this.attrStyle = new GUIStyle(GUI.skin.FindStyle("MenuItem"));

            this.attrsCollection = DatabaseStats.LoadDatabase<DatabaseStats>().attrsAsset.attributes;
        }

        // GUI METHODS: ---------------------------------------------------------------------------

        public override void OnGUI(Rect windowRect)
        {
            if (this.property == null) { this.editorWindow.Close(); return; }
            this.property.serializedObject.Update();

            this.HandleKeyboardInput();

            this.PaintAttrs();

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

        private void PaintAttrs()
        {
            EditorGUILayout.BeginHorizontal(this.suggestionHeaderStyle);
            EditorGUILayout.LabelField("Attributes", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            this.scroll = EditorGUILayout.BeginScrollView(this.scroll);
            int attrsCount = this.attrsCollection.Length;

            if (attrsCount > 0)
            {
                for (int i = 0; i < attrsCount; ++i)
                {
                    if (this.attrsCollection == null) continue;

                    string attrName = this.attrsCollection[i].attribute.uniqueName;
                    if (string.IsNullOrEmpty(attrName)) attrName = "No Name";
                    GUIContent attrContent = new GUIContent(attrName);

                    Rect attrRect = GUILayoutUtility.GetRect(attrContent, this.attrStyle);
                    bool attrHasFocus = (i == this.attrIndex);
                    bool mouseEnter = attrHasFocus && UnityEngine.Event.current.type == EventType.MouseDown;

                    if (UnityEngine.Event.current.type == EventType.Repaint)
                    {
                        this.attrStyle.Draw(
                            attrRect,
                            attrContent,
                            attrHasFocus,
                            attrHasFocus,
                            false,
                            false
                        );
                    }

                    if (this.attrIndex == i) this.attrSelectedRect = attrRect;

                    if (attrHasFocus)
                    {
                        if (mouseEnter || this.keyPressedEnter)
                        {
                            if (this.keyPressedEnter) UnityEngine.Event.current.Use();
                            this.property.objectReferenceValue = this.attrsCollection[i];
                            this.property.serializedObject.ApplyModifiedProperties();
                            this.property.serializedObject.Update();

                            this.editorWindow.Close();
                        }
                    }

                    if (UnityEngine.Event.current.type == EventType.MouseMove &&
                        GUILayoutUtility.GetLastRect().Contains(UnityEngine.Event.current.mousePosition))
                    {
                        this.attrIndex = i;
                    }
                }

                if (this.keyPressedDown && this.attrIndex < attrsCount - 1)
                {
                    this.attrIndex++;
                    UnityEngine.Event.current.Use();
                }
                else if (this.keyPressedUp && this.attrIndex > 0)
                {
                    this.attrIndex--;
                    UnityEngine.Event.current.Use();
                }
            }

            EditorGUILayout.EndScrollView();
            float scrollHeight = GUILayoutUtility.GetLastRect().height;

            if (UnityEngine.Event.current.type == EventType.Repaint && this.keyFlagVerticalMoved)
            {
                this.keyFlagVerticalMoved = false;
                if (this.attrSelectedRect != Rect.zero)
                {
                    bool isUpperLimit = this.scroll.y > this.attrSelectedRect.y;
                    bool isLowerLimit = (this.scroll.y + scrollHeight <
                        this.attrSelectedRect.position.y + this.attrSelectedRect.size.y
                    );

                    if (isUpperLimit)
                    {
                        this.scroll = Vector2.up * (this.attrSelectedRect.position.y);
                        this.editorWindow.Repaint();
                    }
                    else if (isLowerLimit)
                    {
                        float positionY = this.attrSelectedRect.y + this.attrSelectedRect.height - scrollHeight;
                        this.scroll = Vector2.up * positionY;
                        this.editorWindow.Repaint();
                    }
                }
            }
        }
    }
}