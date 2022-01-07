namespace GameCreator.Melee
{
	using UnityEngine;

	public class InputBuffer
	{
        public float timeWindow;

        private float inputTime;
        private CharacterMelee.ActionKey key;

        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public InputBuffer(float timeWindow)
        {
            this.timeWindow = timeWindow;

            this.inputTime = -100f;
            this.key = CharacterMelee.ActionKey.A;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void AddInput(CharacterMelee.ActionKey key)
        {
            this.key = key;
            this.inputTime = Time.time;
        }

        public bool HasInput()
        {
            if (this.inputTime <= 0f) return false;
            return Time.time - this.inputTime <= this.timeWindow;
        }

        public CharacterMelee.ActionKey GetInput()
        {
            return this.key;
        }

        public void ConsumeInput()
        {
            this.inputTime = -100f;
        }
	}
}