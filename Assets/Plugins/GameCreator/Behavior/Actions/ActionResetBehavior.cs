namespace GameCreator.Behavior
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
    using GameCreator.Core;

	[AddComponentMenu("")]
	public class ActionResetBehavior : IAction
	{
        public TargetGameObject behavior = new TargetGameObject(TargetGameObject.Target.GameObject);

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            Behavior _behavior = this.behavior.GetComponent<Behavior>(target);
            if (_behavior != null) _behavior.ResetState();

            return true;
        }

		#if UNITY_EDITOR
        public static new string NAME = "Behavior/Reset Behavior States";
        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Behavior/Icons/Actions/";
        private const string NODE_TITLE = "Force reset Behavior {0} states";

        public override string GetNodeTitle()
        {
            return string.Format(
                NODE_TITLE,
                this.behavior
            );
        }
        #endif
    }
}
