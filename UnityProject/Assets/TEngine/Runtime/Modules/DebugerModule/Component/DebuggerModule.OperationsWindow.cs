using UnityEngine;

namespace TEngine
{
    public sealed partial class DebuggerModule : Module
    {
        private sealed class OperationsWindow : ScrollableDebuggerWindowBase
        {
            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>Operations</b>");
                GUILayout.BeginVertical("box");
                {
                    ObjectPoolModule objectPoolModule = ModuleSystem.GetModule<ObjectPoolModule>();
                    if (objectPoolModule != null)
                    {
                        if (GUILayout.Button("Object Pool Release", GUILayout.Height(30f)))
                        {
                            objectPoolModule.Release();
                        }

                        if (GUILayout.Button("Object Pool Release All Unused", GUILayout.Height(30f)))
                        {
                            objectPoolModule.ReleaseAllUnused();
                        }
                    }

                    ResourceModule resourceModule = ModuleSystem.GetModule<ResourceModule>();
                    if (resourceModule != null)
                    {
                        if (GUILayout.Button("Unload Unused Assets", GUILayout.Height(30f)))
                        {
                            resourceModule.ForceUnloadUnusedAssets(false);
                        }

                        if (GUILayout.Button("Unload Unused Assets and Garbage Collect", GUILayout.Height(30f)))
                        {
                            resourceModule.ForceUnloadUnusedAssets(true);
                        }
                    }

                    if (GUILayout.Button("Shutdown Game Framework (None)", GUILayout.Height(30f)))
                    {
                        ModuleSystem.Shutdown(ShutdownType.None);
                    }
                    if (GUILayout.Button("Shutdown Game Framework (Restart)", GUILayout.Height(30f)))
                    {
                        ModuleSystem.Shutdown(ShutdownType.Restart);
                    }
                    if (GUILayout.Button("Shutdown Game Framework (Quit)", GUILayout.Height(30f)))
                    {
                        ModuleSystem.Shutdown(ShutdownType.Quit);
                    }
                }
                GUILayout.EndVertical();
            }
        }
    }
}
