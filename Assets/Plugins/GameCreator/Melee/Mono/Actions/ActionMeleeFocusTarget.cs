namespace GameCreator.Melee
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using GameCreator.Core;
    using GameCreator.Characters;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    [AddComponentMenu("")]
    public class ActionMeleeFocusTarget : IAction
    {
        public enum Target
        {
            SetTarget,
            ReleaseTarget
        }

        public TargetCharacter character = new TargetCharacter(TargetCharacter.Target.Player);

        [Space]
        public Target target = Target.SetTarget;
        public TargetGameObject value = new TargetGameObject(TargetGameObject.Target.GameObject);


        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            Character _character = this.character.GetCharacter(target);
            if (character == null) return true;

            CharacterMelee melee = _character.GetComponent<CharacterMelee>();
            if (melee != null)
            {
                switch (this.target)
                {
                    case Target.SetTarget:
                        GameObject targetValue = this.value.GetGameObject(target);
                        if (targetValue != null && melee != targetValue)
                        {
                            melee.SetTargetFocus(targetValue.GetComponent<TargetMelee>());
                        }
                        break;

                    case Target.ReleaseTarget:
                        melee.ReleaseTargetFocus();
                        break;
                }
            }

            return true;
        }

#if UNITY_EDITOR

        public static new string NAME = "Melee/Melee Focus Target";
        private const string NODE_TITLE = "{0} on {1}";

        public override string GetNodeTitle()
        {
            return string.Format(
                NODE_TITLE,
                ObjectNames.NicifyVariableName(this.target.ToString()),
                this.character
            );
        }

        private SerializedProperty spCharacter;
        private SerializedProperty spTarget;
        private SerializedProperty spValue;

        protected override void OnEnableEditorChild()
        {
            this.spCharacter = this.serializedObject.FindProperty("character");
            this.spTarget = this.serializedObject.FindProperty("target");
            this.spValue = this.serializedObject.FindProperty("value");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spCharacter);
            EditorGUILayout.PropertyField(this.spTarget);

            EditorGUI.indentLevel++;
            if (this.spTarget.enumValueIndex == (int)Target.SetTarget)
            {
                EditorGUILayout.PropertyField(this.spValue);
            }
            EditorGUI.indentLevel--;

            this.serializedObject.ApplyModifiedProperties();
        }

#endif
    }
}
