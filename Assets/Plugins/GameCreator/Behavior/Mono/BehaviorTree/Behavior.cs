namespace GameCreator.Behavior
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using GameCreator.Variables;
    using GameCreator.Core;

    [AddComponentMenu("Game Creator/Behavior")]
    public class Behavior : LocalVariables
	{
        public class NodeState
        {
            public Node.Return state = Node.Return.None;
            public Actions actions = null;
            public int[] randomIndexes = new int[0];
            public int passCount = 0;
        }

        public enum OnComplete
        {
            RepeatForever,
            RepeatWhileSuccess,
            RepeatWhileFail,
            Stop,
        }

        public enum UpdateTime
        {
            EveryFrame,
            SetFrequency
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public BehaviorGraph behaviorGraph = null;

        public UpdateTime update = UpdateTime.EveryFrame;

        [Range(1, 30)] public int frequency = 1;
        private float updateTime = -100f;

        public OnComplete onComplete = OnComplete.RepeatForever;

        private bool isExecuting = false;
        private bool resetStates = false;

        private Dictionary<int, NodeState> states = new Dictionary<int, NodeState>();

        // INITIALIZERS: --------------------------------------------------------------------------

        protected override void Start()
        {
            base.Start();

            #if UNITY_EDITOR
            if (!Application.isPlaying) return;
            #endif

            this.StartExecuting();
        }

        private void Update()
        {
            switch (this.update)
            {
                case UpdateTime.EveryFrame:
                    if (this.isExecuting) this.Execute();
                    break;

                case UpdateTime.SetFrequency:
                    bool time = Time.unscaledTime > this.updateTime + (1f/this.frequency);
                    if (this.isExecuting && time) this.Execute();
                    break;
            }
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Stop()
        {
            this.isExecuting = false;
        }

        public void ResetState()
        {
            this.resetStates = true;
        }

        public void StartExecuting()
        {
            if (this.behaviorGraph == null)
            {
                this.isExecuting = false;
                return;
            }

            if (this.isExecuting) return;

            this.isExecuting = true;
            this.resetStates = true;
        }

        public void Execute()
        {
            if (this.resetStates)
            {
                this.states = new Dictionary<int, NodeState>();
                this.resetStates = false;
            }

            this.updateTime = Time.unscaledTime;
            Node.Return result = this.behaviorGraph.Execute(gameObject, this);

            switch (result)
            {
                case Node.Return.Success:
                    this.Complete();
                    if (this.onComplete == OnComplete.RepeatForever || 
                        this.onComplete == OnComplete.RepeatWhileSuccess)
                    {
                        this.StartExecuting();
                    }
                    break;

                case Node.Return.Fail:
                    this.Complete();
                    if (this.onComplete == OnComplete.RepeatForever ||
                        this.onComplete == OnComplete.RepeatWhileFail)
                    {
                        this.StartExecuting();
                    }

                    break;
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void Complete()
        {
            this.isExecuting = false;
        }

        // PUBLIC STATE METHODS: ------------------------------------------------------------------

        public void SetState(int instanceID, Node.Return state)
        {
            this.RequireState(instanceID);
            this.states[instanceID].state = state;
        }

        public Node.Return GetState(int instanceID)
        {
            this.RequireState(instanceID);
            return this.states[instanceID].state;
        }

        public void SetActions(int instanceID, Actions actions)
        {
            this.RequireState(instanceID);
            this.states[instanceID].actions = actions;
        }

        public Actions GetActions(int instanceID)
        {
            this.RequireState(instanceID);
            return this.states[instanceID].actions;
        }

        public void StopActions(int instanceID)
        {
            Actions actions = this.GetActions(instanceID);
            if (actions == null) return;

            actions.Stop();
            Destroy(actions.gameObject);
        }

        public int GetPassCount(int instanceID)
        {
            this.RequireState(instanceID);
            return this.states[instanceID].passCount;
        }

        public void SetPassCount(int instanceID, int count)
        {
            this.RequireState(instanceID);
            this.states[instanceID].passCount = count;
        }

        public int[] GetRandomIndexes(int instanceID, int length)
        {
            this.RequireState(instanceID);
            if (this.states[instanceID].randomIndexes.Length != length)
            {
                this.RegenerateRandomIndexes(instanceID, length);
            }

            return this.states[instanceID].randomIndexes;
        }

        public void RegenerateRandomIndexes(int instanceID, int length)
        {
            this.states[instanceID].randomIndexes = this.GetRandomIndexArray(length);
        }

        // PRIVATE STATE METHODS: -----------------------------------------------------------------

        private void RequireState(int instanceID)
        {
            if (!this.states.ContainsKey(instanceID))
            {
                this.states.Add(instanceID, new NodeState());
            }
        }

        private int[] GetRandomIndexArray(int length)
        {
            int[] array = new int[length];
            for (int i = 0; i < array.Length; ++i) array[i] = i;

            while (length > 1)
            {
                int random = Random.Range(0, length);
                length--;

                int temp = array[length];
                array[length] = array[random];
                array[random] = temp;
            }

            return array;
        }
    }
}