namespace GameCreator.Behavior
{
    using System.Collections;
    using UnityEngine;
    using GameCreator.Core;

    public class NodeCoroutine
    {
        public Coroutine coroutine { get; private set; }

        private object result;
        private readonly IEnumerator target;

        // INITIALIZERS: --------------------------------------------------------------------------

        public NodeCoroutine(IEnumerator target)
        {
            this.target = target;
            this.result = Node.Return.Running;
            this.coroutine = CoroutinesManager.Instance.StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            while (this.target.MoveNext())
            {
                this.result = this.target.Current;
                yield return this.result;
            }
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public Node.Return GetReturn()
        {
            if (this.result == null) return Node.Return.None;
            if (this.result is Node.Return) return (Node.Return)this.result;
            return Node.Return.Running;
        }
    }
}