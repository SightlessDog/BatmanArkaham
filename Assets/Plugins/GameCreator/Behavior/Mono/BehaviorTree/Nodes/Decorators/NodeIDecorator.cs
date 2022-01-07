namespace GameCreator.Behavior
{
    using System;
    using UnityEngine;

    [Serializable]
    public abstract class NodeIDecorator : ScriptableObject
    {
        public static string NAME = "(none)";

        // VIRTUAL METHODS: -----------------------------------------------------------------------

        public virtual Node.Return Execute(Node node, GameObject invoker, Behavior behavior)
        {
            if (node.outputs.Count == 1 && node.outputs[0] != null)
            {
                return node.outputs[0].Execute(invoker, behavior);
            }

            return Node.Return.Fail;
        }

        public virtual void Abort(Node node, GameObject invoker, Behavior behavior)
        {
            return;
        }

        // EDITOR METHODS: ------------------------------------------------------------------------

        public virtual string GetName()
        {
            return NAME;
        }
    }
}