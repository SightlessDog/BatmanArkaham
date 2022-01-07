namespace GameCreator.Stats
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;

	#if UNITY_EDITOR
	using UnityEditor;
	#endif

	[AddComponentMenu("")]
	public class ActionDebugFormula : IAction
	{
        public FormulaAsset formula;

        [Space] public float initialValue = 1.0f;
        [Space] public TargetGameObject origin = new TargetGameObject(TargetGameObject.Target.Invoker);
        [Space] public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            if (this.formula != null)
            {
                float result = this.formula.formula.Calculate(
                    this.initialValue,
                    this.origin.GetComponent<Stats>(target),
                    this.target.GetComponent<Stats>(target)
                );

                Debug.LogFormat(
                    "Formula {0}: {1}",
                    this.formula.name,
                    result
                );
            }

            return true;
        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

        #if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Stats/Icons/Actions/";

        public static new string NAME = "Stats/Debug Formula";
        private const string NODE_TITLE = "Debug Formula {0}";

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle()
		{
            return string.Format(
                NODE_TITLE, 
                (this.formula == null ? "(none)" : this.formula.name)
            );
		}

		#endif
	}
}
