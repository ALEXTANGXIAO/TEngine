using System;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    /// <summary>
    /// 红点个体行为。
    /// </summary>
    public class RedNoteBehaviour : UIWidget
    {
        public Action<bool> HaveRedNoteAction;

        //当前红点类型
        public RedNoteNotify RedNoteNotifyType { get; private set; }

        //启用时当作标记，解决带有ID，创建多个类似条目的情况
        public readonly List<ulong> IdParamList = new List<ulong>();
        private readonly List<ulong> _tmpIdParam = new List<ulong>();

        private Image _image;

        private Image Image
        {
            get
            {
                if (_image == null && gameObject != null)
                {
                    _image = gameObject.GetComponent<Image>();
                }
                return _image;
            }
        }

        private Text _text;
        private Text Text
        {
            get
            {
                if (_text == null && gameObject != null)
                {
                    _text = FindChildComponent<Text>(rectTransform, "Text");
                }
                return _text;
            }
        }

        private bool _state = false;

        /// <summary>
        /// 当前红点状态。
        /// </summary>
        public bool CurState
        {
            private set
            {
                _state = value;

                if (Image == null)
                {
                    gameObject.SetActive(_state);
                    return;
                }

                Color c = Image.color;
                c.a = _state ? 1f : 0.01f;
                Image.color = c;

                if (HaveRedNoteAction != null)
                {
                    HaveRedNoteAction(_state);
                }
            }
            get => _state;
        }


        //设置显示状态
        public void SetRedNoteState(bool state)
        {
            CurState = state;
        }

        // 设置红点类型
        public void SetNotifyType(RedNoteNotify notifyType)
        {
            _tmpIdParam.Clear();
            SetNotifyType(notifyType, _tmpIdParam);
        }

        #region 参数重载
        public void SetNotifyType(RedNoteNotify notifyType, ulong param1)
        {
            _tmpIdParam.Clear();
            _tmpIdParam.Add(param1);
            SetNotifyType(notifyType, _tmpIdParam);
        }

        public void SetNotifyType(RedNoteNotify notifyType, ulong param1, ulong param2)
        {
            _tmpIdParam.Clear();
            _tmpIdParam.Add(param1);
            _tmpIdParam.Add(param2);
            SetNotifyType(notifyType, _tmpIdParam);
        }

        public void SetNotifyType(RedNoteNotify notifyType, ulong param1, ulong param2, ulong param3)
        {
            _tmpIdParam.Clear();
            _tmpIdParam.Add(param1);
            _tmpIdParam.Add(param2);
            _tmpIdParam.Add(param3);
            SetNotifyType(notifyType, _tmpIdParam);
        }

        public void SetNotifyType(RedNoteNotify notifyType, params ulong[] param)
        {
            _tmpIdParam.Clear();
            for (int i = 0; i < param.Length; i++)
            {
                _tmpIdParam.Add(param[i]);
            }
            SetNotifyType(notifyType, _tmpIdParam);
        }
        #endregion

        public void SetNotifyType(RedNoteNotify notifyType, List<ulong> paramList)
        {
            RemoveNotifyBind();
            if (notifyType == RedNoteNotify.None) return;

            IdParamList.Clear();
            IdParamList.AddRange(paramList);
            SetRedNoteNotifyProcess(notifyType);
        }

        private void SetRedNoteNotifyProcess(RedNoteNotify notifyType)
        {
            // 移除红点通知的绑定
            if (Image != null)
            {
                Image.rectTransform.SetAsLastSibling();
            }

            RedNoteNotifyType = notifyType;

            RedNoteMgr.Instance.RegisterNotify(RedNoteNotifyType, this);

            if (!RedNoteMgr.Instance.IsNumType(notifyType, IdParamList))
            {
                CurState = RedNoteMgr.Instance.GetNotifyValue(RedNoteNotifyType, IdParamList);
            }
            else
            {
                SetRedNotePointNum(RedNoteMgr.Instance.GetNotifyPointNum(RedNoteNotifyType, IdParamList));
            }
        }

        /// <summary>
        /// 移除红点通知的绑定。
        /// </summary>
        public void RemoveNotifyBind()
        {
            if (RedNoteNotifyType != RedNoteNotify.None)
            {
                RedNoteMgr.Instance.UnRegisterNotify(RedNoteNotifyType, this);
            }

            CurState = false;
        }

        protected override void OnDestroy()
        {
            RemoveNotifyBind();
        }

        public void SetRedNotePointNum(int pointNum)
        {
            if (Text != null)
            {
                Text.text = pointNum > 0 ? pointNum.ToString() : string.Empty;

                CurState = pointNum > 0;
            }
        }
    }
}
