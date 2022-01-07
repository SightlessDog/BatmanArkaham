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

    [CustomEditor(typeof(StatsAsset))]
    public class StatsAssetEditor : MultiSubEditor<StatAssetEditor, StatAsset>
    {
        private const string MSG_UNDEF_STAT_1 = "This Stat is not set as an instance of an object.";
        private const string MSG_UNDEF_STAT_2 = "Check if you disabled or uninstalled a module that defined it.";

        // PROPERTIES: ----------------------------------------------------------------------------

        private StatsAsset instance;

        public SerializedProperty spStats;
        private EditorSortableList editorSortableList;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void OnEnable()
        {
            if (target == null || serializedObject == null) return;
            this.instance = (StatsAsset)target;

            this.spStats = serializedObject.FindProperty("stats");
            this.editorSortableList = new EditorSortableList();

            this.UpdateSubEditors(this.instance.stats);
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
            this.UpdateSubEditors(this.instance.stats);

            int removeIndex = -1;
            bool forceRepaint = false;

            int spActionsSize = this.spStats.arraySize;
            for (int i = 0; i < spActionsSize; ++i)
            {
                bool forceSortRepaint = this.editorSortableList.CaptureSortEvents(this.handleRect[i], i);
                forceRepaint = forceSortRepaint || forceRepaint;

                GUILayout.BeginVertical();
                if (this.PaintStatHeader(i))
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
                            EditorGUILayout.HelpBox(MSG_UNDEF_STAT_1, MessageType.Warning);
                            EditorGUILayout.HelpBox(MSG_UNDEF_STAT_2, MessageType.None);
                        }

                        EditorGUILayout.EndVertical();
                    }
                }

                GUILayout.EndVertical();

                if (Event.current.type == EventType.Repaint)
                {
                    this.objectRect[i] = GUILayoutUtility.GetLastRect();
                }

                this.editorSortableList.PaintDropPoints(this.objectRect[i], i, spActionsSize);
            }

            if (GUILayout.Button("Create Stat"))
            {
                StatAsset statAsset = DatabaseStatsEditor.AddStatsAsset();
                int addIndex = this.spStats.arraySize;

                this.spStats.InsertArrayElementAtIndex(addIndex);
                this.spStats.GetArrayElementAtIndex(addIndex).objectReferenceValue = statAsset;
                this.AddSubEditorElement(statAsset, addIndex, true);
            }

            if (removeIndex >= 0)
            {
                StatAsset source = (StatAsset)this.spStats.GetArrayElementAtIndex(removeIndex).objectReferenceValue;

                this.spStats.RemoveFromObjectArrayAt(removeIndex);
                this.RemoveSubEditorsElement(removeIndex);
                DestroyImmediate(source, true);
            }

            EditorSortableList.SwapIndexes swapIndexes = this.editorSortableList.GetSortIndexes();
            if (swapIndexes != null)
            {
                this.spStats.MoveArrayElement(swapIndexes.src, swapIndexes.dst);
                this.MoveSubEditorsElement(swapIndexes.src, swapIndexes.dst);
            }

            if (EditorApplication.isPlaying) forceRepaint = true;
            if (forceRepaint) this.Repaint();

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private bool PaintStatHeader(int i)
        {
            bool result = false;

            Rect rectHeader = GUILayoutUtility.GetRect(GUIContent.none, CoreGUIStyles.GetToggleButtonNormalOn());
            this.PaintDragHandle(i, rectHeader);

            string name = (this.isExpanded[i].target ? "▾ " : "▸ ");
            name += (this.instance.stats[i] != null
                     ? this.instance.stats[i].GetNodeTitle()
                : "<i>Undefined Stat</i>"
            );

            GUIStyle style = (this.isExpanded[i].target
                ? CoreGUIStyles.GetToggleButtonMidOn()
                : CoreGUIStyles.GetToggleButtonMidOff()
            );

            Rect rectHide = new Rect(
                rectHeader.x + rectHeader.width - 50f,
                rectHeader.y,
                25f,
                rectHeader.height
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
                rectHeader.width - 75f,
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

            GUIContent gcHide = (this.subEditors[i].spIsHidden.boolValue
                ? ClausesUtilities.Get(ClausesUtilities.Icon.EyeClosed)
                : ClausesUtilities.Get(ClausesUtilities.Icon.EyeOpen)
            );

            if (GUI.Button(rectHide, gcHide, CoreGUIStyles.GetButtonMid()))
            {
                this.subEditors[i].serializedObject.ApplyModifiedProperties();
                this.subEditors[i].serializedObject.Update();

                this.subEditors[i].spIsHidden.boolValue = !this.subEditors[i].spIsHidden.boolValue;

                this.subEditors[i].serializedObject.ApplyModifiedProperties();
                this.subEditors[i].serializedObject.Update();
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