using System;
using System.Collections.Generic;
using UnityEngine;

namespace TEngine
{
    public sealed partial class DebuggerModule : Module
    {
        private sealed class MemoryPoolPoolInformationWindow : ScrollableDebuggerWindowBase
        {
            private readonly Dictionary<string, List<MemoryPoolInfo>> m_MemoryPoolInfos = new Dictionary<string, List<MemoryPoolInfo>>(StringComparer.Ordinal);
            private readonly Comparison<MemoryPoolInfo> m_NormalClassNameComparer = NormalClassNameComparer;
            private readonly Comparison<MemoryPoolInfo> m_FullClassNameComparer = FullClassNameComparer;
            private bool m_ShowFullClassName = false;

            public override void Initialize(params object[] args)
            {
            }

            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>Memory Pool Information</b>");
                GUILayout.BeginVertical("box");
                {
                    DrawItem("Enable Strict Check", MemoryPool.EnableStrictCheck.ToString());
                    DrawItem("Memory Pool Count", MemoryPool.Count.ToString());
                }
                GUILayout.EndVertical();

                m_ShowFullClassName = GUILayout.Toggle(m_ShowFullClassName, "Show Full Class Name");
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
                    GUILayout.Label(Utility.Text.Format("<b>Assembly: {0}</b>", assemblyMemoryPoolInfo.Key));
                    GUILayout.BeginVertical("box");
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(m_ShowFullClassName ? "<b>Full Class Name</b>" : "<b>Class Name</b>");
                            GUILayout.Label("<b>Unused</b>", GUILayout.Width(60f));
                            GUILayout.Label("<b>Using</b>", GUILayout.Width(60f));
                            GUILayout.Label("<b>Acquire</b>", GUILayout.Width(60f));
                            GUILayout.Label("<b>Release</b>", GUILayout.Width(60f));
                            GUILayout.Label("<b>Add</b>", GUILayout.Width(60f));
                            GUILayout.Label("<b>Remove</b>", GUILayout.Width(60f));
                        }
                        GUILayout.EndHorizontal();

                        if (assemblyMemoryPoolInfo.Value.Count > 0)
                        {
                            assemblyMemoryPoolInfo.Value.Sort(m_ShowFullClassName ? m_FullClassNameComparer : m_NormalClassNameComparer);
                            foreach (MemoryPoolInfo memoryPoolInfo in assemblyMemoryPoolInfo.Value)
                            {
                                DrawMemoryPoolInfo(memoryPoolInfo);
                            }
                        }
                        else
                        {
                            GUILayout.Label("<i>Memory Pool is Empty ...</i>");
                        }
                    }
                    GUILayout.EndVertical();
                }
            }

            private void DrawMemoryPoolInfo(MemoryPoolInfo memoryPoolInfo)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(m_ShowFullClassName ? memoryPoolInfo.Type.FullName : memoryPoolInfo.Type.Name);
                    GUILayout.Label(memoryPoolInfo.UnusedMemoryCount.ToString(), GUILayout.Width(60f));
                    GUILayout.Label(memoryPoolInfo.UsingMemoryCount.ToString(), GUILayout.Width(60f));
                    GUILayout.Label(memoryPoolInfo.AcquireMemoryCount.ToString(), GUILayout.Width(60f));
                    GUILayout.Label(memoryPoolInfo.ReleaseMemoryCount.ToString(), GUILayout.Width(60f));
                    GUILayout.Label(memoryPoolInfo.AddMemoryCount.ToString(), GUILayout.Width(60f));
                    GUILayout.Label(memoryPoolInfo.RemoveMemoryCount.ToString(), GUILayout.Width(60f));
                }
                GUILayout.EndHorizontal();
            }

            private static int NormalClassNameComparer(MemoryPoolInfo a, MemoryPoolInfo b)
            {
                return a.Type.Name.CompareTo(b.Type.Name);
            }

            private static int FullClassNameComparer(MemoryPoolInfo a, MemoryPoolInfo b)
            {
                return a.Type.FullName.CompareTo(b.Type.FullName);
            }
        }
    }
}
