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
	public class ActionCameraChangeVariable : IAction 
	{
		private const string TOOLTIP_TRANS_TIME = "0: No transition. Values between 0.5 and 1.5 are recommended";
		
		// PROPERTIES: ----------------------------------------------------------------------------

        [VariableFilter(Variable.DataType.GameObject)]
        public VariableProperty variable = new VariableProperty(Variable.VarType.GlobalVariable);

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
                    GameObject value = this.variable.Get(target) as GameObject;
                    if (value != null) motor = value.GetComponent<CameraMotor>();

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

		public static new string NAME = "Camera/Change Camera from Variable";
		private const string NODE_TITLE = "Change to camera {0} ({1})";

		// PROPERTIES: ----------------------------------------------------------------------------

        // INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle()
		{
            string cameraName = "variable";

            return string.Format(
				NODE_TITLE, 
				cameraName,
				(Mathf.Approximately(this.transitionTime, 0f) 
					? "instant" 
					: string.Format("{0:0.00}s", this.transitionTime)
				)
			);
		}

		#endif
	}
}