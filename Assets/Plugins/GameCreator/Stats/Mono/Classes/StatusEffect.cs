namespace GameCreator.Stats
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Localization;

    [Serializable]
    public class StatusEffect
    {
        public enum Type
        {
            Positive,
            Negative,
            Other
        }

        public class Runtime
        {
            public float time = 0.0f;

            private Actions onStart;
            private Actions whileActive;
            private Actions onEnd;

            // CONSTRUCTORS: ----------------------------------------------------------------------

            public Runtime(StatusEffect statusEffect)
            {
                GameObject instStart = GameObject.Instantiate(statusEffect.actionsOnStart.gameObject);
                GameObject instActive = GameObject.Instantiate(statusEffect.actionsWhileActive.gameObject);
                GameObject instEnd = GameObject.Instantiate(statusEffect.actionsOnEnd.gameObject);

                instStart.hideFlags = HideFlags.HideInHierarchy;
                instActive.hideFlags = HideFlags.HideInHierarchy;
                instEnd.hideFlags = HideFlags.HideInHierarchy;

                this.onStart = instStart.GetComponent<Actions>();
                this.whileActive = instActive.GetComponent<Actions>();
                this.onEnd = instEnd.GetComponent<Actions>();
            }

            public Runtime(float time, StatusEffect statusEffect) : this(statusEffect)
            {
                this.time = time;
            }

            // PUBLIC METHODS: --------------------------------------------------------------------

            public void OnStart(GameObject invoker)
            {
                this.onStart.Execute(invoker, null);
            }

            public void OnUpdate(GameObject invoker)
            {
                if (this.whileActive.actionsList.isExecuting) return;
                this.whileActive.Execute(invoker, null);
            }

            public void OnEnd(GameObject invoker)
            {
                this.onEnd.Execute(invoker, null);
                this.whileActive.destroyAfterFinishing = true;
            }
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public string uniqueName = "";
        public string shortName = "";

        [LocStringNoPostProcess] public LocString title = new LocString();
        [LocStringNoPostProcess] public LocString description = new LocString();

        public Sprite icon = null;
        public Color color = Color.grey;

        public Type type = Type.Positive;

        public bool hasDuration = false;
        public float duration = 60f;

        [Range(1,99)]
        public int maxStack = 1;

        public IActionsList actionsOnStart;
        public IActionsList actionsWhileActive;
        public IActionsList actionsOnEnd;
    }

    public class StatusEffectSelectorAttribute : PropertyAttribute
    {
        public StatusEffectSelectorAttribute() { }
    }
}