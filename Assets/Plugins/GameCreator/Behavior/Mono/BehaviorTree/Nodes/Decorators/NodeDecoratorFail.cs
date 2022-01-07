namespace GameCreator.Behavior
{
    using System;
    using UnityEngine;

    [Serializable]
    public class NodeDecoratorFail : NodeIDecorator
    {
        public new static string NAME = "Always Fail";

        // VIRTUAL METHODS: -----------------------------------------------------------------------

        public override Node.Return Execute(Node node, GameObject invoker, Behavior behavior)
        {
            Node.Return result = base.Execute(node, invoker, behavior);
            return (result == Node.Return.Running ? result : Node.Return.Fail);
        }

        public override string GetName()
        {
            return NAME;
        }
    }
}