namespace GameCreator.Stats
{
    using System;
    using System.IO;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using GameCreator.Core;

    [CustomEditor(typeof(StatusEffectAsset))]
    public class StatusEffectAssetEditor : Editor
    {
        public const string PROP_UNIQUE_ID = "uniqueID";

        private const string PROP_ACTIONS_ONSTART = "actionsOnStart";
        private const string PROP_ACTIONS_WHILEACTIVE = "actionsWhileActive";
        private const string PROP_ACTIONS_ONEND = "actionsOnEnd";

        private const string PATH_PREFABS = "Assets/Plugins/GameCreatorData/Stats/Prefabs/StatusEffects/";
        private const string NAME_PREFAB_ONSTART = "OnStart-{0}.prefab";
        private const string NAME_PREFAB_WHILEACTIVE = "WhileActive-{0}.prefab";
        private const string NAME_PREFAB_ONEND = "OnEnd-{0}.prefab";

        private static readonly string[] ACTIONS_OPTS = new string[]
        {
            "On Start",
            "While Active",
            "On End",
        };

        private static readonly string[] ACTIONS_INFO = new string[]
        {
            "Executes these actions when the Status Effect starts",
            "Executes (over and over again) these actions for as long the Status Effect is active",
            "Executes these actions when the Status Effect ends",
        };

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spUniqueID;
        private SerializedProperty spStatusEffect;

        private SerializedProperty spActionsOnStart;
        private SerializedProperty spActionsWhileActive;
        private SerializedProperty spActionsOnEnd;

        private IActionsListEditor editorActionsOnStart;
        private IActionsListEditor editorActionsWhileActive;
        private IActionsListEditor editorActionsOnEnd;

        private int actionsOptIndex = 0;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void OnEnable()
        {
            if (target == null || serializedObject == null) return;

            this.spUniqueID = serializedObject.FindProperty(PROP_UNIQUE_ID);
            this.spStatusEffect = serializedObject.FindProperty("statusEffect");

            if (string.IsNullOrEmpty(this.spUniqueID.stringValue))
            {
                this.spUniqueID.stringValue = Guid.NewGuid().ToString("N");
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                serializedObject.Update();
            }

            this.spActionsOnStart = spStatusEffect.FindPropertyRelative(PROP_ACTIONS_ONSTART);
            this.spActionsWhileActive = spStatusEffect.FindPropertyRelative(PROP_ACTIONS_WHILEACTIVE);
            this.spActionsOnEnd = spStatusEffect.FindPropertyRelative(PROP_ACTIONS_ONEND);

            this.RequireActionsInit(ref this.spActionsOnStart, NAME_PREFAB_ONSTART, true);
            this.RequireActionsInit(ref this.spActionsWhileActive, NAME_PREFAB_WHILEACTIVE, false);
            this.RequireActionsInit(ref this.spActionsOnEnd, NAME_PREFAB_ONEND, true);

            this.editorActionsOnStart = (IActionsListEditor)Editor.CreateEditor(
                this.spActionsOnStart.objectReferenceValue, typeof(IActionsListEditor)
            );

            this.editorActionsWhileActive = (IActionsListEditor)Editor.CreateEditor(
                this.spActionsWhileActive.objectReferenceValue, typeof(IActionsListEditor)
            );

            this.editorActionsOnEnd = (IActionsListEditor)Editor.CreateEditor(
                this.spActionsOnEnd.objectReferenceValue, typeof(IActionsListEditor)
            );
        }

        private void RequireActionsInit(ref SerializedProperty property, string prefabName, bool destroyAtFinish)
        {
            if (property.objectReferenceValue == null)
            {
                prefabName = string.Format(prefabName, GameCreatorUtilities.RandomHash(8));
                GameCreatorUtilities.CreateFolderStructure(PATH_PREFABS);
                string actionsPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(
                    PATH_PREFABS, prefabName)
                );

                GameObject sceneInstance = new GameObject(prefabName);
                sceneInstance.AddComponent<Actions>();

                GameObject prefabInstance = PrefabUtility.SaveAsPrefabAsset(sceneInstance, actionsPath);
                DestroyImmediate(sceneInstance);

                Actions prefabActions = prefabInstance.GetComponent<Actions>();
                prefabActions.destroyAfterFinishing = destroyAtFinish;

                property.objectReferenceValue = prefabActions.actionsList;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                serializedObject.Update();
            }
        }

        public void OnDestroyStatusEffect()
        {
            SerializedProperty[] actions = new SerializedProperty[]
            {
                this.spActionsOnStart,
                this.spActionsWhileActive,
                this.spActionsOnEnd,
            };

            for (int i = 0; i < actions.Length; ++i)
            {
                if (actions[i].objectReferenceValue == null) continue;
                IActionsList aList = (IActionsList)actions[i].objectReferenceValue;

                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(aList.gameObject));
                AssetDatabase.SaveAssets();
            }
        }

        // PAINT METHODS: -------------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            if (target == null || serializedObject == null) return;

            serializedObject.Update();

            EditorGUILayout.PropertyField(this.spStatusEffect);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            this.actionsOptIndex = GUILayout.Toolbar(this.actionsOptIndex, ACTIONS_OPTS);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            switch (this.actionsOptIndex)
            {
                case 0: this.editorActionsOnStart.OnInspectorGUI(); break;
                case 1: this.editorActionsWhileActive.OnInspectorGUI(); break;
                case 2: this.editorActionsOnEnd.OnInspectorGUI(); break;
            }

            EditorGUILayout.HelpBox(ACTIONS_INFO[this.actionsOptIndex], MessageType.Info);

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}