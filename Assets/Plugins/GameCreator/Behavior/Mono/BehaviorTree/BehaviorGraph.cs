namespace GameCreator.Behavior
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using GameCreator.Core;

	[CreateAssetMenu(fileName = "My Behavior", menuName = "Game Creator/Behavior/Behavior Tree")]
    public class BehaviorGraph : ScriptableObject
	{
        // PROPERTIES: ----------------------------------------------------------------------------

        public NodeRoot root = null;
		public List<Node> nodes = new List<Node>();
		
		public Blackboard blackboard = new Blackboard();

        #if UNITY_EDITOR
        public Vector2 position = Vector2.zero;
        public float zoom = 1f;
        #endif

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public Node.Return Execute(GameObject invoker, Behavior behavior)
		{
            if (this.root == null) return Node.Return.Success;
            return this.root.Execute(invoker, behavior);
        }

        public void Abort(GameObject invoker, Behavior behavior)
        {
            if (this.root == null) return;
            this.root.AbortNode(invoker, behavior);
        }

        public List<Blackboard.Item> GetBlackboardItems()
        {
            List<Blackboard.Item> items = new List<Blackboard.Item>(this.blackboard.list);
            for (int i = 0; i < this.nodes.Count; ++i)
            {
                NodeBehaviorGraph node = this.nodes[i] as NodeBehaviorGraph;
                if (node != null && node.behaviorGraph != null)
                {
                    items.AddRange(node.behaviorGraph.GetBlackboardItems());
                }
            }

            return items;
        }

        // OTHER METHODS: -------------------------------------------------------------------------

        private void OnDestroy()
        {
            #if UNITY_EDITOR

            for (int i = this.nodes.Count - 1; i >= 0; --i)
            {
                if (this.nodes[i] == null) continue;
                this.nodes[i].OnDestroy();
            }

            #endif
        }
    }
}