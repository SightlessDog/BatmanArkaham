namespace GameCreator.Behavior
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using GameCreator.Core;

	[Serializable]
	public class NodeTask : Node
	{
        // PUBLIC METHODS: ------------------------------------------------------------------------

        protected override Return UpdateNode(GameObject invoker, Behavior behavior)
        {
            int instanceID = this.GetInstanceID();

            if (this.CheckConditions(invoker, behavior))
            {
                if (!this.useActionsList || this.prefabActionsList == null)
                {
                    return Return.Success;
                }

                Actions actions = behavior.GetActions(instanceID);

                if (behavior.GetActions(instanceID) == null)
                {
                    if (behavior.GetState(instanceID) == Return.Running ||
                        behavior.GetState(instanceID) == Return.Success)
                    {
                        return Return.Success;
                    }

                    GameObject instance = Instantiate(
                        this.prefabActionsList.gameObject,
                        invoker.transform.position,
                        invoker.transform.rotation
                    );

                    instance.hideFlags = instance.hideFlags | HideFlags.HideInHierarchy;
                    actions = instance.GetComponent<Actions>();
                    behavior.SetActions(instanceID, actions);

                    CoroutinesManager.Instance.StartCoroutine(
                        this.DeferStartActions(invoker, behavior)
                    );
                }

                return Return.Running;
            }

            // TODO add check here if conditions allow to skip conditions when Task is running, then
            // don't abort and return running, like if condition was true

            if (behavior.GetState(instanceID) == Return.Running)
            {
                this.AbortNode(invoker, behavior);
            }

            return Return.Fail;
        }

        private IEnumerator DeferStartActions(GameObject invoker, Behavior behavior)
        {
            yield return null;

            int instanceID = this.GetInstanceID();
            if (behavior.GetState(instanceID) == Return.Running)
            {
                Actions actions = behavior.GetActions(instanceID);
                if (actions != null) actions.Execute(invoker);
            }
        }

        public override void AbortNode(GameObject invoker, Behavior behavior)
        {
            int instanceID = this.GetInstanceID();
            if (behavior.GetState(instanceID) == Return.None) return;

            behavior.StopActions(instanceID);
            behavior.SetState(instanceID, Return.None);
        }
    }
}