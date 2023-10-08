using UnityEditor;

namespace TEngine.Editor.Inspector
{
    [CustomEditor(typeof(AudioModule))]
    internal sealed class AudioModuleInspector : GameFrameworkInspector
    {
        private SerializedProperty m_InstanceRoot = null;
        private SerializedProperty m_AudioMixer = null;
        private SerializedProperty m_AudioGroupConfigs = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            AudioModule t = (AudioModule)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                EditorGUILayout.PropertyField(m_InstanceRoot);
                EditorGUILayout.PropertyField(m_AudioMixer);

                EditorGUILayout.PropertyField(m_AudioGroupConfigs, true);
            }
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        protected override void OnCompileComplete()
        {
            base.OnCompileComplete();

            RefreshTypeNames();
        }

        private void OnEnable()
        {
            m_InstanceRoot = serializedObject.FindProperty("m_InstanceRoot");
            m_AudioMixer = serializedObject.FindProperty("m_AudioMixer");
            m_AudioGroupConfigs = serializedObject.FindProperty("m_AudioGroupConfigs");
            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}