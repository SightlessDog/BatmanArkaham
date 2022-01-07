namespace GameCreator.Behavior
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using GameCreator.Core;
    using GameCreator.Variables;

    [AddComponentMenu("")]
    public class ActionPerceptionDistance : IAction
    {
        public TargetGameObject perception = new TargetGameObject(TargetGameObject.Target.GameObject);
        public NumberProperty distance = new NumberProperty(5f);

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            Perception _perception = this.perception.GetComponent<Perception>(target);
            if (_perception != null) _perception.visionDistance = this.distance.GetValue(target);

            return true;
        }

        #if UNITY_EDITOR
        public static new string NAME = "Perception/Distance";
        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Behavior/Icons/Actions/";
        private const string NODE_TITLE = "Perception Distance {0}";

        public override string GetNodeTitle()
        {
            return string.Format(
                NODE_TITLE,
                this.distance
            );
        }
        #endif
    }
}
