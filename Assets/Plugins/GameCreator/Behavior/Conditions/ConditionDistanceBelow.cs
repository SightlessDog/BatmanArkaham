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
	public class ConditionDistanceBelow : ICondition
	{
        public TargetGameObject objectA = new TargetGameObject(TargetGameObject.Target.Invoker);

        [Space]
        public NumberProperty distance = new NumberProperty(5f);

        [Space]
        public TargetGameObject objectB = new TargetGameObject(TargetGameObject.Target.Player);

		// EXECUTABLE: ----------------------------------------------------------------------------

		public override bool Check(GameObject target)
		{
            GameObject goA = this.objectA.GetGameObject(target);
            GameObject goB = this.objectB.GetGameObject(target);

            if (goA == null) return false;
            if (goB == null) return false;

            float dist = this.distance.GetValue(target);
            return Vector3.Distance(goA.transform.position, goB.transform.position) <= dist;
        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

        #if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Behavior/Icons/Conditions/";

        public static new string NAME = "Senses/Distance below";
		private const string NODE_TITLE = "Distance {0} and {1} <= {2}";

        // INSPECTOR METHODS: ---------------------------------------------------------------------

        public override string GetNodeTitle()
		{
			return string.Format(
                NODE_TITLE, 
                this.objectA, 
                this.objectB,
                this.distance
            );
		}

		#endif
	}
}
