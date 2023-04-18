using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
    [Window(UILayer.System)]
    class LogUI : UIWindow
    {
        #region 脚本工具生成的代码
        private Text m_textError;
        private Button m_btnClose;
        public override void ScriptGenerator()
        {
            m_textError = FindChildComponent<Text>("m_textError");
            m_btnClose = FindChildComponent<Button>("m_btnClose");
            m_btnClose.onClick.AddListener(OnClickCloseBtn);
        }
        #endregion

        #region 事件

        private void OnClickCloseBtn()
        {
            Close();
        }
        #endregion

        public override void OnRefresh()
        {
            m_textError.text = UserData.ToString();
        }
    }
}