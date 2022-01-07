namespace GameCreator.Behavior
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;

	[Serializable]
	public class BehaviorWindowEvents
	{
	    public const float MIN_ZOOM = 1.0f;
		public const float MAX_ZOOM = 5.0f;
	    private const bool GRID_SNAP = true;

	    public static Node HOVER_NODE = null;
		public static Vector2 MOUSE_POSITION = Vector2.zero;
		
		public static bool HOVER_IS_INPUT = false;
		public static bool HOVER_IS_OUTPUT = false;
		
		public static bool HOVER_IS_BLACKBOARD = false;
		public static bool HOVER_IS_TOOLBAR = false;
		
	    public static Node DRAGGED_NODE_INPUT = null;
		public static Node DRAGGED_NODE_OUTPUT = null;
	    
		// PROPERTIES: ----------------------------------------------------------------------------

	    public float zoom { private set; get; }
	    public Vector2 pan { private set; get; }
	    
	    public bool draggingNode { private set; get; }
	    public bool draggingPort { private set; get; }

	    private Vector2[] dragOffsets = new Vector2[0];
	    
	    // INITIALIZERS: --------------------------------------------------------------------------

	    public BehaviorWindowEvents(BehaviorWindow window)
	    {
            this.zoom = MIN_ZOOM;
            this.pan = Vector2.zero;

            if (window.behaviorGraph != null)
            {
                this.zoom = window.behaviorGraph.zoom;
                this.pan = window.behaviorGraph.position;
            }

            this.draggingNode = false;
	        this.draggingPort = false;
	    }
	    
		// PUBLIC METHODS: ------------------------------------------------------------------------

		public void SetZoom(float value)
		{
			this.zoom = Mathf.Clamp(value, MIN_ZOOM, MAX_ZOOM);
		}
		
	    public void Update(BehaviorWindow window, Event currentEvent)
	    {
	        window.wantsMouseMove = true;
            switch (currentEvent.type) {
                case EventType.ScrollWheel:
                    this.UpdateZoom(window, currentEvent);
                    break;
                
                case EventType.MouseDrag:
                    if (currentEvent.button == 0)
                    {
                        if (this.draggingNode) this.UpdateDragNode(window, currentEvent);
                        else if (this.draggingPort) this.UpdateDragPort(window, currentEvent);
                    } 
                    else if (currentEvent.button == 1 || currentEvent.button == 2)
                    {
                        this.UpdatePan(window, currentEvent);
                    }
                    break;
                
                case EventType.MouseDown:
	                this.UpdateMouseDown(window, currentEvent);
	                window.Repaint();
                    break;
                
                case EventType.MouseUp:
	                this.UpdateMouseUp(window, currentEvent);
                    window.Repaint();
                    break;
                
                case EventType.ValidateCommand:
                case EventType.ExecuteCommand:
	                this.UpdateCommands(window, currentEvent);
                    break;
            }

		    if (!HOVER_IS_TOOLBAR && !HOVER_IS_BLACKBOARD)
		    {
			    Vector2 position = currentEvent.mousePosition;
			    MOUSE_POSITION = window.WindowToGridPosition(position);
		    }
		}
	    
	    // UPDATE METHODS: ------------------------------------------------------------------------
	    
	    private void UpdateZoom(BehaviorWindow window, Event currentEvent)
	    {
		    if (HOVER_IS_BLACKBOARD || HOVER_IS_TOOLBAR) return;
		    
	        if (currentEvent.delta.y > 0) this.SetZoom(zoom + 0.1f * zoom);
	        else this.SetZoom(zoom - 0.1f * zoom);

            window.behaviorGraphEditor.UpdateGraph(this.pan, this.zoom);
	        window.Repaint();
	    }

	    private void UpdatePan(BehaviorWindow window, Event currentEvent)
	    {
		    if (HOVER_IS_BLACKBOARD || HOVER_IS_TOOLBAR) return;
		    
	        Vector2 tmpOffset = this.pan;
	        tmpOffset += currentEvent.delta * zoom;
	        tmpOffset.x = Mathf.Round(tmpOffset.x);
	        tmpOffset.y = Mathf.Round(tmpOffset.y);
	        this.pan = tmpOffset;

            window.behaviorGraphEditor.UpdateGraph(this.pan, this.zoom);
            window.Repaint();
	    }

	    private void UpdateDragNode(BehaviorWindow window, Event currentEvent)
	    {
	        Vector2 mousePosition = window.WindowToGridPosition(currentEvent.mousePosition);
	        for (int i = 0; i < Selection.objects.Length; ++i)
	        {
	            if (Selection.objects[i] is Node) 
	            {
	                Node node = Selection.objects[i] as Node;
	                int nodeID = node.GetInstanceID();
	                NodeEditor nodeEditor = window.behaviorGraphEditor.nodeEditors[nodeID];
	                
	                Vector2 nodePosition = mousePosition + dragOffsets[i];
	                
	                if (GRID_SNAP) 
	                {
	                    nodePosition = new Vector2(
                            Mathf.Round(nodePosition.x),
                            Mathf.Round(nodePosition.y)
	                    );
	                }

	                nodeEditor.SetEditorPosition(nodePosition);
		            window.behaviorGraphEditor.RearrangeExecutionIndexes(node.input);
	            }
	        }

	        window.Repaint();
	    }
	    
	    private void UpdateDragPort(BehaviorWindow window, Event currentEvent)
	    {
	        window.Repaint();
	    }

	    private void UpdateMouseDown(BehaviorWindow window, Event currentEvent)
	    {
		    if (currentEvent.button != 0) return;
		    if (HOVER_IS_BLACKBOARD || HOVER_IS_TOOLBAR) return;
	        
		    if (HOVER_IS_OUTPUT)
	        {
	            this.draggingPort = true;
		        DRAGGED_NODE_OUTPUT = HOVER_NODE;
	        }
	        else if (HOVER_NODE != null)
	        {
	            this.draggingNode = true;
	            List<UnityEngine.Object> selection = new List<UnityEngine.Object>();
	            selection.AddRange(Selection.objects);
                            
	            if (!currentEvent.control && !currentEvent.shift && 
	                !currentEvent.command && !selection.Contains(HOVER_NODE))
	            {
	                selection.Clear();
	            }
                            
	            selection.Add(HOVER_NODE);
	            Selection.objects = selection.ToArray();
	        }
                        
	        this.RecalculateDragOffsets(window, currentEvent);
	    }

		private void UpdateMouseUp(BehaviorWindow window, Event currentEvent)
		{
			if (HOVER_IS_BLACKBOARD || HOVER_IS_TOOLBAR)
			{
				this.draggingNode = false;
				this.draggingPort = false;
				return;
			}
			
			if (currentEvent.button == 0)
			{
				if (this.draggingPort)
				{
					if (HOVER_IS_INPUT)
					{
						DRAGGED_NODE_INPUT = HOVER_NODE;
						if (DRAGGED_NODE_INPUT != null && DRAGGED_NODE_OUTPUT != null)
						{
							window.behaviorGraphEditor.CreateConnection(
								DRAGGED_NODE_OUTPUT, 
								DRAGGED_NODE_INPUT
							);
						
							window.behaviorGraphEditor.RearrangeExecutionIndexes(
								DRAGGED_NODE_OUTPUT
							);
						}	
					}
					else
					{
						Node parent = DRAGGED_NODE_OUTPUT;
						window.behaviorGraphEditor.ShowCreateMenu(parent);
					}
				}
				else if (HOVER_NODE == null)
				{
					Selection.objects = new Node[0];
					Selection.activeObject = null;
				}
			}
			else if (currentEvent.button == 1)
			{
                if (HOVER_NODE != null)
                {
                    if (HOVER_IS_INPUT)
                    {
                        window.behaviorGraphEditor.DisconnectInput(HOVER_NODE);
                    }
                    else if (HOVER_IS_OUTPUT)
                    {
                        window.behaviorGraphEditor.DisconnectOutput(HOVER_NODE);
                    }
                    else
                    {
                        window.behaviorGraphEditor.ShowNodeMenu(HOVER_NODE);
                    }
                }
            }
                    
			this.draggingNode = false;
			this.draggingPort = false;
		}

		private void UpdateCommands(BehaviorWindow window, Event currentEvent)
		{
			bool isDeleteCommand = (currentEvent.commandName == "SoftDelete");
			isDeleteCommand |= (
				SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX &&
				currentEvent.commandName == "Delete"
			); 
				
			if (isDeleteCommand) 
			{
				if (currentEvent.type == EventType.ExecuteCommand)
				{
					this.RemoveSelectedNodes(window);
				}
				        
				currentEvent.Use();
				window.Repaint();
			}
		}
	    
	    // PRIVATE METHODS: -----------------------------------------------------------------------
	    
	    private void RecalculateDragOffsets(BehaviorWindow window, Event currentEvent) 
	    {
	        this.dragOffsets = new Vector2[Selection.objects.Length];
	        for (int i = 0; i < Selection.objects.Length; i++) 
	        {
	            if (Selection.objects[i] is Node) 
	            {
	                Node node = Selection.objects[i] as Node;
	                this.dragOffsets[i] = node.position - window.WindowToGridPosition(currentEvent.mousePosition);
	            }
	            else
	            {
	                this.dragOffsets[i] = Vector2.zero;
	            }
	        }
	    }
		
		private void RemoveSelectedNodes(BehaviorWindow window)
		{
			for (int i = Selection.objects.Length - 1; i >= 0; --i)
			{
				if (Selection.objects[i] is Node) 
				{
					Node node = Selection.objects[i] as Node;
					window.behaviorGraphEditor.RemoveNode(node);
				}
			}
		}
	}
}