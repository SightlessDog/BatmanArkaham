namespace GameCreator.Stats
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.AnimatedValues;
    using UnityEditorInternal;
    using GameCreator.Core;

    [CustomEditor(typeof(AttrsAsset))]
    public class AttrsAssetEditor : MultiSubEditor<AttrAssetEditor, AttrAsset>
    {
        private const string MSG_UNDEF_ATTRS_1 = "This Attribute is not set as an instance of an object.";
        private const string MSG_UNDEF_ATTRS_2 = "Check if you disabled or uninstalled a module that defined it.";

        // PROPERTIES: ----------------------------------------------------------------------------

        private AttrsAsset instance;

        public SerializedProperty spAttribute;
        private EditorSortableList editorSortableList;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void OnEnable()
        {
            if (target == null || serializedObject == null) return;
            this.instance = (AttrsAsset)target;

            this.spAttribute = serializedObject.FindProperty("attributes");
            this.editorSortableList = new EditorSortableList();

            this.UpdateSubEditors(this.instance.attributes);
        }

        private void OnDisable()
        {
            this.editorSortableList = null;
            this.CleanSubEditors();
        }

        // PAINT METHODS: -------------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            if (target == null || serializedObject == null) return;

            serializedObject.Update();
            this.UpdateSubEditors(this.instance.attributes);

            int removeIndex = -1;
            bool forceRepaint = false;

            int spActionsSize = this.spAttribute.arraySize;
            for (int i = 0; i < spActionsSize; ++i)
            {
                bool forceSortRepaint = this.editorSortableList.CaptureSortEvents(this.handleRect[i], i);
                forceRepaint = forceSortRepaint || forceRepaint;

                GUILayout.BeginVertical();
                if (this.PaintAttrHeader(i))
                {
                    removeIndex = i;
                }

                using (var group = new EditorGUILayout.FadeGroupScope(this.isExpanded[i].faded))
                {
                    if (group.visible)
                    {
                        EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());
                        if (this.subEditors[i] != null) this.subEditors[i].OnInspectorGUI();
                        else
                        {
                            EditorGUILayout.HelpBox(MSG_UNDEF_ATTRS_1, MessageType.Warning);
                            EditorGUILayout.HelpBox(MSG_UNDEF_ATTRS_2, MessageType.None);
                        }

                        EditorGUILayout.EndVertical();
                    }
                }

                GUILayout.EndVertical();

                if (UnityEngine.Event.current.type == EventType.Repaint)
                {
                    this.objectRect[i] = GUILayoutUtility.GetLastRect();
                }

                this.editorSortableList.PaintDropPoints(this.objectRect[i], i, spActionsSize);
            }

            if (GUILayout.Button("Create Attribute"))
            {
                AttrAsset attrAsset = DatabaseStatsEditor.AddAttributeAsset();
                int addIndex = this.spAttribute.arraySize;

                this.spAttribute.InsertArrayElementAtIndex(addIndex);
                this.spAttribute.GetArrayElementAtIndex(addIndex).objectReferenceValue = attrAsset;
                this.AddSubEditorElement(attrAsset, addIndex, true);
            }

            if (removeIndex >= 0)
            {
                AttrAsset source = (AttrAsset)this.spAttribute.GetArrayElementAtIndex(removeIndex).objectReferenceValue;

                this.spAttribute.RemoveFromObjectArrayAt(removeIndex);
                this.RemoveSubEditorsElement(removeIndex);
                DestroyImmediate(source, true);
            }

            EditorSortableList.SwapIndexes swapIndexes = this.editorSortableList.GetSortIndexes();
            if (swapIndexes != null)
            {
                this.spAttribute.MoveArrayElement(swapIndexes.src, swapIndexes.dst);
                this.MoveSubEditorsElement(swapIndexes.src, swapIndexes.dst);
            }

            if (EditorApplication.isPlaying) forceRepaint = true;
            if (forceRepaint) this.Repaint();

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private bool PaintAttrHeader(int i)
        {
            bool result = false;

            Rect rectHeader = GUILayoutUtility.GetRect(GUIContent.none, CoreGUIStyles.GetToggleButtonNormalOn());
            this.PaintDragHandle(i, rectHeader);

            string name = (this.isExpanded[i].target ? "▾ " : "▸ ");
            name += (this.instance.attributes[i] != null
                     ? this.instance.attributes[i].GetNodeTitle()
                : "<i>Undefined Attribute</i>"
            );

            GUIStyle style = (this.isExpanded[i].target
                ? CoreGUIStyles.GetToggleButtonMidOn()
                : CoreGUIStyles.GetToggleButtonMidOff()
            );

            Rect rectDelete = new Rect(
                rectHeader.x + rectHeader.width - 25f,
                rectHeader.y,
                25f,
                rectHeader.height
            );

            Rect rectMain = new Rect(
                rectHeader.x + 25f,
                rectHeader.y,
                rectHeader.width - 50f,
                rectHeader.height
            );

            if (GUI.Button(rectMain, name, style))
            {
                this.ToggleExpand(i);
            }

            GUIContent gcDelete = ClausesUtilities.Get(ClausesUtilities.Icon.Delete);
            if (GUI.Button(rectDelete, gcDelete, CoreGUIStyles.GetButtonRight()))
            {
                result = true;
            }

            return result;
        }

        private void PaintDragHandle(int i, Rect rectHeader)
        {
            Rect handle = new Rect(rectHeader.x, rectHeader.y, 25f, rectHeader.height);
            GUI.Label(handle, "=", CoreGUIStyles.GetButtonLeft());

            if (UnityEngine.Event.current.type == EventType.Repaint)
            {
                this.handleRect[i] = handle;
            }

            EditorGUIUtility.AddCursorRect(this.handleRect[i], MouseCursor.Pan);
        }
    }
}