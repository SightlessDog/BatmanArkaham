namespace GameCreator.Behavior
{
    using System;
    using UnityEngine;
    using UnityEngine.Events;

    [Serializable]
    public class Tracker
    {
        [Serializable] public class EventSense : UnityEvent<bool, GameObject> { }

        // PROPERTIES: ----------------------------------------------------------------------------

        public GameObject reference;
        private CharacterController characterController;

        private EventSense eventSense = new EventSense();
        private bool state;

        private Vector3 memorizedPosition = Vector3.zero;
        private Quaternion memorizedRotation = Quaternion.identity;

        // INITIALIZER: ---------------------------------------------------------------------------

        public Tracker(GameObject reference)
        {
            this.reference = reference;
            this.characterController = this.reference.GetComponent<CharacterController>();

            this.UpdateMemory();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void UpdateMemory()
        {
            if (this.reference == null) return;

            this.memorizedPosition = CalculatePosition(this);
            this.memorizedRotation = CalculateRotation(this);
        }

        public void AddEvent(UnityAction<bool, GameObject> callback)
        {
            this.eventSense.AddListener(callback);
        }

        public void RemoveEvent(UnityAction<bool, GameObject> callback)
        {
            this.eventSense.RemoveListener(callback);
        }

        // GETTERS: -------------------------------------------------------------------------------

        public Vector3 GetPosition()
        {
            return this.memorizedPosition;
        }

        public Quaternion GetRotation()
        {
            return this.memorizedRotation;
        }

        public bool GetState()
        {
            return this.state;
        }

        // SETTERS: -------------------------------------------------------------------------------

        public void SetState(bool state)
        {
            if (this.state != state && this.eventSense != null)
            {
                this.eventSense.Invoke(state, this.reference);
            }

            this.state = state;
        }

        // HELPER METHODS: ------------------------------------------------------------------------

        public static Vector3 CalculatePosition(Tracker tracker)
        {
            Vector3 position = tracker.reference.transform.position;
            if (tracker.characterController != null)
            {
                position += tracker.characterController.center;
            }

            return position;
        }

        public static Quaternion CalculateRotation(Tracker tracker)
        {
            Quaternion rotation = tracker.reference.transform.rotation;
            return rotation;
        }
    }
}