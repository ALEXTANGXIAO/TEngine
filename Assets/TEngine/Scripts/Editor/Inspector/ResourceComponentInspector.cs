using System.Reflection;
using TEngine.Runtime;
using UnityEditor;

namespace TEngine.Editor
{
    [CustomEditor(typeof(ResourceComponent))]
    internal sealed class ResourceComponentInspector :TEngineInspector
    {
        private static readonly string[] ResourceModeNames = new string[] { "Package", "Updatable", "Updatable While Playing" };
        
        private SerializedProperty m_ResourceMode = null;
        
        private HelperInfo<ResourceHelperBase> m_ResourceHelperInfo = new HelperInfo<ResourceHelperBase>("Resource");
        
        private int m_ResourceModeIndex = 0;
        
        
        private void OnEnable()
        {
            m_ResourceMode = serializedObject.FindProperty("ResourceMode");
            
            m_ResourceHelperInfo.Init(serializedObject);

            RefreshModes();
            RefreshTypeNames();
        }
        
        private void RefreshModes()
        {
            m_ResourceModeIndex = m_ResourceMode.enumValueIndex > 0 ? m_ResourceMode.enumValueIndex - 1 : 0;
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            serializedObject.Update();

            ResourceComponent t = (ResourceComponent)target;
            
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
                {
                    EditorGUILayout.EnumPopup("Resource Mode", t.ResourceMode);
                }
                else
                {
                    int selectedIndex = EditorGUILayout.Popup("Resource Mode", m_ResourceModeIndex, ResourceModeNames);
                    if (selectedIndex != m_ResourceModeIndex)
                    {
                        m_ResourceModeIndex = selectedIndex;
                        m_ResourceMode.enumValueIndex = selectedIndex + 1;
                    }
                }
            }
            EditorGUI.EndDisabledGroup();
            
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                m_ResourceHelperInfo.Draw();
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

        private void RefreshTypeNames()
        {
            m_ResourceHelperInfo.Refresh();
            serializedObject.ApplyModifiedProperties();
        }
    }
}