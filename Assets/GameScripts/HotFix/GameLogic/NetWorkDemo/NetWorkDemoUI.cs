using GameLogic;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

[Window(UILayer.UI)]
class NetWorkDemoUI : UIWindow
{
    #region 脚本工具生成的代码

    private GameObject m_goScrollView;
    private Transform m_tfContent;
    private GameObject m_itemNetLog;
    private GameObject m_goConnect;
    private Button m_btnConnect;
    private GameObject m_goLogin;
    private InputField m_inputPassWord;
    private InputField m_inputName;
    private Button m_btnLogin;
    private Button m_btnRegister;

    public override void ScriptGenerator()
    {
        m_goScrollView = FindChild("Panel/m_goScrollView").gameObject;
        m_tfContent = FindChild("Panel/m_goScrollView/Viewport/m_tfContent");
        m_itemNetLog = FindChild("Panel/m_goScrollView/Viewport/m_tfContent/m_itemNetLog").gameObject;
        m_goConnect = FindChild("Panel/m_goConnect").gameObject;
        m_btnConnect = FindChildComponent<Button>("Panel/m_goConnect/m_btnConnect");
        m_goLogin = FindChild("Panel/m_goLogin").gameObject;
        m_inputPassWord = FindChildComponent<InputField>("Panel/m_goLogin/m_inputPassWord");
        m_inputName = FindChildComponent<InputField>("Panel/m_goLogin/m_inputName");
        m_btnLogin = FindChildComponent<Button>("Panel/m_goLogin/m_btnLogin");
        m_btnRegister = FindChildComponent<Button>("Panel/m_goLogin/m_btnRegister");
        m_btnConnect.onClick.AddListener(OnClickConnectBtn);
        m_btnLogin.onClick.AddListener(OnClickLoginBtn);
        m_btnRegister.onClick.AddListener(OnClickRegisterBtn);
    }

    #endregion

    #region 事件

    private void OnClickConnectBtn()
    {
        GameClient.Instance.Connect("127.0.0.1:20000");
    }

    private void OnClickLoginBtn()
    {
        if (GameClient.Instance.Status == GameClientStatus.StatusInit)
        {
            Log.Info("没有连接到服务器、请先点击连接到服务器按钮在进行此操作");
            return;
        }

        if (string.IsNullOrEmpty(m_inputName.text) || string.IsNullOrEmpty(m_inputPassWord.text))
        {
            Log.Info("请输入账号和密码");
            return;
        }
        PlayerNetSys.Instance.DoLoginReq(m_inputName.text,m_inputPassWord.text);
    }

    private void OnClickRegisterBtn()
    {
        if (GameClient.Instance.Status == GameClientStatus.StatusInit)
        {
            Log.Info("没有连接到服务器、请先点击连接到服务器按钮在进行此操作");
            return;
        }

        if (string.IsNullOrEmpty(m_inputName.text) || string.IsNullOrEmpty(m_inputPassWord.text))
        {
            Log.Info("请输入账号和密码");
            return;
        }
        PlayerNetSys.Instance.DoRegisterReq(m_inputName.text,m_inputPassWord.text);
    }

    #endregion

    private void RefreshLog()
    {
    }
}

namespace GameLogic
{
    [Window(UILayer.UI)]
    class NetWorkDemoLog : UIWindow
    {
        #region 脚本工具生成的代码

        private Text m_textInfo;

        public override void ScriptGenerator()
        {
            m_textInfo = FindChildComponent<Text>("m_textInfo");
        }

        #endregion

        public void SetText(string value)
        {
            m_textInfo.text = value;
        }
    }
}