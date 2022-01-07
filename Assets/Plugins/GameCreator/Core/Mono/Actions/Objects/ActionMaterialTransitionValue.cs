namespace GameCreator.Core
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Variables;
	#if UNITY_EDITOR
	using UnityEditor;
	#endif

	[AddComponentMenu("")]
	public class ActionMaterialTransitionValue : IAction
	{
		public enum ValueType
		{
			Float,
			Color
		}
		
		public TargetGameObject targetObject = new TargetGameObject();
		public bool applyToChildObjects = false;
		public float duration = 1f;

		[Space] 
		public string propertyName = "_MyValue";
		
		public ValueType propertyValueType = ValueType.Float;
		public NumberProperty number = new NumberProperty(1f);
		public ColorProperty color = new ColorProperty(Color.white);

		public override IEnumerator Execute(GameObject target, IAction[] actions, int index)
		{
			GameObject _targetObject = this.targetObject.GetGameObject(target);
			if (_targetObject != null)
			{
				Renderer[] renderers = this.applyToChildObjects
					? _targetObject.GetComponentsInChildren<Renderer>()
					: new[] { _targetObject.GetComponent<Renderer>() };

				float startTime = Time.time;
				while (Time.time - startTime <= this.duration)
				{
					float t = Mathf.Clamp01((Time.time - startTime) / this.duration);
					
					foreach (Renderer renderer in renderers)
					{
						if (renderer != null && renderer.material != null && 
						    renderer.material.HasProperty(this.propertyName))
						{
							switch (propertyValueType)
							{
								case ValueType.Float:
									float originalFloat = renderer.material.GetFloat(this.propertyName);
									renderer.material.SetFloat(this.propertyName, Mathf.Lerp(
										originalFloat, this.number.GetValue(target), t
									));
									break;
									
								case ValueType.Color:
									Color originalColor = renderer.material.GetColor(this.propertyName);
									renderer.material.SetColor(this.propertyName, Color.Lerp(
										originalColor, this.color.GetValue(target), t
									));
									break;
							}
						}
					}

					yield return null;
				}
			}
			
			yield return 0;
		}

		#if UNITY_EDITOR
        public static new string NAME = "Object/Material Transition";
        private const string NODE_TITLE = "Change {0} in {1} seconds";

        public override string GetNodeTitle()
        {
	        return string.Format(NODE_TITLE, this.targetObject, this.duration);
        }
        
        private SerializedProperty spTargetObject;
        private SerializedProperty spApplyToChildObjects;
        private SerializedProperty spDuration;

        private SerializedProperty spPropertyName;
		
        private SerializedProperty spPropertyValueType;
        private SerializedProperty spNumber;
        private SerializedProperty spColor;

        protected override void OnEnableEditorChild()
        {
	        spTargetObject = this.serializedObject.FindProperty("targetObject");
	        spApplyToChildObjects = this.serializedObject.FindProperty("applyToChildObjects");
	        spDuration = this.serializedObject.FindProperty("duration");
	        spPropertyName = this.serializedObject.FindProperty("propertyName");
	        spPropertyValueType = this.serializedObject.FindProperty("propertyValueType");
	        spNumber = this.serializedObject.FindProperty("number");
	        spColor = this.serializedObject.FindProperty("color");
        }

        public override void OnInspectorGUI()
        {
	        this.serializedObject.Update();
	        
	        EditorGUILayout.PropertyField(this.spTargetObject);
	        EditorGUILayout.PropertyField(this.spApplyToChildObjects);
	        EditorGUILayout.PropertyField(this.spDuration);
	        
	        EditorGUILayout.PropertyField(this.spPropertyName);
	        EditorGUILayout.PropertyField(this.spPropertyValueType);

	        if (this.spPropertyValueType.intValue == (int) ValueType.Float)
	        {
		        EditorGUILayout.PropertyField(this.spNumber);
	        }
	        else if (this.spPropertyValueType.intValue == (int) ValueType.Color)
	        {
		        EditorGUILayout.PropertyField(this.spColor);
	        }

	        this.serializedObject.ApplyModifiedProperties();
        }

		#endif
	}
}
