using UnityEngine.UI;
using System;
using TEngine;

namespace GameMain
{
    public enum MessageShowType
    {
        None = 0,
        OneButton = 1,
        TwoButton = 2,
        ThreeButton = 3,
    }

    [Window(UILayer.UI, fromResources: true, location: "AssetLoad/UILoadTip")]
    public class UILoadTip : UIWindow
    {
        public Action OnOk;
        public Action OnCancel;
        public MessageShowType ShowType = MessageShowType.None;

        #region 脚本工具生成的代码
        private Button m_btnPackage;
        private Text m_textTittle;
        private Text m_textInfo;
        private Button m_btnIgnore;
        private Button m_btnUpdate;
        protected override void ScriptGenerator()
        {
            m_btnPackage = FindChildComponent<Button>("BgImage/m_btnPackage");
            m_textTittle = FindChildComponent<Text>("BgImage/m_textTittle");
            m_textInfo = FindChildComponent<Text>("BgImage/m_textInfo");
            m_btnIgnore = FindChildComponent<Button>("BgImage/Group/m_btnIgnore");
            m_btnUpdate = FindChildComponent<Button>("BgImage/Group/m_btnUpdate");
            m_btnPackage.onClick.AddListener(OnClickPackageBtn);
            m_btnIgnore.onClick.AddListener(OnClickIgnoreBtn);
            m_btnUpdate.onClick.AddListener(OnClickUpdateBtn);
        }
        #endregion

        #region 事件

        private void OnClickPackageBtn()
        {
            if (OnOk == null)
            {
                m_textInfo.text = "<color=#BA3026>该按钮不应该存在</color>";
            }
            else
            {
                OnOk();
                Close();
            }
        }

        private void OnClickIgnoreBtn()
        {
            if (OnCancel == null)
            {
                m_textInfo.text = "<color=#BA3026>该按钮不应该存在</color>";
            }
            else
            {
                OnCancel();
                Close();
            }
        }

        private void OnClickUpdateBtn()
        {
            if (OnOk == null)
            {
                m_textInfo.text = "<color=#BA3026>该按钮不应该存在</color>";
            }
            else
            {
                OnOk();
                Close();
            }
        }

        #endregion

        protected override void OnRefresh()
        {
            OnOk = (Action)userDatas[1];
            OnCancel = (Action)userDatas[2];
            ShowType = (MessageShowType)userDatas[3];

            var loadStyleUI = gameObject.GetComponent<LoadStyle>();
            if (loadStyleUI)
            {
                loadStyleUI.SetStyle((LoadStyle.StyleEnum)userDatas[4]);
            }
            
            base.OnRefresh();
            m_btnIgnore.gameObject.SetActive(false);
            m_btnPackage.gameObject.SetActive(false);
            m_btnUpdate.gameObject.SetActive(false);
            switch (ShowType)
            {
                case MessageShowType.OneButton:
                    m_btnUpdate.gameObject.SetActive(true);
                    break;
                case MessageShowType.TwoButton:
                    m_btnUpdate.gameObject.SetActive(true);
                    m_btnIgnore.gameObject.SetActive(true);
                    break;
                case MessageShowType.ThreeButton:
                    m_btnIgnore.gameObject.SetActive(true);
                    m_btnPackage.gameObject.SetActive(true);
                    m_btnUpdate.gameObject.SetActive(true);
                    break;
            }

            m_textInfo.text = UserData.ToString();
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
            GameModule.UI.ShowUI<UILoadTip>(desc,onOk,onCancel,showtype,style);
        }
    }
}