namespace GameCreator.Behavior
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
    using GameCreator.Core;
	using GameCreator.Variables;

	[AddComponentMenu("")]
	public class ActionPerceptionFoV : IAction
	{
        public TargetGameObject perception = new TargetGameObject(TargetGameObject.Target.GameObject);
        public NumberProperty angle = new NumberProperty(160f);

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            Perception _perception = this.perception.GetComponent<Perception>(target);
            if (_perception != null) _perception.fieldOfView = this.angle.GetValue(target);

            return true;
        }

		#if UNITY_EDITOR
        public static new string NAME = "Perception/Field of View";
        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Behavior/Icons/Actions/";
        private const string NODE_TITLE = "Perception FOV {0}";

        public override string GetNodeTitle()
        {
            return string.Format(
                NODE_TITLE,
                this.angle
            );
        }
        #endif
    }
}
