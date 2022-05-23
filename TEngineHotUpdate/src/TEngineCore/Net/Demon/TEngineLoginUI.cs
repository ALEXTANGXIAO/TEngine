using TEngineCore;
using TEngineCore.Net;
using TEngineProto;
using UnityEngine;
using UnityEngine.UI;

namespace TEngineCore
{
    public class TEngineLoginUI : UIWindow
    {
        #region 脚本工具生成的代码
        private Image m_imgbg;
        private Text m_textTittle;
        private Text m_textVer;
        private Image m_imgLogo;
        private GameObject m_goLoading;
        private GameObject m_goLoginRoot;
        private InputField m_inputName;
        private InputField m_inputPassword;
        private Button m_btnLogin;
        protected override void ScriptGenerator()
        {
            m_imgbg = FindChildComponent<Image>("m_imgbg");
            m_textTittle = FindChildComponent<Text>("m_textTittle");
            m_textVer = FindChildComponent<Text>("m_textVer");
            m_imgLogo = FindChildComponent<Image>("m_imgLogo");
            m_goLoading = FindChild("m_goLoading").gameObject;
            m_goLoginRoot = FindChild("m_goLoginRoot").gameObject;
            m_inputName = FindChildComponent<InputField>("m_goLoginRoot/m_inputName");
            m_inputPassword = FindChildComponent<InputField>("m_goLoginRoot/m_inputPassword");
            m_btnLogin = FindChildComponent<Button>("m_goLoginRoot/m_btnLogin");
            m_btnLogin.onClick.AddListener(OnClickLoginBtn);
        }
        #endregion

        #region 事件
        private void OnClickLoginBtn()
        {
            var loginPack = new MainPack();
            loginPack.Requestcode = RequestCode.User;
            loginPack.Actioncode = ActionCode.Login;
            loginPack.LoginPack = new LoginPack
            {
                Username = m_inputName.text,
                Password = m_inputName.text,
            };
            GameClient.Instance.SendCsMsg(loginPack, CallBack);
        }

        private void CallBack(MainPack pack)
        {
            TLogger.LogInfoSuccessd("MainPack" + pack);
        }
        #endregion

    }
}