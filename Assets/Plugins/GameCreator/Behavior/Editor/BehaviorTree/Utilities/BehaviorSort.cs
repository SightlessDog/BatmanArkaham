using System.Runtime.CompilerServices;
namespace GameCreator.Behavior
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class BehaviorSort
    {
        public class Data
        {
            public int nodeID;

            public float nodeWidth;
            public float nodeHeight;

            public float blockWidth;
            public float blockHeight;

            public Vector2 position;

            public Data parent;
            public List<Data> children;

            public Data(Node node, float width, float height, Data parent)
            {
                this.nodeID = node.GetInstanceID();

                this.nodeWidth = width;
                this.nodeHeight = height;

                this.blockWidth = 0f;
                this.blockHeight = 0f;

                this.position = Vector2.zero;

                this.parent = parent;
                this.children = new List<Data>();
            }
        }

        private const float LEVEL_SEPARATION = 50f;
        private const float SIBLING_SEPARATION = 20f;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static List<Data> Sort(Node node, BehaviorGraphEditor editor)
        {
            if (node == null) return new List<Data>();

            Data root = BuildData(editor, node, null);
            CalculateSizes(root);
            CalculatePositions(root, 0f);

            Stack<Data> candidates = new Stack<Data>();
            candidates.Push(root);

            List<Data> list = new List<Data>();
            while (candidates.Count > 0)
            {
                Data data = candidates.Pop();
                list.Add(data);

                for (int i = 0; i < data.children.Count; ++i)
                {
                    candidates.Push(data.children[i]);
                }
            }

            return list;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static Data BuildData(BehaviorGraphEditor editor, Node node, Data parent)
        {
            Data data = new Data(
                node,
                GetWidth(editor, node),
                GetHeight(editor, node),
                parent
            );

            int count = node.outputs.Count;
            for (int i = 0; i < count; ++i)
            {
                Data currChild = BuildData(editor, node.outputs[i], data);
                data.children.Add(currChild);
            }

            return data;
        }

        private static float CalculateSizes(Data data)
        {
            float childrenWidth = 0.0f;
            int childrenCount = data.children.Count;

            for (int i = 0; i < childrenCount; ++i)
            {
                childrenWidth += CalculateSizes(data.children[i]);
            }

            data.blockWidth = Mathf.Max(
                data.nodeWidth, 
                childrenWidth + (SIBLING_SEPARATION * (childrenCount - 1))
            );

            return data.blockWidth;
        }

        private static float CalculatePositions(Data data, float offset)
        {
            data.position = new Vector2(
                (data.blockWidth / 2f) - (data.nodeWidth / 2f) + offset,
                (data.parent == null ? 0f : data.parent.blockHeight)
            );

            data.blockHeight = data.position.y + data.nodeHeight + LEVEL_SEPARATION;
            int childrenCount = data.children.Count;
            float avgCenters = 0f;

            for (int i = 0; i < childrenCount; ++i)
            {
                offset += CalculatePositions(
                    data.children[i], 
                    offset + (SIBLING_SEPARATION * i)
                );

                avgCenters += (
                    data.children[i].position.x + 
                    (data.children[i].nodeWidth / 2f)
                );
            }

            if (childrenCount > 0)
            {
                data.position.x = (avgCenters / childrenCount);
                data.position.x -= (data.nodeWidth / 2f);
            }

            return data.blockWidth;
        }

        // UTILITY METHODS: -----------------------------------------------------------------------

        private static float GetWidth(BehaviorGraphEditor editor, Node node)
        {
            int nodeID = node.GetInstanceID();
            return editor.nodeEditors[nodeID].GetNodeWidth();
        }

        private static float GetHeight(BehaviorGraphEditor editor, Node node)
        {
            int nodeID = node.GetInstanceID();
            return editor.nodeEditors[nodeID].GetNodeHeight();
        }
    }
}
