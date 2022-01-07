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
	public class ConditionCanSee : ICondition
	{
        public enum Condition
        {
            CanSee,
            CanNotSee
        }

        public TargetGameObject observer = new TargetGameObject(TargetGameObject.Target.Invoker);
        public Condition condition = Condition.CanSee;
        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);

		// EXECUTABLE: ----------------------------------------------------------------------------

		public override bool Check(GameObject target)
		{
            GameObject goObserver = this.observer.GetGameObject(target);
            GameObject goTarget = this.target.GetGameObject(target);

            if (goObserver == null) return false;
            if (goTarget == null) return false;

            Perception senses = goObserver.GetComponentInChildren<Perception>();
            if (senses == null) return false;

            PerceptronSight sight = senses.GetPerceptron(Perception.Type.Sight) as PerceptronSight;
            if (sight == null) return false;

            switch (this.condition)
            {
                case Condition.CanSee: return sight.CanSee(goTarget);
                case Condition.CanNotSee: return !sight.CanSee(goTarget);
                default: return false;
            }
        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

        #if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Behavior/Icons/Conditions/";

        public static new string NAME = "Senses/Can See";
		private const string NODE_TITLE = "Can {0} {1} {2}";

		// PROPERTIES: ----------------------------------------------------------------------------

		private SerializedProperty spObserver;
        private SerializedProperty spCondition;
        private SerializedProperty spTarget;

        // INSPECTOR METHODS: ---------------------------------------------------------------------

        public override string GetNodeTitle()
		{
			return string.Format(
                NODE_TITLE, 
                this.observer, 
                this.condition == Condition.CanSee ? "see" : "not see",
                this.target
            );
		}

		protected override void OnEnableEditorChild ()
		{
			this.spObserver = this.serializedObject.FindProperty("observer");
            this.spCondition = this.serializedObject.FindProperty("condition");
            this.spTarget = this.serializedObject.FindProperty("target");
        }

		protected override void OnDisableEditorChild ()
		{
			this.spObserver = null;
            this.spCondition = null;
            this.spTarget = null;
        }

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

			EditorGUILayout.PropertyField(this.spObserver);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spCondition);
            EditorGUILayout.PropertyField(this.spTarget);

            this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}
