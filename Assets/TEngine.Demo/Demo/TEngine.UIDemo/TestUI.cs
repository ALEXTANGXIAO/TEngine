using TEngine;
using TEngine.Runtime;
using UnityEngine;
using UnityEngine.UI;

using TEngine.Runtime.UIModule;

class TestUI : UIWindow
{
    public static int TestEvent = StringId.StringToHash("TestEvent");
    
    #region 脚本工具生成的代码
    private Text m_text233;
    protected override void ScriptGenerator()
    {
        m_text233 = FindChildComponent<Text>("m_text233");
    }
    #endregion

    protected override void RegisterEvent()
    {
        base.RegisterEvent();
        AddUIEvent(TestEvent,Test);
    }

    private void Test()
    {
        Log.Fatal("Test Trigger");
    }

    #region 事件
    #endregion

}