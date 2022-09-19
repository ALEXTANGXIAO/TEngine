using TEngine.Runtime;
using UnityEngine;

namespace TEngine.Runtime.Actor
{
    public class ActorComponentPool: BehaviourSingleton<ActorComponentPool>
    {
        private ActorComponent _listComponentHead;
        private ActorComponent _listStartComponentHead;
        private ActorTimerMgr _timerMgr;
        public override void Awake()
        {
            _timerMgr = ActorTimerMgr.Instance;
        }
        
        
        public T AllocComponent<T>() where T : ActorComponent,new()
        {
            var component = new T();
            AddToList(ref _listStartComponentHead, component);

            return component;
        }

        public void FreeComponent(ActorComponent component)
        {
            if (component.m_destroy)
            {
                return;
            }

            component.m_destroy = true;
            if (!component.m_callStart)
            {
                RmvFromList(ref _listStartComponentHead, component);

                Debug.Assert(GameTimer.IsNull(component.m_updateTimer));
                Debug.Assert(GameTimer.IsNull(component.m_lateUpdateTimer));
            }
            else
            {
                RmvFromList(ref _listComponentHead, component);

                var timerMgr = _timerMgr;
                timerMgr.DestroyTimer(ref component.m_updateTimer);
                timerMgr.DestroyTimer(ref component.m_lateUpdateTimer);
            }

            Debug.Assert(component.m_next == null && component.m_prev == null);
        }
        
        public override void Update()
        {
            var timerMgr = _timerMgr;

            while (_listStartComponentHead != null)
            {
                var start = _listStartComponentHead;
                Debug.Assert(!start.m_destroy);
                start.CallStart();
                if (start.m_destroy)
                {
                    Debug.Assert(_listStartComponentHead != start);
                    continue;
                }

                RmvFromList(ref _listStartComponentHead, start);
                AddToList(ref _listComponentHead, start);
                start.AfterOnStartAction(timerMgr);
            }
        }

        public void RequestOnceUpdate(ActorComponent component)
        {
            component.m_updateTimer = _timerMgr.CreateOnceFrameTimer(component.GetType().FullName, component.Update);
        }

        public void RequestLoopUpdate(ActorComponent component, string detail = null)
        {
            Debug.Assert(GameTimer.IsNull(component.m_updateTimer));
            var timerName = component.GetType().FullName;

#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(detail))
            {
                timerName += ("_" + detail);
            }
#endif

            _timerMgr.CreateLoopFrameTimer(ref component.m_updateTimer, timerName, component.Update);
        }

        public void StopLoopUpdate(ActorComponent component)
        {
            _timerMgr.DestroyTimer(ref component.m_updateTimer);
        }

        public void RequestLoopLateUpdate(ActorComponent component)
        {
            Debug.Assert(GameTimer.IsNull(component.m_lateUpdateTimer));
            _timerMgr.CreateLoopFrameLateTimer(ref component.m_lateUpdateTimer, component.GetType().FullName, component.LateUpdate);
        }

        public void StopLoopLateUpdate(ActorComponent component)
        {
            _timerMgr.DestroyTimer(ref component.m_lateUpdateTimer);
        }

        private void AddToList(ref ActorComponent head, ActorComponent component)
        {
            if (head != null)
            {
                component.m_next = head;
                head.m_prev = component;
            }

            head = component;
        }

        private void RmvFromList(ref ActorComponent head, ActorComponent component)
        {
            var prev = component.m_prev;
            var next = component.m_next;

            if (prev != null)
            {
                prev.m_next = next;
            }

            if (next != null)
            {
                next.m_prev = prev;
            }
            
            component.m_next = null;
            component.m_prev = null;

            if (component == head)
            {
                head = next;
            }
            else
            {
                Debug.Assert(prev != null);
            }
        }
    }
}