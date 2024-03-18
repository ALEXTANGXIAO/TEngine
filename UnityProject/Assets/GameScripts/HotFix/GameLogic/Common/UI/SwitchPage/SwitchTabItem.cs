using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    public class SwitchTabItem : UIEventItem<SwitchTabItem>
    {
        //选中时的文本
        protected TextMeshProUGUI m_selectText;
        //未选中时的文本
        protected TextMeshProUGUI m_noSelectText;
        //选中时的图片
        protected Image m_selectImage;
        //未选中时的图片
        protected Image m_noSelectImage;
        //选中时的节点
        protected Transform m_selectNode;
        //未选中时的节点
        protected Transform m_noSelectNode;
        
        //选中时的Icon
        protected Image m_selectIcon;
        //未选中时的Icon
        protected Image m_noSelectIcon;

        protected GameObject m_goRedNote;

        //是否选中
        public bool IsSelect
        {
            get { return m_isSelect; }
            set { SetState(value); }
        }

        protected bool m_isSelect = false;

        //private RedNoteBehaviour m_redNote;
        public RedNoteWidget m_redNote { get; private set; }

        public virtual Vector2 SelectRedPointPos
        {
            get
            {
                return Vector2.zero;
            }
        }

        public virtual Vector2 NoSelectRedPointPos
        {
            get
            {
                return Vector2.zero;
            }
        }

        protected override void BindMemberProperty()
        {
            m_selectNode = FindChild("SelectNode");
            m_noSelectNode = FindChild("NoSelectNode");
            if (m_selectNode != null)
            {
                m_selectText = FindChildComponent<TextMeshProUGUI>(m_selectNode, "SelectText");
                m_selectImage = FindChildComponent<Image>(m_selectNode, "SelectImage");
                m_selectIcon = FindChildComponent<Image>(m_selectNode, "SelectIcon");
            }
            if (m_noSelectNode != null)
            {
                m_noSelectText = FindChildComponent<TextMeshProUGUI>(m_noSelectNode, "NoSelectText");
                m_noSelectImage = FindChildComponent<Image>(m_noSelectNode, "NoSelectImage");
                m_noSelectIcon = FindChildComponent<Image>(m_noSelectNode, "NoSelectIcon");
            }
            
            var tf = FindChild("m_goRedNote");
            if (tf != null)
            {
                m_goRedNote = tf.gameObject;
                m_redNote = CreateWidgetByType<RedNoteWidget>(tf);
                m_redNote.rectTransform.anchoredPosition = Vector2.zero;
                SetRedNote(false);
                //CreateDefaultRedNote();
            }
        }

        #region 红点相关
        public void SetRedNoteType(RedNoteNotify type)
        {
            if (m_redNote != null)
            {
                m_redNote.m_redNote.SetNotifyType(type);
            }
            SetRedNote(true);
        }

        public void SetRedNoteType(RedNoteNotify type, ulong param1)
        {
            if (m_redNote != null)
            {
                m_redNote.m_redNote.SetNotifyType(type, param1);
            }
            SetRedNote(true);
        }

        public void SetRedNoteType(RedNoteNotify type, ulong param1, ulong param2)
        {
            if (m_redNote != null)
            {
                m_redNote.m_redNote.SetNotifyType(type, param1, param2);
            }
            SetRedNote(true);
        }

        public void SetRedNoteType(RedNoteNotify type, ulong param1, ulong param2, ulong param3)
        {
            if (m_redNote != null)
            {
                m_redNote.m_redNote.SetNotifyType(type, param1, param2, param3);
            }
            SetRedNote(true);
        }

        public void SetRedNoteState(bool state)
        {
            if (m_redNote != null)
            {
                m_redNote.m_redNote.SetRedNoteState(state);
            }
            SetRedNote(state);
        }

        #endregion

        public void UpdateTabName(string tabName)
        {
            if (m_selectText != null)
            {
                m_selectText.text = tabName;
                m_selectText.rectTransform.sizeDelta = new Vector2(m_selectText.preferredWidth, m_selectText.rectTransform.sizeDelta.y);
            }
            if (m_noSelectText != null)
            {
                m_noSelectText.text = tabName;
                m_noSelectText.rectTransform.sizeDelta = new Vector2(m_noSelectText.preferredWidth, m_noSelectText
                   .rectTransform.sizeDelta.y);
            }
        }

        // public void UpdateTabImage(string selectImageName,string noSelectImageName)
        // {
        //     if (m_selectImage != null)
        //     {
        //         UISpriteHelper.Instance.SetSprite(m_selectImage, selectImageName);
        //     }
        //     if (m_noSelectImage != null)
        //     {
        //         UISpriteHelper.Instance.SetSprite(m_noSelectImage, noSelectImageName);
        //     }
        // }
        //
        // public void UpdateIcon(string selectIconName, string noSelectIconName)
        // {
        //     if (m_selectIcon != null)
        //     {
        //         DUISpriteHelper.Instance.SetSprite(m_selectIcon, selectIconName, true);
        //     }
        //     if (m_noSelectIcon != null)
        //     {
        //         DUISpriteHelper.Instance.SetSprite(m_noSelectIcon, noSelectIconName, true);
        //     }
        // }
        //
        // public virtual void CreateDefaultRedNote()
        // {
        //     if (m_goRedNote != null)
        //     {
        //         UIEffectHelper.CreateUIEffectGoById((uint)CommonEffectID.EffectRedPoint, m_goRedNote.transform);
        //     }
        // }

        public void SetRedNote(bool show)
        {
            if (m_goRedNote != null)
            {
                m_goRedNote.SetActive(show);
            }
        }

        public virtual void SetState(bool select)
        {
            m_isSelect = select;
            if (m_selectNode != null)
            {
                m_selectNode.gameObject.SetActive(select);
            }
            if (m_noSelectNode != null)
            {
                m_noSelectNode.gameObject.SetActive(!select);
            }

            if (m_goRedNote != null)
            {
                if (select)
                {
                    if (SelectRedPointPos != Vector2.zero)
                    {
                        ((RectTransform)m_goRedNote.transform).anchoredPosition = SelectRedPointPos;
                    }
                }
                else
                {
                    if (NoSelectRedPointPos != Vector2.zero)
                    {
                        ((RectTransform)m_goRedNote.transform).anchoredPosition = NoSelectRedPointPos;
                    }
                }
            }
        }

        public void SetITabTextFontSize(int fontSize)
        {
            if (m_selectText != null)
            {
                m_selectText.fontSize = fontSize;
            }
            if (m_noSelectText != null)
            {
                m_noSelectText.fontSize = fontSize;
            }
        }

        #region UI-ID
        /// <summary>
        /// UI-ID
        /// </summary>
        public uint uiID;

        /// <summary>
        /// UI标识
        /// </summary>
        protected string m_uiFlag;

        /// <summary>
        /// 检查方法
        /// </summary>
        protected Func<bool> m_funcCheck;

        /// <summary>
        /// 页签索引
        /// </summary>
        public int tabIndex = -1;

        /// <summary>
        /// UI标识
        /// </summary>
        /// <returns></returns>
        public virtual string GetUiFlag()
        {
            return string.IsNullOrEmpty(m_uiFlag) ? uiID.ToString() : m_uiFlag;
        }
        #endregion
    }
}