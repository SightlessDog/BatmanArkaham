namespace GameCreator.Behavior
{
    using System;
    using UnityEngine;

    [Serializable]
    public class NodeDecoratorRepeater : NodeIDecorator
    {
        public new static string NAME = "Repeater";

        [Tooltip("Use 0 to repeat an infinite number of times")]
        public int count = 0;

        // VIRTUAL METHODS: -----------------------------------------------------------------------

        public override Node.Return Execute(Node node, GameObject invoker, Behavior behavior)
        {
            int instanceID = node.GetInstanceID();
            int passCount = behavior.GetPassCount(instanceID);

            if (this.count <= 0 || passCount < this.count)
            {
                Node.Return result = base.Execute(node, invoker, behavior);

                if (result == Node.Return.Success || result == Node.Return.Fail)
                {
                    behavior.SetPassCount(instanceID, passCount + 1);
                    node.ResetOutputStates(invoker, behavior, true);
                }

                return Node.Return.Running;
            }

            return Node.Return.Success;
        }

        public override string GetName()
        {
            return NAME;
        }
    }
}