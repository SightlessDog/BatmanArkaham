namespace GameCreator.Behavior
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using GameCreator.Core;

	[Serializable]
	public class NodeRoot : Node
	{
        protected override Return UpdateNode(GameObject invoker, Behavior behavior)
        {
            if (this.outputs.Count != 1 || this.outputs[0] == null)
            {
                return Return.None;
            }

            Return result = this.outputs[0].Execute(invoker, behavior);
            return result;
        }

        public override void AbortNode(GameObject invoker, Behavior behavior)
        {
            int instanceID = this.GetInstanceID();
            if (behavior.GetState(instanceID) == Return.None) return;

            behavior.SetState(instanceID, Return.None);

            for (int i = 0; i < this.outputs.Count; ++i)
            {
                this.outputs[i].AbortNode(invoker, behavior);
            }
        }
    }
}