using UnityEditor;
using UnityEngine;

namespace TEngine.Editor.Resource
{
    /// <summary>
    /// 资源引用编辑器拓展。
    /// </summary>
    [CustomEditor(typeof(AssetReference), true)]
    public class AssetReferenceEditor : UnityEditor.Editor
    {
        private AssetReference _target;

        private void OnEnable()
        {
            _target = (AssetReference)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.EndHorizontal();
        }
    }
}