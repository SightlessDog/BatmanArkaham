namespace GameCreator.Melee
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
    using GameCreator.Core;
    using GameCreator.Characters;
    using UnityEditorInternal;

    [CustomEditor(typeof(MeleeWeapon))]
	public class MeleeWeaponEditor : IMeleeEditor
	{
        private static readonly Color ODD_LAYER = new Color(0, 0, 0, 0.1f);
        private static readonly Color ACTIVE_LAYER = new Color(0, 0, 0, 0.3f);

        private static readonly GUIContent GC_REACTION = new GUIContent("Hit Reaction");

        private const float PADDING = 4f;
        private const float SPACING = 2f;

        // PRIVATE PROPERTIES: --------------------------------------------------------------------

        private MeleeWeapon instance;

        private Section sectionGeneral;
        private Section sectionModel;
        private Section sectionEffects;

        private SerializedProperty spName;
        private SerializedProperty spDescription;

        private SerializedProperty spDefaultShield;
        private SerializedProperty spCharacterState;
        private SerializedProperty spAvatarMask;

        private SerializedProperty spPrefab;
        private SerializedProperty spAttachment;
        private SerializedProperty spPosition;
        private SerializedProperty spRotation;

        private SerializedProperty spAudioSheathe;
        private SerializedProperty spAudioDraw;
        private SerializedProperty spAudioImpactNormal;
        private SerializedProperty spAudioImpactKnockback;

        private SerializedProperty spPrefabImpactNormal;
        private SerializedProperty spPrefabImpactKnockback;

        private SerializedProperty spGroundHitReactionFront;
        private SerializedProperty spGroundHitReactionBehind;
        private SerializedProperty spAirborneHitReactionFront;
        private SerializedProperty spAirborneHitReactionBehind;
        private SerializedProperty spKnockbackReactions;

        private SerializedProperty spCombos;

        private ReorderableList groundHitReactionsFrontList;
        private ReorderableList groundHitReactionsBehindList;
        private ReorderableList airborneHitReactionsFrontList;
        private ReorderableList airborneHitReactionsBehindList;
        private ReorderableList knockbackReactionsList;

        private ReorderableList comboList;

        // PROPERTIES: ----------------------------------------------------------------------------

        

        // INITIALIZER: ---------------------------------------------------------------------------

        private void OnEnable()
        {
            this.instance = this.target as MeleeWeapon;

            this.sectionGeneral = new Section("General", this.LoadIcon("General"), this.Repaint);
            this.sectionModel = new Section("Weapon Model", this.LoadIcon("Sword"), this.Repaint);
            this.sectionEffects = new Section("Effects", this.LoadIcon("Effects"), this.Repaint);

            this.spName = this.serializedObject.FindProperty("weaponName");
            this.spDescription = this.serializedObject.FindProperty("weaponDescription");

            this.spDefaultShield = this.serializedObject.FindProperty("defaultShield");
            this.spCharacterState = this.serializedObject.FindProperty("characterState");
            this.spAvatarMask = this.serializedObject.FindProperty("characterMask");

            this.spPrefab = this.serializedObject.FindProperty("prefab");
            this.spAttachment = this.serializedObject.FindProperty("attachment");
            this.spPosition = this.serializedObject.FindProperty("positionOffset");
            this.spRotation = this.serializedObject.FindProperty("rotationOffset");

            this.spAudioSheathe = this.serializedObject.FindProperty("audioSheathe");
            this.spAudioDraw = this.serializedObject.FindProperty("audioDraw");
            this.spAudioImpactNormal = this.serializedObject.FindProperty("audioImpactNormal");
            this.spAudioImpactKnockback = this.serializedObject.FindProperty("audioImpactKnockback");

            this.spPrefabImpactNormal = this.serializedObject.FindProperty("prefabImpactNormal");
            this.spPrefabImpactKnockback = this.serializedObject.FindProperty("prefabImpactKnockback");

            this.spGroundHitReactionFront = this.serializedObject.FindProperty("groundHitReactionsFront");
            this.spGroundHitReactionBehind = this.serializedObject.FindProperty("groundHitReactionsBehind");
            this.spAirborneHitReactionFront = this.serializedObject.FindProperty("airborneHitReactionsFront");
            this.spAirborneHitReactionBehind = this.serializedObject.FindProperty("airborneHitReactionsBehind");
            this.spKnockbackReactions = this.serializedObject.FindProperty("knockbackReaction");

            this.groundHitReactionsFrontList = new ReorderableList(
                this.serializedObject,
                this.spGroundHitReactionFront,
                true, true, true, true
            );

            this.groundHitReactionsBehindList = new ReorderableList(
                this.serializedObject,
                this.spGroundHitReactionBehind,
                true, true, true, true
            );

            this.airborneHitReactionsFrontList = new ReorderableList(
                this.serializedObject,
                this.spAirborneHitReactionFront,
                true, true, true, true
            );

            this.airborneHitReactionsBehindList = new ReorderableList(
                this.serializedObject,
                this.spAirborneHitReactionBehind,
                true, true, true, true
            );

            this.knockbackReactionsList = new ReorderableList(
                this.serializedObject,
                this.spKnockbackReactions,
                true, true, true, true
            );

            this.groundHitReactionsFrontList.drawHeaderCallback += this.PaintHitGroundFront_Title;
            this.groundHitReactionsBehindList.drawHeaderCallback += this.PaintHitGroundBehind_Title;
            this.airborneHitReactionsFrontList.drawHeaderCallback += this.PaintHitAirFront_Title;
            this.airborneHitReactionsBehindList.drawHeaderCallback += this.PaintHitAirBehind_Title;
            this.knockbackReactionsList.drawHeaderCallback += this.PaintKnockback_Title;

            this.groundHitReactionsFrontList.drawElementCallback += this.PaintHitGroundFront_Element;
            this.groundHitReactionsBehindList.drawElementCallback += this.PaintHitGroundBehind_Element;
            this.airborneHitReactionsFrontList.drawElementCallback += this.PaintHitAirFront_Element;
            this.airborneHitReactionsBehindList.drawElementCallback += this.PaintHitAirBehind_Element;
            this.knockbackReactionsList.drawElementCallback += this.PaintHitKnockback_Element;

            this.spCombos = this.serializedObject.FindProperty("combos");

            this.comboList = new ReorderableList(
                this.serializedObject,
                this.spCombos,
                true, true, true, true
            );

            this.comboList.elementHeight = ComboPD.GetHeight() + PADDING * 2F;
            this.comboList.drawHeaderCallback += this.PaintCombo_Header;
            this.comboList.drawElementBackgroundCallback += this.PaintCombo_ElementBg;
            this.comboList.drawElementCallback += this.PaintCombo_Element;
        }

        // PAINT METHODS: -------------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            this.serializedObject.ApplyModifiedProperties();

            GUILayout.Space(SPACING);
            this.PaintSectionGeneral();

            GUILayout.Space(SPACING);
            this.PaintSectionModel();

            GUILayout.Space(SPACING);
            this.PaintSectionEffects();

            EditorGUILayout.Space();
            this.comboList.DoLayoutList();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Hit Reactions", EditorStyles.boldLabel);

            EditorGUILayout.Space();
            this.groundHitReactionsFrontList.DoLayoutList();

            EditorGUILayout.Space();
            this.groundHitReactionsBehindList.DoLayoutList();

            EditorGUILayout.Space();
            this.airborneHitReactionsFrontList.DoLayoutList();

            EditorGUILayout.Space();
            this.airborneHitReactionsBehindList.DoLayoutList();

            EditorGUILayout.Space();
            this.knockbackReactionsList.DoLayoutList();

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
                    EditorGUILayout.PropertyField(this.spDefaultShield);

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(this.spCharacterState);
                    EditorGUILayout.PropertyField(this.spAvatarMask);

                    EditorGUILayout.EndVertical();
                }
            }
        }

        private void PaintSectionModel()
        {
            this.sectionModel.PaintSection();
            using (var group = new EditorGUILayout.FadeGroupScope(this.sectionModel.state.faded))
            {
                if (group.visible)
                {
                    EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());

                    EditorGUILayout.PropertyField(this.spPrefab);
                    EditorGUILayout.PropertyField(this.spAttachment);

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(this.spPosition);
                    EditorGUILayout.PropertyField(this.spRotation);

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

                    EditorGUILayout.PropertyField(this.spAudioDraw);
                    EditorGUILayout.PropertyField(this.spAudioSheathe);

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(this.spAudioImpactNormal);
                    EditorGUILayout.PropertyField(this.spAudioImpactKnockback);

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(this.spPrefabImpactNormal);
                    EditorGUILayout.PropertyField(this.spPrefabImpactKnockback);

                    EditorGUILayout.EndVertical();
                }
            }
        }

        // HIT REACTIONS: -------------------------------------------------------------------------

        private void PaintHitGroundFront_Title(Rect rect)
        {
            EditorGUI.LabelField(rect, "Hit Reaction - Grounded & Front");
        }

        private void PaintHitGroundBehind_Title(Rect rect)
        {
            EditorGUI.LabelField(rect, "Hit Reaction - Grounded & Behind");
        }

        private void PaintHitAirFront_Title(Rect rect)
        {
            EditorGUI.LabelField(rect, "Hit Reaction - Airborne & Frontal");
        }

        private void PaintHitAirBehind_Title(Rect rect)
        {
            EditorGUI.LabelField(rect, "Hit Reaction - Airborne & Behind");
        }

        private void PaintKnockback_Title(Rect rect)
        {
            EditorGUI.LabelField(rect, "Hit Reaction - Knockback");
        }

        private void PaintHitGroundFront_Element(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect = new Rect(
                rect.x, rect.y + (rect.height - EditorGUIUtility.singleLineHeight) / 2f,
                rect.width, EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(
                rect, this.spGroundHitReactionFront.GetArrayElementAtIndex(index),
                GC_REACTION, true
            );
        }

        private void PaintHitGroundBehind_Element(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect = new Rect(
                rect.x, rect.y + (rect.height - EditorGUIUtility.singleLineHeight) / 2f,
                rect.width, EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(
                rect, this.spGroundHitReactionBehind.GetArrayElementAtIndex(index),
                GC_REACTION, true
            );
        }

        private void PaintHitAirFront_Element(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect = new Rect(
                rect.x, rect.y + (rect.height - EditorGUIUtility.singleLineHeight) / 2f,
                rect.width, EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(
                rect, this.spAirborneHitReactionFront.GetArrayElementAtIndex(index),
                GC_REACTION, true
            );
        }

        private void PaintHitAirBehind_Element(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect = new Rect(
                rect.x, rect.y + (rect.height - EditorGUIUtility.singleLineHeight) / 2f,
                rect.width, EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(
                rect, this.spAirborneHitReactionBehind.GetArrayElementAtIndex(index),
                GC_REACTION, true
            );
        }

        private void PaintHitKnockback_Element(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect = new Rect(
                rect.x, rect.y + (rect.height - EditorGUIUtility.singleLineHeight) / 2f,
                rect.width, EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(
                rect, this.spKnockbackReactions.GetArrayElementAtIndex(index),
                GC_REACTION, true
            );
        }

        // COMBO METHODS: -------------------------------------------------------------------------

        private void PaintCombo_Header(Rect rect)
        {
            EditorGUI.LabelField(rect, "Combo Creator");
        }

        private void PaintCombo_ElementBg(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect = new Rect(rect.x + 2f, rect.y, rect.width - 4f, rect.height);
            if (isActive || isFocused) EditorGUI.DrawRect(rect, ACTIVE_LAYER);
            else if (index % 2 == 0) EditorGUI.DrawRect(rect, ODD_LAYER);
        }

        private void PaintCombo_Element(Rect rect, int index, bool isActive, bool isFocused)
        {
            this.serializedObject.ApplyModifiedProperties();
            this.serializedObject.Update();

            Rect rectCombo = new Rect(
                rect.x,
                rect.y + PADDING,
                rect.width,
                rect.height - (PADDING * 2f)
            );

            EditorGUI.PropertyField(rectCombo, this.spCombos.GetArrayElementAtIndex(index), true);

            this.serializedObject.ApplyModifiedProperties();
            this.serializedObject.Update();
        }
    }
}
