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
            Style_Default = 0,          //Ĭ��
            Style_QuitApp = 1,          //�˳�Ӧ��
            Style_RestartApp = 2,       //����Ӧ��
            Style_Retry = 3,            //����
            Style_StartUpdate_Notice = 4,//��ʾ����
            Style_DownLoadApk = 5,      //���صװ�
            Style_Clear = 6,            //�޸��ͻ���
            Style_DownZip = 7,          //��������ѹ����
        }

        public enum BtnEnum
        {
            BtnOK = 0,    //ȷ����ť
            BtnIgnore = 1,//ȡ����ť
            BtnOther = 2, //������ť
        }

        /// <summary>
        /// ������ť����ʽ
        /// </summary>
        private class StyleItem
        {
            public Alignment Align;     //���䷽ʽ
            public bool Show;           //�Ƿ�����
            public string Desc;         //��ť����
        }
        /// <summary>
        /// ���뷽ʽ
        /// </summary>
        private enum Alignment
        {
            Left = 0,
            Middle = 1,
            Right = 2
        }
#if UNITY_EDITOR
        private static string[] Desc = {"","�˳�Ӧ�õ���������ʽ",
            "����Ӧ�õ���������ʽ",
            "���Ե���������ʽ",
            "�ȸ���ʾ����������ʽ",
            "�װ����ص���������ʽ",
            "������Դ����������ʽ",
            "ѹ�������ص���������ʽ"};
        private static string[] Btn = { "ȷ����ť", "ȡ����ť", "������ť" };
        private static Dictionary<StyleEnum, Dictionary<BtnEnum, StyleItem>> loadEditorConfig = null;
        private static GUISkin _guiSkin;
        private static Vector2 vecScroll = new Vector2(0, 0);

        [MenuItem("TEngine/UITip��ʽ����")]
        private static void Open()
        {
            _guiSkin = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/TEngine/Editor/UI/UIStyleSkin.guiskin");
            var window = EditorWindow.GetWindow(typeof(CustomMessageBox), true, "UITips��ʽ") as CustomMessageBox;
            if (window == null) return;

            window.minSize = new Vector2(500, 560);
            window.maxSize = new Vector2(500, 560);
            window.Show();

            string url = $"{Application.dataPath}/TResources/{ConfigPath}";
            if (!String.IsNullOrEmpty(url))
            {
                string finalPath;
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
                            GUILayout.Label("���룺", GUILayout.Width(35));
                            subitem.Value.Align = (Alignment)EditorGUILayout.Popup((int)subitem.Value.Align, new string[] { "Left", "Middle", "Right" }, GUILayout.Width(100));
                            EditorGUILayout.EndHorizontal();
                            GUILayout.Space(2);
                            subitem.Value.Show = GUILayout.Toggle(subitem.Value.Show, "�Ƿ���ʾ");
                            GUILayout.Space(2);
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("������", GUILayout.Width(35));
                            subitem.Value.Desc = EditorGUILayout.TextField(subitem.Value.Desc, GUILayout.Width(150));
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                        }
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(2);
                    }
                    GUILayout.EndScrollView();

                    if (GUILayout.Button("����"))
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
                            Debug.Log("����ɹ���·����" + url);
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
        #region �༭������
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