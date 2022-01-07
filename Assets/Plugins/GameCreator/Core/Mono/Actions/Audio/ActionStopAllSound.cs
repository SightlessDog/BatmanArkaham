namespace GameCreator.Core
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;

	#if UNITY_EDITOR
	using UnityEditor;
	#endif

	[AddComponentMenu("")]
	public class ActionStopAllSound : IAction
	{
        [Range(0f, 5f)]
        public float fadeOut;

		// EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            AudioManager.Instance.StopAllSounds(fadeOut);
            return true;
        }

		// +--------------------------------------------------------------------------------------+
		// | EDITOR                                                                               |
		// +--------------------------------------------------------------------------------------+

		#if UNITY_EDITOR

		public static new string NAME = "Audio/Stop All Sound";
		private const string NODE_TITLE = "Stop All Sounds";

        public override string GetNodeTitle()
		{
			return NODE_TITLE;
		}

		#endif
	}
}
