namespace GameCreator.Behavior
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;

	[Serializable]
	public class BehaviorWindowToolbar
	{
		private const float ZOOM_WIDTH = 150f;
		
		private static readonly GUIContent GC_BLACKBOARD = new GUIContent("Blackboard");
		private static readonly GUIContent GC_SORT = new GUIContent("Sort");
		private static readonly GUIContent GC_CREATE = new GUIContent("Create");
	    
		// PUBLIC METHODS: ------------------------------------------------------------------------

	    public void Update(BehaviorWindow window, Event currentEvent)
	    {
		    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

		    this.PaintBlackboard(window, currentEvent);
		    this.PaintZoom(window, currentEvent);
		    this.PaintSort(window, currentEvent);
		    this.PaintCreateNode(window, currentEvent);

            GUILayout.FlexibleSpace();

		    this.PaintFullscreen(window, currentEvent);
		    
		    EditorGUILayout.EndHorizontal();

		    Rect rectToolbar = GUILayoutUtility.GetLastRect();
		    if (Event.current.type != EventType.Layout)
		    {
			    bool isHover = rectToolbar.Contains(currentEvent.mousePosition);
			    BehaviorWindowEvents.HOVER_IS_TOOLBAR = isHover;
		    }
	    }
		
		// PRIVATE METHODS: -----------------------------------------------------------------------

		private void PaintBlackboard(BehaviorWindow window, Event currentEvent)
		{
			bool show = GUILayout.Toggle(
				window.windowBlackboard.GetShow(),
				GC_BLACKBOARD,
				EditorStyles.toolbarButton
			);
			
			window.windowBlackboard.Show(show);
			window.Repaint();
		}
		
		private void PaintZoom(BehaviorWindow window, Event currentEvent)
		{
			float zoom = GUILayout.HorizontalSlider(
				window.windowEvents.zoom,
				BehaviorWindowEvents.MIN_ZOOM,
				BehaviorWindowEvents.MAX_ZOOM,
				GUILayout.Width(ZOOM_WIDTH)
			);

			if (!Mathf.Approximately(zoom, window.windowEvents.zoom))
			{
				window.windowEvents.SetZoom(zoom);
				window.Repaint();
			}
		}
		
		private void PaintSort(BehaviorWindow window, Event currentEvent)
		{
			if (GUILayout.Button(GC_SORT, EditorStyles.toolbarButton))
			{
                List<BehaviorSort.Data> datas = BehaviorSort.Sort(
                    window.behaviorGraph.root, 
                    window.behaviorGraphEditor
                );

                if (datas.Count > 0)
                {
                    Vector2 offset = new Vector2(
                        datas[0].blockWidth / 2f,
                        100f
                    );

                    for (int i = 0; i < datas.Count; ++i)
                    {
                        BehaviorSort.Data data = datas[i];
                        NodeEditor editor = window.behaviorGraphEditor.nodeEditors[data.nodeID];
                        editor.SetEditorPosition(data.position - offset);
                    }
                }
            }
		}

        private void PaintCreateNode(BehaviorWindow window, Event currentEvent)
		{
			if (GUILayout.Button(GC_CREATE, EditorStyles.toolbarPopup))
			{
				window.behaviorGraphEditor.ShowCreateMenu();
			}
		}

		private void PaintFullscreen(BehaviorWindow window, Event currentEvent)
		{
			string text = (window.maximized ? "Minimize" : "Maximize");
			window.maximized = GUILayout.Toggle(
				window.maximized,
				text,
				EditorStyles.toolbarButton
			);
		}
	}
}