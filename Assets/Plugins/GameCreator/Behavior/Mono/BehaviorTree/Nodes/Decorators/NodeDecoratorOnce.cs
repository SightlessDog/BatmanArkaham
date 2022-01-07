namespace GameCreator.Behavior
{
    using System;
    using UnityEngine;

    [Serializable]
    public class NodeDecoratorOnce : NodeIDecorator
    {
        public new static string NAME = "Just Once";

        // VIRTUAL METHODS: -----------------------------------------------------------------------

        public override Node.Return Execute(Node node, GameObject invoker, Behavior behavior)
        {
            int instanceID = node.GetInstanceID();

            int passCount = behavior.GetPassCount(instanceID);
            if (passCount > 0) return behavior.GetState(instanceID);

            Node.Return result = base.Execute(node, invoker, behavior);

            switch (result)
            {
                case Node.Return.Fail:
                case Node.Return.Success:
                    behavior.SetPassCount(instanceID, passCount + 1);
                    return result;
            }

            return Node.Return.Running;
        }

        public override string GetName()
        {
            return NAME;
        }
    }
}