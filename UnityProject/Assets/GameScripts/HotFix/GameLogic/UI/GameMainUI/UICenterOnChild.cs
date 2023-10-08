using System;
#if ENABLE_CANTMOVE
using System.Collections.Generic;
#endif
using TEngine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameLogic
{
    public class UICenterOnChild
    {
        private float _elasticity = 0.1f;
        private ScrollRect _scrollRect;
        private RectTransform _content;
        private EventTrigger _eventTrigger;
        private Action<int> _callback;
        private Action<int> _onEndCallback;
        private Action<int> _onDragCallback;
        private Action<int> _onEndMovingCallback;

        private int _targetIdx;
        private bool _isMoving = false;
        private Vector2 _velocity = Vector2.zero;
        private readonly Vector3[] _cornersArray = new Vector3[4];

        //用阈值处理翻页，调整背包拖动问题
        private float _checkThresholdX = 0f;
        private float _startX;

#if ENABLE_CANTMOVE
         //不能作为目标id的id列表
         private List<int> _canNotMoveIdxs = new List<int>();
#endif

        public void Init(ScrollRect scrollRect, float checkThreshold = 0f)
        {
            _scrollRect = scrollRect;
            _scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
            _scrollRect.inertia = false;
            _content = _scrollRect.content;
            _checkThresholdX = checkThreshold;

            _eventTrigger = _scrollRect.GetComponent<EventTrigger>();
            if (_eventTrigger == null)
            {
                _eventTrigger = _scrollRect.gameObject.AddComponent<EventTrigger>();
            }

            EventTrigger.Entry dragEntry = new EventTrigger.Entry();
            dragEntry.eventID = EventTriggerType.Drag;
            dragEntry.callback.AddListener(OnDrag);
            _eventTrigger.triggers.Add(dragEntry);
            EventTrigger.Entry endDragEntry = new EventTrigger.Entry();
            endDragEntry.eventID = EventTriggerType.EndDrag;
            endDragEntry.callback.AddListener(OnEndDrag);
            _eventTrigger.triggers.Add(endDragEntry);
            EventTrigger.Entry startDragEntry = new EventTrigger.Entry();
            startDragEntry.eventID = EventTriggerType.BeginDrag;
            startDragEntry.callback.AddListener(OnStartDrag);
            _eventTrigger.triggers.Add(startDragEntry);
        }

        public void Update()
        {
            if (_isMoving)
            {
                if (0 <= _targetIdx && _targetIdx < _content.childCount)
                {
                    Vector2 center = GetCenter(_scrollRect.viewport);
                    Vector2 pos = GetCenter(_content.GetChild(_targetIdx) as RectTransform);
                    Vector2 offset = pos - center;
                    _content.anchoredPosition = Vector2.SmoothDamp(
                        _content.anchoredPosition, 
                        _content.anchoredPosition - offset, 
                        ref _velocity,
                        _elasticity, 
                        float.MaxValue, 
                        GameTime.deltaTime);
                    if (_velocity.magnitude < 1)
                    {
                        if (_onEndMovingCallback != null)
                        {
                            _onEndMovingCallback(_targetIdx);
                        }
                        _isMoving = false;
                    }
                }
                else
                {
                    if (_onEndMovingCallback != null)
                    {
                        _onEndMovingCallback(_targetIdx);
                    }

                    _isMoving = false;
                }
            }
        }

        public void SetOnDragCallBack(Action<int> callback)
        {
            _onDragCallback = callback;
        }

        /// <summary>
        /// 设置回调，返回当前居中的 childIndex
        /// </summary>
        public void SetCallback(Action<int> callback)
        {
            _callback = callback;
        }

        public void SetEndCallBack(Action<int> endCallback)
        {
            _onEndCallback = endCallback;
        }

        /// <summary>
        /// 设置停止移动时回调
        /// </summary>
        /// <param name="callBack"></param>
        public void SetOnEndMovingCallback(Action<int> callBack)
        {
            _onEndMovingCallback = callBack;
        }

        /// <summary>
        /// 设置弹力，默认为0.1
        /// </summary>
        public void SetElasticity(float elasticity)
        {
            _elasticity = elasticity;
        }

        //设置不能移动的索引
        public void SetCanNotMoveId(int id)
        {
#if ENABLE_CANTMOVE
             if (!_canNotMoveIdxs.Contains(id))
             {
                 _canNotMoveIdxs.Add(id);
             }
#endif
        }

        //移除不能移动的索引
        public void RemoveCanNotMoveId(int id)
        {
#if ENABLE_CANTMOVE
             if (_canNotMoveIdxs.Contains(id))
             {
                 _canNotMoveIdxs.Remove(id);
             }
#endif
        }

        /// <summary>
        /// 居中到 childIndex
        /// </summary>
        public void CenterOnChild(int childIndex, bool isImmediately = true)
        {
            if (0 <= childIndex && childIndex < _content.childCount)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
                _targetIdx = childIndex;
                if (isImmediately)
                {
                    Vector2 center = GetCenter(_scrollRect.viewport);
                    Vector2 pos = GetCenter(_content.GetChild(_targetIdx) as RectTransform);
                    Vector2 offset = pos - center;
                    _content.anchoredPosition = _content.anchoredPosition - offset;
                }
                else
                {
                    _isMoving = true;
                }

                if (_callback != null)
                {
                    _callback(_targetIdx);
                }

                if (_onEndCallback != null)
                {
                    _onEndCallback(_targetIdx);
                }
            }
        }

        private void OnStartDrag(BaseEventData param)
        {
            _startX = _content.anchoredPosition.x;
        }

        private void OnDrag(BaseEventData param)
        {
            _isMoving = false;
            if (_onDragCallback != null)
            {
                _onDragCallback(_targetIdx);
            }
        }

        private void OnEndDrag(BaseEventData param)
        {
            if (_checkThresholdX == 0f)
            {
                FindClosest();
            }
            else
            {
                var moved = _content.anchoredPosition.x - _startX;

                int newIdx = _targetIdx;
                if (moved > _checkThresholdX)
                {
#if ENABLE_CANTMOVE
                    newIdx = Math.Max(0, m_targetIdx - 1);
#endif
                    for (int i = 0; i < _targetIdx; i++)
                    {
#if ENABLE_CANTMOVE
                        //新id不能是m_canNotBeTargetIdx
                         if (_canNotMoveIdxs.Contains(i))
                         {
                             continue;
                         }
#endif
                        newIdx = i;
                    }
                }
                else if (moved < -_checkThresholdX)
                {
#if ENABLE_CANTMOVE
                    //newIdx = Math.Min(m_content.childCount - 1, m_targetIdx + 1);
#endif
                    for (int i = _content.childCount - 1; i >= _targetIdx + 1; i--)
                    {
#if ENABLE_CANTMOVE
                         if (_canNotMoveIdxs.Contains(i))
                         {
                             continue;
                             
                         }
#endif
                        newIdx = i;
                    }
                }

                if (_targetIdx != newIdx)
                {
                    _targetIdx = newIdx;
                    if (_callback != null)
                    {
                        _callback(_targetIdx);
                    }
                }

                if (_onEndCallback != null)
                {
                    _onEndCallback(_targetIdx);
                }

                _isMoving = true;
            }
        }

        private void FindClosest()
        {
            if (_content.childCount > 0)
            {
                Vector2 center = GetCenter(_scrollRect.viewport);
                float minDist = float.MaxValue;
                for (int i = 0; i < _content.childCount; i++)
                {
#if ENABLE_CANTMOVE
                     if (_canNotMoveIdxs.Contains(i))
                     {
                         continue;
                     }
#endif
                    Vector2 pos = GetCenter(_content.GetChild(i) as RectTransform);
                    Vector2 offset = pos - center;
                    float sqrDist = Vector2.SqrMagnitude(offset);
                    if (sqrDist < minDist)
                    {
                        minDist = sqrDist;
                        _targetIdx = i;
                    }
                }

                if (_callback != null)
                {
                    _callback(_targetIdx);
                }

                _isMoving = true;
            }
        }

        private Vector2 GetCenter(RectTransform rectTransform)
        {
            Vector3 offset = _scrollRect.viewport.InverseTransformPoint(rectTransform.position);
            rectTransform.GetLocalCorners(_cornersArray);
            offset = offset + (_cornersArray[0] + _cornersArray[2]) * 0.5f;
            return offset;
        }

        public bool GetIsMoving()
        {
            return _isMoving;
        }
    }
}