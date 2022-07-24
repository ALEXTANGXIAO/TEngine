using UnityEngine;
using UnityEngine.UI;
using System;

namespace TEngine.UI
{
    public enum MessageShowType
    {
        None      = 0,
        OneButton = 1,
        TwoButton = 2,
        ThreeButton = 3,
    }
    public class UILoadTip : UIBase
    {
        public Button _btn_update;
        public Button _btn_ignore;
        public Button _btn_package;
        public Text _label_desc;

        public Action OnOk;
        public Action OnCancle;
        public MessageShowType Showtype = MessageShowType.None;

        void Start()
        {
            EventTriggerListener.Get(_btn_update.gameObject).OnClick = _OnGameUpdate;
            EventTriggerListener.Get(_btn_ignore.gameObject).OnClick = _OnGameIgnor;
            EventTriggerListener.Get(_btn_package.gameObject).OnClick = _OnInvoke;
        }

        public override void OnEnter(object data)
        {
            _btn_ignore.gameObject.SetActive(false);
            _btn_package.gameObject.SetActive(false);
            _btn_update.gameObject.SetActive(false);
            switch (Showtype)
            {
                case MessageShowType.OneButton:
                    _btn_update.gameObject.SetActive(true);
                    break;
                case MessageShowType.TwoButton:
                    _btn_update.gameObject.SetActive(true);
                    _btn_ignore.gameObject.SetActive(true);
                    break;
                case MessageShowType.ThreeButton:
                    _btn_ignore.gameObject.SetActive(true);
                    _btn_package.gameObject.SetActive(true);
                    _btn_package.gameObject.SetActive(true);
                    break;
            }
            _label_desc.text = data.ToString();            
        }

        private void _OnGameUpdate(GameObject obj)
        {
            if (OnOk == null)
            {
                _label_desc.text = "<color=#BA3026>该按钮不应该存在</color>";
            }
            else
            {
                OnOk();
                _OnClose();   
            }
        }

        private void _OnGameIgnor(GameObject obj)
        {
            if (OnCancle == null)
            {
                _label_desc.text = "<color=#BA3026>该按钮不应该存在</color>";
            }
            else
            {
                OnCancle();
                _OnClose();   
            }
        }
        
        private void _OnInvoke(GameObject obj)
        {
            if (OnOk == null)
            {
                _label_desc.text = "<color=#BA3026>该按钮不应该存在</color>";
            }
            else
            {
                OnOk();
                _OnClose();   
            }
        }

        private void _OnClose()
        {
            UILoadMgr.Hide(UIDefine.UILoadTip);
        }
    }
}