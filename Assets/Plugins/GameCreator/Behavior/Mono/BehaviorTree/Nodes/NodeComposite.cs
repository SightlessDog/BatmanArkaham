namespace GameCreator.Behavior
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using GameCreator.Core;

	[Serializable]
	public class NodeComposite : Node
	{
		public enum Composite
        {
            Selector,
            Sequence,
            RandomSelector,
            RandomSequence,
            Parallel
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public Composite composite = Composite.Selector;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        protected override Return UpdateNode(GameObject invoker, Behavior behavior)
        {
            Return result = Return.Fail;
            if (this.CheckConditions(invoker, behavior))
            {
                switch (this.composite)
                {
                    case Composite.Selector:
                        result = this.RunSelector(invoker, behavior);
                        break;

                    case Composite.Sequence:
                        result = this.RunSequence(invoker, behavior);
                        break;

                    case Composite.RandomSelector:
                        result = this.RunRandomSelector(invoker, behavior);
                        break;

                    case Composite.RandomSequence:
                        result = this.RunRandomSequence(invoker, behavior);
                        break;

                    case Composite.Parallel:
                        result = this.RunParallel(invoker, behavior);
                        break;
                }
            } 
            else
            {
                result = Return.Fail;
            }

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

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private Return RunSelector(GameObject invoker, Behavior behavior)
        {
            int[] order = this.GetOrderForward();
            return this.RunSelectorWithOrder(invoker, behavior, order);
        }

        private Return RunSequence(GameObject invoker, Behavior behavior)
        {
            int[] order = this.GetOrderForward();
            return this.RunSequenceWithOrder(invoker, behavior, order);
        }

        private Return RunRandomSelector(GameObject invoker, Behavior behavior)
        {
            int instanceID = this.GetInstanceID();
            int[] order = behavior.GetRandomIndexes(instanceID, this.outputs.Count);

            Return result = this.RunSelectorWithOrder(invoker, behavior, order);
            if (result == Return.Success || result == Return.Fail)
            {
                behavior.RegenerateRandomIndexes(instanceID, this.outputs.Count);
            }

            return result;
        }

        private Return RunRandomSequence(GameObject invoker, Behavior behavior)
        {
            int instanceID = this.GetInstanceID();
            int[] order = behavior.GetRandomIndexes(instanceID, this.outputs.Count);

            Return result = this.RunSequenceWithOrder(invoker, behavior, order);
            if (result == Return.Success || result == Return.Fail)
            {
                behavior.RegenerateRandomIndexes(instanceID, this.outputs.Count);
            }

            return result;
        }

        private Return RunParallel(GameObject invoker, Behavior behavior)
        {
            Return result = Return.Running;

            int numSuccess = 0;
            int numFailing = 0;

            int outputsCount = this.outputs.Count;
            if (outputsCount == 0) return Return.Success;

            for (int i = 0; i < outputsCount; ++i)
            {
                Return outputResult = this.outputs[i].Execute(invoker, behavior);

                if (outputResult == Return.Success) numSuccess++;
                else if (outputResult == Return.Fail) numFailing++;
            }

            if (numSuccess + numFailing >= outputsCount)
            {
                if (numSuccess > 0) result = Return.Success;
                else result = Return.Fail;
            }

            return result;
        }

        private Return RunSelectorWithOrder(GameObject invoker, Behavior behavior, int[] order)
        {
            Return result = Return.Success;
            bool stopNodes = false;

            for (int i = 0; i < this.outputs.Count; ++i)
            {
                if (stopNodes)
                {
                    this.outputs[order[i]].AbortNode(invoker, behavior);
                    continue;
                }

                result = this.outputs[order[i]].Execute(invoker, behavior);

                switch (result)
                {
                    case Return.Running:
                        stopNodes = true;
                        result = Return.Running;
                        break;

                    case Return.Success:
                        stopNodes = true;
                        result = Return.Success;
                        break;
                }
            }

            return result;
        }

        private Return RunSequenceWithOrder(GameObject invoker, Behavior behavior, int[] order)
        {
            Return result = Return.Success;
            bool stopNodes = false;

            for (int i = 0; i < this.outputs.Count; ++i)
            {
                if (stopNodes)
                {
                    this.outputs[order[i]].AbortNode(invoker, behavior);
                    continue;
                }

                result = this.outputs[order[i]].Execute(invoker, behavior);
                if (result == Return.Fail)
                {
                    stopNodes = true;
                    this.outputs[order[i]].AbortNode(invoker, behavior);
                }

                if (result == Return.Running)
                {
                    break;
                }
            }

            return result;
        }
    }
}