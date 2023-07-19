using System;
using System.Collections.Generic;
using GameLogic;
using TEngine;
using TEngine.Core.Network;
using UnityEngine;
using UnityEngine.UI;

class QuickGmButton
{
    public string m_name;
    public Action<string> m_action;
    public string m_gmText;

    public QuickGmButton(string name, string gmText, Action<string> action)
    {
        m_name = name;
        m_gmText = gmText;
        m_action = action;
    }

    public void Action()
    {
        m_action(m_gmText);
    }
}

[Window(UILayer.Top)]
public class GMPanel : UIWindow
{
    private InputField m_input;
    private InputField m_labelResult; // 结果文本
    private Button m_closeBtn;

    #region 常用的命令布局

    private Transform m_leftBtnGroupRoot;

    ///左侧快捷按钮
    private Transform m_rightBtnGroupRoot;

    ///右侧快捷按钮
    private GameObject m_goTemplateRmdItem; // 推荐图标模板

    private Text m_openLogLabel;

    #endregion

    #region 常用GM

    private int m_gmCommendindex = 0;

    /// <summary>
    /// 推荐的GM
    /// key => gm名
    /// val => 处理方法
    /// </summary>
    private List<QuickGmButton> m_listRmdGmLeft = new List<QuickGmButton>();

    private List<QuickGmButton> m_listRmdGmRight = new List<QuickGmButton>();

    /// <summary>
    /// 推荐gm别名
    /// key => gm名
    /// val => 别名
    /// </summary>
    private Dictionary<string, string> m_dicGmAlias = new Dictionary<string, string>();

    private List<QuickGmButton> ListRmdGmLeft
    {
        get
        {
            if (m_listRmdGmLeft.Count < 1)
            {
                m_listRmdGmLeft.Add(new QuickGmButton("货币大全", "addAllMoney", RmdGmAction_AddAllMoney));
                m_listRmdGmLeft.Add(new QuickGmButton("清理背包", "clearbag", ExcuteGM));
                m_listRmdGmLeft.Add(new QuickGmButton("道具大全", "addItem", RmdGmAction_AddItem));
                m_listRmdGmLeft.Add(new QuickGmButton("装备大全", "", ShowEquipUI));
                m_listRmdGmLeft.Add(new QuickGmButton("清场", "$killall", ExcuteGM));
            }

            return m_listRmdGmLeft;
        }
    }

    private List<QuickGmButton> ListRmdGmRight
    {
        get
        {
            if (m_listRmdGmRight.Count < 1)
            {
                m_listRmdGmRight.Add(new QuickGmButton("清理面板日志", "", ClearPanel));
                m_listRmdGmRight.Add(new QuickGmButton("查看当前时间", "seetime", ExcuteGM));
                m_listRmdGmRight.Add(new QuickGmButton("还原服务器时间", "settime", ExcuteGM));
                m_listRmdGmRight.Add(new QuickGmButton("服务器时间加24H", "settimeNextDay", SetTimeNextDay));
                m_listRmdGmRight.Add(new QuickGmButton("快一小时", "nexthour", ExcuteGM));
                m_listRmdGmRight.Add(new QuickGmButton("查看属性", "$selfattr", ExcuteGM));
                m_listRmdGmRight.Add(new QuickGmButton("目标属性", "$targetattr", ExcuteGM));
            }

            return m_listRmdGmRight;
        }
    }

    #endregion

    public override void BindMemberProperty()
    {
        m_input = FindChildComponent<InputField>("center/InputText");
        m_labelResult = FindChildComponent<InputField>("center/Panel/Scroll View/Viewport/Content/InputField");
        m_closeBtn = FindChildComponent<Button>("center/BtnClose");
    }

    public override void OnCreate()
    {
        m_input.onEndEdit.AddListener(OnEndEditor);
        m_closeBtn.onClick.AddListener(OnClose);

        GameClient.Instance.RegisterMsgHandler(OuterOpcode.CmdGmRes, HandleGmRes);

        CreateQuickButton();

        if (UserData != null)
        {
            var txt = (string)UserData;
            if (string.IsNullOrEmpty(txt))
            {
                return;
            }
            ShowText(txt);
        }
    }

    public override void OnDestroy()
    {
        GameClient.Instance.UnRegisterMsgHandler(OuterOpcode.CmdGmRes, HandleGmRes);
    }

    // public override void SetWindowParam(UIWindowParam param)
    // {
    //     base.SetWindowParam(param);
    //     if (param == null) return;
    //     var txt = param.GetObj<string>("ShowText");
    //     if (string.IsNullOrEmpty(txt))
    //     {
    //         return;
    //     }
    //     if (param.GetVal<bool>("ClearPanel"))
    //     {
    //         ClearPanel();
    //     }
    //     ShowText(txt);
    // }

    void CreateQuickButton()
    {
        var listLeft = ListRmdGmLeft;
        var listRight = ListRmdGmRight;
        var leftListRoot = FindChild("center/LeftQuickList");
        var rightListRoot = FindChild("center/RightQuickList");
        var leftGoTemp = FindChild(leftListRoot, "ButtonTemp");
        var rightGoTemp = FindChild(rightListRoot, "ButtonTemp");
        foreach (var btnInfo in listLeft)
        {
            var newButtonGo = UnityEngine.Object.Instantiate(leftGoTemp.gameObject, leftListRoot);
            newButtonGo.SetActive(true);
            var buttonText = FindChildComponent<Text>(newButtonGo.transform, "Text");
            buttonText.text = btnInfo.m_name;
            var button = newButtonGo.GetComponent<Button>();
            button.onClick.AddListener(btnInfo.Action);
        }

        foreach (var btnInfo in listRight)
        {
            var newButtonGo = UnityEngine.Object.Instantiate(rightGoTemp.gameObject, rightListRoot);
            newButtonGo.SetActive(true);

            var buttonText = FindChildComponent<Text>(newButtonGo.transform, "Text");
            buttonText.text = btnInfo.m_name;
            var button = newButtonGo.GetComponent<Button>();
            button.onClick.AddListener(btnInfo.Action);
        }
    }

    void OnEndEditor(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        //提取GM命令并且存储
        ClientGm.Instance.AddCommend(text);
        m_gmCommendindex = 0;

        string[] inputGMs = text.Split('\n');

        m_input.text = "";
        m_labelResult.text = "";

        for (int i = 0; i < inputGMs.Length; i++)
        {
            var strGM = inputGMs[i].Trim();
            if (strGM != "")
            {
                ExcuteGM(strGM);
            }
        }
    }

    void OnClose()
    {
        Close();
    }


    private string FormatSize(int size)
    {
        int mSize = size / (1024 * 1024);
        int kSize = (size % (1024 * 1024)) / 1024;
        int bSize = size % (1024);

        string val = "";
        if (mSize > 0)
        {
            val += mSize + "M";
        }

        if (kSize > 0)
        {
            val += kSize + "k";
        }

        if (bSize > 0)
        {
            val += bSize;
        }

        if (mSize > 0 || kSize > 0)
        {
            val += "(" + size + ")";
        }

        return val;
    }

    // Update is called once per frame
    public override void OnUpdate()
    {
        ProcessInput();
    }

    private void SetTimeNextDay(string gm)
    {
        // int ss = TimeUtil.GetServerSeconds();
        // System.DateTime nextd = new System.DateTime(1970, 1, 1).AddSeconds(ss + 24 * 60 * 60).ToLocalTime();
        // ExcuteGM($"settime {nextd.ToString("yyyy-MM-dd HH:mm:ss")}");
    }

    // 推荐gm行为-添加所有的货币
    private void RmdGmAction_AddAllMoney(string gm)
    {
        ExcuteGM(string.Format("addcountrymoney 9999999"));
        ExcuteGM(string.Format("addwarmerit 9999999"));
        ExcuteGM(string.Format("addg 9999999"));
        ExcuteGM(string.Format("addd 9999999"));
        ExcuteGM(string.Format("addbinddiamond 9999999"));
        ExcuteGM(string.Format("addpvpmoney 9999999"));
        ExcuteGM(string.Format("addexploit 9999999"));
        ExcuteGM(string.Format("addguildmoney 9999999"));
        ExcuteGM(string.Format("addguildcontribute 9999999"));
        ExcuteGM(string.Format("adddouqi 9999999"));
        ExcuteGM(string.Format("addyin 9999999"));
        ExcuteGM(string.Format("addbinddiamond 9999999"));
        ExcuteGM(string.Format("addmoney 8 9999999"));
        ExcuteGM(string.Format("addmoney 5 9999999"));
    }

    private void RmdGmAction_AddItem(string gm)
    {
        // UISys.Mgr.ShowUI(GAME_UI_TYPE.GMGoodsUI);
    }

    private void ShowEquipUI(string gm)
    {
        // UISys.Mgr.ShowUI(GAME_UI_TYPE.GMEquipUI);
    }

    public void ClearPanel()
    {
        ClearPanel(string.Empty);
    }

    private void ClearPanel(string gm)
    {
        m_labelResult.text = string.Empty;
    }


    private void ExcuteGM(string strGM)
    {
        Log.Warning("input : {0}", strGM);

        //判断是否是$开头，是的话为客户端的GM
        string text = strGM;
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        if (text.ToCharArray()[0] == '$')
        {
            string gmInfo = text.Substring(1);
            ClientGm.Instance.HandleClientGm(gmInfo);
            return;
        }

        CmdGmReq req = new CmdGmReq
        {
            input = text
        };
        GameClient.Instance.Send(req);
    }

    public void ShowText(string text)
    {
        var allTex = m_labelResult.text;
        allTex += "\r\n";
        allTex += text;

        m_labelResult.text = allTex;
    }

    private void HandleGmRes(IResponse result)
    {
        CmdGmRes res = (CmdGmRes)result;
        if (NetworkUtils.CheckError(res))
        {
            ShowText("发送GM失败:" + result);
            return;
        }
        ShowText(res.msg);
    }

    protected override void OnSetVisible(bool value)
    {
        if (true)
        {
            m_input.Select();
            m_input.ActivateInputField();
        }
    }

    public void AddQuality()
    {
        QualitySettings.IncreaseLevel(true);
        ShowShadowParma();
    }

    void ShowShadowParma()
    {
        string text = "shadowCascades:" + QualitySettings.shadowCascades + "\r\n";
        text += "shadowDistance:" + QualitySettings.shadowDistance + "\r\n";
        text += "shadowProjection:" + QualitySettings.shadowProjection;

        ShowText(text);
    }

    public void DecQuality()
    {
        QualitySettings.DecreaseLevel(true);
        ShowShadowParma();
    }

    public void DebugQuickEndLevel()
    {
    }

    public void CloseGM()
    {
        Close();
    }

    private void ProcessInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (ClientGm.Instance.GetCommendByIndex(m_gmCommendindex, out string commend))
            {
                ++m_gmCommendindex;
            }
            else
            {
                m_gmCommendindex = string.IsNullOrEmpty(commend) ? 0 : 1;
            }

            if (!string.IsNullOrEmpty(commend))
            {
                m_input.text = commend;
            }
        }
    }
}