using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TEngine.Editor
{
    [CustomEditor(typeof(MemPoolComponent))]
    internal sealed class MemPoolInspector : TEngineInspector
    {
        private readonly HashSet<string> m_OpenedItems = new HashSet<string>();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
                return;
            }

            MemPoolComponent t = (MemPoolComponent)target;

            if (IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Object Pool Count", t.Count.ToString());

                var objectPools = t.GetAllObjectPools();
                foreach (var objectPool in objectPools)
                {
                    DrawObjectPool(objectPool);
                }
            }

            Repaint();
        }

        private void OnEnable()
        {
        }

        private void DrawObjectPool(IMemPoolBase objectPool)
        {
            bool lastState = m_OpenedItems.Contains(objectPool.GetName());
            bool currentState = EditorGUILayout.Foldout(lastState, objectPool.GetName());
            if (currentState != lastState)
            {
                if (currentState)
                {
                    m_OpenedItems.Add(objectPool.GetName());
                }
                else
                {
                    m_OpenedItems.Remove(objectPool.GetName());
                }
            }

            if (currentState)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Name", objectPool.GetName());
                    EditorGUILayout.LabelField("Type", objectPool.GetName());
                    EditorGUILayout.LabelField("Capacity", objectPool.GetPoolItemCount().ToString());
                    EditorGUILayout.LabelField("Used Count", objectPool.GetPoolItemCount().ToString());
                    EditorGUILayout.LabelField("Can Release Count", objectPool.GetPoolItemCount().ToString());
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Separator();
            }
        }
    }
}