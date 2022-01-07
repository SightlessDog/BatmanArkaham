namespace GameCreator.Stats
{
    using System;
    using System.IO;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.AnimatedValues;
    using UnityEditor.SceneManagement;
    using UnityEditorInternal;
    using System.Linq;
    using System.Reflection;
    using GameCreator.Core;

    [CustomEditor(typeof(DatabaseStats))]
    public class DatabaseStatsEditor : IDatabaseEditor
    {
        private const string ASSETS_PATH = "Assets/Plugins/GameCreatorData/Stats/";
        private const string STATSASSET_FILE = "StatsAsset.asset";
        private const string ATTRSASSET_FILE = "AttributesAsset.asset";
        private const string STAEFASSET_FILE = "StatusEffectAsset.asset";

        private static readonly string[] OPTIONS = new string[]
        {
            "Stats",
            "Attributes",
            "Status Effects",
        };

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spStatsAsset;
        private SerializedProperty spAttributesAsset;
        private SerializedProperty spStatusEffectsAsset;

        private StatsAssetEditor statsAssetEditor;
        private AttrsAssetEditor attrsAssetEditor;
        private StatusEffectsAssetEditor statusEffectsAssetEditor;

        private int optionsIndex = 0;

        // INITIALIZE: ----------------------------------------------------------------------------

        private void OnEnable()
        {
            if (target == null || serializedObject == null) return;

            this.spStatsAsset = serializedObject.FindProperty("statsAsset");
            this.spAttributesAsset = serializedObject.FindProperty("attrsAsset");
            this.spStatusEffectsAsset = serializedObject.FindProperty("statusEffectsAsset");

            if (this.spStatsAsset.objectReferenceValue == null)
            {
                string path = Path.Combine(ASSETS_PATH, STATSASSET_FILE);
                StatsAsset statsAsset = AssetDatabase.LoadAssetAtPath<StatsAsset>(path);

                if (statsAsset == null) statsAsset = CreateStatsAsset();
                this.spStatsAsset.objectReferenceValue = statsAsset;

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                serializedObject.Update();
            }

            if (this.spAttributesAsset.objectReferenceValue == null)
            {
                string path = Path.Combine(ASSETS_PATH, ATTRSASSET_FILE);
                AttrsAsset attributesAsset = AssetDatabase.LoadAssetAtPath<AttrsAsset>(path);

                if (attributesAsset == null) attributesAsset = CreateAttributesAsset();
                this.spAttributesAsset.objectReferenceValue = attributesAsset;

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                serializedObject.Update();
            }

            if (this.spStatusEffectsAsset.objectReferenceValue == null)
            {
                string path = Path.Combine(ASSETS_PATH, STAEFASSET_FILE);
                StatusEffectsAsset statusEffectsAsset = AssetDatabase.LoadAssetAtPath<StatusEffectsAsset>(path);

                if (statusEffectsAsset == null) statusEffectsAsset = CreateStatusEffectsAsset();
                this.spStatusEffectsAsset.objectReferenceValue = statusEffectsAsset;

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                serializedObject.Update();
            }

            this.statsAssetEditor = (StatsAssetEditor)CreateEditor(this.spStatsAsset.objectReferenceValue);
            this.attrsAssetEditor = (AttrsAssetEditor)CreateEditor(this.spAttributesAsset.objectReferenceValue);
            this.statusEffectsAssetEditor = (StatusEffectsAssetEditor)CreateEditor(this.spStatusEffectsAsset.objectReferenceValue);
        }

        // OVERRIDE METHODS: ----------------------------------------------------------------------

        public override string GetDocumentationURL()
        {
            return "https://docs.gamecreator.io/stats";
        }

        public override string GetName()
        {
            return "Stats";
        }

        public override bool CanBeDecoupled()
        {
            return true;
        }

        // GUI METHODS: ---------------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            this.optionsIndex = GUILayout.Toolbar(this.optionsIndex, OPTIONS);
            switch (this.optionsIndex)
            {
                case 0 : this.PaintStats(); break;
                case 1 : this.PaintAttrs(); break;
                case 2 : this.PaintStEff(); break;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private void PaintStats()
        {
            this.statsAssetEditor.OnInspectorGUI();
        }

        private void PaintAttrs()
        {
            this.attrsAssetEditor.OnInspectorGUI();
        }

        private void PaintStEff()
        {
            this.statusEffectsAssetEditor.OnInspectorGUI();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        public static StatsAsset CreateStatsAsset()
        {
            string filepath = ASSETS_PATH;
            string filename = STATSASSET_FILE;

            StatsAsset asset = ScriptableObject.CreateInstance<StatsAsset>();

            GameCreatorUtilities.CreateFolderStructure(filepath);
            string path = Path.Combine(filepath, filename);
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.ImportAsset(path);
            return asset;
        }

        public static AttrsAsset CreateAttributesAsset()
        {
            string filepath = ASSETS_PATH;
            string filename = ATTRSASSET_FILE;

            AttrsAsset asset = ScriptableObject.CreateInstance<AttrsAsset>();

            GameCreatorUtilities.CreateFolderStructure(filepath);
            string path = Path.Combine(filepath, filename);
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.ImportAsset(path);
            return asset;
        }

        public static StatusEffectsAsset CreateStatusEffectsAsset()
        {
            string filepath = ASSETS_PATH;
            string filename = STAEFASSET_FILE;

            StatusEffectsAsset asset = ScriptableObject.CreateInstance<StatusEffectsAsset>();

            GameCreatorUtilities.CreateFolderStructure(filepath);
            string path = Path.Combine(filepath, filename);
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.ImportAsset(path);
            return asset;
        }

        public static StatAsset AddStatsAsset()
        {
            StatAsset statAsset = ScriptableObject.CreateInstance<StatAsset>();
            statAsset.name = GameCreatorUtilities.RandomHash(8);

            string path = Path.Combine(ASSETS_PATH, STATSASSET_FILE);
            AssetDatabase.AddObjectToAsset(statAsset, path);

            return statAsset;
        }

        public static AttrAsset AddAttributeAsset()
        {
            AttrAsset attrAsset = ScriptableObject.CreateInstance<AttrAsset>();
            attrAsset.name = GameCreatorUtilities.RandomHash(8);

            string path = Path.Combine(ASSETS_PATH, ATTRSASSET_FILE);
            AssetDatabase.AddObjectToAsset(attrAsset, path);

            return attrAsset;
        }

        public static StatusEffectAsset AddStatusEffectAsset()
        {
            StatusEffectAsset statusEffectAsset = ScriptableObject.CreateInstance<StatusEffectAsset>();
            statusEffectAsset.name = GameCreatorUtilities.RandomHash(8);

            string path = Path.Combine(ASSETS_PATH, STAEFASSET_FILE);
            AssetDatabase.AddObjectToAsset(statusEffectAsset, path);

            return statusEffectAsset;
        }

        // PUBLIC STATIC METHODS: -----------------------------------------------------------------

        public static StatsAsset GetStatsAsset()
        {
            string path = Path.Combine(ASSETS_PATH, STATSASSET_FILE);
            StatsAsset statsAsset = AssetDatabase.LoadAssetAtPath<StatsAsset>(path);
            if (statsAsset == null) statsAsset = CreateStatsAsset();
            return statsAsset;
        }

        public static AttrsAsset GetAttrsAsset()
        {
            string path = Path.Combine(ASSETS_PATH, ATTRSASSET_FILE);
            AttrsAsset attrsAsset = AssetDatabase.LoadAssetAtPath<AttrsAsset>(path);
            if (attrsAsset == null) attrsAsset = CreateAttributesAsset();
            return attrsAsset;
        }

        public static StatusEffectsAsset GetStatusEffectsAsset()
        {
            string path = Path.Combine(ASSETS_PATH, STAEFASSET_FILE);
            StatusEffectsAsset statusEffectsAsset = AssetDatabase.LoadAssetAtPath<StatusEffectsAsset>(path);
            if (statusEffectsAsset == null) statusEffectsAsset = CreateStatusEffectsAsset();
            return statusEffectsAsset;
        }
    }
}