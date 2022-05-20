using System;
using TEngine;
using UnityEditor;
using UnityEngine;

namespace TEngine.Editor
{
    [CustomEditor(typeof(AssetTag), true)]
    public class AssetTagEditor : UnityEditor.Editor
    {
        private AssetTag _target;
        private void OnEnable()
        {
            _target = (AssetTag)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("AssetPath", _target.Path);
            if (GUILayout.Button("定位资源", GUILayout.Width(68)))
            {
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>($"{AssetConfig.AssetRootPath}/{_target.Path}");
                if (obj)
                {
                    EditorGUIUtility.PingObject(obj);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }

}