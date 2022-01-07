namespace GameCreator.Behavior
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
	public class ActionSightTracking : IAction
	{
        public enum Operation
        {
            StartTracking,
            StopTracking
        }

        public TargetGameObject observer = new TargetGameObject(TargetGameObject.Target.Invoker);

        public Operation operation = Operation.StartTracking;
        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            GameObject goObserver = this.observer.GetGameObject(target);
            GameObject goTarget = this.target.GetGameObject(target);

            if (goObserver == null) return false;
            if (goTarget == null) return false;

            Perception perception = goObserver.GetComponentInChildren<Perception>();
            if (perception == null) return false;

            switch (this.operation)
            {
                case Operation.StartTracking:
                    perception.StartListenPerceptron(Perception.Type.Sight, goTarget, null);
                    break;

                case Operation.StopTracking:
                    perception.StopListenPerceptron(Perception.Type.Sight, goTarget, null);
                    break;
            }

            return true;
        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

        #if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Behavior/Icons/Actions/";

        public static new string NAME = "Perception/Sight Tracking";
		private const string NODE_TITLE = "{0} {1} {2}";

		// PROPERTIES: ----------------------------------------------------------------------------

		private SerializedProperty spObserver;
        private SerializedProperty spOperation;
        private SerializedProperty spTarget;

        // INSPECTOR METHODS: ---------------------------------------------------------------------

        public override string GetNodeTitle()
		{
			return string.Format(NODE_TITLE, this.observer, this.operation, this.target);
		}

		protected override void OnEnableEditorChild ()
		{
			this.spObserver = this.serializedObject.FindProperty("observer");
            this.spOperation = this.serializedObject.FindProperty("operation");
            this.spTarget = this.serializedObject.FindProperty("target");
        }

		protected override void OnDisableEditorChild ()
		{
			this.spObserver = null;
            this.spOperation = null;
            this.spTarget = null;
        }

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

			EditorGUILayout.PropertyField(this.spObserver);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spOperation);
            EditorGUILayout.PropertyField(this.spTarget);

            this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}
