namespace GameCreator.Behavior
{
    using System;
    using UnityEngine;

    [Serializable]
    public class NodeDecoratorRepeatWhileSuccess : NodeIDecorator
    {
        public new static string NAME = "Repeat while Success";

        // VIRTUAL METHODS: -----------------------------------------------------------------------

        public override Node.Return Execute(Node node, GameObject invoker, Behavior behavior)
        {
            int instanceID = node.GetInstanceID();

            Node.Return state = behavior.GetState(instanceID);
            if (state == Node.Return.Fail) return Node.Return.Fail;

            Node.Return result = base.Execute(node, invoker, behavior);

            switch (result)
            {
                case Node.Return.Fail:
                    return Node.Return.Fail;

                case Node.Return.Success:
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