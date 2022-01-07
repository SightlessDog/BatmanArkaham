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

        private SerializedProperty spPrefabs;

        private SerializedProperty spAudioSheathe;
        private SerializedProperty spAudioDraw;
        private SerializedProperty spAudioImpactNormal;
        private SerializedProperty spAudioImpactKnockback;

        private SerializedProperty spPrefabImpactNormal;
        private SerializedProperty spPrefabImpactKnockback;

        private SerializedProperty spGroundHitReactionsFrontUpper;
        private SerializedProperty spGroundHitReactionsFrontMiddle;
        private SerializedProperty spGroundHitReactionsFrontLower;

        private SerializedProperty spGroundHitReactionsBackUpper;
        private SerializedProperty spGroundHitReactionsBackMiddle;
        private SerializedProperty spGroundHitReactionsBackLower;

        private SerializedProperty spAirborneHitReactionsFrontUpper;
        private SerializedProperty spAirborneHitReactionsFrontMiddle;
        private SerializedProperty spAirborneHitReactionsFrontLower;

        private SerializedProperty spAirborneHitReactionsBackUpper;
        private SerializedProperty spAirborneHitReactionsBackMiddle;
        private SerializedProperty spAirborneHitReactionsBackLower;

        private SerializedProperty spKnockbackReaction;

        private SerializedProperty spCombos;

        private ReorderableList groundHitReactionsFrontUpper;
        private ReorderableList groundHitReactionsFrontMiddle;
        private ReorderableList groundHitReactionsFrontLower;

        private ReorderableList groundHitReactionsBackUpper;
        private ReorderableList groundHitReactionsBackMiddle;
        private ReorderableList groundHitReactionsBackLower;

        private ReorderableList airborneHitReactionsFrontUpper;
        private ReorderableList airborneHitReactionsFrontMiddle;
        private ReorderableList airborneHitReactionsFrontLower;

        private ReorderableList airborneHitReactionsBackUpper;
        private ReorderableList airborneHitReactionsBackMiddle;
        private ReorderableList airborneHitReactionsBackLower;

        private ReorderableList knockbackReaction;

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

            this.spPrefabs = this.serializedObject.FindProperty("prefabs");

            this.spAudioSheathe = this.serializedObject.FindProperty("audioSheathe");
            this.spAudioDraw = this.serializedObject.FindProperty("audioDraw");
            this.spAudioImpactNormal = this.serializedObject.FindProperty("audioImpactNormal");
            this.spAudioImpactKnockback = this.serializedObject.FindProperty("audioImpactKnockback");

            this.spPrefabImpactNormal = this.serializedObject.FindProperty("prefabImpactNormal");
            this.spPrefabImpactKnockback = this.serializedObject.FindProperty("prefabImpactKnockback");

            spGroundHitReactionsFrontUpper    = this.serializedObject.FindProperty("groundHitReactionsFrontUpper");
            spGroundHitReactionsFrontMiddle   = this.serializedObject.FindProperty("groundHitReactionsFrontMiddle");
            spGroundHitReactionsFrontLower    = this.serializedObject.FindProperty("groundHitReactionsFrontLower");

            spGroundHitReactionsBackUpper     = this.serializedObject.FindProperty("groundHitReactionsBackUpper");
            spGroundHitReactionsBackMiddle    = this.serializedObject.FindProperty("groundHitReactionsBackMiddle");
            spGroundHitReactionsBackLower     = this.serializedObject.FindProperty("groundHitReactionsBackLower");

            spAirborneHitReactionsFrontUpper  = this.serializedObject.FindProperty("airborneHitReactionsFrontUpper");
            spAirborneHitReactionsFrontMiddle = this.serializedObject.FindProperty("airborneHitReactionsFrontMiddle");
            spAirborneHitReactionsFrontLower  = this.serializedObject.FindProperty("airborneHitReactionsFrontLower");

            spAirborneHitReactionsBackUpper   = this.serializedObject.FindProperty("airborneHitReactionsBackUpper");
            spAirborneHitReactionsBackMiddle  = this.serializedObject.FindProperty("airborneHitReactionsBackMiddle");                                                                      
            spAirborneHitReactionsBackLower   = this.serializedObject.FindProperty("airborneHitReactionsBackLower");

            spKnockbackReaction               = this.serializedObject.FindProperty("knockbackReaction");

            //############# Ground Front ###################
            this.groundHitReactionsFrontUpper = new ReorderableList(
                this.serializedObject,
                this.spGroundHitReactionsFrontUpper,
                true, true, true, true
            );

            this.groundHitReactionsFrontMiddle = new ReorderableList(
                this.serializedObject,
                this.spGroundHitReactionsFrontMiddle,
                true, true, true, true
            );

            this.groundHitReactionsFrontLower = new ReorderableList(
                this.serializedObject,
                this.spGroundHitReactionsFrontLower,
                true, true, true, true
            );
            //############# Ground Back ###################
            this.groundHitReactionsBackUpper = new ReorderableList(
                this.serializedObject,
                this.spGroundHitReactionsBackUpper,
                true, true, true, true
            );

            this.groundHitReactionsBackMiddle = new ReorderableList(
                this.serializedObject,
                this.spGroundHitReactionsBackMiddle,
                true, true, true, true
            );

            this.groundHitReactionsBackLower = new ReorderableList(
                this.serializedObject,
                this.spGroundHitReactionsBackLower,
                true, true, true, true
            );
            //############# Air Front ###################
            this.airborneHitReactionsFrontUpper = new ReorderableList(
                this.serializedObject,
                this.spAirborneHitReactionsFrontUpper,
                true, true, true, true
            );

            this.airborneHitReactionsFrontMiddle = new ReorderableList(
                this.serializedObject,
                this.spAirborneHitReactionsFrontMiddle,
                true, true, true, true
            );

            this.airborneHitReactionsFrontLower = new ReorderableList(
                this.serializedObject,
                this.spAirborneHitReactionsFrontLower,
                true, true, true, true
            );
            //############# Ground Back ###################
            this.airborneHitReactionsBackUpper = new ReorderableList(
                this.serializedObject,
                this.spAirborneHitReactionsBackUpper,
                true, true, true, true
            );

            this.airborneHitReactionsBackMiddle = new ReorderableList(
                this.serializedObject,
                this.spAirborneHitReactionsBackMiddle,
                true, true, true, true
            );

            this.airborneHitReactionsBackLower = new ReorderableList(
                this.serializedObject,
                this.spAirborneHitReactionsBackLower,
                true, true, true, true
            );

            //############# knockback ###################
            this.knockbackReaction = new ReorderableList(
                this.serializedObject,
                this.spKnockbackReaction,
                true, true, true, true
            );

            groundHitReactionsFrontUpper        .drawHeaderCallback += this.PaintHitGroundFrontUpper_Title;
            groundHitReactionsFrontMiddle       .drawHeaderCallback += this.PaintHitGroundFrontMiddle_Title;
            groundHitReactionsFrontLower        .drawHeaderCallback += this.PaintHitGroundFrontLower_Title;

            groundHitReactionsBackUpper         .drawHeaderCallback += this.PaintHitGroundBackUpper_Title;
            groundHitReactionsBackMiddle        .drawHeaderCallback += this.PaintHitGroundBackMiddle_Title;
            groundHitReactionsBackLower         .drawHeaderCallback += this.PaintHitGroundBackLower_Title;

            airborneHitReactionsFrontUpper      .drawHeaderCallback += this.PaintHitAirFrontUpper_Title;
            airborneHitReactionsFrontMiddle     .drawHeaderCallback += this.PaintHitAirFrontMiddle_Title;
            airborneHitReactionsFrontLower      .drawHeaderCallback += this.PaintHitAirFrontLower_Title;

            airborneHitReactionsBackUpper       .drawHeaderCallback += this.PaintHitAirBackUpper_Title;
            airborneHitReactionsBackMiddle      .drawHeaderCallback += this.PaintHitAirBackMiddle_Title;
            airborneHitReactionsBackLower       .drawHeaderCallback += this.PaintHitAirBackLower_Title;

            knockbackReaction                   .drawHeaderCallback  += this.PaintHitKnockback_Title;

            groundHitReactionsFrontUpper        .drawElementCallback += this.PaintHitGroundFrontUpper_Element;
            groundHitReactionsFrontMiddle       .drawElementCallback += this.PaintHitGroundFrontMiddle_Element;
            groundHitReactionsFrontLower        .drawElementCallback += this.PaintHitGroundFrontLower_Element;

            groundHitReactionsBackUpper         .drawElementCallback += this.PaintHitGroundBackUpper_Element;
            groundHitReactionsBackMiddle        .drawElementCallback += this.PaintHitGroundBackMiddle_Element;
            groundHitReactionsBackLower         .drawElementCallback += this.PaintHitGroundBackLower_Element;

            airborneHitReactionsFrontUpper      .drawElementCallback += this.PaintHitAirFrontUpper_Element;
            airborneHitReactionsFrontMiddle     .drawElementCallback += this.PaintHitAirFrontMiddle_Element;
            airborneHitReactionsFrontLower      .drawElementCallback += this.PaintHitAirFrontLower_Element;

            airborneHitReactionsBackUpper       .drawElementCallback += this.PaintHitAirBackUpper_Element;
            airborneHitReactionsBackMiddle      .drawElementCallback += this.PaintHitAirBackMiddle_Element;
            airborneHitReactionsBackLower       .drawElementCallback += this.PaintHitAirBackLower_Element;

            knockbackReaction                   .drawElementCallback += this.PaintHitKnockback_Element;


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
            groundHitReactionsFrontUpper.DoLayoutList();
            EditorGUILayout.Space();
            groundHitReactionsFrontMiddle.DoLayoutList();
            EditorGUILayout.Space();
            groundHitReactionsFrontLower.DoLayoutList();

            EditorGUILayout.Space();
            groundHitReactionsBackUpper.DoLayoutList();
            EditorGUILayout.Space();
            groundHitReactionsBackMiddle.DoLayoutList();
            EditorGUILayout.Space();
            groundHitReactionsBackLower.DoLayoutList();

            EditorGUILayout.Space();
            airborneHitReactionsFrontUpper.DoLayoutList();
            EditorGUILayout.Space();
            airborneHitReactionsFrontMiddle.DoLayoutList();
            EditorGUILayout.Space();
            airborneHitReactionsFrontLower.DoLayoutList();

            EditorGUILayout.Space();
            airborneHitReactionsBackUpper.DoLayoutList();
            EditorGUILayout.Space();
            airborneHitReactionsBackMiddle.DoLayoutList();
            EditorGUILayout.Space();
            airborneHitReactionsBackLower.DoLayoutList();

            EditorGUILayout.Space();
            knockbackReaction.DoLayoutList();

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

                    EditorGUILayout.PropertyField(this.spPrefabs);

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
            //Titles------------

        private void PaintHitGroundFrontUpper_Title(Rect rect)
        {
            EditorGUI.LabelField(rect, "Hit Reaction - Grounded Front Upper body");
        }
        private void PaintHitGroundFrontMiddle_Title(Rect rect)
        {
            EditorGUI.LabelField(rect, "Hit Reaction - Grounded & Front Mid body");
        }
        private void PaintHitGroundFrontLower_Title(Rect rect)
        {
            EditorGUI.LabelField(rect, "Hit Reaction - Grounded & Front Lower body");
        }

        private void PaintHitGroundBackUpper_Title(Rect rect)
        {
            EditorGUI.LabelField(rect, "Hit Reaction - Grounded & Behind Upper body");
        }
        private void PaintHitGroundBackMiddle_Title(Rect rect)
        {
            EditorGUI.LabelField(rect, "Hit Reaction - Grounded & Behind Mid body");
        }
        private void PaintHitGroundBackLower_Title(Rect rect)
        {
            EditorGUI.LabelField(rect, "Hit Reaction - Grounded & Behind Lower body");
        }

        private void PaintHitAirFrontUpper_Title(Rect rect)
        {
            EditorGUI.LabelField(rect, "Hit Reaction - Airborne & Frontal Upper body");
        }
        private void PaintHitAirFrontMiddle_Title(Rect rect)
        {
            EditorGUI.LabelField(rect, "Hit Reaction - Airborne & Frontal Mid body");
        }
        private void PaintHitAirFrontLower_Title(Rect rect)
        {
            EditorGUI.LabelField(rect, "Hit Reaction - Airborne & Frontal Upper body");
        }

        private void PaintHitAirBackUpper_Title(Rect rect)
        {
            EditorGUI.LabelField(rect, "Hit Reaction - Airborne & Behind Upper body");
        }
        private void PaintHitAirBackMiddle_Title(Rect rect)
        {
            EditorGUI.LabelField(rect, "Hit Reaction - Airborne & Behind Mid body");
        }
        private void PaintHitAirBackLower_Title(Rect rect)
        {
            EditorGUI.LabelField(rect, "Hit Reaction - Airborne & Behind Upper body");
        }

        private void PaintHitKnockback_Title(Rect rect)
        {
            EditorGUI.LabelField(rect, "Hit Reaction - Knockback");
        }
            //Elements --- Ground Front
        private void PaintHitGroundFrontUpper_Element(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect = new Rect(
                rect.x, rect.y + (rect.height - EditorGUIUtility.singleLineHeight) / 2f,
                rect.width, EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(
                rect, this.spGroundHitReactionsFrontUpper.GetArrayElementAtIndex(index),
                GC_REACTION, true
            );
        }
        private void PaintHitGroundFrontMiddle_Element(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect = new Rect(
                rect.x, rect.y + (rect.height - EditorGUIUtility.singleLineHeight) / 2f,
                rect.width, EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(
                rect, this.spGroundHitReactionsFrontMiddle.GetArrayElementAtIndex(index),
                GC_REACTION, true
            );
        }
        private void PaintHitGroundFrontLower_Element(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect = new Rect(
                rect.x, rect.y + (rect.height - EditorGUIUtility.singleLineHeight) / 2f,
                rect.width, EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(
                rect, this.spGroundHitReactionsFrontLower.GetArrayElementAtIndex(index),
                GC_REACTION, true
            );
        }
        //Elements --- Ground Back
        private void PaintHitGroundBackUpper_Element(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect = new Rect(
                rect.x, rect.y + (rect.height - EditorGUIUtility.singleLineHeight) / 2f,
                rect.width, EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(
                rect, this.spGroundHitReactionsBackUpper.GetArrayElementAtIndex(index),
                GC_REACTION, true
            );
        }
        private void PaintHitGroundBackMiddle_Element(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect = new Rect(
                rect.x, rect.y + (rect.height - EditorGUIUtility.singleLineHeight) / 2f,
                rect.width, EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(
                rect, this.spGroundHitReactionsBackMiddle.GetArrayElementAtIndex(index),
                GC_REACTION, true
            );
        }
        private void PaintHitGroundBackLower_Element(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect = new Rect(
                rect.x, rect.y + (rect.height - EditorGUIUtility.singleLineHeight) / 2f,
                rect.width, EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(
                rect, this.spGroundHitReactionsBackLower.GetArrayElementAtIndex(index),
                GC_REACTION, true
            );
        }

        //Elements --- Air Front
        private void PaintHitAirFrontUpper_Element(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect = new Rect(
                rect.x, rect.y + (rect.height - EditorGUIUtility.singleLineHeight) / 2f,
                rect.width, EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(
                rect, this.spAirborneHitReactionsFrontUpper.GetArrayElementAtIndex(index),
                GC_REACTION, true
            );
        }
        private void PaintHitAirFrontMiddle_Element(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect = new Rect(
                rect.x, rect.y + (rect.height - EditorGUIUtility.singleLineHeight) / 2f,
                rect.width, EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(
                rect, this.spAirborneHitReactionsFrontMiddle.GetArrayElementAtIndex(index),
                GC_REACTION, true
            );
        }
        private void PaintHitAirFrontLower_Element(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect = new Rect(
                rect.x, rect.y + (rect.height - EditorGUIUtility.singleLineHeight) / 2f,
                rect.width, EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(
                rect, this.spAirborneHitReactionsFrontLower.GetArrayElementAtIndex(index),
                GC_REACTION, true
            );
        }

        //Elements --- Ground Back
        private void PaintHitAirBackUpper_Element(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect = new Rect(
                rect.x, rect.y + (rect.height - EditorGUIUtility.singleLineHeight) / 2f,
                rect.width, EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(
                rect, this.spAirborneHitReactionsBackUpper.GetArrayElementAtIndex(index),
                GC_REACTION, true
            );
        }
        private void PaintHitAirBackMiddle_Element(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect = new Rect(
                rect.x, rect.y + (rect.height - EditorGUIUtility.singleLineHeight) / 2f,
                rect.width, EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(
                rect, this.spAirborneHitReactionsBackMiddle.GetArrayElementAtIndex(index),
                GC_REACTION, true
            );
        }
        private void PaintHitAirBackLower_Element(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect = new Rect(
                rect.x, rect.y + (rect.height - EditorGUIUtility.singleLineHeight) / 2f,
                rect.width, EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(
                rect, this.spAirborneHitReactionsBackLower.GetArrayElementAtIndex(index),
                GC_REACTION, true
            );
        }

        //Elements --- knockback
        private void PaintHitKnockback_Element(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect = new Rect(
                rect.x, rect.y + (rect.height - EditorGUIUtility.singleLineHeight) / 2f,
                rect.width, EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(
                rect, this.spKnockbackReaction.GetArrayElementAtIndex(index),
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
