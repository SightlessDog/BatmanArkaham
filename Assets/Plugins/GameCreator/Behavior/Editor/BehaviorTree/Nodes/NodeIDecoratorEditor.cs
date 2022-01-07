namespace GameCreator.Behavior
{
	using UnityEngine;
	using UnityEditor;

	[CustomEditor(typeof(NodeIDecorator), true)]
	public class NodeIDecoratorEditor : Editor
	{
        public override void OnInspectorGUI()
        {
            if (this.serializedObject.targetObject != null)
            {
                this.serializedObject.Update();
                SerializedProperty iterator = this.serializedObject.GetIterator();
                bool enterChildren = true;

                while (iterator.NextVisible(enterChildren))
                {
                    enterChildren = false;

                    if ("m_Script" == iterator.propertyPath) continue;
                    if ("isExpanded" == iterator.propertyPath) continue;
                    EditorGUILayout.PropertyField(iterator, true, new GUILayoutOption[0]);
                }

                this.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}