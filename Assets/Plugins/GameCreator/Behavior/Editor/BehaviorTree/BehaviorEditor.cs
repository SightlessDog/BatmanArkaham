namespace GameCreator.Behavior
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using GameCreator.Core;
	using GameCreator.Variables;

	[CustomEditor(typeof(Behavior))]
	public class BehaviorEditor : MultiSubEditor<MBVariableEditor, MBVariable>
	{
        private const string MSG_REPEATED_VARIABLE = "There are two variables with the same name";

        private static readonly GUIContent GC_PARAMS = new GUIContent("Parameters");

        private const string VAR_RUNTIME = "{0} (runtime)";

        private static Color COLOR_SECTION_BACKGR = new Color(0, 0, 0, 0.2f);
        private static Color COLOR_SECTION_BORDER = new Color(0, 0, 0, 0.4f);

        // PROPERTIES: ----------------------------------------------------------------------------

        private Behavior behavior;
		
		private SerializedProperty spBehaviorGraph;
        private SerializedProperty spOnComplete;

        private SerializedProperty spUpdate;
        private SerializedProperty spFrequency;

		private SerializedProperty spReferences;
		
		// INITIALIZERS: --------------------------------------------------------------------------

		private void OnEnable()
		{
			if (target == null) return;
			this.behavior = (Behavior) this.target;
			
			this.spBehaviorGraph = serializedObject.FindProperty("behaviorGraph");
            this.spOnComplete = serializedObject.FindProperty("onComplete");

            this.spUpdate = serializedObject.FindProperty("update");
            this.spFrequency = serializedObject.FindProperty("frequency");

            this.spReferences = serializedObject.FindProperty("references");
		}
		
		private void OnDisable()
		{
			this.CleanSubEditors();
		}

        // INSPECTOR PAINT METHOD: ----------------------------------------------------------------

        public override bool UseDefaultMargins()
        {
            return false;
        }

        public override void OnInspectorGUI()
		{
			if (target == null) return;
			serializedObject.Update();

            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
            this.PaintBehaviorGraph();
            EditorGUILayout.EndVertical();

            this.PaintSection(GC_PARAMS);
			
			this.UpdateSubEditors(this.behavior.references);
			this.SyncBehaviorGraph();
			this.PaintLocalVariables();

            EditorGUILayout.Space();
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
		
		// PAINT METHODS: -------------------------------------------------------------------------

        private void PaintBehaviorGraph()
        {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spBehaviorGraph);

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(this.spOnComplete);
            EditorGUILayout.PropertyField(this.spUpdate);

            if (this.spUpdate.enumValueIndex == (int)Behavior.UpdateTime.SetFrequency)
            {
                EditorGUILayout.PropertyField(this.spFrequency);
            }

            EditorGUI.indentLevel--;
        }

        private void PaintSection(GUIContent content)
		{
            Rect rect = GUILayoutUtility.GetRect(content, BehaviorStyles.GetInspectorSection());
            rect.xMin -= 4f;
            rect.xMax += 4f;

            GUI.DrawTexture(
                rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, 
                false, 1f, COLOR_SECTION_BACKGR, 0f, 0f
            );

            GUI.DrawTexture(
                rect, Texture2D.whiteTexture, ScaleMode.StretchToFill,
                false, 1f, COLOR_SECTION_BORDER, new Vector4(0, 1, 0, 1), 0f
            );

            EditorGUI.LabelField(rect, content, BehaviorStyles.GetInspectorSection());
		}

		private void PaintLocalVariables()
		{
            if (this.spBehaviorGraph.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("No BehaviorGraph found", MessageType.Warning);
                return;
            }

            int referencesLength = this.spReferences.arraySize;
            if (referencesLength == 0)
            {
                EditorGUILayout.HelpBox("No Parameters", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);

            HashSet<string> variables = new HashSet<string>();
            bool repeatedVariableName = false;

            for (int i = 0; i < referencesLength; ++i)
			{
				if (this.subEditors[i] != null)
				{
                    string variableName = this.subEditors[i].spVariableName.stringValue;
                    if (variables.Contains(variableName)) repeatedVariableName = true;
                    else variables.Add(variableName);

                    this.subEditors[i].serializedObject.Update();

					if (EditorApplication.isPlaying)
					{
						this.PaintVariableRuntime(this.subEditors[i]);
					}
					else
					{
						this.PaintVariableEditor(this.subEditors[i]);
					}
					
					this.subEditors[i].serializedObject.ApplyModifiedPropertiesWithoutUndo();
				}
			}

            if (repeatedVariableName)
            {
                EditorGUILayout.HelpBox(MSG_REPEATED_VARIABLE, MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
		}

		private void PaintVariableRuntime(MBVariableEditor editor)
		{
			string label = string.Format(
				VAR_RUNTIME,
				editor.spVariableName.stringValue
			);
			
			object variable = editor.GetRuntimeVariable().Get();
			
			Rect rect = GUILayoutUtility.GetRect(
				EditorGUIUtility.fieldWidth + EditorGUIUtility.fieldWidth,
				EditorGUIUtility.singleLineHeight
			);

			EditorGUI.LabelField(
				rect,
				label, 
				variable == null ? "(null)" : variable.ToString()
			);
		}
		
		private void PaintVariableEditor(MBVariableEditor editor)
		{
			editor.serializedObject.Update();

			GUIContent label = new GUIContent(editor.spVariableName.stringValue);
			switch ((Variable.DataType)editor.spVariableType.intValue)
			{
				case Variable.DataType.String : this.PaintProperty(editor.spVariableStr, label); break;
				case Variable.DataType.Number: this.PaintProperty(editor.spVariableNum, label); break;
				case Variable.DataType.Bool: this.PaintProperty(editor.spVariableBol, label); break;
				case Variable.DataType.Color: this.PaintProperty(editor.spVariableCol, label); break;
				case Variable.DataType.Vector2: this.PaintProperty(editor.spVariableVc2, label); break;
				case Variable.DataType.Vector3: this.PaintProperty(editor.spVariableVc3, label); break;
				case Variable.DataType.Texture2D: this.PaintProperty(editor.spVariableTxt, label); break;
				case Variable.DataType.Sprite: this.PaintProperty(editor.spVariableSpr, label); break;
				case Variable.DataType.GameObject: this.PaintProperty(editor.spVariableObj, label); break;
			}
			
			editor.serializedObject.ApplyModifiedPropertiesWithoutUndo();
		}
		
		// PRIVATE METHODS: -----------------------------------------------------------------------

		private void PaintProperty(SerializedProperty property, GUIContent label)
		{
			Rect rect = GUILayoutUtility.GetRect(
				EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth,
				EditorGUI.GetPropertyHeight(property)
			);
			
			Rect rectLabel = new Rect(
				rect.x,
				rect.y,
				EditorGUIUtility.labelWidth,
				rect.height
			);
			
			Rect rectField = new Rect(
				rectLabel.x + rectLabel.width,
				rect.y,
				rect.width - rectLabel.width,
				rect.height
			);

			EditorGUI.PrefixLabel(rectLabel, label);
			EditorGUI.PropertyField(rectField, property, GUIContent.none);
			GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
		}

		private void SyncBehaviorGraph()
		{
			if (this.spBehaviorGraph.objectReferenceValue == null) return;
			BehaviorGraph behaviorGraph = (BehaviorGraph)this.spBehaviorGraph.objectReferenceValue;

			int referencesSize = this.spReferences.arraySize;
            List<Blackboard.Item> addList = behaviorGraph.GetBlackboardItems();
            List<int> removeList = new List<int>();
			
			for (int i = 0; i < referencesSize; ++i)
			{
				string refName = this.subEditors[i].spVariableName.stringValue;
				int refType = this.subEditors[i].spVariableType.intValue;

				int addListIndex = addList.FindIndex(item => item.name == refName);
				if (addListIndex >= 0)
				{
					if (refType != (int) addList[addListIndex].type)
					{
						int type = (int) addList[addListIndex].type;
						this.subEditors[i].serializedObject.Update();
						this.subEditors[i].spVariableType.intValue = type;
						this.subEditors[i].serializedObject.ApplyModifiedPropertiesWithoutUndo();
					}
					
					addList.RemoveAt(addListIndex);
				}
				else
				{
					removeList.Add(i);
				}
			}

			for (int i = removeList.Count - 1; i >= 0; --i)
			{
				MBVariable source = (MBVariable)this.spReferences
					.GetArrayElementAtIndex(removeList[i])
					.objectReferenceValue;

				this.spReferences.RemoveFromObjectArrayAt(removeList[i]);
				this.RemoveSubEditorsElement(removeList[i]);
				DestroyImmediate(source, true);
			}

			int addListCount = addList.Count;
			for (int i = 0; i < addListCount; ++i)
			{
				name = addList[i].name;
                if (string.IsNullOrEmpty(name)) continue;

				MBVariable variable = this.behavior.gameObject.AddComponent<MBVariable>();
				variable.variable.name = addList[i].name;
				variable.variable.type = (int)addList[i].type;
            
				this.spReferences.AddToObjectArray<MBVariable>(variable);
				this.AddSubEditorElement(variable, -1, true);
			}
		}
	}
}