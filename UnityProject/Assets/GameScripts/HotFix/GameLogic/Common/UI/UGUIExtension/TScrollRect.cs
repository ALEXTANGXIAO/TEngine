using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameLogic
{
    [AddComponentMenu("UI/TScrollRect Rect", 37)]
    [SelectionBase]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof (RectTransform))]
    public class TScrollRect : ScrollRect
    {
        private event Action ActionBeginDrag;
        private event Action ActionOnDrag;
        private event Action ActionEndDrag;

        public List<ScrollRect> parentScrollRectList;

        protected override void Awake()
        {
            base.Awake();
            if (parentScrollRectList != null && parentScrollRectList.Count > 0)
            {
                for (int i = 0; i < parentScrollRectList.Count; i++)
                {
                    AddParentScrollRect(parentScrollRectList[i]);
                }
            }
        }

        private List<ScrollRect> m_listParentScrollRect;

        public void AddParentScrollRect(ScrollRect parentScrollRect)
        {
            if (parentScrollRect == null)
            {
                return;
            }

            if (m_listParentScrollRect == null)
            {
                m_listParentScrollRect = new List<ScrollRect>();
            }

            if (!m_listParentScrollRect.Contains(parentScrollRect))
            {
                m_listParentScrollRect.Add(parentScrollRect);
            }
        }

        public void AddBeginDragListener(Action action)
        {
            ActionBeginDrag += action;
        }

        public void RemoveBeginDragListener(Action action)
        {
            ActionBeginDrag -= action;
        }

        public void AddOnDragListener(Action action)
        {
            ActionOnDrag += action;
        }

        public void RemoveOnDragListener(Action action)
        {
            ActionOnDrag -= action;
        }

        public void AddEndDragListener(Action action)
        {
            ActionEndDrag += action;
        }

        public void RemoveEndDragListener(Action action)
        {
            ActionEndDrag -= action;
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            if (ActionBeginDrag != null)
            {
                ActionBeginDrag();
            }

            if (m_listParentScrollRect != null)
            {
                for (int i = 0; i < m_listParentScrollRect.Count; i++)
                {
                    var parentScrollRect = m_listParentScrollRect[i];
                    parentScrollRect.OnBeginDrag(eventData);
                }
            }
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
            if (ActionOnDrag != null)
            {
                ActionOnDrag();
            }

            if (m_listParentScrollRect != null)
            {
                for (int i = 0; i < m_listParentScrollRect.Count; i++)
                {
                    var parentScrollRect = m_listParentScrollRect[i];
                    parentScrollRect.OnDrag(eventData);
                }
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            if (ActionEndDrag != null)
            {
                ActionEndDrag();
            }

            if (m_listParentScrollRect != null)
            {
                for (int i = 0; i < m_listParentScrollRect.Count; i++)
                {
                    var parentScrollRect = m_listParentScrollRect[i];
                    parentScrollRect.OnEndDrag(eventData);
                }
            }
        }
    }
}