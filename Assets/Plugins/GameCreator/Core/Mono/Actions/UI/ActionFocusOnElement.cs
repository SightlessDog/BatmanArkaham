namespace GameCreator.Core
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using UnityEngine.UI;
	using UnityEngine.Events;
	using UnityEngine.EventSystems;

#if UNITY_EDITOR
	using UnityEditor;
	#endif

	[AddComponentMenu("")]
	public class ActionFocusOnElement : IAction
	{
		public TargetGameObject uiTarget = new TargetGameObject();

		// EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
	        GameObject go = this.uiTarget.GetGameObject(target);
	        if (go != null) EventSystem.current.SetSelectedGameObject(go);
	        return true;
        }

        // +--------------------------------------------------------------------------------------+
		// | EDITOR                                                                               |
		// +--------------------------------------------------------------------------------------+

		#if UNITY_EDITOR

	    public static new string NAME = "UI/Focus on Element";
		private const string NODE_TITLE = "Focus on {0}";

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle()
		{
            return string.Format(NODE_TITLE, this.uiTarget);
		}

		#endif
	}
}
