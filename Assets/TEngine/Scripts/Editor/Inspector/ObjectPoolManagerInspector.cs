using TEngine.Runtime;
using UnityEditor;
using UnityEngine;

namespace TEngine.Editor
{
    [CustomEditor(typeof(GameObjectPoolManager))]
    internal sealed class ObjectPoolManagerInspector : TEngineInspector
    {
        public override void OnInspectorGUI()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
                return;
            }
            
            if (GameObjectPoolManager.Instance.Helper.ObjectPools.Count == 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("No Runtime Data!");
                GUILayout.EndHorizontal();
            }

            foreach (var pool in GameObjectPoolManager.Instance.Helper.ObjectPools)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label(pool.Key + ": " + pool.Value.Count);
                GUILayout.FlexibleSpace();
                GUI.enabled = pool.Value.Count > 0;
                if (GUILayout.Button("Clear", EditorStyles.miniButton))
                {
                    pool.Value.Clear();
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
        }
    }
}