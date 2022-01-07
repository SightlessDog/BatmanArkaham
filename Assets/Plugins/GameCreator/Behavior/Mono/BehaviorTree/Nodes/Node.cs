namespace GameCreator.Behavior
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using GameCreator.Core;

    #if UNITY_EDITOR
    using UnityEditor;
    #endif

    [Serializable]
	public abstract class Node : ScriptableObject
	{
		public enum Return
		{
            None,
			Fail,
			Success,
			Running
		}

        public enum ConditionMode
        {
            FailImmidiatelly,
            AllowToComplete,
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public BehaviorGraph graph = null;

        public bool useConditionsList = false;
        public IConditionsList prefabConditionsList = null;

        public bool useActionsList = false;
        public IActionsList prefabActionsList = null;

		public Node input = null;
		public List<Node> outputs = new List<Node>();

        public ConditionMode mode = ConditionMode.FailImmidiatelly;

        #if UNITY_EDITOR
        public string editorName = "";
		public Vector2 position = Vector2.zero;
        #endif

        private int[] orderForward = new int[0];

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public Return Execute(GameObject invoker, Behavior behavior)
        {
            Return state = this.UpdateNode(invoker, behavior);
            behavior.SetState(this.GetInstanceID(), state);

            return state;
        }

        // VIRTUAL METHODS: -----------------------------------------------------------------------

        protected abstract Return UpdateNode(GameObject invoker, Behavior behavior);

        public abstract void AbortNode(GameObject invoker, Behavior behavior);

        public virtual void OnDestroy()
        {
            #if UNITY_EDITOR

            if (this.prefabConditionsList != null)
            {
                string path = AssetDatabase.GetAssetPath(this.prefabConditionsList.gameObject);
                AssetDatabase.DeleteAsset(path);
            }

            if (this.prefabActionsList != null)
            {
                string path = AssetDatabase.GetAssetPath(this.prefabActionsList.gameObject);
                AssetDatabase.DeleteAsset(path);
            }

            #endif
        }

        public virtual void ResetOutputStates(GameObject invoker, Behavior behavior, bool caller)
        {
            behavior.SetState(this.GetInstanceID(), Return.None);
            if (!caller) behavior.SetPassCount(this.GetInstanceID(), 0);

            for (int i = 0; i < this.outputs.Count; ++i)
            {
                this.outputs[i].ResetOutputStates(invoker, behavior, false);
            }
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected bool CheckConditions(GameObject invoker, Behavior behavior)
        {
            if (!this.useConditionsList) return true;
            int instanceID = this.GetInstanceID();

            if (this.mode == ConditionMode.AllowToComplete && 
                behavior.GetState(instanceID) == Return.Running)
            {
                return true;
            }

            return this.prefabConditionsList.Check(invoker);
        }

        protected int[] GetOrderForward()
        {
            if (this.orderForward.Length != this.outputs.Count)
            {
                this.orderForward = new int[this.outputs.Count];
                for (int i = 0; i < this.orderForward.Length; ++i)
                {
                    this.orderForward[i] = i;
                }
            }

            return this.orderForward;
        }
    }
}