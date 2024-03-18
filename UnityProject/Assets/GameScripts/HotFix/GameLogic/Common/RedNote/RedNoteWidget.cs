using TEngine;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    public class RedNoteWidget : UIWidget
    {
        #region 脚本工具生成的代码
        protected override void ScriptGenerator()
        {
        }
        #endregion

        private Image m_image;
        public RedNoteBehaviour m_redNote;
        protected override void OnCreate()
        {
            m_redNote = CreateWidget<RedNoteBehaviour>(gameObject);
            m_image = gameObject.GetComponent<Image>();
            rectTransform.anchoredPosition = Vector2.zero;
            SetNotifyState(false);
        }

        public void SetNotifyType(RedNoteNotify notifyType)
        {
            m_redNote.SetNotifyType(notifyType);
        }
        public void SetNotifyType(RedNoteNotify notifyType, ulong param1)
        {
            m_redNote.SetNotifyType(notifyType, param1);
        }
        public void SetNotifyType(RedNoteNotify notifyType, ulong param1, ulong param2)
        {
            m_redNote.SetNotifyType(notifyType, param1, param2);
        }

        public void SetNotifyState(bool state)
        {
            m_redNote.SetRedNoteState(state);
        }

        public void SetSprite(string sprite)
        {
            // m_image.SetSprite(sprite);
        }

        protected override void OnUpdate()
        {
            /*if (!m_redNote.CurState)
            {
                return;
            }
            gameObject.transform.localScale = RedNoteMgr.Instance.GlobalTwScale;*/
        }
    }
}