using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TEngine.Editor.Inspector
{
    [CustomEditor(typeof(MemoryPoolModule))]
    internal sealed class MemoryPoolModuleInspector : GameFrameworkInspector
    {
        private readonly Dictionary<string, List<MemoryPoolInfo>> m_MemoryPoolInfos = new Dictionary<string, List<MemoryPoolInfo>>(StringComparer.Ordinal);
        private readonly HashSet<string> m_OpenedItems = new HashSet<string>();

        private SerializedProperty m_EnableStrictCheck = null;

        private bool m_ShowFullClassName = false;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            MemoryPoolModule t = (MemoryPoolModule)target;

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                bool enableStrictCheck = EditorGUILayout.Toggle("Enable Strict Check", t.EnableStrictCheck);
                if (enableStrictCheck != t.EnableStrictCheck)
                {
                    t.EnableStrictCheck = enableStrictCheck;
                }

                EditorGUILayout.LabelField("Memory Pool Count", MemoryPool.Count.ToString());
                m_ShowFullClassName = EditorGUILayout.Toggle("Show Full Class Name", m_ShowFullClassName);
                m_MemoryPoolInfos.Clear();
                MemoryPoolInfo[] memoryPoolInfos = MemoryPool.GetAllMemoryPoolInfos();
                foreach (MemoryPoolInfo memoryPoolInfo in memoryPoolInfos)
                {
                    string assemblyName = memoryPoolInfo.Type.Assembly.GetName().Name;
                    List<MemoryPoolInfo> results = null;
                    if (!m_MemoryPoolInfos.TryGetValue(assemblyName, out results))
                    {
                        results = new List<MemoryPoolInfo>();
                        m_MemoryPoolInfos.Add(assemblyName, results);
                    }

                    results.Add(memoryPoolInfo);
                }

                foreach (KeyValuePair<string, List<MemoryPoolInfo>> assemblyMemoryPoolInfo in m_MemoryPoolInfos)
                {
                    bool lastState = m_OpenedItems.Contains(assemblyMemoryPoolInfo.Key);
                    bool currentState = EditorGUILayout.Foldout(lastState, assemblyMemoryPoolInfo.Key);
                    if (currentState != lastState)
                    {
                        if (currentState)
                        {
                            m_OpenedItems.Add(assemblyMemoryPoolInfo.Key);
                        }
                        else
                        {
                            m_OpenedItems.Remove(assemblyMemoryPoolInfo.Key);
                        }
                    }

                    if (currentState)
                    {
                        EditorGUILayout.BeginVertical("box");
                        {
                            EditorGUILayout.LabelField(m_ShowFullClassName ? "Full Class Name" : "Class Name", "Unused\tUsing\tAcquire\tRelease\tAdd\tRemove");
                            assemblyMemoryPoolInfo.Value.Sort(Comparison);
                            foreach (MemoryPoolInfo memoryPoolInfo in assemblyMemoryPoolInfo.Value)
                            {
                                DrawMemoryPoolInfo(memoryPoolInfo);
                            }

                            if (GUILayout.Button("Export CSV Data"))
                            {
                                string exportFileName = EditorUtility.SaveFilePanel("Export CSV Data", string.Empty, Utility.Text.Format("Memory Pool Data - {0}.csv", assemblyMemoryPoolInfo.Key), string.Empty);
                                if (!string.IsNullOrEmpty(exportFileName))
                                {
                                    try
                                    {
                                        int index = 0;
                                        string[] data = new string[assemblyMemoryPoolInfo.Value.Count + 1];
                                        data[index++] = "Class Name,Full Class Name,Unused,Using,Acquire,Release,Add,Remove";
                                        foreach (MemoryPoolInfo memoryPoolInfo in assemblyMemoryPoolInfo.Value)
                                        {
                                            data[index++] = Utility.Text.Format("{0},{1},{2},{3},{4},{5},{6},{7}", memoryPoolInfo.Type.Name, memoryPoolInfo.Type.FullName, memoryPoolInfo.UnusedMemoryCount.ToString(), memoryPoolInfo.UsingMemoryCount.ToString(), memoryPoolInfo.AcquireMemoryCount.ToString(), memoryPoolInfo.ReleaseMemoryCount.ToString(), memoryPoolInfo.AddMemoryCount.ToString(), memoryPoolInfo.RemoveMemoryCount.ToString());
                                        }

                                        File.WriteAllLines(exportFileName, data, Encoding.UTF8);
                                        Debug.Log(Utility.Text.Format("Export memory pool CSV data to '{0}' success.", exportFileName));
                                    }
                                    catch (Exception exception)
                                    {
                                        Debug.LogError(Utility.Text.Format("Export memory pool CSV data to '{0}' failure, exception is '{1}'.", exportFileName, exception.ToString()));
                                    }
                                }
                            }
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.Separator();
                    }
                }
            }
            else
            {
                EditorGUILayout.PropertyField(m_EnableStrictCheck);
            }

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        private void OnEnable()
        {
            m_EnableStrictCheck = serializedObject.FindProperty("m_EnableStrictCheck");
        }

        private void DrawMemoryPoolInfo(MemoryPoolInfo memoryPoolInfo)
        {
            EditorGUILayout.LabelField(m_ShowFullClassName ? memoryPoolInfo.Type.FullName : memoryPoolInfo.Type.Name, Utility.Text.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", memoryPoolInfo.UnusedMemoryCount.ToString(), memoryPoolInfo.UsingMemoryCount.ToString(), memoryPoolInfo.AcquireMemoryCount.ToString(), memoryPoolInfo.ReleaseMemoryCount.ToString(), memoryPoolInfo.AddMemoryCount.ToString(), memoryPoolInfo.RemoveMemoryCount.ToString()));
        }

        private int Comparison(MemoryPoolInfo a, MemoryPoolInfo b)
        {
            if (m_ShowFullClassName)
            {
                return a.Type.FullName.CompareTo(b.Type.FullName);
            }
            else
            {
                return a.Type.Name.CompareTo(b.Type.Name);
            }
        }
    }
}
