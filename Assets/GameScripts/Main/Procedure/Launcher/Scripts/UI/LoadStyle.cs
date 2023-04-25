using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TEngine;

namespace GameMain
{
#pragma warning disable CS0649
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
            Style_Default = 0,//默认
            Style_QuitApp = 1,//退出应用
            Style_RestartApp = 2,//重启应用
            Style_Retry = 3,//重试
            Style_StartUpdate_Notice = 4,//提示更新
            Style_DownLoadApk = 5,//下载底包
            Style_Clear = 6,//修复客户端
            Style_DownZip = 7,//继续下载压缩包
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
            public Alignment Align;//对其方式
            public bool Show;//是否隐藏
            public string Desc;//按钮描述
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

        private void Awake()
        {
            //设置按钮的默认描述
            _label_ignore.text = LoadText.Instance.Label_Btn_Ignore;
            _label_update.text = LoadText.Instance.Label_Btn_Update;
            _label_package.text = LoadText.Instance.Label_Btn_Package;

            InitConfig();
        }

        private void InitConfig()
        {
            // string url = AssetUtility.Config.GetConfigAsset(ConfigPath);
            // if (!String.IsNullOrEmpty(url))
            // {
            //     string finalPath = SetFilePath(url);
            //     InitConfigDic(finalPath);
            // }
        }

        #region 初始化配置文件
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
        /// 设置样式
        /// </summary>
        /// <param name="type">样式对应的id</param>
        public void SetStyle(StyleEnum type)
        {
            if (type == StyleEnum.Style_Default)
                return;

            if (loadConfig == null)
            {
                Log.Error("LoadConfig is null");
                return;
            }

            var style = loadConfig[type];
            if (style == null)
            {
                Log.Error($"LoadConfig, Can not find type:{type},please check it");
                return;
            }
            SetButtonStyle(style);
        }

        /// <summary>
        /// 设置按钮的描述,是否隐藏
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
        /// 设置按钮位置
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