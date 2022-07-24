using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace TEngine.UI
{
    public class LoadStyle : MonoBehaviour
    {
        public Button _btn_ignore;
        public Button _btn_update;
        public Button _btn_package;

        public Text _label_ignore;
        public Text _label_update;
        public Text _label_package;
        private Dictionary<StyleEnum, Dictionary<BtnEnum, StyleItem>> loadConfig;

        private const string ConfigPath = "RawBytes/UIStyle/Style.json";

        public enum StyleEnum
        {
            Style_Default = 0,//Ĭ��
            Style_QuitApp = 1,//�˳�Ӧ��
            Style_RestartApp = 2,//����Ӧ��
            Style_Retry = 3,//����
            Style_StartUpdate_Notice = 4,//��ʾ����
            Style_DownLoadApk = 5,//���صװ�
            Style_Clear = 6,//�޸��ͻ���
            Style_DownZip = 7,//��������ѹ����
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
            public Alignment Align;//���䷽ʽ
            public bool Show;//�Ƿ�����
            public string Desc;//��ť����
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

        private void Awake()
        {
            //���ð�ť��Ĭ������
            _label_ignore.text = LoadText.Instance.Label_Btn_Ignore;
            _label_update.text = LoadText.Instance.Label_Btn_Update;
            _label_package.text = LoadText.Instance.Label_Btn_Package;

            InitConfig();
        }

        private void InitConfig()
        {
            string url = ResMgr.GetRawBytesFullPath(ConfigPath);
            if (!String.IsNullOrEmpty(url))
            {
                string finalPath = SetFilePath(url);
                InitConfigDic(finalPath);
            }
        }

        #region ��ʼ�������ļ�
        private string SetFilePath(string path)
        {
#if UNITY_ANDROID
            if (path.StartsWith(Application.persistentDataPath))
                path = $"file://{path}";
#elif UNITY_IOS
            if (path.StartsWith(Application.persistentDataPath)||path.StartsWith(Application.streamingAssetsPath))
                path = $"file://{path}";
#endif
            return path;
        }

        private void InitConfigDic(string path)
        {
            UnityWebRequest www = UnityWebRequest.Get(path);
            UnityWebRequestAsyncOperation request = www.SendWebRequest();
            while (!request.isDone)
            {
            }

            if (!String.IsNullOrEmpty(www.downloadHandler.text))
            {
                loadConfig = JsonConvert.DeserializeObject<Dictionary<StyleEnum, Dictionary<BtnEnum, StyleItem>>>(www.downloadHandler.text);
            }
            www.Dispose();
        }
        #endregion
        /// <summary>
        /// ������ʽ
        /// </summary>
        /// <param name="type">��ʽ��Ӧ��id</param>
        public void SetStyle(StyleEnum type)
        {
            //Ĭ����ʽ��ɶҲ������
            if (type == StyleEnum.Style_Default)
                return;

            if (loadConfig == null)
            {
                TLogger.LogInfo("LoadConfig is null");
                return;
            }

            var style = loadConfig[type];
            if (style == null)
            {
                TLogger.LogError($"LoadConfig, Can not find type:{type},please check it");
                return;
            }
            SetButtonStyle(style);
        }

        /// <summary>
        /// ���ð�ť������,�Ƿ�����
        /// </summary>
        private void SetButtonStyle(Dictionary<BtnEnum, StyleItem> list)
        {
            foreach (var item in list)
            {
                switch (item.Key)
                {
                    case BtnEnum.BtnOK:
                        _label_update.text = item.Value.Desc;
                        _btn_update.gameObject.SetActive(item.Value.Show);
                        SetButtonPos(item.Value.Align, _btn_update.transform);
                        break;
                    case BtnEnum.BtnIgnore:
                        _label_ignore.text = item.Value.Desc;
                        _btn_ignore.gameObject.SetActive(item.Value.Show);
                        SetButtonPos(item.Value.Align, _btn_ignore.transform);
                        break;
                    case BtnEnum.BtnOther:
                        _label_package.text = item.Value.Desc;
                        _btn_package.gameObject.SetActive(item.Value.Show);
                        SetButtonPos(item.Value.Align, _btn_package.transform);
                        break;
                }
            }
        }

        /// <summary>
        /// ���ð�ťλ��
        /// </summary>
        private void SetButtonPos(Alignment align, Transform item)
        {
            switch (align)
            {
                case Alignment.Left:
                    item.SetSiblingIndex(0);
                    break;
                case Alignment.Middle:
                    item.SetSiblingIndex(1);
                    break;
                case Alignment.Right:
                    item.SetSiblingIndex(2);
                    break;
            }
        }
    }
}