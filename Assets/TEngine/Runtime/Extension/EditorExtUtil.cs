#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TEngine
{
    public static class EditorExtUtil
    {
        public static void SavePrefabAsset(GameObject asset)
        {
#if UNITY_EDITOR
            PrefabUtility.SavePrefabAsset(asset);
#else
#endif
        }

        public static void CreateAsset(Object asset, string path)
        {
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(asset, path);
#else
#endif
        }

        public static string AssetPathToGUID(string path)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.AssetPathToGUID(path);
#else
        return "";
#endif
        }

        public static UnityEngine.Object GetPrefabParent(UnityEngine.Object obj)
        {
#if UNITY_EDITOR
            return UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(obj);
#else
        return null;
#endif
        }

        public static string GetAssetPath(GameObject obj)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetAssetPath(obj);
#else
        return "";
#endif
        }

        public static string GetAssetPath(ScriptableObject obj)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetAssetPath(obj);
#else
        return "";
#endif
        }

        public static string[] GetAssetBundleDependencies(string assetBundleName, bool recursive)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetAssetBundleDependencies(assetBundleName, recursive);
#else
        return null;
#endif
        }

        public static string GUIDToAssetPath(string guid)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
#else
        return null;
#endif
        }

        public static T LoadAssetAtPath<T>(string assetPath) where T : Object
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
#else
        return null;
#endif
        }

        public static string[] GetAssetPathsFromAssetBundle(string assetBundleName)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
#else
        return null;
#endif
        }

        public static void ToggleScriptingDefineSymbols(string def, bool isOn)
        {
#if UNITY_EDITOR
            ToggleScriptingDefineSymbols(def, isOn, (int)BuildTargetGroup.Standalone);
            ToggleScriptingDefineSymbols(def, isOn, (int)BuildTargetGroup.Android);
#endif
        }

        static void ToggleScriptingDefineSymbols(string def, bool isOn, int type)
        {
#if UNITY_EDITOR
            var targetGroup = (BuildTargetGroup)type;
            string ori = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            List<string> defineSymbols = new List<string>(ori.Split(';'));
            if (isOn)
            {
                if (!defineSymbols.Contains(def))
                {
                    defineSymbols.Add(def);
                }
            }
            else
            {
                defineSymbols.Remove(def);
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", defineSymbols.ToArray()));
#endif
        }


        public static void SetDirty(ScriptableObject obj)
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(obj);
#endif
        }

        public static void ImportAsset(string path)
        {
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.ImportAsset(path);
#endif
        }

        public static void SaveTblConfig(ScriptableObject confg)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(confg);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var path = AssetDatabase.GetAssetPath(confg);
            AssetDatabase.ImportAsset(path);
            AssetDatabase.ImportAsset(path.Replace(".asset", ".csv"));
            EditorWindow.focusedWindow?.ShowNotification(new GUIContent("Done"));
#endif
        }


        public static GameObject Instantiate(
            GameObject original,
            Vector3 position,
            Quaternion rotation,
            Transform parent)
        {
            if (original == null) return null;
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                return Object.Instantiate(original, position, rotation, parent);
            }

            var go = PrefabUtility.InstantiatePrefab(original, parent) as GameObject;
            go.transform.position = position;
            go.transform.rotation = rotation;
            return go;
#else
        return  Object.Instantiate( original, position, rotation, parent);
#endif
        }

        public static void StopGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}