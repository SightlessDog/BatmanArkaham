using GameCreator.Characters;

namespace GameCreator.Behavior
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;

	public static class BehaviorStyles
	{
		private static readonly Color COLOR_LIGHT_TEXT = new Color(0.85f, 0.85f, 0.85f);
		private static readonly Color COLOR_DARK_TEXT = new Color(0.2f, 0.2f, 0.2f);
		
		public static readonly Color COLOR_BG_PRO = new Color(0.22f, 0.22f, 0.22f);
		public static readonly Color COLOR_BG_PERSONAL = new Color(0.76f, 0.76f, 0.76f);
		
		// PROPERTIES: ----------------------------------------------------------------------------

		private static GUIStyle NODE_HEAD = null;
		private static GUIStyle NODE_INPUT = null;
		
        private static GUIStyle BLACKBOARD = null;
		private static GUIStyle BLACKBOARD_HEAD = null;
		private static GUIStyle BLACKBOARD_BODY = null;

        private static GUIStyle SECTION = null;
        private static GUIStyle NODE_CONDITION = null;
        private static GUIStyle NODE_ACTION = null;

        private static GUIStyle BREADCRUMB_LEFT = null;
        private static GUIStyle BREADCRUMB_MID = null;

        // PUBLIC METHODS: -----------------------------------------------------------------------

        public static GUIStyle GetNodeHead()
		{
			if (NODE_HEAD == null)
			{
				NODE_HEAD = new GUIStyle(EditorStyles.label);
				NODE_HEAD.normal.textColor = COLOR_LIGHT_TEXT;
				NODE_HEAD.alignment = TextAnchor.MiddleCenter;
			}

			return NODE_HEAD;
		}
		
		public static GUIStyle GetNodeInput()
		{
			if (NODE_INPUT == null)
			{
				NODE_INPUT = new GUIStyle(EditorStyles.miniLabel);
				NODE_INPUT.normal.textColor = COLOR_DARK_TEXT;
				NODE_INPUT.clipping = TextClipping.Overflow;
				NODE_INPUT.alignment = TextAnchor.MiddleCenter;
				NODE_INPUT.fontStyle = FontStyle.Bold;
			}

			return NODE_INPUT;
		}

		public static GUIStyle GetBlackboard()
		{
			if (BLACKBOARD == null)
			{
				BLACKBOARD = new GUIStyle();
			}

			return BLACKBOARD;
		}

		public static GUIStyle GetBlackboardHeader()
		{
			if (BLACKBOARD_HEAD == null)
			{
				BLACKBOARD_HEAD = new GUIStyle(EditorStyles.label);
				BLACKBOARD_HEAD.alignment = TextAnchor.MiddleCenter;
				BLACKBOARD_HEAD.fontStyle = FontStyle.Bold;
				
				Texture2D texture = new Texture2D(1,1);
				texture.SetPixel(0, 0, new Color(256, 256, 256, 0.1f));
				texture.Apply();
				
				BLACKBOARD_HEAD.normal.background = texture;
			}

			return BLACKBOARD_HEAD;
		}
		
		public static GUIStyle GetBlackboardBody()
		{
			if (BLACKBOARD_BODY == null)
			{
				BLACKBOARD_BODY = new GUIStyle();
				BLACKBOARD_BODY.padding = new RectOffset(4,4,4,4);
			}

			return BLACKBOARD_BODY;
		}

        public static GUIStyle GetInspectorSection()
        {
            if (SECTION == null)
            {
                SECTION = new GUIStyle(EditorStyles.boldLabel);
                SECTION.alignment = TextAnchor.MiddleLeft;
                SECTION.padding = new RectOffset(16, 4, 4, 4);
            }

            return SECTION;
        }

        public static GUIStyle GetNodeCondition()
        {
            if (NODE_CONDITION == null)
            {
                NODE_CONDITION = new GUIStyle(EditorStyles.label);
                NODE_CONDITION.normal.textColor = COLOR_LIGHT_TEXT;
                NODE_CONDITION.richText = true;
                NODE_CONDITION.alignment = TextAnchor.MiddleLeft;
                NODE_CONDITION.padding = new RectOffset(5, 5, 3, 3);
            }

            return NODE_CONDITION;
        }

        public static GUIStyle GetNodeAction()
        {
            if (NODE_ACTION == null)
            {
                NODE_ACTION = new GUIStyle(EditorStyles.label);
                NODE_ACTION.normal.textColor = COLOR_LIGHT_TEXT;
                NODE_ACTION.richText = true;
                NODE_ACTION.alignment = TextAnchor.MiddleLeft;
                NODE_ACTION.padding = new RectOffset(5,5,3,3);
            }

            return NODE_ACTION;
        }

        public static GUIStyle GetBreadcrumbLeft()
        {
            if (BREADCRUMB_LEFT == null)
            {
                GUIStyle breadcrumb = GUI.skin.GetStyle("GUIEditor.BreadcrumbLeft");
                BREADCRUMB_LEFT = new GUIStyle(EditorStyles.toolbarButton);

                BREADCRUMB_LEFT.margin = new RectOffset(0, 0, 0, 0);
                BREADCRUMB_LEFT.padding = new RectOffset(0, 15, 0, 0);
                BREADCRUMB_LEFT.border = breadcrumb.border;

                BREADCRUMB_LEFT.normal = breadcrumb.normal;
                BREADCRUMB_LEFT.active = breadcrumb.active;
                BREADCRUMB_LEFT.hover = breadcrumb.hover;
                BREADCRUMB_LEFT.focused = breadcrumb.focused;
                BREADCRUMB_LEFT.onNormal = breadcrumb.onNormal;
                BREADCRUMB_LEFT.onActive = breadcrumb.onActive;
                BREADCRUMB_LEFT.onHover = breadcrumb.onHover;
                BREADCRUMB_LEFT.onFocused = breadcrumb.onFocused;
            }

            return BREADCRUMB_LEFT;
        }

        public static GUIStyle GetBreadcrumbMid()
        {
            if (BREADCRUMB_MID == null)
            {
                GUIStyle breadcrumb = GUI.skin.GetStyle("GUIEditor.BreadcrumbMid");
                BREADCRUMB_MID = new GUIStyle(EditorStyles.toolbarButton);

                BREADCRUMB_MID.margin = new RectOffset(0, 0, 0, 0);
                BREADCRUMB_MID.padding = new RectOffset(15, 15, 0, 0);
                BREADCRUMB_MID.border = breadcrumb.border;

                BREADCRUMB_MID.normal = breadcrumb.normal;
                BREADCRUMB_MID.active = breadcrumb.active;
                BREADCRUMB_MID.hover = breadcrumb.hover;
                BREADCRUMB_MID.focused = breadcrumb.focused;
                BREADCRUMB_MID.onNormal = breadcrumb.onNormal;
                BREADCRUMB_MID.onActive = breadcrumb.onActive;
                BREADCRUMB_MID.onHover = breadcrumb.onHover;
                BREADCRUMB_MID.onFocused = breadcrumb.onFocused;

            }

            return BREADCRUMB_MID;
        }
    }
}