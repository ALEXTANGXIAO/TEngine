using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace TEngineCore.Editor
{
    /// <summary>
    /// 用于游戏中shader自动收集变体
    /// </summary>
    public static class ShaderVariantCollector
    {
        private static UnityEngine.Object _saveAddr;
        private static readonly string _buildPath = "Build";
        private static readonly string _shaderAssetBundleName = "shader_variant";
        private static string _variantPath = $"Assets/VariantCollection.shadervariants";
        /// <summary>
        /// GUI部分
        /// </summary>
        [MenuItem("TEngine/Shader工具|Shader/变体收集")]
        private static void Open()
        {
            var window = EditorWindow.GetWindow(typeof(CustomMessageBox), true, "收集变体") as CustomMessageBox;
            if (window == null) return;

            EditorUtility.ClearProgressBar();
            EditorCoroutines.EditorCoroutine it = null;

            window.Info = "收集项目中使用了的变体";
            window.minSize = new Vector2(600, 150);
            window.maxSize = new Vector2(600, 150);
            window.Show();
            window.OnClose = (button, returnValue) =>
            {
                EditorCoroutines.StopAllCoroutines();
                EditorUtility.ClearProgressBar();
            };

            window.OnGUIFunc = () =>
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("保存地址：", GUILayout.Width(60));
                _saveAddr = EditorGUILayout.ObjectField(_saveAddr, typeof(UnityEngine.Object), true, GUILayout.Width(450));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("开始收集变体"))
                {
                    string tempPath = AssetDatabase.GetAssetPath(_saveAddr);
                    if (string.IsNullOrEmpty(tempPath))
                    {
                        Debug.LogError("请选择路径");
                    }
                    else
                    {
                        GUI.enabled = it == null;
                        it = window.StartCoroutine(ShaderVariantsCollector());
                    }
                }
                GUILayout.EndHorizontal();

                return 0;
            };
        }
        /// <summary>
        /// 收集变体
        /// </summary>
        /// <returns></returns>
        public static IEnumerator ShaderVariantsCollector()
        {
            ClearCurrentShaderVariantCollection();

            var shaderCount1 = GetCurrentShaderVariantCollectionShaderCount();
            var shaderVariantCount1 = GetCurrentShaderVariantCollectionVariantCount();
            Debug.LogErrorFormat("{0} shaders {1} total variants.", shaderCount1, shaderVariantCount1);

            yield return null;

            var scenePath = _GetScenes();
            if (scenePath.Count <= 0)
            {
                yield break;
            }

            var allMaterials = _CollectAllMaterials();
            Debug.LogFormat("========开始遍历场景收集变体========");
            //int index = 0;
            foreach (var item in scenePath)
            {
                EditorSceneManager.OpenScene(item);
                yield return null;
                _CreateProxyRenderers(allMaterials);
                yield return null;
                var shaderCount = GetCurrentShaderVariantCollectionShaderCount();
                var shaderVariantCount = GetCurrentShaderVariantCollectionVariantCount();
                Debug.LogFormat("{0} shaders {1} total variants.", shaderCount, shaderVariantCount);
                yield return new WaitForEndOfFrame();
            }
            Debug.LogFormat("========完成变体收集========");
            yield return new WaitForSeconds(1);
            SaveShaderVariant();
        }


        public static void ShaderVariantsCollector(Action action = null)
        {
            ClearCurrentShaderVariantCollection();
            var scenePath = _GetScenes();
            if (scenePath.Count <= 0)
            {
                return;
            }

            var allMaterials = _CollectAllMaterials();
            Debug.LogFormat("========开始遍历场景收集变体========");
            int index = 0;
            foreach (var item in scenePath)
            {
                EditorSceneManager.OpenScene(item);
                _CreateProxyRenderers(allMaterials);
                var shaderCount = GetCurrentShaderVariantCollectionShaderCount();
                var shaderVariantCount = GetCurrentShaderVariantCollectionVariantCount();
                Debug.LogFormat("{0} shaders {1} total variants.", shaderCount, shaderVariantCount);
                EditorUtility.DisplayProgressBar("进度", item, index / scenePath.Count);
            }
            Debug.LogFormat("========完成变体收集========");
            EditorUtility.ClearProgressBar();
            SaveShaderVariant();
            action?.Invoke();
        }

        static void _CreateProxyRenderers(List<string> materials)
        {
            int totalMaterials = materials.Count;
            var camera = GameObject.FindObjectOfType<Camera>();
            if (camera == null)
            {
                return;
            }

            float aspect = camera.aspect;
            float height = Mathf.Sqrt(totalMaterials / aspect) + 1;
            float width = Mathf.Sqrt(totalMaterials / aspect) * aspect + 1;

            float halfHeight = Mathf.CeilToInt(height / 2f);
            float halfWidth = Mathf.CeilToInt(width / 2f);

            camera.orthographic = true;
            camera.orthographicSize = halfHeight;
            camera.transform.position = new Vector3(0f, 0f, -10f);

            Selection.activeGameObject = camera.gameObject;

            int xMax = (int)(width - 1);

            int x = 0;
            int y = 0;

            for (int i = 0; i < materials.Count; i++)
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(materials[i]);
                var position = new Vector3(x - halfWidth + 1f, y - halfHeight + 1f, 0f);
                CreateSphere(material, position, x, y, i);

                if (x == xMax)
                {
                    x = 0;
                    y++;
                }
                else
                {
                    x++;
                }
            }
        }

        private static void CreateSphere(Material material, Vector3 position, int x, int y, int index)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.GetComponent<Renderer>().material = material;
            go.transform.position = position;
            go.name = string.Format("Sphere_{0}|{1}_{2}|{3}", index, x, y, material.name);
        }

        /// <summary>
        /// 收集所有的材质
        /// </summary>
        /// <returns></returns>
        static List<string> _CollectAllMaterials()
        {
            List<string> material = new List<string>();
            string[] temp = AssetDatabase.FindAssets("t:material");
            for (int i = 0; i < temp.Length; i++)
            {
                material.Add(AssetDatabase.GUIDToAssetPath(temp[i]));
            }

            if (material.Count <= 0)
            {
                Debug.LogError("未收集到材质");
            }

            return material;
        }

        /// <summary>
        /// 收集场景
        /// </summary>
        static List<string> _GetScenes()
        {
            List<string> scenes = new List<string>();
            string[] temp = AssetDatabase.FindAssets("t:scene");
            for (int i = 0; i < temp.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(temp[i]);
                if (path.StartsWith($"Assets/"))
                {
                    scenes.Add(path);
                }
            }

            if (scenes.Count <= 0)
            {
                Debug.LogError("未收集到场景信息");
            }
            return scenes;
        }

        /// <summary>
        /// 重新导入shader，防止shader修改了
        /// </summary>
        [UnityEditor.MenuItem("TEngine/Shader工具|Shader/重新导入shader资源")]
        public static void ReimportAllShaderAssets()
        {
            try
            {
                var all = ShaderUtil.GetAllShaderInfo();
                for (var i = 0; i < all.Length; ++i)
                {
                    var shader = Shader.Find(all[i].name);
                    if (shader == null) continue;
                    if (EditorUtility.DisplayCancelableProgressBar("Reimport Shader...", shader.name, (float)i / all.Length))
                    {
                        break;
                    }
                    var assetPath = AssetDatabase.GetAssetPath(shader);
                    //路径为空或者是内置shader就不处理了
                    if (string.IsNullOrEmpty(assetPath) || _IsUnityDefaultResource(assetPath)) continue;
                    var importer = AssetImporter.GetAtPath(assetPath) as ShaderImporter;
                    if (importer == null) continue;
                    var before = AssetDatabase.GetDependencies(assetPath);
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.DontDownloadFromCacheServer);
                    var after = AssetDatabase.GetDependencies(assetPath);
                    if (!before.SequenceEqual(after))
                    {
                        Debug.LogWarningFormat("Reimport shader: {0} for error dependencies!", shader.name);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        public static HashSet<string> ClollectSharderAndVariant()
        {
            var shaders = new HashSet<string>();
            var all = ShaderUtil.GetAllShaderInfo();
            for (int i = 0; i < all.Length; i++)
            {
                var shader = Shader.Find(all[i].name);
                var assetPath = AssetDatabase.GetAssetPath(shader);
                _CollectShaderDependencies(assetPath, shaders);
            }

            if (File.Exists(_variantPath))
            {
                //AssetImporter importer = AssetImporter.GetAtPath(_variantPath);
                //importer.assetBundleName = _shaderAssetBundleName;
                shaders.Add(_variantPath);
            }

            return shaders;
        }

        internal static string GetShaderVariantAbName()
        {
            return _shaderAssetBundleName;
        }

        /// <summary>
        /// 打包shader，将shader和变体集合打包到同一个ab文件里面
        /// </summary>
        public static void BuildAllShaderAssets()
        {
            HashSet<string> shaders = ClollectSharderAndVariant();
            if (shaders.Count == 0)
            {
                Debug.LogError("No Shader asset in project");
                return;
            }

            string buildPath = $"{Application.dataPath}/{_buildPath}";
            if (!File.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
            }

            BuildTarget target = BuildTarget.NoTarget;
#if UNITY_EDITOR_WIN
            target = BuildTarget.StandaloneWindows64;
#endif

            _BuildShaderAssetBundle(shaders.ToList(), buildPath, target);
        }

        /// <summary>
        /// 获取shader得引用关系
        /// </summary>
        static void _CollectShaderDependencies(string shaderAssetPath, HashSet<string> shaderPaths)
        {
            if (!string.IsNullOrEmpty(shaderAssetPath) && !_IsUnityDefaultResource(shaderAssetPath))
            {
                var deps = AssetDatabase.GetDependencies(shaderAssetPath);
                for (int j = 0; j < deps.Length; j++)
                {
                    if (!string.IsNullOrEmpty(deps[j]) && !_IsUnityDefaultResource(deps[j]) && deps[j].EndsWith(".shader"))
                    {
                        shaderPaths.Add(deps[j]);
                    }
                }

            }
        }

        static void _BuildShaderAssetBundle(List<string> shader, string outputPath, BuildTarget buildTarget)
        {
            outputPath = $"{outputPath}/{EditorUserBuildSettings.activeBuildTarget.ToString()}";
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            var assetBuild = new AssetBundleBuild
            {
                assetBundleName = _shaderAssetBundleName,
                assetBundleVariant = string.Empty,
                assetNames = shader.ToArray()
            };

            var options = BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression;
            BuildPipeline.BuildAssetBundles(outputPath, new[] { assetBuild }, options, buildTarget);
        }

        /// <summary>
        /// 保存收集到的变体
        /// </summary>
        public static void SaveShaderVariant()
        {
            if (File.Exists(_variantPath))
            {
                AssetDatabase.DeleteAsset(_variantPath);
            }

            SaveCurrentShaderVariantCollection(_variantPath);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
            Debug.LogError("保存成功");
        }

        /// <summary>
        /// 是不是unity内置资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        static bool _IsUnityDefaultResource(String path)
        {
            return String.IsNullOrEmpty(path) == false &&
                   (path == "Resources/unity_builtin_extra" ||
                    path == "Library/unity default resources");
        }

        #region 内部方法
        static readonly object[] BoxedEmpty = new object[] { };

        public static void SaveCurrentShaderVariantCollection(String path)
        {
            RflxStaticCall(
                typeof(ShaderUtil),
                "SaveCurrentShaderVariantCollection", new object[] { path });
        }

        internal static void ClearCurrentShaderVariantCollection()
        {
            RflxStaticCall(
                typeof(ShaderUtil),
                "ClearCurrentShaderVariantCollection", null);
        }

        internal static int GetCurrentShaderVariantCollectionShaderCount()
        {
            var shaderCount = RflxStaticCall(
                typeof(ShaderUtil),
                "GetCurrentShaderVariantCollectionShaderCount", null);
            return (int)shaderCount;
        }

        internal static int GetCurrentShaderVariantCollectionVariantCount()
        {
            var shaderVariantCount = RflxStaticCall(
                typeof(ShaderUtil),
                "GetCurrentShaderVariantCollectionVariantCount", null);
            return (int)shaderVariantCount;
        }

        internal static object RflxStaticCall(Type type, String funcName, object[] parameters = null)
        {
            if (type != null)
            {
                var f = type.GetMethod(funcName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                if (f != null)
                {
                    var r = f.Invoke(null, parameters ?? BoxedEmpty);
                    return r;
                }
            }
            Debug.LogErrorFormat("RflxStaticCall( \"{0}\", \"{1}\", {2} ) failed!",
                type != null ? type.FullName : "null", funcName, parameters ?? BoxedEmpty);
            return null;
        }
        #endregion

        #region 编辑器方法
        public class CustomMessageBox : EditorWindow
        {
            public delegate void OnWindowClose(int button, int returnValue);
            public string Info = string.Empty;
            public Func<int> OnGUIFunc;
            public OnWindowClose OnClose;
            public string[] Buttons = null;
            public int ReturnValue;
            int _CloseButton = -1;

            public void OnDestroy()
            {
                if (OnClose != null)
                {
                    try
                    {
                        OnClose(_CloseButton, ReturnValue);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }

            public void OnGUI()
            {
                GUILayout.Space(10);
                if (!string.IsNullOrEmpty(Info))
                {
                    EditorGUILayout.HelpBox(Info, MessageType.None);
                }
                GUILayout.Space(10);
                if (OnGUIFunc != null)
                {
                    ReturnValue = OnGUIFunc();
                }
            }
        }
        #endregion
    }
}