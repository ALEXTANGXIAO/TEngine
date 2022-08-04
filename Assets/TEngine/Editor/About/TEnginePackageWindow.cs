using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine.Networking;

namespace TEngine.Editor
{
    public class PackageManagerInternal : EditorWindow
    {
        private const string OnlineUrl = "http://1.12.241.46:8081/TEngine/";

        [MenuItem("TEngine/模块商店|在线Package Manager", priority = 1500)]
        private static void Open()
        {
            var window = GetWindow<PackageManagerInternal>("在线模块商店|Package Manager");
            window.minSize = new Vector2(600f, 400f);
            window.Show();
        }

        /// <summary>
        /// 资源包数据结构
        /// </summary>
        [Serializable]
        private class PackageTemplate
        {
            /// <summary>
            /// 名称
            /// </summary>
            public string name = string.Empty;
            /// <summary>
            /// 作者
            /// </summary>
            public string author = string.Empty;
            /// <summary>
            /// 版本
            /// </summary>
            public string version = string.Empty;
            /// <summary>
            /// 发布日期
            /// </summary>
            public string releasedDate = string.Empty;
            /// <summary>
            /// 简介
            /// </summary>
            public string description = string.Empty;
            /// <summary>
            /// 依赖项
            /// </summary>
            public string[] dependencies = null;
        }

        //资源包信息列表
        private List<List<PackageTemplate>> packages;
        //折叠状态
        private Dictionary<string, bool> foldoutDic;
        //列表滚动视图
        private Vector2 listScroll;
        //最后更新日期
        private string lastUpdateDate;
        //当前选中的资源包信息
        private PackageTemplate currentSelected;
        //搜索内容
        private string searchContent;

        private void OnEnable()
        {
            packages = new List<List<PackageTemplate>>();
            foldoutDic = new Dictionary<string, bool>();
            EditorCoroutineRunner.StartEditorCoroutine(GetPackagesInfo());
        }

        private void OnGUI()
        {
            //水平布局
            GUILayout.BeginHorizontal("Toolbar");
            {
                //搜索
                OnSearchGUI();
            }
            GUILayout.EndHorizontal();

            //水平布局
            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            {
                //垂直布局 设置左侧列表宽度
                GUILayout.BeginVertical(GUILayout.Width(200f));
                {
                    //绘制列表
                    OnListGUI();
                }
                GUILayout.EndVertical();

                //分割线
                GUILayout.Box(string.Empty, "EyeDropperVerticalLine", GUILayout.ExpandHeight(true), GUILayout.Width(1f));

                //垂直布局
                GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
                {
                    //绘制详情
                    OnDetailGUI();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private void OnSearchGUI()
        {
            var newSearchContent = GUILayout.TextField(searchContent, "SearchTextField");
            if (newSearchContent != searchContent)
            {
                searchContent = newSearchContent;
                currentSelected = null;
                Repaint();
            }
            if (UnityEngine.Event.current.type == EventType.MouseDown && !GUILayoutUtility.GetLastRect().Contains(UnityEngine.Event.current.mousePosition))
            {
                GUI.FocusControl(null);
            }
        }

        private void OnListGUI()
        {
            //滚动视图
            listScroll = GUILayout.BeginScrollView(listScroll);
            {
                GUIStyle versionStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Italic };
                for (int i = 0; i < packages.Count; i++)
                {
                    List<PackageTemplate> list = packages[i];
                    PackageTemplate first = list[0];
                    if (!string.IsNullOrEmpty(searchContent) && !first.name.ToLower().Contains(searchContent.ToLower())) continue;
                    if (foldoutDic[first.name])
                    {
                        foldoutDic[first.name] = EditorGUILayout.Foldout(foldoutDic[first.name], first.name);
                        for (int n = 0; n < list.Count; n++)
                        {
                            GUILayout.BeginHorizontal(currentSelected == list[n] ? "SelectionRect" : "IN Title");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label(list[n].version, versionStyle);
                            GUILayout.Space(30f);
                            GUILayout.EndHorizontal();

                            if (GUILayoutUtility.GetLastRect().Contains(UnityEngine.Event.current.mousePosition) && UnityEngine.Event.current.type == EventType.MouseDown)
                            {
                                currentSelected = list[n];
                                UnityEngine.Event.current.Use();
                            }
                        }
                    }
                    else
                    {
                        GUILayout.BeginHorizontal(currentSelected == first ? "SelectionRect" : "Toolbar");
                        {
                            foldoutDic[first.name] = EditorGUILayout.Foldout(foldoutDic[first.name], first.name);
                            GUILayout.FlexibleSpace();
                            GUILayout.Label(first.version, versionStyle);
                        }
                        GUILayout.EndHorizontal();

                        //鼠标点击选中
                        if (GUILayoutUtility.GetLastRect().Contains(UnityEngine.Event.current.mousePosition) && UnityEngine.Event.current.type == EventType.MouseDown)
                        {
                            currentSelected = first;
                            UnityEngine.Event.current.Use();
                        }
                    }
                }
            }
            GUILayout.EndScrollView();

            //分割线
            GUILayout.Box(string.Empty, "EyeDropperHorizontalLine", GUILayout.ExpandWidth(true), GUILayout.Height(1f));

            //水平布局 设置高度
            GUILayout.BeginHorizontal(GUILayout.Height(23f));
            {
                //最后更新日期
                GUILayout.Label(lastUpdateDate);
                //刷新按钮
                if (GUILayout.Button(EditorGUIUtility.IconContent("Refresh"), GUILayout.Width(30f)))
                {
                    //清空当前的资源包信息列表
                    packages.Clear();
                    //清空折叠栏信息
                    foldoutDic.Clear();
                    //当前选中的资源包设为空
                    currentSelected = null;
                    //发起网络请求
                    EditorCoroutineRunner.StartEditorCoroutine(GetPackagesInfo());
                }
            }
            GUILayout.EndHorizontal();
        }
        private void OnDetailGUI()
        {
            if (currentSelected != null)
            {
                //名称
                GUILayout.Label(currentSelected.name, new GUIStyle(GUI.skin.label) { fontSize = 25, fontStyle = FontStyle.Bold });
                EditorGUILayout.Space();
                //作者
                GUILayout.Label(currentSelected.author, new GUIStyle(GUI.skin.label) { fontSize = 12 });
                EditorGUILayout.Space();
                //版本 + 发布日期
                GUILayout.Label($"Version {currentSelected.version} - {currentSelected.releasedDate}", new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold });
                EditorGUILayout.Space();

                //分割线
                GUILayout.Box(string.Empty, GUILayout.ExpandWidth(true), GUILayout.Height(1f));

                //简介
                GUILayout.Label(currentSelected.description);
            }
            GUILayout.FlexibleSpace();

            //分割线
            GUILayout.Box(string.Empty, "EyeDropperHorizontalLine", GUILayout.ExpandWidth(true), GUILayout.Height(1f));

            //水平布局 设置高度
            GUILayout.BeginHorizontal(GUILayout.Height(21f));
            {
                GUILayout.FlexibleSpace();
                //下载并导入
                if (GUILayout.Button("Import", GUILayout.Width(50f)))
                {
                    if (currentSelected != null)
                    {
                        EditorCoroutineRunner.StartEditorCoroutine(DownloadPackage(currentSelected));
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        //获取资源包信息
        private IEnumerator GetPackagesInfo()
        {
            string url = $"{OnlineUrl}packages.json";
            UnityWebRequest www = UnityWebRequest.Get(url);
            UnityWebRequestAsyncOperation request = www.SendWebRequest();
            while (true)
            {
                if (request.isDone)
                    break;
            }
            yield return request.isDone;
            if (www.error == null)
            {
                List<PackageTemplate> list = JsonMapper.ToObject<List<PackageTemplate>>(www.downloadHandler.text);

                for (int i = 0; i < list.Count; i++)
                {
                    var package = list[i] as PackageTemplate;
                    //查找列表中是否已经存在该资源包其他版本
                    int index = packages.FindIndex(m => m != null && m.Count > 0 && m[0].name == package.name);
                    if (index == -1)
                    {
                        var newList = new List<PackageTemplate> { package };
                        packages.Add(newList);
                        foldoutDic.Add(package.name, false);
                    }
                    else
                    {
                        packages[index].Add(package);
                    }
                }
                //更新最后刷新日期
                lastUpdateDate = DateTime.Now.ToString();
            }
            else
            {
                Debug.LogError(www.error);
            }
        }
        //下载并导入资源包
        private IEnumerator DownloadPackage(PackageTemplate package)
        {
            string url = $"{OnlineUrl}packages/{package.name}/{package.version}/{package.name}.unitypackage";
            //Debug.Log(url);
            UnityWebRequest www = UnityWebRequest.Get(url);
            UnityWebRequestAsyncOperation request = www.SendWebRequest();
            while (true)
            {
                if (request.isDone)
                    break;
            }
            yield return request.isDone;
            if (www.error == null)
            {
                byte[] bytes = www.downloadHandler.data;
                string path = $"{Application.dataPath}/{package.name}-{package.version}.unitypackage";
                //写入本地
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }
                //导入
                AssetDatabase.ImportPackage(path, false);
                //删除
                File.Delete(path);
            }
            else
            {
                Debug.LogError(www.error);
            }
        }
    }

    public static class EditorCoroutineRunner
    {
        private class EditorCoroutine : IEnumerator
        {
            private Stack<IEnumerator> executionStack;

            public EditorCoroutine(IEnumerator iterator)
            {
                executionStack = new Stack<IEnumerator>();
                executionStack.Push(iterator);
            }

            public bool MoveNext()
            {
                IEnumerator i = executionStack.Peek();

                if (i.MoveNext())
                {
                    object result = i.Current;
                    if (result != null && result is IEnumerator)
                    {
                        executionStack.Push((IEnumerator)result);
                    }

                    return true;
                }
                else
                {
                    if (executionStack.Count > 1)
                    {
                        executionStack.Pop();
                        return true;
                    }
                }

                return false;
            }

            public void Reset()
            {
                throw new NotSupportedException("This Operation Is Not Supported.");
            }

            public object Current
            {
                get { return executionStack.Peek().Current; }
            }

            public bool Find(IEnumerator iterator)
            {
                return executionStack.Contains(iterator);
            }
        }

        private static List<EditorCoroutine> editorCoroutineList;
        private static List<IEnumerator> buffer;

        public static IEnumerator StartEditorCoroutine(IEnumerator iterator)
        {
            if (editorCoroutineList == null)
            {
                editorCoroutineList = new List<EditorCoroutine>();
            }
            if (buffer == null)
            {
                buffer = new List<IEnumerator>();
            }
            if (editorCoroutineList.Count == 0)
            {
                EditorApplication.update += Update;
            }
            buffer.Add(iterator);
            return iterator;
        }

        private static bool Find(IEnumerator iterator)
        {
            foreach (EditorCoroutine editorCoroutine in editorCoroutineList)
            {
                if (editorCoroutine.Find(iterator))
                {
                    return true;
                }
            }
            return false;
        }

        private static void Update()
        {
            editorCoroutineList.RemoveAll(coroutine => { return coroutine.MoveNext() == false; });
            if (buffer.Count > 0)
            {
                foreach (IEnumerator iterator in buffer)
                {
                    if (!Find(iterator))
                    {
                        editorCoroutineList.Add(new EditorCoroutine(iterator));
                    }
                }
                buffer.Clear();
            }
            if (editorCoroutineList.Count == 0)
            {
                EditorApplication.update -= Update;
            }
        }
    }
}