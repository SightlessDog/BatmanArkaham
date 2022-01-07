namespace GameCreator.Behavior
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using GameCreator.Core.Hooks;

    [AddComponentMenu("Game Creator/Perception")]
    public class Perception : MonoBehaviour
    {
        public enum Type
        {
            Sight,
            // Hearing, | coming soon!
            // Smell    | coming soon!
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        [Range(1, 360f)]
        public float fieldOfView = PerceptronSight.DEFAULT_FOV;
        public float visionDistance = PerceptronSight.DEFAULT_DISTANCE;
        public LayerMask sightLayerMask = -1;

        private readonly PerceptronBase[] perceptrons = {
            new PerceptronSight(),
        };

        // INITIALIZERS: --------------------------------------------------------------------------

        private void Awake()
        {
            for (int i = 0; i < this.perceptrons.Length; ++i)
            {
                this.perceptrons[i].Awake(this);
            }
        }

        // UPDATE: --------------------------------------------------------------------------------

        private void Update()
        {
            for (int i = 0; i < this.perceptrons.Length; ++i)
            {
                this.perceptrons[i].Update();
            }
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < this.perceptrons.Length; ++i)
            {
                this.perceptrons[i].FixedUpdate();
            }
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void StartListenPerceptron(Perception.Type type, GameObject target, UnityAction<bool, GameObject> callback)
        {
            this.perceptrons[(int)type].StartListenPerceptron(target, callback);
        }

        public void StopListenPerceptron(Perception.Type type, GameObject target, UnityAction<bool, GameObject> callback)
        {
            this.perceptrons[(int)type].StopListenPerceptron(target, callback);
        }

        public PerceptronBase GetPerceptron(Perception.Type type)
        {
            return this.perceptrons[(int)type];
        }
    }
}