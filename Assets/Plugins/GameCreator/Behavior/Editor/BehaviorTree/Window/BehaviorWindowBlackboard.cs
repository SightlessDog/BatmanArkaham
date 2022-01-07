using GameCreator.Variables;

namespace GameCreator.Behavior
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using UnityEditor.AnimatedValues;
	using UnityEditorInternal;

	[Serializable]
	public class BehaviorWindowBlackboard
	{
		private static Color COLOR_SEPARATOR = new Color(0,0,0, 0.3f);
		
		private const float WINDOW_WIDTH = 250f;
		private const float WINDOW_BOTTOM = 46f;
		private const float WNDOW_MARGIN_TOP = 10f;
		
	    // PROPERTIES: ----------------------------------------------------------------------------
		
		private AnimBool isOpen;
		private Vector2 scroll;

		private ReorderableList list;
	    
	    // INITIALIZERS: --------------------------------------------------------------------------

	    public BehaviorWindowBlackboard(BehaviorWindow window)
	    {
		    if (window.behaviorGraphEditor == null ||
			    window.behaviorGraphEditor.target == null ||
		        window.behaviorGraphEditor.serializedObject == null)
		    {
			    return;
		    }
		    
		    this.isOpen = new AnimBool(false);
		    this.isOpen.valueChanged.AddListener(window.Repaint);
		    this.isOpen.speed = 3f;

		    this.scroll = Vector2.zero;
		    this.list = new ReorderableList(
			    window.behaviorGraphEditor.serializedObject,
				window.behaviorGraphEditor.spBlackboardList,
			    true, true, true, true
			);

		    this.list.drawHeaderCallback = DrawHeaderCallback;
		    this.list.drawElementCallback = DrawElementCallback;
		    this.list.elementHeight = EditorGUIUtility.singleLineHeight + 10f;
	    }

		// LIST METHODS: --------------------------------------------------------------------------

		private void DrawHeaderCallback(Rect rect)
		{
			EditorGUI.LabelField(rect, "Parameters", EditorStyles.miniLabel);
		}
		
		private void DrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
		{
			SerializedProperty property = this.list.serializedProperty.GetArrayElementAtIndex(index);
			
			Rect rectName = new Rect(
				rect.x,
				rect.y + (rect.height / 2.0f - EditorGUIUtility.singleLineHeight / 2.0f),
				rect.width / 2.0f,
				EditorGUIUtility.singleLineHeight
			);
			
			Rect rectType = new Rect(
				rectName.x + rectName.width + EditorGUIUtility.standardVerticalSpacing,
				rectName.y,
				rect.width / 2.0f - EditorGUIUtility.standardVerticalSpacing,
				rectName.height
			);

			string name = property.FindPropertyRelative("name").stringValue;
			name = EditorGUI.DelayedTextField(
				rectName, 
				GUIContent.none,
				property.FindPropertyRelative("name").stringValue 
			);

			if (name != property.FindPropertyRelative("name").stringValue)
			{
				property.FindPropertyRelative("name").stringValue =
					VariableEditor.ProcessName(name);
			}
			
			EditorGUI.PropertyField(rectType, property.FindPropertyRelative("type"), GUIContent.none);
		}

		// PUBLIC METHODS: ------------------------------------------------------------------------

		public void Show(bool show)
		{
			this.isOpen.target = show;
		}

		public bool GetShow()
		{
			return this.isOpen.target;
		}
		
	    public void Update(BehaviorWindow window, Event currentEvent)
	    {
		    if (this.isOpen.faded < 0.0001) return;
		    
		    Rect rect = new Rect(
			    (this.isOpen.faded * WINDOW_WIDTH) - WINDOW_WIDTH, 
			    EditorStyles.toolbar.fixedHeight + WNDOW_MARGIN_TOP, 
			    WINDOW_WIDTH, 
			    window.position.height - (WNDOW_MARGIN_TOP + WINDOW_BOTTOM)
			);

		    if (Event.current.type != EventType.Layout)
		    {
			    BehaviorWindowEvents.HOVER_IS_BLACKBOARD = rect.Contains(currentEvent.mousePosition);   
		    }

		    Color color = (EditorGUIUtility.isProSkin
				? BehaviorStyles.COLOR_BG_PRO
				: BehaviorStyles.COLOR_BG_PERSONAL
			); 
		    
		    GUI.DrawTexture(
			    rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 
			    1.0f, color, 0f, 0f
			);
		    
		    GUILayout.BeginArea(rect, BehaviorStyles.GetBlackboard());
		    window.behaviorGraphEditor.serializedObject.Update();
		    
		    this.PaintHeader(window, currentEvent);
		    this.PaintWindow(window, currentEvent);
		    
		    window.behaviorGraphEditor.serializedObject.ApplyModifiedPropertiesWithoutUndo();
			GUILayout.EndArea();
	    }
		
		// PRIVATE METHODS: -----------------------------------------------------------------------

		private void PaintHeader(BehaviorWindow window, Event currentEvent)
		{
			Rect rect = GUILayoutUtility.GetRect(0f, 4000f, 30f, 30f);
			
			GUI.DrawTexture(
				new Rect(rect.x, rect.y + rect.height - 1f, rect.width, 1f), 
				Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 
				1.0f, COLOR_SEPARATOR, 0f, 0f
			);
			
			GUI.Box(rect, "Blackboard", BehaviorStyles.GetBlackboardHeader());
		}
		
		private void PaintWindow(BehaviorWindow window, Event currentEvent)
		{
			this.scroll = EditorGUILayout.BeginScrollView(this.scroll, BehaviorStyles.GetBlackboardBody());
			this.list.DoLayoutList();
			EditorGUILayout.EndScrollView();
		}
	}
}