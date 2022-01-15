namespace GameCreator.Camera
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using GameCreator.Core;
    using GameCreator.Variables;

#if UNITY_EDITOR
	using UnityEditor;
#endif

    [AddComponentMenu("")]
	public class ActionOrbitCamera : IAction
	{
        public bool mainCameraMotor = false;
        public CameraMotor cameraMotor;

        public bool allowOrbitInput = true;
        public bool allowZoom = true;
        public bool avoidWallClip = true;

        public bool changeTargetOffset = false;
        public bool changePivotOffset = false;
		public bool changeInitialPitch = false;
        public float duration = 0.2f;

        public TargetPosition targetOffset = new TargetPosition();
        public TargetPosition pivotOffset = new TargetPosition();
        public NumberProperty newPitch = new NumberProperty(25.0f);
        private Vector3 vTargetOffset;
        private Vector3 vPivotOffset;
        private float vNewPitch;
              

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            CameraMotor motor = (this.mainCameraMotor ? CameraMotor.MAIN_MOTOR : this.cameraMotor);
            if (motor != null && motor.cameraMotorType.GetType() == typeof(CameraMotorTypeOrbit))
            {
                CameraMotorTypeOrbit orbitMotor = (CameraMotorTypeOrbit)motor.cameraMotorType;
                orbitMotor.allowOrbitInput = this.allowOrbitInput;
                orbitMotor.allowZoom = this.allowZoom;
                orbitMotor.avoidWallClip = this.avoidWallClip;
            }

            return !this.changeTargetOffset && !this.changePivotOffset && !this.changeInitialPitch;
        }

        public override IEnumerator Execute(GameObject target, IAction[] actions, int index)
        {
            CameraMotor motor = (this.mainCameraMotor ? CameraMotor.MAIN_MOTOR : this.cameraMotor);
            if (motor != null && motor.cameraMotorType.GetType() == typeof(CameraMotorTypeOrbit))
            {
                float initTime = Time.time;
                CameraMotorTypeOrbit orbitMotor = (CameraMotorTypeOrbit)motor.cameraMotorType;

                vTargetOffset = this.targetOffset.GetPosition(target);
                Vector3 aTargetOffset = orbitMotor.targetOffset;
				Vector3 bTargetOffset = this.changeTargetOffset ? this.vTargetOffset : aTargetOffset;

                vPivotOffset = this.pivotOffset.GetPosition(target);
                Vector3 aPivotOffset = orbitMotor.pivotOffset;
                Vector3 bPivotOffset = this.changePivotOffset ? this.vPivotOffset : aPivotOffset;

                vNewPitch = this.newPitch.GetValue(target);
                float aNewPitch = orbitMotor.initialPitch;
                float bNewPitch = this.changeInitialPitch ? this.vNewPitch : aNewPitch;

                while (initTime + this.duration >= Time.time)
                {
                    float t = Mathf.Clamp01((Time.time - initTime) / this.duration);
                    orbitMotor.targetOffset = Vector3.Lerp(aTargetOffset, bTargetOffset, t);
                    orbitMotor.pivotOffset = Vector3.Lerp(aPivotOffset, bPivotOffset, t);

                    yield return null;
                }

                orbitMotor.targetOffset = bTargetOffset;
                orbitMotor.pivotOffset = bPivotOffset;
				orbitMotor.initialPitch = bNewPitch;
				
            }

            yield return 0;
        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

#if UNITY_EDITOR
		public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreatorHub/Icons/Actions/";

        public static new string NAME = "Camera/Orbit Camera Settings";
        private const string NODE_TITLE = "Change {0} Orbit Camera settings";

		// PROPERTIES: ----------------------------------------------------------------------------

		private SerializedProperty spMainCameraMotor;
        private SerializedProperty spCameraMotor;

        private SerializedProperty spAllowOrbitInput;
        private SerializedProperty spAllowZoom;
        private SerializedProperty spAvoidWallClip;

        private SerializedProperty spChangeTargetOffset;
        private SerializedProperty spChangePivotOffset;
        private SerializedProperty spChangePitch;
        private SerializedProperty spDuration;
        private SerializedProperty spTargetOffset;
        private SerializedProperty spPivotOffset;
        private SerializedProperty spNewPitch;

        // INSPECTOR METHODS: ---------------------------------------------------------------------

        public override string GetNodeTitle()
		{
            string motor = (this.mainCameraMotor
                ? "[Main Camera]"
                : (this.cameraMotor == null ? "none" : this.cameraMotor.gameObject.name)
            );
            
			return string.Format(NODE_TITLE, motor);
		}

		protected override void OnEnableEditorChild ()
		{
            this.spMainCameraMotor = this.serializedObject.FindProperty("mainCameraMotor");
            this.spCameraMotor = this.serializedObject.FindProperty("cameraMotor");
            this.spAllowOrbitInput = this.serializedObject.FindProperty("allowOrbitInput");
            this.spAllowZoom = this.serializedObject.FindProperty("allowZoom");
            this.spAvoidWallClip = this.serializedObject.FindProperty("avoidWallClip");

            this.spChangeTargetOffset = this.serializedObject.FindProperty("changeTargetOffset");
            this.spChangePivotOffset = this.serializedObject.FindProperty("changePivotOffset");
			this.spChangePitch = this.serializedObject.FindProperty("changeInitialPitch");
            this.spDuration = this.serializedObject.FindProperty("duration");
            this.spTargetOffset = this.serializedObject.FindProperty("targetOffset");
            this.spPivotOffset = this.serializedObject.FindProperty("pivotOffset");
			this.spNewPitch = this.serializedObject.FindProperty("newPitch");
        }

		protected override void OnDisableEditorChild ()
		{
            this.spMainCameraMotor = null;
            this.spCameraMotor = null;
            this.spAllowOrbitInput = null;
            this.spAllowZoom = null;
            this.spAvoidWallClip = null;
            this.spChangeTargetOffset = null;
            this.spChangePivotOffset = null;
			this.spChangePitch = null;
            this.spDuration = null;
            this.spTargetOffset = null;
            this.spPivotOffset = null;
			this.spNewPitch = null;
        }

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spMainCameraMotor);
            EditorGUI.BeginDisabledGroup(this.spMainCameraMotor.boolValue);
            EditorGUILayout.PropertyField(this.spCameraMotor);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.spAllowOrbitInput);
            EditorGUILayout.PropertyField(this.spAllowZoom);
            EditorGUILayout.PropertyField(this.spAvoidWallClip);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spDuration);
            EditorGUILayout.PropertyField(this.spChangeTargetOffset);
            if (this.spChangeTargetOffset.boolValue)
            {
                EditorGUILayout.PropertyField(this.spTargetOffset);
            }

            EditorGUILayout.PropertyField(this.spChangePivotOffset);
            if (this.spChangePivotOffset.boolValue)
            {
                EditorGUILayout.PropertyField(this.spPivotOffset);
            }
			
			EditorGUILayout.PropertyField(this.spChangePitch);
			if (this.spChangePitch.boolValue)
            {
                EditorGUILayout.PropertyField(this.spNewPitch);
            }

            this.serializedObject.ApplyModifiedProperties();
		}

#endif
    }
}
