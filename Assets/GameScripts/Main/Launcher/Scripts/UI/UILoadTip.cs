using UnityEngine;
using UnityEngine.UI;
using System;
using TEngine;
using TMPro;

namespace GameMain
{
    public enum MessageShowType
    {
        None = 0,
        OneButton = 1,
        TwoButton = 2,
        ThreeButton = 3,
    }

    public class UILoadTip : UIBase
    {
        public Button _btn_update;
        public Button _btn_ignore;
        public Button _btn_package;
        public TextMeshProUGUI _label_desc;
        public TextMeshProUGUI _label_tittle;

        public Action OnOk;
        public Action OnCancle;
        public MessageShowType Showtype = MessageShowType.None;

        void Start()
        {
            EventTriggerListener.Get(_btn_update.gameObject).OnClick = _OnGameUpdate;
            EventTriggerListener.Get(_btn_ignore.gameObject).OnClick = _OnGameIgnor;
            EventTriggerListener.Get(_btn_package.gameObject).OnClick = _OnInvoke;
        }

        public override void OnEnter(object data)
        {
            _btn_ignore.gameObject.SetActive(false);
            _btn_package.gameObject.SetActive(false);
            _btn_update.gameObject.SetActive(false);
            switch (Showtype)
            {
                case MessageShowType.OneButton:
                    _btn_update.gameObject.SetActive(true);
                    break;
                case MessageShowType.TwoButton:
                    _btn_update.gameObject.SetActive(true);
                    _btn_ignore.gameObject.SetActive(true);
                    break;
                case MessageShowType.ThreeButton:
                    _btn_ignore.gameObject.SetActive(true);
                    _btn_package.gameObject.SetActive(true);
                    _btn_package.gameObject.SetActive(true);
                    break;
            }

            _label_desc.text = data.ToString();
        }

        private void _OnGameUpdate(GameObject obj)
        {
            if (OnOk == null)
            {
                _label_desc.text = "<color=#BA3026>该按钮不应该存在</color>";
            }
            else
            {
                OnOk();
                _OnClose();
            }
        }

        private void _OnGameIgnor(GameObject obj)
        {
            if (OnCancle == null)
            {
                _label_desc.text = "<color=#BA3026>该按钮不应该存在</color>";
            }
            else
            {
                OnCancle();
                _OnClose();
            }
        }

        private void _OnInvoke(GameObject obj)
        {
            if (OnOk == null)
            {
                _label_desc.text = "<color=#BA3026>该按钮不应该存在</color>";
            }
            else
            {
                OnOk();
                _OnClose();
            }
        }

        private void _OnClose()
        {
            UILoadMgr.Hide(UIDefine.UILoadTip);
        }

        /// <summary>
        /// 显示提示框，目前最多支持三个按钮
        /// </summary>
        /// <param name="desc">描述</param>
        /// <param name="showtype">类型（MessageShowType）</param>
        /// <param name="style">StyleEnum</param>
        /// <param name="onOk">点击事件</param>
        /// <param name="onCancel">取消事件</param>
        /// <param name="onPackage">更新事件</param>
        public static void ShowMessageBox(string desc, MessageShowType showtype = MessageShowType.OneButton,
            LoadStyle.StyleEnum style = LoadStyle.StyleEnum.Style_Default,
            Action onOk = null,
            Action onCancel = null,
            Action onPackage = null)
        {
            UILoadMgr.Show(UIDefine.UILoadTip, desc);
            var ui = UILoadMgr.GetActiveUI(UIDefine.UILoadTip) as UILoadTip;
            if (ui == null) return;
            ui.OnOk = onOk;
            ui.OnCancle = onCancel;
            ui.Showtype = showtype;
            ui.OnEnter(desc);

            var loadStyleUI = ui.GetComponent<LoadStyle>();
            if (loadStyleUI)
            {
                loadStyleUI.SetStyle(style);
            }
        }
    }
}