namespace GameCreator.Behavior
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;

	[Serializable]
	public class BehaviorWindowBreadcrumb
    {
		// PUBLIC METHODS: ------------------------------------------------------------------------

	    public void Update(BehaviorWindow window, Event currentEvent)
	    {
            GUILayout.BeginArea(new Rect(
                0f, 
                window.position.height - 18f,
                window.position.width,
                18f
            ));

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            int index = 0;
            if (window.stackBehaviorGraphs != null)
            {
                foreach (BehaviorGraph behaviorGraph in window.stackBehaviorGraphs)
                {
                    if (behaviorGraph == null) continue;
                    this.PaintBreadcrumb(
                        index,
                        behaviorGraph.name,
                        behaviorGraph
                    );
                    ++index;
                }
            }

            BehaviorGraph currentGraph = window.behaviorGraph;
            if (currentGraph != null)
            {
                this.PaintBreadcrumb(
                    index, 
                    currentGraph.name,
                    currentGraph, 
                    true
                );
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
		
		// PRIVATE METHODS: -----------------------------------------------------------------------

        private void PaintBreadcrumb(int index, string text, BehaviorGraph graph, bool on = false)
        {
            GUIStyle style = (index == 0
                ? BehaviorStyles.GetBreadcrumbLeft()
                : BehaviorStyles.GetBreadcrumbMid()
            );

            GUIContent content = new GUIContent(ObjectNames.NicifyVariableName(text));
            Rect rect = this.GetBreadcrumbRect(content, style);

            switch (on)
            {
                case true: GUI.Toggle(rect, true, content, style); break;
                case false:
                    if (GUI.Button(rect, content, style))
                    {
                        BehaviorWindow.WINDOW.OpenStackBehaviorGraph(graph);
                    }
                    break;
            }
        }

        private Rect GetBreadcrumbRect(GUIContent text, GUIStyle style)
        {
            Rect rect = GUILayoutUtility.GetRect(text, style);
            rect.xMax += 15;
            return rect;
        }
    }
}