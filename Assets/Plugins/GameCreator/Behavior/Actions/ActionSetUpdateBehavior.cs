namespace GameCreator.Behavior
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
    using GameCreator.Core;

	[AddComponentMenu("")]
	public class ActionSetUpdateBehavior : IAction
	{
        public TargetGameObject behavior = new TargetGameObject(TargetGameObject.Target.GameObject);
        public Behavior.UpdateTime update = Behavior.UpdateTime.SetFrequency;
        [Range(1, 30)] public int frequency = 1;

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            Behavior _behavior = this.behavior.GetComponent<Behavior>(target);
            if (_behavior != null)
            {
                switch (this.update)
                {
                    case Behavior.UpdateTime.EveryFrame:
                        _behavior.update = Behavior.UpdateTime.EveryFrame;
                        break;

                    case Behavior.UpdateTime.SetFrequency:
                        _behavior.update = Behavior.UpdateTime.SetFrequency;
                        _behavior.frequency = this.frequency;
                        break;
                }
            }

            return true;
        }

		#if UNITY_EDITOR
        public static new string NAME = "Behavior/Set Behavior Frequency";
        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Behavior/Icons/Actions/";
        private const string NODE_TITLE = "Set Behavior update frequency to {0}";

        public override string GetNodeTitle()
        {
            return string.Format(
                NODE_TITLE,
                this.update
            );
        }
        #endif
    }
}
