namespace GameCreator.Behavior
{
    using System;
    using UnityEngine;

    [Serializable]
    public class NodeDecoratorRunning : NodeIDecorator
    {
        public new static string NAME = "Always Running";

        // VIRTUAL METHODS: -----------------------------------------------------------------------

        public override Node.Return Execute(Node node, GameObject invoker, Behavior behavior)
        {
            base.Execute(node, invoker, behavior);
            return Node.Return.Running;
        }

        public override string GetName()
        {
            return NAME;
        }
    }
}