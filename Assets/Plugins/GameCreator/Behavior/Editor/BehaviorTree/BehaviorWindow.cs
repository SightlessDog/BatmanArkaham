namespace GameCreator.Behavior
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using UnityEngine;
	using UnityEditor;
	using UnityEditor.Callbacks;

    public class BehaviorWindow : EditorWindow
    {
        private const string TITLE = "Behavior Editor";

        public static BehaviorWindow WINDOW { private set; get; }

        // PROPERTIES: ----------------------------------------------------------------------------

        public Behavior runtimeBehavior { private set; get; }
        public BehaviorGraph behaviorGraph { private set; get; }
        public BehaviorGraphEditor behaviorGraphEditor { private set; get; }

        public BehaviorWindowEvents windowEvents { private set; get; }
        public BehaviorWindowToolbar windowToolbar { private set; get; }
        public BehaviorWindowBreadcrumb windowBreadcrumbs { private set; get; }
        public BehaviorWindowBlackboard windowBlackboard { private set; get; }

        public Stack<BehaviorGraph> stackBehaviorGraphs { private set; get; }

        private Func<bool> isDocked;
        private BehaviorGraph tmpBehaviorGraph;

        // INITIALIZERS: --------------------------------------------------------------------------

        [MenuItem("Window/" + TITLE)]
        private static void OpenWindow()
        {
            InitializeWindow();
        }

        [OnOpenAsset(0)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            BehaviorGraph graph = EditorUtility.InstanceIDToObject(instanceID) as BehaviorGraph;
            if (graph != null)
            {
                InitializeWindow(graph);
                return true;
            }

            return false;
        }

        private static void InitializeWindow(Stack<BehaviorGraph> behaviorGraphs)
        {
            WINDOW = GetWindow<BehaviorWindow>(false, TITLE, true);
            WINDOW.wantsMouseMove = true;

            if (behaviorGraphs.Count > 0)
            {
                BehaviorGraph current = behaviorGraphs.Pop();
                WINDOW.behaviorGraph = current;
                WINDOW.stackBehaviorGraphs = behaviorGraphs;
                WINDOW.OnEnable();
            }

            WINDOW.Show();
        }

        private static void InitializeWindow(BehaviorGraph behaviorGraph = null)
        {
            Stack<BehaviorGraph> stack = new Stack<BehaviorGraph>();
            stack.Push(behaviorGraph);
            InitializeWindow(stack);
        }

        private void OnEnable()
        {
            this.behaviorGraphEditor = Editor.CreateEditor(
                this.behaviorGraph,
                typeof(BehaviorGraphEditor)
            ) as BehaviorGraphEditor;

            this.windowEvents = new BehaviorWindowEvents(this);
            this.windowToolbar = new BehaviorWindowToolbar();
            this.windowBreadcrumbs = new BehaviorWindowBreadcrumb();
            this.windowBlackboard = new BehaviorWindowBlackboard(this);
        }

        // PAINT METHODS: -------------------------------------------------------------------------

        private void OnGUI()
        {
            WINDOW = this;
            this.GetRuntimeBehavior();

            Matrix4x4 matrix = GUI.matrix;

            if (this.behaviorGraph == null) return;
            this.windowEvents.Update(this, Event.current);
            int topPadding = (this.IsDocked() ? 19 : 22);

            this.PaintGrid(this.position, this.windowEvents.zoom, this.windowEvents.pan);

            this.BeginZoomView(this.position, this.windowEvents.zoom, topPadding);

            this.PaintNodes();
            this.PaintDragConnection();

            this.EndZoomView(this.position, this.windowEvents.zoom, topPadding);

            this.windowBreadcrumbs.Update(this, Event.current);
            this.windowToolbar.Update(this, Event.current);
            this.windowBlackboard.Update(this, Event.current);

            GUI.matrix = matrix;
            if (EditorApplication.isPlaying)
            {
                this.Repaint();
            }
        }

        private void PaintGrid(Rect rect, float zoom, Vector2 pan)
        {

            rect.position = Vector2.zero;
            Vector2 center = rect.size / 2.0f;
            Texture2D texture = BehaviorResources.GetTexture(
                BehaviorResources.Name.Grid,
                BehaviorResources.Format.LowRes
            );

            float xOffset = -(center.x * zoom + pan.x) / texture.width;
            float yOffset = ((center.y - rect.size.y) * zoom + pan.y) / texture.height;

            Vector2 tileOffset = new Vector2(xOffset, yOffset);

            float tileAmountX = Mathf.Round(rect.size.x * zoom) / texture.width;
            float tileAmountY = Mathf.Round(rect.size.y * zoom) / texture.height;
            Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);

            GUI.DrawTextureWithTexCoords(rect, texture, new Rect(tileOffset, tileAmount));
        }

        private void PaintNodes()
        {
            if (Event.current.type != EventType.Layout)
            {
                BehaviorWindowEvents.HOVER_NODE = null;
                BehaviorWindowEvents.HOVER_IS_INPUT = false;
                BehaviorWindowEvents.HOVER_IS_OUTPUT = false;
            }

            foreach (KeyValuePair<int, NodeEditor> item in this.behaviorGraphEditor.nodeEditors)
            {
                NodeEditor nodeEditor = item.Value;
                if (nodeEditor == null || nodeEditor.node == null) continue;

                Rect nodeRect = nodeEditor.PaintNode(this);

                if (Event.current.type != EventType.Layout)
                {
                    if (nodeRect.Contains(Event.current.mousePosition))
                    {
                        BehaviorWindowEvents.HOVER_NODE = nodeEditor.node;
                    }
                }
            }

            foreach (KeyValuePair<int, NodeEditor> item in this.behaviorGraphEditor.nodeEditors)
            {
                NodeEditor nodeEditor = item.Value;
                if (nodeEditor == null || nodeEditor.node == null) continue;

                Rect inputRect = nodeEditor.PaintNodeInput(this);
                Rect outputRect = nodeEditor.PaintNodeOutput(this);

                if (Event.current.type != EventType.Layout)
                {
                    if (inputRect.Contains(Event.current.mousePosition))
                    {
                        BehaviorWindowEvents.HOVER_IS_INPUT = true;
                        BehaviorWindowEvents.HOVER_NODE = nodeEditor.node;
                    }

                    if (outputRect.Contains(Event.current.mousePosition))
                    {
                        BehaviorWindowEvents.HOVER_IS_OUTPUT = true;
                        BehaviorWindowEvents.HOVER_NODE = nodeEditor.node;
                    }
                }
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void GetRuntimeBehavior()
        {
            for (int i = 0; i < Selection.gameObjects.Length; ++i)
            {
                Behavior behavior = Selection.gameObjects[i].GetComponent<Behavior>();
                if (behavior != null) this.runtimeBehavior = behavior;
            }
        }

        private void BeginZoomView(Rect rect, float zoom, float topPadding)
        {
            GUI.EndClip();

            GUIUtility.ScaleAroundPivot(Vector2.one / zoom, rect.size * 0.5f);
            Vector4 padding = new Vector4(0, topPadding, 0, 0);
            padding *= zoom;

            GUI.BeginClip(new Rect(
                -((rect.width * zoom) - rect.width) * 0.5f,
                -(((rect.height * zoom) - rect.height) * 0.5f) + (topPadding * zoom),
                rect.width * zoom,
                rect.height * zoom)
            );
        }

        private void EndZoomView(Rect rect, float zoom, float topPadding)
        {
            GUIUtility.ScaleAroundPivot(Vector2.one * zoom, rect.size * 0.5f);
            Vector3 offset = new Vector3(
                (((rect.width * zoom) - rect.width) * 0.5f),
                (((rect.height * zoom) - rect.height) * 0.5f) + (-topPadding * zoom) + topPadding,
                0
            );

            GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one);
        }

        private void PaintDragConnection()
        {
            if (!this.windowEvents.draggingPort) return;
            if (BehaviorWindowEvents.DRAGGED_NODE_OUTPUT == null) return;

            int nodeID = BehaviorWindowEvents.DRAGGED_NODE_OUTPUT.GetInstanceID();
            this.behaviorGraphEditor.nodeEditors[nodeID].PaintDragConnection(this);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public Vector2 GridToWindowPositionNoClipped(Vector2 gridPosition)
        {
            Vector2 center = this.position.size * 0.5f;
            float xOffset = (center.x * this.windowEvents.zoom + (this.windowEvents.pan.x + gridPosition.x));
            float yOffset = (center.y * this.windowEvents.zoom + (this.windowEvents.pan.y + gridPosition.y));
            return new Vector2(xOffset, yOffset);
        }

        public Vector2 WindowToGridPosition(Vector2 windowPosition)
        {
            return (
                windowPosition - (this.position.size * 0.5f) -
                (this.windowEvents.pan / this.windowEvents.zoom)
            ) * this.windowEvents.zoom;
        }

        public bool IsDocked()
        {
            if (this.isDocked == null)
            {
                BindingFlags fullBinding = (
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.Static
                );

                MethodInfo isDockedMethod = typeof(BehaviorWindow)
                    .GetProperty("docked", fullBinding)
                    .GetGetMethod(true);

                this.isDocked = (Func<bool>)Delegate.CreateDelegate(
                    typeof(Func<bool>),
                    this,
                    isDockedMethod
                );
            }

            return this.isDocked.Invoke();
        }

        public void OpenBehaviorGraphNode(BehaviorGraph behaviorGraph)
        {
            this.tmpBehaviorGraph = behaviorGraph;
            EditorApplication.update += this.EditorUpdate_OpenBehaviorGraphNode;
        }

        private void EditorUpdate_OpenBehaviorGraphNode()
        {
            EditorApplication.update -= this.EditorUpdate_OpenBehaviorGraphNode;

            if (this.tmpBehaviorGraph == null) return;
            Stack<BehaviorGraph> stack = new Stack<BehaviorGraph>();
            if (this.stackBehaviorGraphs != null)
            {
                stack = new Stack<BehaviorGraph>(this.stackBehaviorGraphs);
            }

            stack.Push(this.behaviorGraph);
            stack.Push(this.tmpBehaviorGraph);
            InitializeWindow(stack);
        }

        public void OpenStackBehaviorGraph(BehaviorGraph behaviorGraph)
        {
            this.tmpBehaviorGraph = behaviorGraph;
            EditorApplication.update += this.EditorUpdate_OpenStackBehaviorGraph;
        }

        private void EditorUpdate_OpenStackBehaviorGraph()
        {
            EditorApplication.update -= this.EditorUpdate_OpenStackBehaviorGraph;
            if (this.tmpBehaviorGraph == null) return;

            Stack<BehaviorGraph> stack = new Stack<BehaviorGraph>();
            if (this.stackBehaviorGraphs != null)
            {
                stack = new Stack<BehaviorGraph>(this.stackBehaviorGraphs);
            }

            while (stack.Count > 0 && stack.Peek() != this.tmpBehaviorGraph) stack.Pop();
            InitializeWindow(stack);
        }
    }
}