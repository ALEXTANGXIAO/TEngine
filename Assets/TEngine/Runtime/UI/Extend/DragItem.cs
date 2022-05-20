using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace UI
{
    public enum UIDragType
    {
        Draging,
        Drop
    }

    public class DragItem : UIEventItem<DragItem>
    {
        private UIDragType m_DragState = UIDragType.Drop;
        private Vector3 m_ItemOldPos;
        private Vector3 m_ItemCachePos;
        private bool m_CanDrag = false;

        /// <summary>
        /// 是否可以拖拽
        /// </summary>
        public bool CanDrag
        {
            get
            {
                return m_CanDrag;
            }
            set
            {
                m_CanDrag = value;
                if (m_CanDrag)
                {
                    BindDrag();
                }
            }
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            BindDrag();
        }

        private void BindDrag()
        {
            if (m_CanDrag)
            {
                BindBeginDragEvent(delegate (DragItem item, PointerEventData data)
                {
                    if (!m_CanDrag)
                    {
                        return;
                    }
                    StartDragItem(UIDragType.Draging);
                });

                BindEndDragEvent(delegate (DragItem item, PointerEventData data)
                {
                    if (!m_CanDrag)
                    {
                        return;
                    }
                    EndDrag();
                });
            }
        }

        protected override void OnUpdate()
        {
            if (!m_CanDrag)
            {
                return;
            }
            UpdateDragPos();
        }

        private void StartDragItem(UIDragType type)
        {
            if (type != UIDragType.Drop)
            {
                m_ItemOldPos = transform.position;
                Vector3 pos;
                UISys.Mgr.GetMouseDownUiPos(out pos);
                m_ItemCachePos = pos;
                UpdateDragPos();
                m_DragState = type;
            }
        }

        private void EndDrag()
        {
            m_DragState = UIDragType.Drop;
            transform.position = m_ItemOldPos;
#if UNITY_EDITOR
            //Debug.LogError("m_ItemCachePos.y - m_ItemOldPos.y " + (m_ItemCachePos.y - m_ItemOldPos.y));
#endif
            if (m_ItemCachePos.y - m_ItemOldPos.y > 3)
            {

            }
        }

        private void UpdateDragPos()
        {
            if (m_DragState == UIDragType.Drop)
            {
                return;
            }

            Vector3 pos;
            UISys.Mgr.GetMouseDownUiPos(out pos);
            transform.position += (pos - m_ItemCachePos);
            m_ItemCachePos = pos;
        }
    }
}