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
	public class ActionDebugStat : IAction
	{
        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Invoker);

        [StatSelector]
        public StatAsset stat;

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            if (this.stat != null)
            {
                Stats stats = this.target.GetGameObject(target).GetComponentInChildren<Stats>();
                if (stats == null)
                {
                    Debug.Log("Debug Stat: No Stats found in target");
                    return true;
                }

                Debug.LogFormat(
                    "Stat {0}: {1}",
                    this.stat.stat.uniqueName,
                    stats.GetStat(this.stat.stat.uniqueName)
                );
            }
            else
            {
                Debug.LogError("Stat is null");
            }

            return true;
        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

        #if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Stats/Icons/Actions/";

        public static new string NAME = "Stats/Debug Stat";
        private const string NODE_TITLE = "Debug Stat {0} of {1}";

		// PROPERTIES: ----------------------------------------------------------------------------

		private SerializedProperty spTarget;
        private SerializedProperty spStat;

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle()
		{
            return string.Format(
                NODE_TITLE, 
                (this.stat == null ? "(none)" : this.stat.stat.uniqueName),
                this.target.ToString()
            );
		}

		protected override void OnEnableEditorChild ()
		{
            this.spTarget = this.serializedObject.FindProperty("target");
            this.spStat = this.serializedObject.FindProperty("stat");
		}

		protected override void OnDisableEditorChild ()
		{
            this.spTarget = null;
            this.spStat = null;
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spTarget);
            EditorGUILayout.PropertyField(this.spStat);

			this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}
