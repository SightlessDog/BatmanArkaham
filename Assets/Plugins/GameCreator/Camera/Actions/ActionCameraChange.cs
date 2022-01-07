using UnityEngine.Serialization;

namespace GameCreator.Camera
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;
	using GameCreator.Core.Hooks;
    using GameCreator.Variables;

#if UNITY_EDITOR
    using UnityEditor;
	#endif

	[AddComponentMenu("")]
	public class ActionCameraChange : IAction 
	{
		private const string TOOLTIP_TRANS_TIME = "0: No transition. Values between 0.5 and 1.5 are recommended";

        public enum CameraMotorFrom
        {
            CameraMotor,
            Variable
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool mainCameraMotor = false;
        public CameraMotor cameraMotor;

        [Tooltip(TOOLTIP_TRANS_TIME)] 
		[Range(0.0f, 60.0f)] 
		public float transitionTime = 0.0f;

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            if (HookCamera.Instance != null)
            {
                CameraController cameraController = HookCamera.Instance.Get<CameraController>();
                if (cameraController != null)
                {
                    CameraMotor motor = null;
                    if (this.mainCameraMotor) motor = CameraMotor.MAIN_MOTOR;
                    else motor = this.cameraMotor; 

                    if (motor != null)
                    {
                        cameraController.ChangeCameraMotor(
                            motor, 
                            this.transitionTime
                        );
                    }
                }
            }

            return true;
        }

		// +--------------------------------------------------------------------------------------+
		// | EDITOR                                                                               |
		// +--------------------------------------------------------------------------------------+

		#if UNITY_EDITOR

		public static new string NAME = "Camera/Change Camera";
		private const string NODE_TITLE = "Change to camera {0} ({1})";

		// PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spMainCameraMotor;
        private SerializedProperty spCameraMotor;
        private SerializedProperty spTransitionTime;

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle()
		{
            string cameraName = "";
            if (this.mainCameraMotor) cameraName = "Main Camera Motor";
            else cameraName = this.cameraMotor == null ? "none" : this.cameraMotor.gameObject.name;

            return string.Format(
				NODE_TITLE, 
				cameraName,
				(Mathf.Approximately(this.transitionTime, 0f) 
					? "instant" 
					: string.Format("{0:0.00}s", this.transitionTime)
				)
			);
		}
		
		protected override void OnEnableEditorChild ()
		{
            this.spMainCameraMotor = this.serializedObject.FindProperty("mainCameraMotor");
            this.spCameraMotor = this.serializedObject.FindProperty("cameraMotor");
            this.spTransitionTime = this.serializedObject.FindProperty("transitionTime");
		}
  
		protected override void OnDisableEditorChild ()
		{
            this.spMainCameraMotor = null;
            this.spCameraMotor = null;
            this.spTransitionTime = null;
        }
  
		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();
  
            EditorGUILayout.PropertyField(this.spMainCameraMotor);
  
            EditorGUI.BeginDisabledGroup(this.spMainCameraMotor.boolValue);
            EditorGUILayout.PropertyField(this.spCameraMotor);
            EditorGUI.EndDisabledGroup();
  
			EditorGUILayout.PropertyField(this.spTransitionTime);
  
			this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}