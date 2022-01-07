namespace GameCreator.Behavior
{
    using System;
    using UnityEngine;

    [Serializable]
    public class NodeDecoratorInvert : NodeIDecorator
    {
        public new static string NAME = "Invert";

        // VIRTUAL METHODS: -----------------------------------------------------------------------

        public override Node.Return Execute(Node node, GameObject invoker, Behavior behavior)
        {
            Node.Return result = base.Execute(node, invoker, behavior);

            switch (result)
            {
                case Node.Return.Fail: return Node.Return.Success;
                case Node.Return.Success: return Node.Return.Fail;
                default: return result;
            }
        }

        public override string GetName()
        {
            return NAME;
        }
    }
}