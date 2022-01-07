namespace GameCreator.Stats
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;

	#if UNITY_EDITOR
	using UnityEditor;
	#endif

	[AddComponentMenu("")]
    public class ActionDebugAttribute : IAction
	{
        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Invoker);

        [AttributeSelector]
        public AttrAsset attribute;

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            if (this.attribute != null)
            {
                Stats stats = this.target.GetGameObject(target).GetComponentInChildren<Stats>();
                if (stats == null)
                {
                    Debug.Log("Debug Attribute: No Stats found in target");
                    return true;
                }

                Debug.LogFormat(
                    "Attribute {0}: {1}/{2}",
                    this.attribute.attribute.uniqueName,
                    stats.GetAttrValue(this.attribute.attribute.uniqueName),
                    stats.GetAttrMaxValue(this.attribute.attribute.uniqueName)
                );
            }
            else
            {
                Debug.LogError("Attribute is null");
            }

            return true;
        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

        #if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Stats/Icons/Actions/";

        public static new string NAME = "Stats/Debug Attribute";
        private const string NODE_TITLE = "Debug Attribute {0} of {1}";

		// PROPERTIES: ----------------------------------------------------------------------------

		private SerializedProperty spTarget;
        private SerializedProperty spAttribute;

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle()
		{
            return string.Format(
                NODE_TITLE, 
                (this.attribute == null ? "(none)" : this.attribute.attribute.uniqueName),
                this.target.ToString()
            );
		}

		protected override void OnEnableEditorChild ()
		{
            this.spTarget = this.serializedObject.FindProperty("target");
            this.spAttribute = this.serializedObject.FindProperty("attribute");
		}

		protected override void OnDisableEditorChild ()
		{
            this.spTarget = null;
            this.spAttribute = null;
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spTarget);
            EditorGUILayout.PropertyField(this.spAttribute);

			this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}
