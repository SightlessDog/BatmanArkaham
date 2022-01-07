namespace GameCreator.Behavior
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using GameCreator.Core;

	[Serializable]
	public class NodeBehaviorGraph : Node
	{
        // PROPERTIES: ----------------------------------------------------------------------------

        public BehaviorGraph behaviorGraph = null;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        protected override Return UpdateNode(GameObject invoker, Behavior behavior)
        {
            if (this.behaviorGraph == null) return Return.Fail;

            if (this.CheckConditions(invoker, behavior))
            {
                return this.behaviorGraph.Execute(invoker, behavior);
            }

            // TODO add check here if conditions allow to skip conditions when BT is running, then
            // don't abort:

            if (behavior.GetState(this.GetInstanceID()) == Return.Running)
            {
                this.AbortNode(invoker, behavior);
            }

            return Return.Fail;
        }

        public override void AbortNode(GameObject invoker, Behavior behavior)
        {
            if (this.behaviorGraph == null) return;
            if (behavior.GetState(this.GetInstanceID()) == Return.None) return;

            this.behaviorGraph.Abort(invoker, behavior);
        }
    }
}