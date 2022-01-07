namespace GameCreator.Melee
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
    using GameCreator.Core;
    using GameCreator.Characters;
    using UnityEditorInternal;

    [CustomEditor(typeof(MeleeShield))]
	public class MeleeShieldEditor : IMeleeEditor
	{
        private static readonly GUIContent GC_REACTION = new GUIContent("Block Reaction");

        private const float PADDING = 4f;
        private const float SPACING = 2f;

        // PRIVATE PROPERTIES: --------------------------------------------------------------------

        private MeleeShield instance;

        private Section sectionGeneral;
        private Section sectionEffects;
        private Section sectionCombat;

        private SerializedProperty spName;
        private SerializedProperty spDescription;

        private SerializedProperty spPrefab;
        private SerializedProperty spAttachment;
        private SerializedProperty spPositionOffset;
        private SerializedProperty spRotationOffset;

        private SerializedProperty spDefenseState;
        private SerializedProperty spDefenseMask;
        private SerializedProperty spLowerBodyRotation;

        private SerializedProperty spDefenseAngle;
        private SerializedProperty spPerfectBlockWindow;

        private SerializedProperty spDefenseRecoverRate;
        private SerializedProperty spMaxDefense;
        private SerializedProperty spDelayDefense;

        private SerializedProperty spAudioBlock;
        private SerializedProperty spAudioPerfectBlock;
        private SerializedProperty spPrefabImpactBlock;
        private SerializedProperty spPrefabImpactPerfectBlock;

        private SerializedProperty spPerfectBlockClip;
        private SerializedProperty spBlockHitReactions;
        private SerializedProperty spGroundPerfectBlockReaction;
        private SerializedProperty spAirbornPerfectBlockReaction;

        private ReorderableList blockHitReactionsList;


        // INITIALIZER: ---------------------------------------------------------------------------

        private void OnEnable()
        {
            this.instance = this.target as MeleeShield;

            this.sectionGeneral = new Section("General", this.LoadIcon("General"), this.Repaint);
            this.sectionEffects = new Section("Effects", this.LoadIcon("Effects"), this.Repaint);
            this.sectionCombat = new Section("Combat", this.LoadIcon("Combat"), this.Repaint);

            this.spName = this.serializedObject.FindProperty("shieldName");
            this.spDescription = this.serializedObject.FindProperty("shieldDescription");

            this.spPrefab = this.serializedObject.FindProperty("prefab");
            this.spAttachment = this.serializedObject.FindProperty("attachment");
            this.spPositionOffset = this.serializedObject.FindProperty("positionOffset");
            this.spRotationOffset = this.serializedObject.FindProperty("rotationOffset");

            this.spDefenseState = this.serializedObject.FindProperty("defendState");
            this.spDefenseMask = this.serializedObject.FindProperty("defendMask");
            this.spLowerBodyRotation = this.serializedObject.FindProperty("lowerBodyRotation");

            this.spDefenseAngle = this.serializedObject.FindProperty("defenseAngle");
            this.spPerfectBlockWindow = this.serializedObject.FindProperty("perfectBlockWindow");

            this.spDefenseRecoverRate = this.serializedObject.FindProperty("defenseRecoveryRate");
            this.spMaxDefense = this.serializedObject.FindProperty("maxDefense");
            this.spDelayDefense = this.serializedObject.FindProperty("delayDefense");

            this.spPerfectBlockClip = this.serializedObject.FindProperty("perfectBlockClip");
            this.spBlockHitReactions = this.serializedObject.FindProperty("blockingHitReaction");
            this.spGroundPerfectBlockReaction = this.serializedObject.FindProperty("groundPerfectBlockReaction");
            this.spAirbornPerfectBlockReaction = this.serializedObject.FindProperty("airbornPerfectBlockReaction");

            this.spAudioBlock = this.serializedObject.FindProperty("audioBlock");
            this.spAudioPerfectBlock = this.serializedObject.FindProperty("audioPerfectBlock");
            this.spPrefabImpactBlock = this.serializedObject.FindProperty("prefabImpactBlock");
            this.spPrefabImpactPerfectBlock = this.serializedObject.FindProperty("prefabImpactPerfectBlock");

            this.blockHitReactionsList = new ReorderableList(
                this.serializedObject,
                this.spBlockHitReactions,
                true, true, true, true
            );

            this.blockHitReactionsList.drawHeaderCallback += this.PaintHitBlockReaction_Title;
            this.blockHitReactionsList.drawElementCallback += this.PaintHitBlockReaction_Element;
        }

        // PAINT METHODS: -------------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            this.serializedObject.ApplyModifiedProperties();

            GUILayout.Space(SPACING);
            this.PaintSectionGeneral();

            GUILayout.Space(SPACING);
            this.PaintSectionEffects();

            GUILayout.Space(SPACING);
            this.PaintSectionCombat();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Blocking Hits", EditorStyles.boldLabel);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spPerfectBlockClip);

            EditorGUILayout.Space();
            this.blockHitReactionsList.DoLayoutList();

            EditorGUILayout.LabelField("Perfect Block Reactions", EditorStyles.boldLabel);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spGroundPerfectBlockReaction);
            EditorGUILayout.PropertyField(this.spAirbornPerfectBlockReaction);

            this.serializedObject.ApplyModifiedProperties();
        }

        private void PaintSectionGeneral()
        {
            this.sectionGeneral.PaintSection();
            using (var group = new EditorGUILayout.FadeGroupScope(this.sectionGeneral.state.faded))
            {
                if (group.visible)
                {
                    EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());

                    EditorGUILayout.PropertyField(this.spName);
                    EditorGUILayout.PropertyField(this.spDescription);

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(this.spPrefab);
                    EditorGUILayout.PropertyField(this.spAttachment);

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(this.spPositionOffset);
                    EditorGUILayout.PropertyField(this.spRotationOffset);

                    EditorGUILayout.EndVertical();
                }
            }
        }

        private void PaintSectionEffects()
        {
            this.sectionEffects.PaintSection();
            using (var group = new EditorGUILayout.FadeGroupScope(this.sectionEffects.state.faded))
            {
                if (group.visible)
                {
                    EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());

                    EditorGUILayout.PropertyField(this.spAudioBlock);
                    EditorGUILayout.PropertyField(this.spAudioPerfectBlock);

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(this.spPrefabImpactBlock);
                    EditorGUILayout.PropertyField(this.spPrefabImpactPerfectBlock);

                    EditorGUILayout.EndVertical();
                }
            }
        }

        private void PaintSectionCombat()
        {
            this.sectionCombat.PaintSection();
            using (var group = new EditorGUILayout.FadeGroupScope(this.sectionCombat.state.faded))
            {
                if (group.visible)
                {
                    EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());

                    EditorGUILayout.PropertyField(this.spDefenseState);
                    EditorGUILayout.PropertyField(this.spDefenseMask);
                    EditorGUILayout.PropertyField(this.spLowerBodyRotation);

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(this.spDefenseAngle);

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(this.spPerfectBlockWindow);

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(this.spDefenseRecoverRate);

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(this.spMaxDefense);

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(this.spDelayDefense);

                    EditorGUILayout.EndVertical();
                }
            }
        }

        // HIT REACTIONS: -------------------------------------------------------------------------

        private void PaintHitBlockReaction_Title(Rect rect)
        {
            EditorGUI.LabelField(rect, "Block Hit Reactions");
        }

        private void PaintHitBlockReaction_Element(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect = new Rect(
                rect.x, rect.y + (rect.height - EditorGUIUtility.singleLineHeight) / 2f,
                rect.width, EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(
                rect, this.spBlockHitReactions.GetArrayElementAtIndex(index),
                GC_REACTION, true
            );
        }
    }
}
