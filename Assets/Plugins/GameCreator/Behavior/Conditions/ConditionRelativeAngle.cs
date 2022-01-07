namespace GameCreator.Behavior
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;
    using GameCreator.Variables;

    #if UNITY_EDITOR
    using UnityEditor;
    #endif

    [AddComponentMenu("")]
	public class ConditionRelativeAngle : ICondition
	{
        public TargetGameObject observer = new TargetGameObject(TargetGameObject.Target.Invoker);

        [Space]
        public TargetGameObject otherObject = new TargetGameObject(TargetGameObject.Target.Player);

        [Space]
        public NumberProperty angle = new NumberProperty(90f);

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool Check(GameObject target)
		{
            GameObject goA = this.observer.GetGameObject(target);
            GameObject goB = this.otherObject.GetGameObject(target);

            if (goA == null) return false;
            if (goB == null) return false;

            float maxAngle = this.angle.GetValue(target);
            float curAngle = Vector3.Angle(
                goA.transform.TransformDirection(Vector3.forward),
                goB.transform.position - goA.transform.position
            );

            return curAngle <= maxAngle;
        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

        #if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Behavior/Icons/Conditions/";

        public static new string NAME = "Senses/Relative Angle below";
		private const string NODE_TITLE = "Angle {0} and {1} <= {2}";

        // INSPECTOR METHODS: ---------------------------------------------------------------------

        public override string GetNodeTitle()
		{
			return string.Format(
                NODE_TITLE, 
                this.observer, 
                this.otherObject,
                this.angle
            );
		}

		#endif
	}
}
