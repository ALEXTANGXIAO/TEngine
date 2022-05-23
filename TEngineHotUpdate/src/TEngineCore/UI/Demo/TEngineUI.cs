using UnityEngine.UI;

namespace TEngineCore
{
    public class TEngineUI : UIWindow
    {
        #region 脚本工具生成的代码
        private Image m_imgbg;
        private Text m_textTittle;
        private Text m_textVer;
        protected override void ScriptGenerator()
        {
            m_imgbg = FindChildComponent<Image>("m_imgbg");
            m_textTittle = FindChildComponent<Text>("m_textTittle");
            m_textVer = FindChildComponent<Text>("m_textVer");

            TLogger.LogInfo("TEngineUI ScriptGenerator");
        }
        #endregion

        protected override void RegisterEvent()
        {
            GameEventMgr.Instance.AddEventListener<string>(2,((s) =>
            {
                TLogger.LogWarning("RegisterEvent");
            }));
        }

        protected override void BindMemberProperty()
        {
            base.BindMemberProperty();

            TLogger.LogInfo("TEngineUI BindMemberProperty");
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            TLogger.LogInfo("TEngineUI OnCreate");
        }

        protected override void OnVisible()
        {
            base.OnVisible();

            TLogger.LogInfo("TEngineUI OnVisible");
        }

        protected override void OnUpdate()
        {

        }

        #region 事件
        #endregion

    }
}
