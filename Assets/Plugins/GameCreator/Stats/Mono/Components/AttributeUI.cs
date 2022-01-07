namespace GameCreator.Stats
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using GameCreator.Core;

    [AddComponentMenu("UI/Game Creator/Attribute UI", 0)]
	public class AttributeUI : MonoBehaviour
    {
        private const float TRANSITION_SMOOTH = 0.25f;

        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);

        [AttributeSelector]
        public AttrAsset attribute;

        public Image icon;
        public Graphic color;
        public Text title;
        public Text description;
        public Text shortName;

        [Tooltip("{0}: The current value of the Attribute\n{1}: The maximum value or stat value\n{2}: The minimum value")]
        public string valueFormat = "{0}/{1}";
        public Text value;

        public Image valueFillImage;
        public RectTransform valueScaleX;
        public RectTransform valueScaleY;

        public bool smoothTransitionUp;
        public bool smoothTransitionDown;
        public float transitionSpeed = TRANSITION_SMOOTH;
        public float transitionDelay = 0.2f;

        private float transitionVelocity;
        private float transitionCurrentPercent;
        private float transitionTargetPercent;
        private float transitionTime;

        private bool exitingApplication;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void Start()
        {
            Stats stats = this.GetStatsTarget();
            if (!stats) return;

            stats.AddOnChangeAttr(this.UpdateAttrUI);
            this.UpdateAttrUI(null);
        }

        private void OnDestroy()
        {
            if (this.exitingApplication) return;

            Stats stats = this.GetStatsTarget();
            if (!stats) return;

            stats.RemoveOnChangeAttr(this.UpdateAttrUI);
        }

        private void OnApplicationQuit()
        {
            this.exitingApplication = true;
        }

        private void Update()
        {
            float targetPercent = Time.time > this.transitionTime + this.transitionDelay
                ? this.transitionTargetPercent
                : this.transitionCurrentPercent;

            this.transitionCurrentPercent = Mathf.SmoothDamp(
                this.transitionCurrentPercent,
                targetPercent,
                ref this.transitionVelocity,
                this.transitionSpeed
            );

            if (this.valueFillImage)
            {
                this.valueFillImage.fillAmount = this.transitionCurrentPercent;
            }

            if (this.valueScaleX) this.valueScaleX.localScale = new Vector3(
                this.transitionCurrentPercent,
                this.valueScaleX.localScale.y,
                this.valueScaleX.localScale.z
            );

            if (this.valueScaleY) this.valueScaleY.localScale = new Vector3(
                this.valueScaleY.localScale.x,
                this.transitionCurrentPercent,
                this.valueScaleY.localScale.z
            );
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private Stats GetStatsTarget()
        {
            if (!this.attribute) return null;

            GameObject targetGO = this.target.GetGameObject(gameObject);
            if (!targetGO) return null;

            Stats stats = targetGO.GetComponentInChildren<Stats>(true);
            if (!stats) return null;

            return stats;
        }

        public void UpdateAttrUI()
        {
            this.UpdateAttrUI(null);
        }

        private void UpdateAttrUI(Stats.EventArgs args)
        {
            Stats stats = this.GetStatsTarget();
            if (!stats) return;

            string attrID = this.attribute.attribute.uniqueName;

            if (this.icon) this.icon.overrideSprite = stats.GetAttrIcon(attrID);
            if (this.color) this.color.color = stats.GetAttrColor(attrID);
            if (this.title) this.title.text = stats.GetAttrTitle(attrID);
            if (this.description) this.description.text = stats.GetAttrDescription(attrID);
            if (this.shortName) this.shortName.text = stats.GetAttrShortName(attrID);

            float curAttr = stats.GetAttrValue(attrID);
            float maxAttr = stats.GetAttrMaxValue(attrID);
            float minAttr = this.attribute.attribute.minValue;

            if (this.value) this.value.text = string.Format(
                this.valueFormat,
                curAttr,
                maxAttr,
                minAttr
            );

            float percent = Mathf.InverseLerp(minAttr, maxAttr, curAttr);
            this.transitionTargetPercent = percent;
            this.transitionVelocity = 0f;
            this.transitionTime = Time.time;

            if ((this.transitionCurrentPercent < percent && !this.smoothTransitionUp) ||
                (this.transitionCurrentPercent > percent && !this.smoothTransitionDown))
            {
                this.transitionCurrentPercent = percent;
            }
        }
    }
}
