using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using TEngine;

namespace TEngine.UIEditor
{
    public class LoadStyle : MonoBehaviour
    {
        private const string ConfigPath = "UIStyle/Style.json";

        public enum StyleEnum
        {
            Style_Default = 0,          //默认
            Style_QuitApp = 1,          //退出应用
            Style_RestartApp = 2,       //重启应用
            Style_Retry = 3,            //重试
            Style_StartUpdate_Notice = 4,//提示更新
            Style_DownLoadApk = 5,      //下载底包
            Style_Clear = 6,            //修复客户端
            Style_DownZip = 7,          //继续下载压缩包
        }

        public enum BtnEnum
        {
            BtnOK = 0,    //确定按钮
            BtnIgnore = 1,//取消按钮
            BtnOther = 2, //其他按钮
        }

        /// <summary>
        /// 单个按钮的样式
        /// </summary>
        private class StyleItem
        {
            public Alignment Align;     //对其方式
            public bool Show;           //是否隐藏
            public string Desc;         //按钮描述
        }
        /// <summary>
        /// 对齐方式
        /// </summary>
        private enum Alignment
        {
            Left = 0,
            Middle = 1,
            Right = 2
        }
#if UNITY_EDITOR
        private static string[] Desc = {"","退出应用弹窗功能样式",
            "重启应用弹窗功能样式",
            "重试弹窗功能样式",
            "热更提示弹窗功能样式",
            "底包下载弹窗功能样式",
            "清理资源弹窗功能样式",
            "压缩包下载弹窗功能样式"};
        private static string[] Btn = { "确定按钮", "取消按钮", "其他按钮" };
        private static Dictionary<StyleEnum, Dictionary<BtnEnum, StyleItem>> loadEditorConfig = null;
        private static GUISkin _guiSkin;
        private static Vector2 vecScroll = new Vector2(0, 0);

        [MenuItem("TEngine/UITip样式设置")]
        private static void Open()
        {
            _guiSkin = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/TEngine/Editor/UI/UIStyleSkin.guiskin");
            var window = EditorWindow.GetWindow(typeof(CustomMessageBox), true, "UITips样式") as CustomMessageBox;
            if (window == null) return;

            window.minSize = new Vector2(500, 560);
            window.maxSize = new Vector2(500, 560);
            window.Show();

            string url = $"{Application.dataPath}/TResources/{ConfigPath}";
            if (!String.IsNullOrEmpty(url))
            {
#if UNITY_ANDROID
                if (url.StartsWith(Application.persistentDataPath))
                    url = $"file://{url}";
#elif UNITY_IOS
            if (url.StartsWith(Application.persistentDataPath)||url.StartsWith(Application.streamingAssetsPath))
                url = $"file://{url}";
#endif
                UnityWebRequest www = UnityWebRequest.Get(url);
                UnityWebRequestAsyncOperation request = www.SendWebRequest();
                while (!request.isDone)
                {
                }

                if (!String.IsNullOrEmpty(www.downloadHandler.text))
                {
                    loadEditorConfig =
                        JsonConvert.DeserializeObject<Dictionary<StyleEnum, Dictionary<BtnEnum, StyleItem>>>(
                            www.downloadHandler.text);
                }

                www.Dispose();
            }
            window.OnGUIFunc = () =>
            {
                EditorGUILayout.BeginHorizontal();
                if (loadEditorConfig != null)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    vecScroll = GUILayout.BeginScrollView(vecScroll, false, true);
                    foreach (var item in loadEditorConfig)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        GUILayout.Label(Desc[(int)item.Key] + ":", _guiSkin.label);
                        foreach (var subitem in item.Value)
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            GUILayout.Label(Btn[(int)subitem.Key]);
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("对齐：", GUILayout.Width(35));
                            subitem.Value.Align = (Alignment)EditorGUILayout.Popup((int)subitem.Value.Align, new string[] { "Left", "Middle", "Right" }, GUILayout.Width(100));
                            EditorGUILayout.EndHorizontal();
                            GUILayout.Space(2);
                            subitem.Value.Show = GUILayout.Toggle(subitem.Value.Show, "是否显示");
                            GUILayout.Space(2);
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("描述：", GUILayout.Width(35));
                            subitem.Value.Desc = EditorGUILayout.TextField(subitem.Value.Desc, GUILayout.Width(150));
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                        }
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(2);
                    }
                    GUILayout.EndScrollView();

                    if (GUILayout.Button("保存"))
                    {
                        try
                        {
                            string result = JsonConvert.SerializeObject(loadEditorConfig);
                            string dir = $"{Application.dataPath}/TResources/UIStyle/";
                            if (!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }

                            File.WriteAllText(url, result);
                            Debug.Log("保存成功，路径：" + url);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }

                    EditorGUILayout.EndVertical();

                }
                return 0;
            };
        }
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
                if (OnGUIFunc != null)
                {
                    ReturnValue = OnGUIFunc();
                }
            }
        }
        #endregion
#endif
    }
}