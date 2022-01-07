namespace GameCreator.Behavior
{
    using System;
    using UnityEngine;

    [Serializable]
    public class NodeDecoratorRepeatWhileFail : NodeIDecorator
    {
        public new static string NAME = "Repeat while Fail";

        // VIRTUAL METHODS: -----------------------------------------------------------------------

        public override Node.Return Execute(Node node, GameObject invoker, Behavior behavior)
        {
            int instanceID = node.GetInstanceID();

            Node.Return state = behavior.GetState(instanceID);
            if (state == Node.Return.Success) return Node.Return.Success;

            Node.Return result = base.Execute(node, invoker, behavior);

            switch (result)
            {
                case Node.Return.Success:
                    return Node.Return.Success;

                case Node.Return.Fail:
                    node.ResetOutputStates(invoker, behavior, true);
                    break;
            }

            return Node.Return.Running;
        }

        public override string GetName()
        {
            return NAME;
        }
    }
}