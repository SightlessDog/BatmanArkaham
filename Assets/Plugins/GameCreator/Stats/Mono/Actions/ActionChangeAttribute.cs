namespace GameCreator.Stats
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;
    using GameCreator.Variables;

	#if UNITY_EDITOR
	using UnityEditor;
	#endif

	[AddComponentMenu("")]
    public class ActionChangeAttribute : IAction
	{
        public enum Operation
        {
            Set,
            Add,
            Subtract,
            Multiply,
            Divide
        }

        public enum ValueType
        {
            Value,
            Formula
        }

        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);
        public TargetGameObject opponent = new TargetGameObject(TargetGameObject.Target.Invoker);

        [AttributeSelector]
        public AttrAsset attribute;

        public Operation operation = Operation.Add;
        public ValueType valueType = ValueType.Value;

        public NumberProperty amount = new NumberProperty(1f);
        public FormulaAsset formula;

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            if (this.attribute != null)
            {
                Stats componentTarget = this.target.GetGameObject(target).GetComponentInChildren<Stats>();
                if (componentTarget == null) return true;

                string id = this.attribute.attribute.uniqueName;

                float value = 0.0f;
                switch (this.valueType)
                {
                    case ValueType.Value : 
                        value = this.amount.GetValue(target); 
                        break;

                    case ValueType.Formula : 
                        Stats componentOther = this.opponent.GetGameObject(target).GetComponentInChildren<Stats>();
                        value = this.formula.formula.Calculate(0.0f, componentTarget, componentOther);
                        break;
                }

                switch (this.operation)
                {
                    case Operation.Set : componentTarget.SetAttrValue(id, value); break;
                    case Operation.Add: componentTarget.AddAttrValue(id, value); break;
                    case Operation.Subtract: componentTarget.AddAttrValue(id, -value); break;
                    case Operation.Multiply: componentTarget.MultiplyAttrValue(id, value); break;
                    case Operation.Divide: componentTarget.MultiplyAttrValue(id, (1f/value)); break;
                }
            }

            return true;
        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

        #if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Stats/Icons/Actions/";

        public static new string NAME = "Stats/Change Attribute";
        private const string NODE_TITLE = "{0} attribute {1} of {2}";

		// PROPERTIES: ----------------------------------------------------------------------------

		private SerializedProperty spTarget;
        private SerializedProperty spOpponent;
        private SerializedProperty spAttr;
        private SerializedProperty spOperation;
        private SerializedProperty spValueType;

        private SerializedProperty spAmount;
        private SerializedProperty spFormula;

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle()
		{
            return string.Format(
                NODE_TITLE, 
                this.operation.ToString(), 
                (this.attribute == null ? "(none)" : this.attribute.attribute.uniqueName),
                this.target.ToString()
            );
		}

		protected override void OnEnableEditorChild ()
		{
            this.spTarget = this.serializedObject.FindProperty("target");
            this.spOpponent = this.serializedObject.FindProperty("opponent");

            this.spAttr = this.serializedObject.FindProperty("attribute");
            this.spOperation = this.serializedObject.FindProperty("operation");
            this.spValueType = this.serializedObject.FindProperty("valueType");

            this.spAmount = this.serializedObject.FindProperty("amount");
            this.spFormula = this.serializedObject.FindProperty("formula");
		}

		protected override void OnDisableEditorChild ()
		{
            this.spTarget = null;
            this.spOpponent = null;
            this.spAttr = null;
            this.spOperation = null;
            this.spValueType = null;
            this.spAmount = null;
            this.spFormula = null;
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spTarget);
            EditorGUILayout.PropertyField(this.spAttr);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.spOperation);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spValueType);
            EditorGUI.indentLevel++;
            if (this.spValueType.intValue == (int)ValueType.Value)
            {
                EditorGUILayout.PropertyField(this.spAmount);
            }
            else
            {
                EditorGUILayout.PropertyField(this.spFormula);
                EditorGUILayout.PropertyField(this.spOpponent);
            }
            EditorGUI.indentLevel--;

			this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}
