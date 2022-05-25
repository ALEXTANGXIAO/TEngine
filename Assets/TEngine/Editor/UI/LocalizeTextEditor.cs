using UnityEditor;
using UnityEditor.UI;

namespace TEngine
{
    [CustomEditor(typeof(LocalizeText), true)]
    [CanEditMultipleObjects]
    public class LocalizeTextEditor : TextEditor
    {
        public SerializedProperty Key;

        protected override void OnEnable()
        {
            UnityEngine.Debug.Log(serializedObject.FindProperty("Key"));
            base.OnEnable();
            Key = serializedObject.FindProperty("Key");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            serializedObject.Update();
            EditorGUILayout.PropertyField(Key);
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}
