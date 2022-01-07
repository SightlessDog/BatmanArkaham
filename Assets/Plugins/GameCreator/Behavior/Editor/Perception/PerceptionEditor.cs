namespace GameCreator.Behavior
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEngine.Events;
    using UnityEditor.AnimatedValues;
    using GameCreator.Core;

    [CustomEditor(typeof(Perception))]
    public class PerceptionEditor : Editor
    {
        public class Section
        {
            private const string KEY_STATE = "character-section-{0}";

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

        // CONSTANTS: -----------------------------------------------------------------------------

        private const float ANIM_BOOL_SPEED = 3f;

        private static readonly Color COLOR_SIGHT = new Color(0, 0, 0, 0.5f);
        private static readonly Color COLOR_SIGHT_ON = new Color(0, 256, 0, 0.95f);
        private static readonly Color COLOR_SIGHT_OFF = new Color(256, 0, 0, 0.5f);

        // PROPERTIES: ----------------------------------------------------------------------------

        private Perception perception;

        private Section sectionSight;

        private SerializedProperty spFieldOfView;
        private SerializedProperty spVisionDistance;
        private SerializedProperty spSightLayerMask;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void OnEnable()
        {
            this.perception = (Perception)this.target;

            this.spFieldOfView = serializedObject.FindProperty("fieldOfView");
            this.spVisionDistance = serializedObject.FindProperty("visionDistance");
            this.spSightLayerMask = serializedObject.FindProperty("sightLayerMask");

            this.sectionSight = new Section("Sight", null, this.Repaint);
        }

        // PAINT METHODS: -------------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.Space();

            this.PaintSight();

            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();
        }

        private void PaintSight()
        {
            this.sectionSight.PaintSection();
            using (var group = new EditorGUILayout.FadeGroupScope(this.sectionSight.state.faded))
            {
                if (group.visible)
                {
                    EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());

                    EditorGUILayout.PropertyField(this.spFieldOfView);
                    EditorGUILayout.PropertyField(this.spVisionDistance);
                    EditorGUILayout.PropertyField(this.spSightLayerMask);

                    EditorGUILayout.EndVertical();
                }
            }
        }

        // SCENE GUI: -----------------------------------------------------------------------------

        private void OnSceneGUI()
        {
            Transform sight = PerceptronSight.GetEyes(this.perception);

            Handles.color = Color.white;
            Handles.DrawWireArc(
                sight.position,
                Vector3.up,
                perception.transform.forward,
                360f,
                this.perception.visionDistance
            );

            Handles.color = COLOR_SIGHT;
            Handles.DrawSolidArc(
                sight.position,
                Vector3.up,
                PerceptronBase.DirectionFromAngle(-this.perception.fieldOfView / 2.0f, this.perception.transform),
                this.perception.fieldOfView,
                this.perception.visionDistance
            );

            if (!Application.isPlaying) return;
            PerceptronBase perceptronSight = this.perception.GetPerceptron(Perception.Type.Sight);

            foreach(KeyValuePair<int, Tracker> item in perceptronSight.trackers)
            {
                Handles.color = item.Value.GetState() ? COLOR_SIGHT_ON : COLOR_SIGHT_OFF;
                Handles.DrawLine(
                    sight.position, 
                    Tracker.CalculatePosition(item.Value)
                );
            }
        }
    }
}