using System;
using TEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameLogic
{
    class UIEventItem<T> : UIWidget where T : UIEventItem<T>
    {
        private object _eventParam1;
        private object _eventParam2;
        private object _eventParam3;
        public object EventParam1 => _eventParam1;
        public object EventParam2 => _eventParam2;
        public object EventParam3 => _eventParam3;
        private Action<T> _clickAction;
        private Action<T, bool> _pressAction;
        private Action<T, PointerEventData> _beginDragAction;
        private Action<T, PointerEventData> _dragAction;
        private Action<T, PointerEventData> _endDragAction;

        public void BindClickEvent(Action<T> clickAction, object eParam1 = null, object eParam2 = null, object eParam3 = null,Selectable.Transition transition = Selectable.Transition.ColorTint)
        {
            if (_clickAction != null)
            {
                _clickAction = clickAction;
            }
            else
            {
                _clickAction = clickAction;
                var button = DUnityUtil.AddMonoBehaviour<Button>(gameObject);
                button.transition = transition;
                button.onClick.AddListener(() =>
                {
                    if (_clickAction != null)
                    {
                        _clickAction(this as T);
                    }
                });
            }
            SetEventParam(eParam1, eParam2, eParam3);
        }

        public void BindClickEventEx(Action<T> clickAction, object eParam1 = null, object eParam2 = null, object eParam3 = null)
        {
            if (_clickAction != null)
            {
                _clickAction = clickAction;
            }
            else
            {
                _clickAction = clickAction;
                var button = DUnityUtil.AddMonoBehaviour<Button>(gameObject);
                button.onClick.AddListener(() =>
                {
                    if (_clickAction != null)
                    {
                        _clickAction(this as T);
                    }
                });
            }
            SetEventParam(eParam1, eParam2, eParam3);
        }

        public void BindBeginDragEvent(Action<T, PointerEventData> dragAction, object eParam1 = null, object eParam2 = null, object eParam3 = null)
        {
            if (_beginDragAction != null)
            {
                _beginDragAction = dragAction;
            }
            else
            {
                _beginDragAction = dragAction;
                var trigger = DUnityUtil.AddMonoBehaviour<EventTrigger>(gameObject);
                EventTrigger.Entry entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.BeginDrag,
                    callback = new EventTrigger.TriggerEvent()
                };
                entry.callback.AddListener(data =>
                {
                    var pointerEventData = (PointerEventData)data;
                    if (_beginDragAction != null)
                    {
                        _beginDragAction(this as T, pointerEventData);
                    }
                });
                trigger.triggers.Add(entry);
            }
            SetEventParam(eParam1, eParam2, eParam3);
        }

        public void BindDragEvent(Action<T, PointerEventData> dragAction, object eParam1 = null, object eParam2 = null, object eParam3 = null)
        {
            if (_dragAction != null)
            {
                _dragAction = dragAction;
            }
            else
            {
                _dragAction = dragAction;
                var trigger = DUnityUtil.AddMonoBehaviour<EventTrigger>(gameObject);
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.Drag;
                entry.callback = new EventTrigger.TriggerEvent();
                entry.callback.AddListener(data =>
                {
                    var pointerEventData = (PointerEventData)data;
                    if (_dragAction != null)
                    {
                        _dragAction(this as T, pointerEventData);
                    }
                });
                trigger.triggers.Add(entry);
            }
            SetEventParam(eParam1, eParam2, eParam3);
        }

        public void BindEndDragEvent(Action<T, PointerEventData> dragendAction, object eParam1 = null, object eParam2 = null, object eParam3 = null)
        {
            if (_endDragAction != null)
            {
                _endDragAction = dragendAction;
            }
            else
            {
                _endDragAction = dragendAction;
                var trigger = DUnityUtil.AddMonoBehaviour<EventTrigger>(gameObject);
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.EndDrag;
                entry.callback = new EventTrigger.TriggerEvent();
                entry.callback.AddListener((data) =>
                {
                    if (_endDragAction != null)
                    {
                        _endDragAction(this as T, (PointerEventData)data);
                    }
                });
                trigger.triggers.Add(entry);
            }
            SetEventParam(eParam1, eParam2, eParam3);
        }

        public void BindPressEvent(Action<T, bool> pressAction, object eParam1 = null, object eParam2 = null, object eParam3 = null)
        {
            if (_pressAction != null)
            {
                _pressAction = pressAction;
            }
            else
            {
                _pressAction = pressAction;
                var trigger = DUnityUtil.AddMonoBehaviour<EventTrigger>(gameObject);
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerDown;
                entry.callback = new EventTrigger.TriggerEvent();
                entry.callback.AddListener(data =>
                {
                    if (_pressAction != null)
                    {
                        _pressAction(this as T, true);
                    }
                });
                trigger.triggers.Add(entry);
                entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerUp;
                entry.callback = new EventTrigger.TriggerEvent();
                entry.callback.AddListener((data) =>
                {
                    if (_pressAction != null)
                    {
                        _pressAction(this as T, false);
                    }
                });
                trigger.triggers.Add(entry);
            }
            SetEventParam(eParam1, eParam2, eParam3);
        }

        public void BindPressEventEx(Action<T, bool> pressAction, object eParam1 = null, object eParam2 = null, object eParam3 = null,float durationThreshold = 1)
        {
            if (_pressAction != null)
            {
                _pressAction = pressAction;
            }
            else
            {
                _pressAction = pressAction;
                var button = DUnityUtil.AddMonoBehaviour<UIButtonSuper>(gameObject);
                button.m_LongPressDurationTime = durationThreshold;
                button.onPress.AddListener(() =>
                {
                    if (_pressAction != null)
                    {
                        _pressAction(this as T, true);
                    }
                });
            }
            SetEventParam(eParam1, eParam2, eParam3);
        }

        public void SetEventParam(object eParam1, object eParam2 = null, object eParam3 = null)
        {
            _eventParam1 = eParam1;
            _eventParam2 = eParam2;
            _eventParam3 = eParam3;
        }

        public void OnTriggerBtnEvent()
        {
            if (_clickAction != null)
            {
                _clickAction(this as T);
            }
        }
    }
}