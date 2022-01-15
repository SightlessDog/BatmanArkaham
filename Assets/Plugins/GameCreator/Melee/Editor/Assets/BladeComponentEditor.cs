namespace GameCreator.Melee
{
    using UnityEditor;

    [CustomEditor(typeof(BladeComponent))]
    public class BladeComponentEditor : Editor
    {
        // PRIVATE PROPERTIES: --------------------------------------------------------------------

        private SerializedProperty spCaptureMode;
        private SerializedProperty spLayerMask;
        private SerializedProperty spSegmentResolution;
        private SerializedProperty spPointA;
        private SerializedProperty spPointB;
        private SerializedProperty spRadius;
        private SerializedProperty spOffset;
        private SerializedProperty spBoxInterframePredictions;
        private SerializedProperty spBoxCenter;
        private SerializedProperty spBoxSize;
        private SerializedProperty spEnableDebug;

        private SerializedProperty spEnableTrail;
        private SerializedProperty spTrailMaterial;
        private SerializedProperty spTrailGranularity;
        private SerializedProperty spTrailDuration;

        private SerializedProperty spEventAttackStart;
        private SerializedProperty spEventAttackEnd;
        private SerializedProperty spEventAttackActivation;
        private SerializedProperty spEventAttackRecovery;

        // INITIALIZER: ---------------------------------------------------------------------------

        private void OnEnable()
        {
            this.spCaptureMode = this.serializedObject.FindProperty("captureHits");
            this.spLayerMask = this.serializedObject.FindProperty("layerMask");

            this.spSegmentResolution = this.serializedObject.FindProperty("segmentResolution");
            this.spPointA = this.serializedObject.FindProperty("pointA");
            this.spPointB = this.serializedObject.FindProperty("pointB");
            this.spRadius = this.serializedObject.FindProperty("radius");
            this.spOffset = this.serializedObject.FindProperty("offset");
            this.spBoxInterframePredictions =
                this.serializedObject.FindProperty("boxInterframePredictions");
            this.spBoxCenter = this.serializedObject.FindProperty("boxCenter");
            this.spBoxSize = this.serializedObject.FindProperty("boxSize");
            this.spEnableDebug = this.serializedObject.FindProperty("debugMode");

            this.spEnableTrail = this.serializedObject.FindProperty("enableWeaponTrail");
            this.spTrailMaterial = this.serializedObject.FindProperty("trailMaterial");
            this.spTrailGranularity = this.serializedObject.FindProperty("trailGranularity");
            this.spTrailDuration = this.serializedObject.FindProperty("trailDuration");

            this.spEventAttackStart = this.serializedObject.FindProperty("EventAttackStart");
            this.spEventAttackEnd = this.serializedObject.FindProperty("EventAttackEnd");
            this.spEventAttackActivation = this.serializedObject.FindProperty("EventAttackActivation");
            this.spEventAttackRecovery = this.serializedObject.FindProperty("EventAttackRecovery");
        }

        // PAINT METHODS: -------------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spEnableDebug);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Blade Edge", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(this.spPointA);
            EditorGUILayout.PropertyField(this.spPointB);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Capture Hits", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(this.spCaptureMode);
            EditorGUILayout.PropertyField(this.spLayerMask);

            EditorGUI.indentLevel++;

            switch (this.spCaptureMode.enumValueIndex)
            {
                case (int)BladeComponent.CaptureHitModes.Segment:
                    EditorGUILayout.PropertyField(this.spSegmentResolution);
                    break;

                case (int)BladeComponent.CaptureHitModes.Sphere:
                    EditorGUILayout.PropertyField(this.spRadius);
                    EditorGUILayout.PropertyField(this.spOffset);
                    break;

                case (int)BladeComponent.CaptureHitModes.Box:
                    EditorGUILayout.PropertyField(this.spBoxInterframePredictions);
                    EditorGUILayout.PropertyField(this.spBoxCenter);
                    EditorGUILayout.PropertyField(this.spBoxSize);
                    break;
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Blade Trail", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(this.spEnableTrail);

            EditorGUI.BeginDisabledGroup(!this.spEnableTrail.boolValue);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(this.spTrailMaterial);
            EditorGUILayout.PropertyField(this.spTrailGranularity);
            EditorGUILayout.PropertyField(this.spTrailDuration);

            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(this.spEventAttackStart);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spEventAttackEnd);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spEventAttackActivation);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spEventAttackRecovery);


            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
 