using System;
using TEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameLogic
{
    public class UIEventItem<T> : UIWidget where T : UIEventItem<T>
    {
        private object m_eventParam1;
        private object m_eventParam2;
        private object m_eventParam3;
        public object EventParam1 => m_eventParam1;

        public object EventParam2 => m_eventParam2;

        public object EventParam3 => m_eventParam3;
        private Action<T> m_clickAction;
        private Action<T, bool> m_pressAction;
        private Action<T, PointerEventData> m_beginDragAction;
        private Action<T, PointerEventData> m_dragAction;
        private Action<T, PointerEventData> m_endDragAction;

        public void BindClickEvent(Action<T> clickAction, object eParam1 = null, object eParam2 = null, object eParam3 = null,
            Selectable.Transition transition = Selectable.Transition.ColorTint)
        {
            if (m_clickAction != null)
            {
                m_clickAction = clickAction;
            }
            else
            {
                m_clickAction = clickAction;
                var button = gameObject.GetOrAddComponent<Button>();
                button.transition = transition;
                button.onClick.AddListener(() =>
                {
                    if (m_clickAction != null)
                    {
                        m_clickAction(this as T);
                    }
                });
            }

            SetEventParam(eParam1, eParam2, eParam3);
        }

        public void BindClickEventEx(Action<T> clickAction, object eParam1 = null, object eParam2 = null, object eParam3 = null)
        {
            if (m_clickAction != null)
            {
                m_clickAction = clickAction;
            }
            else
            {
                m_clickAction = clickAction;
                var button = gameObject.GetOrAddComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    if (m_clickAction != null)
                    {
                        m_clickAction(this as T);
                    }
                });
            }

            SetEventParam(eParam1, eParam2, eParam3);
        }

        public void BindBeginDragEvent(Action<T, PointerEventData> dragAction, object eParam1 = null, object eParam2 = null, object eParam3 = null)
        {
            if (m_beginDragAction != null)
            {
                m_beginDragAction = dragAction;
            }
            else
            {
                m_beginDragAction = dragAction;
                var trigger = gameObject.GetOrAddComponent<EventTrigger>();
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.BeginDrag;
                entry.callback = new EventTrigger.TriggerEvent();
                entry.callback.AddListener((data) =>
                {
                    var pointerEventData = (PointerEventData)data;
                    if (m_beginDragAction != null)
                    {
                        m_beginDragAction(this as T, pointerEventData);
                    }
                });
                trigger.triggers.Add(entry);
            }

            SetEventParam(eParam1, eParam2, eParam3);
        }

        public void BindDragEvent(Action<T, PointerEventData> dragAction, object eParam1 = null, object eParam2 = null, object eParam3 = null)
        {
            if (m_dragAction != null)
            {
                m_dragAction = dragAction;
            }
            else
            {
                m_dragAction = dragAction;
                var trigger = gameObject.GetOrAddComponent<EventTrigger>();
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.Drag;
                entry.callback = new EventTrigger.TriggerEvent();
                entry.callback.AddListener((data) =>
                {
                    var pointerEventData = (PointerEventData)data;
                    if (m_dragAction != null)
                    {
                        m_dragAction(this as T, pointerEventData);
                    }
                });
                trigger.triggers.Add(entry);
            }

            SetEventParam(eParam1, eParam2, eParam3);
        }

        public void BindEndDragEvent(Action<T, PointerEventData> dragendAction, object eParam1 = null, object eParam2 = null, object eParam3 = null)
        {
            if (m_endDragAction != null)
            {
                m_endDragAction = dragendAction;
            }
            else
            {
                m_endDragAction = dragendAction;
                var trigger = gameObject.GetOrAddComponent<EventTrigger>();
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.EndDrag;
                entry.callback = new EventTrigger.TriggerEvent();
                entry.callback.AddListener((data) =>
                {
                    if (m_endDragAction != null)
                    {
                        m_endDragAction(this as T, (PointerEventData)data);
                    }
                });
                trigger.triggers.Add(entry);
            }

            SetEventParam(eParam1, eParam2, eParam3);
        }

        public void BindPressEvent(Action<T, bool> pressAction, object eParam1 = null, object eParam2 = null, object eParam3 = null)
        {
            if (m_pressAction != null)
            {
                m_pressAction = pressAction;
            }
            else
            {
                m_pressAction = pressAction;
                var trigger = gameObject.GetOrAddComponent<EventTrigger>();
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerDown;
                entry.callback = new EventTrigger.TriggerEvent();
                entry.callback.AddListener((data) =>
                {
                    if (m_pressAction != null)
                    {
                        m_pressAction(this as T, true);
                    }
                });
                trigger.triggers.Add(entry);
                entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerUp;
                entry.callback = new EventTrigger.TriggerEvent();
                entry.callback.AddListener((data) =>
                {
                    if (m_pressAction != null)
                    {
                        m_pressAction(this as T, false);
                    }
                });
                trigger.triggers.Add(entry);
            }

            SetEventParam(eParam1, eParam2, eParam3);
        }

        public void BindPressEventEx(Action<T, bool> pressAction, object eParam1 = null, object eParam2 = null, object eParam3 = null,
            float durationThreshold = 1)
        {
            if (m_pressAction != null)
            {
                m_pressAction = pressAction;
            }
            else
            {
                m_pressAction = pressAction;
                var button = gameObject.GetOrAddComponent<UIButtonSuper>();
                button.m_LongPressDurationTime = durationThreshold;
                button.onPress.AddListener(() =>
                {
                    if (m_pressAction != null)
                    {
                        m_pressAction(this as T, true);
                    }
                });
            }

            SetEventParam(eParam1, eParam2, eParam3);
        }

        public void SetEventParam(object eParam1, object eParam2 = null, object eParam3 = null)
        {
            m_eventParam1 = eParam1;
            m_eventParam2 = eParam2;
            m_eventParam3 = eParam3;
        }

        public void OnTriggerBtnEvent()
        {
            if (m_clickAction != null)
            {
                m_clickAction(this as T);
            }
        }
    }
}