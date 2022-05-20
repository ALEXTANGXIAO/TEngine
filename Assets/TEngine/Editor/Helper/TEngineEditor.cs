using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TEngine.Editor
{
    public class TEngineEditor
    {
#if UNITY_EDITOR
        internal class EditorMenus
        {
            [UnityEditor.MenuItem("TEngine/Open TEngine Document")]
            public static void OpenTEngineDocument()
            {
                Application.OpenURL("http://1.12.241.46:5000/");
            }

            //[UnityEditor.MenuItem("TEngine/Install TEngineWithToolKits")]
            //public static void InstallPackageKit()
            //{
            //    Application.OpenURL("http://1.12.241.46:9000");
            //}
        }

        public static void LoadData(string filePath, ICollection<string> data)
        {
            try
            {
                FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
                StreamReader streamReader = new StreamReader(fileStream);
                string content = streamReader.ReadLine();
                while (!string.IsNullOrEmpty(content))
                {
                    data.Add(content);
                    content = streamReader.ReadLine();
                }
                streamReader.Close();
            }
            catch (Exception ex)
            {
                Debug.LogError("读取文件失败：" + ex);
            }
        }

        public static void SaveData(string filePath, ICollection<string> data)
        {
            try
            {
                FileStream fileStream = new FileStream(filePath, FileMode.Create);
                StreamWriter streamWriter = new StreamWriter(fileStream);
                foreach (var content in data)
                {
                    streamWriter.WriteLine(content);
                }
                streamWriter.Flush();
                streamWriter.Close();
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogError("写入文件失败：" + ex);
            }
        }

        [MenuItem("Assets/导出Unity资源包", false, 20)]
        static void ExportPackage()
        {
            if (Selection.objects.Length == 0)
            {
                return;
            }

            var assetPaths = new string[Selection.objects.Length];
            for (var i = 0; i < assetPaths.Length; i++)
            {
                assetPaths[i] = AssetDatabase.GetAssetPath(Selection.objects[i]);
            }

            ExportPackage(assetPaths);
        }

        public static void ExportPackage(string[] assetPaths)
        {
            var path = EditorUtility.SaveFilePanel("导出Unity资源包", "", "", "unitypackage");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            assetPaths = AssetDatabase.GetDependencies(assetPaths);
            AssetDatabase.ExportPackage(assetPaths, path, ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
        }
#endif
    }
}
