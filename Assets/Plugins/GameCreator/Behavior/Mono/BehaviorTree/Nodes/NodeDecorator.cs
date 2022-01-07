namespace GameCreator.Behavior
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using GameCreator.Core;

	[Serializable]
	public class NodeDecorator : Node
	{
        // PROPERTIES: ----------------------------------------------------------------------------

        public NodeIDecorator decorator = null;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        protected override Return UpdateNode(GameObject invoker, Behavior behavior)
        {
            if (this.decorator == null) return Return.Fail;
            return this.decorator.Execute(this, invoker, behavior);
        }

        public override void AbortNode(GameObject invoker, Behavior behavior)
        {
            int instanceID = this.GetInstanceID();
            if (behavior.GetState(instanceID) == Return.None) return;

            if (this.decorator != null)
            {
                this.decorator.Abort(this, invoker, behavior);
            }

            behavior.SetState(instanceID, Return.None);
            for (int i = 0; i < this.outputs.Count; ++i)
            {
                this.outputs[i].AbortNode(invoker, behavior);
            }
        }
    }
}