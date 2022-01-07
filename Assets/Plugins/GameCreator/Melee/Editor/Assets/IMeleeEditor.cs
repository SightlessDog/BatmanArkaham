namespace GameCreator.Melee
{
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEditor;
    using UnityEditor.AnimatedValues;
    using GameCreator.Core;

    public abstract class IMeleeEditor : Editor
    {
        private const float ANIM_BOOL_SPEED = 3f;
        private const string TEXTURE = "Assets/Plugins/GameCreator/Melee/Extra/Toolbar/{0}.png";

        protected class Section
        {
            private const string KEY_STATE = "melee-section-{0}";

            public GUIContent name;
            public AnimBool state;

            public Section(string name, Texture2D icon, UnityAction repaint)
            {
                this.name = new GUIContent(string.Format(" {0}", name), icon);
                this.state = new AnimBool(this.GetState());
                this.state.speed = ANIM_BOOL_SPEED;
                this.state.valueChanged.AddListener(repaint);
            }

            public void PaintSection()
            {
                GUIStyle buttonStyle = (this.state.target
                    ? CoreGUIStyles.GetToggleButtonNormalOn()
                    : CoreGUIStyles.GetToggleButtonNormalOff()
                );

                if (GUILayout.Button(this.name, buttonStyle))
                {
                    this.state.target = !this.state.target;
                    string key = string.Format(KEY_STATE, this.name.text.GetHashCode());
                    EditorPrefs.SetBool(key, this.state.target);
                }
            }

            private bool GetState()
            {
                string key = string.Format(KEY_STATE, this.name.text.GetHashCode());
                return EditorPrefs.GetBool(key, true);
            }
        }

        protected Texture2D LoadIcon(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(
                TEXTURE, name
            ));
        }
    }
}