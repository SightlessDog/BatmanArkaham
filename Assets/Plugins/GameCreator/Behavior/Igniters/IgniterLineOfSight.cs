namespace GameCreator.Behavior
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Variables;

    [AddComponentMenu("")]
    public class IgniterLineOfSight : Igniter 
	{
        public enum Operation
        {
            OnSight,
            OnLoseSight,
        }

        #if UNITY_EDITOR
        public new static string ICON_PATH = "Assets/Plugins/GameCreator/Behavior/Icons/Igniters/";
        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Behavior/Icons/Igniters/";

        public new static string NAME = "Behavior/Line of Sight";
        #endif

        public TargetGameObject observer = new TargetGameObject(TargetGameObject.Target.Invoker);

        [Space]
        public Operation when = Operation.OnSight;
        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.GameObject);

        private void Start()
        {
            this.StartCoroutine(this.DelayedStart());
        }

        private IEnumerator DelayedStart()
        {
            yield return null;

            GameObject goObserver = this.observer.GetGameObject(gameObject);
            GameObject goTarget = this.target.GetGameObject(gameObject);

            if (goObserver != null && goTarget != null)
            {
                Perception sensesObserver = goObserver.GetComponent<Perception>();
                if (sensesObserver == null) yield break;

                sensesObserver.StartListenPerceptron(Perception.Type.Sight, goTarget, this.Callback);
            }
        }

        private void Callback(bool inSight, GameObject perceptionTarget)
        {
            GameObject goObserver = this.observer.GetGameObject(gameObject);
            if (goObserver == null) return;

            switch (this.when)
            {
                case Operation.OnSight: if (inSight) this.ExecuteTrigger(goObserver); break;
                case Operation.OnLoseSight: if (!inSight) this.ExecuteTrigger(goObserver); break;
            }
        }

        private void OnDestroy()
        {
            GameObject goObserver = this.observer.GetGameObject(gameObject);
            GameObject goTarget = this.target.GetGameObject(gameObject);

            if (goObserver != null && goTarget != null)
            {
                Perception sensesObserver = goObserver.GetComponent<Perception>();
                if (sensesObserver == null) return;

                sensesObserver.StopListenPerceptron(Perception.Type.Sight, goTarget, this.Callback);
            }
        }
    }
}