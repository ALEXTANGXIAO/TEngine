using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TEngine.Runtime.UIModule
{
    public class UIEventItem<T> : UIWindowWidget where T : UIEventItem<T>
    {
        protected Button m_buttonClick;
        private object[] m_eventParam;

        public object EventParam1
        {
            get { return m_eventParam != null && m_eventParam.Length > 0 ? m_eventParam[0] : null; }
        }

        public object EventParam2
        {
            get { return m_eventParam != null && m_eventParam.Length > 1 ? m_eventParam[1] : null; }
        }

        public object EventParam3
        {
            get { return m_eventParam != null && m_eventParam.Length > 2 ? m_eventParam[2] : null; }
        }

        public object this[int index]
        {
            get { return m_eventParam != null && index < m_eventParam.Length ? m_eventParam[index] : null; }
        }

        private Action<T> m_clickAction;
        private Action<T, bool, PointerEventData> m_pressAction;
        private Action<T, PointerEventData> m_beginDragAction;
        private Action<T, PointerEventData> m_dragAction;
        private Action<T, PointerEventData> m_endDragAction;

        public void BindClickEvent(Action<T> clickAction, params object[] arg)
        {
            if (m_clickAction != null)
            {
                m_clickAction = clickAction;
            }
            else
            {
                m_clickAction = clickAction;
                m_buttonClick = UnityUtil.AddMonoBehaviour<Button>(gameObject);
                m_buttonClick.transition = Selectable.Transition.None;
                m_buttonClick.onClick.AddListener(OnButtonClick);
            }

            SetEventParam(arg);
        }

        private void OnButtonClick()
        {
            if (m_clickAction != null)
            {
                //AudioSys.GameMgr.PlayUISoundEffect(UiSoundType.UI_SOUND_CLICK);
                m_clickAction(this as T);
            }
        }

        public void BindBeginDragEvent(Action<T, PointerEventData> dragAction, params object[] arg)
        {
            if (m_beginDragAction != null)
            {
                m_beginDragAction = dragAction;
            }
            else
            {
                m_beginDragAction = dragAction;
                var trigger = UnityUtil.AddMonoBehaviour<EventTrigger>(gameObject);
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.BeginDrag;
                entry.callback = new EventTrigger.TriggerEvent();
                entry.callback.AddListener(OnTriggerBeginDrag);
                trigger.triggers.Add(entry);
            }

            SetEventParam(arg);
        }

        private void OnTriggerBeginDrag(BaseEventData data)
        {
            var pointerEventData = (PointerEventData)data;
            if (m_beginDragAction != null)
            {
                m_beginDragAction(this as T, pointerEventData);
            }
        }

        public void BindDragEvent(Action<T, PointerEventData> dragAction, params object[] arg)
        {
            if (m_dragAction != null)
            {
                m_dragAction = dragAction;
            }
            else
            {
                m_dragAction = dragAction;
                var trigger = UnityUtil.AddMonoBehaviour<EventTrigger>(gameObject);
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.Drag;
                entry.callback = new EventTrigger.TriggerEvent();
                entry.callback.AddListener(OnTriggerDrag);
                trigger.triggers.Add(entry);
            }

            SetEventParam(arg);
        }

        private void OnTriggerDrag(BaseEventData data)
        {
            var pointerEventData = (PointerEventData)data;
            if (m_dragAction != null)
            {
                m_dragAction(this as T, pointerEventData);
            }
        }

        public void BindEndDragEvent(Action<T, PointerEventData> dragendAction, params object[] arg)
        {
            if (m_endDragAction != null)
            {
                m_endDragAction = dragendAction;
            }
            else
            {
                m_endDragAction = dragendAction;
                var trigger = UnityUtil.AddMonoBehaviour<EventTrigger>(gameObject);
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.EndDrag;
                entry.callback = new EventTrigger.TriggerEvent();
                entry.callback.AddListener(OnTriggerEndDrag);
                trigger.triggers.Add(entry);
            }

            SetEventParam(arg);
        }

        private void OnTriggerEndDrag(BaseEventData data)
        {
            if (m_endDragAction != null)
            {
                m_endDragAction(this as T, (PointerEventData)data);
            }
        }

        public void BindPressEvent(Action<T, bool, PointerEventData> pressAction, params object[] arg)
        {
            if (m_pressAction != null)
            {
                m_pressAction = pressAction;
            }
            else
            {
                m_pressAction = pressAction;
                var trigger = UnityUtil.AddMonoBehaviour<EventTrigger>(gameObject);
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerDown;
                entry.callback = new EventTrigger.TriggerEvent();
                entry.callback.AddListener(OnTriggerPointerDown);
                trigger.triggers.Add(entry);
                entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerUp;
                entry.callback = new EventTrigger.TriggerEvent();
                entry.callback.AddListener(OnTriggerPointerUp);
                trigger.triggers.Add(entry);
            }

            SetEventParam(arg);
        }

        private void OnTriggerPointerUp(BaseEventData data)
        {
            if (m_pressAction != null)
            {
                m_pressAction(this as T, false, data as PointerEventData);
            }
        }

        private void OnTriggerPointerDown(BaseEventData data)
        {
            if (m_pressAction != null)
            {
                m_pressAction(this as T, true, data as PointerEventData);
            }
        }

        public void SetEventParam(params object[] arg)
        {
            m_eventParam = arg;
        }
    }
}