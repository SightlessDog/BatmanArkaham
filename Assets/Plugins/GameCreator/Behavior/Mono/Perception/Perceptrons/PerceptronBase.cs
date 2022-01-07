namespace GameCreator.Behavior
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    [Serializable]
    public abstract class PerceptronBase
    {
        public Dictionary<int, Tracker> trackers { private set; get; }

        protected Perception perception;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected PerceptronBase()
        {
            this.trackers = new Dictionary<int, Tracker>();
        }

        public virtual void Awake(Perception perception)
        {
            this.perception = perception;
        }

        // METHODS: -------------------------------------------------------------------------------

        public virtual void Update() 
        { }

        public virtual void FixedUpdate()
        { }

        // EVENT METHODS: -------------------------------------------------------------------------

        public virtual void StartListenPerceptron(GameObject target, UnityAction<bool, GameObject> callback)
        {
            int instanceID = target.GetInstanceID();
            if (!this.trackers.ContainsKey(instanceID))
            {
                this.trackers.Add(instanceID, new Tracker(target));
            }

            Tracker tracker = this.trackers[instanceID];
            if (callback != null) tracker.AddEvent(callback);
        }

        public virtual void StopListenPerceptron(GameObject target, UnityAction<bool, GameObject> callback)
        {
            int instanceID = target.GetInstanceID();

            if (!this.trackers.ContainsKey(instanceID)) return;
            if (callback == null) return;

            this.trackers[instanceID].RemoveEvent(callback);
        }

        // HELPER METHODS: ------------------------------------------------------------------------

        public static Vector3 DirectionFromAngle(float angle, Transform pivot = null)
        {
            if (pivot != null) angle += pivot.eulerAngles.y;
            return new Vector3(
                Mathf.Sin(angle * Mathf.Deg2Rad),
                0f,
                Mathf.Cos(angle * Mathf.Deg2Rad)
            );
        }
    }
}