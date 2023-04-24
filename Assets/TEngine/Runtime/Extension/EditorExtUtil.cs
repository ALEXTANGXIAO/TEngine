#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TEngine
{
    /// <summary>
    /// 编辑器运行时工具。
    /// </summary>
    public static class EditorExtUtil
    {
        /// <summary>
        /// 游戏物体保存为预制体。
        /// </summary>
        /// <param name="asset">游戏物体。</param>
        public static void SavePrefabAsset(GameObject asset)
        {
#if UNITY_EDITOR
            PrefabUtility.SavePrefabAsset(asset);
#else
#endif
        }

        /// <summary>
        /// 创建资源。
        /// </summary>
        /// <param name="asset">游戏物体。</param>
        /// <param name="path">资源路径。</param>
        public static void CreateAsset(Object asset, string path)
        {
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(asset, path);
#else
#endif
        }

        /// <summary>
        /// 文件路径转化成guid。
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>guid。</returns>
        public static string AssetPathToGUID(string path)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.AssetPathToGUID(path);
#else
            return "";
#endif
        }

        /// <summary>
        /// 获取资源来源预制体。
        /// </summary>
        /// <param name="obj">资源实例。</param>
        /// <returns>来源预制体。</returns>
        public static UnityEngine.Object GetPrefabParent(UnityEngine.Object obj)
        {
#if UNITY_EDITOR
            return UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(obj);
#else
            return null;
#endif
        }

        /// <summary>
        /// 获取资源路径。
        /// </summary>
        /// <param name="obj">资源。</param>
        /// <returns>路径。</returns>
        public static string GetAssetPath(GameObject obj)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetAssetPath(obj);
#else
        return "";
#endif
        }

        /// <summary>
        /// 获取资源路径。
        /// </summary>
        /// <param name="obj">资源。</param>
        /// <returns>路径。</returns>
        public static string GetAssetPath(ScriptableObject obj)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetAssetPath(obj);
#else
        return "";
#endif
        }

        /// <summary>
        /// 获取AB包中的资源依赖。
        /// </summary>
        /// <param name="assetBundleName">The name of the AssetBundle for which dependencies are required.</param>
        /// <param name="recursive">If false, returns only AssetBundles which are direct dependencies of the input; if true, includes all indirect dependencies of the input.</param>
        /// <returns>returns the list of AssetBundles that it depends on.</returns>
        public static string[] GetAssetBundleDependencies(string assetBundleName, bool recursive)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetAssetBundleDependencies(assetBundleName, recursive);
#else
        return null;
#endif
        }

        /// <summary>
        /// guid转化成文件路径。
        /// </summary>
        /// <param name="guid">guid。</param>
        /// <returns>文件路径。</returns>
        public static string GUIDToAssetPath(string guid)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
#else
        return null;
#endif
        }

        /// <summary>
        /// 编辑器下加载资源。
        /// </summary>
        /// <param name="assetPath">资源地址。</param>
        /// <typeparam name="T">资源类型。</typeparam>
        /// <returns>资源实例。</returns>
        public static T LoadAssetAtPath<T>(string assetPath) where T : Object
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
#else
            return null;
#endif
        }

        /// <summary>
        /// 获取资源包名称里的资源列表。
        /// </summary>
        /// <param name="assetBundleName">资源包名称。</param>
        public static string[] GetAssetPathsFromAssetBundle(string assetBundleName)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
#else
            return null;
#endif
        }

        /// <summary>
        /// 开关宏定义。
        /// </summary>
        /// <param name="def">宏定义。</param>
        /// <param name="isOn">是否开启。</param>
        public static void ToggleScriptingDefineSymbols(string def, bool isOn)
        {
#if UNITY_EDITOR
            ToggleScriptingDefineSymbols(def, isOn, (int)BuildTargetGroup.Standalone);
            ToggleScriptingDefineSymbols(def, isOn, (int)BuildTargetGroup.Android);
            ToggleScriptingDefineSymbols(def, isOn, (int)BuildTargetGroup.iOS);
#endif
        }

        /// <summary>
        /// 开关宏定义。
        /// </summary>
        /// <param name="def">宏定义。</param>
        /// <param name="isOn">是否开启。</param>
        /// <param name="type">BuildTargetGroup打包平台类型。</param>
        public static void ToggleScriptingDefineSymbols(string def, bool isOn, int type)
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


        /// <summary>
        /// 设置脏标记。
        /// </summary>
        /// <param name="obj">ScriptableObject。</param>
        public static void SetDirty(this ScriptableObject obj)
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(obj);
#endif
        }

        /// <summary>
        /// 导入资源。
        /// </summary>
        /// <param name="path">资源定位地址。</param>
        public static void ImportAsset(string path)
        {
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.ImportAsset(path);
#endif
        }

        /// <summary>
        /// 把ScriptableObject保存为CSV表格。
        /// </summary>
        /// <param name="config">ScriptableObject。</param>
        public static void SaveTblConfig(this ScriptableObject config)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var path = AssetDatabase.GetAssetPath(config);
            AssetDatabase.ImportAsset(path);
            AssetDatabase.ImportAsset(path.Replace(".asset", ".csv"));
            EditorWindow.focusedWindow?.ShowNotification(new GUIContent("Done"));
#endif
        }


        /// <summary>
        /// 实例化游戏物体操作。
        /// </summary>
        /// <param name="original">来源游戏物体。</param>
        /// <param name="position">实例化的位置。</param>
        /// <param name="rotation">实例化的四元数。</param>
        /// <param name="parent">实例化的父节点。</param>
        /// <returns>实例化游戏物体。</returns>
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

        /// <summary>
        /// 停止运行游戏。
        /// </summary>
        public static void StopGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}