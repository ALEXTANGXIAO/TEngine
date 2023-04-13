using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TEngine
{
    public class DragHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private OnDragEvent _onDrag;
        private OnDragEvent _onBeginDrag;
        private OnDragEvent _onEndDrag;
        private bool _isOnDrag = false;
        private RectTransform _rect;

        private void Start()
        {
            _rect = (transform.parent) ? transform.parent.GetComponent<RectTransform>() : null;
        }

        public OnDragEvent onDrag
        {
            get
            {
                if (_onDrag == null)
                {
                    _onDrag = new OnDragEvent();
                }
                return _onDrag;
            }
        }

        public OnDragEvent onBeginDrag
        {
            get
            {
                if (_onBeginDrag == null)
                {
                    _onBeginDrag = new OnDragEvent();
                }
                return _onBeginDrag;
            }
        }

        public OnDragEvent onEndDrag
        {
            get
            {
                if (_onEndDrag == null)
                {
                    _onEndDrag = new OnDragEvent();
                }
                return _onEndDrag;
            }
        }

        public static DragHandler Get(GameObject go)
        {
            DragHandler listener = go.GetComponent<DragHandler>();
            if (listener == null) listener = go.AddComponent<DragHandler>();
            return listener;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            _isOnDrag = true;
            if (onDrag != null)
            {
                Vector3 position = new Vector3(eventData.position.x, eventData.position.y, 0);
                if (_rect)
                {
                    RectTransformUtility.ScreenPointToWorldPointInRectangle(_rect, eventData.position, eventData.pressEventCamera, out position);
                    position.z = 0;
                }
                onDrag.Invoke(position, eventData);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            _isOnDrag = true;
            if (onBeginDrag != null)
            {
                Vector3 position = new Vector3(eventData.position.x, eventData.position.y, 0);
                if (_rect)
                {
                    RectTransformUtility.ScreenPointToWorldPointInRectangle(_rect, eventData.position, eventData.pressEventCamera, out position);
                    //position.z = eventData.pressEventCamera.transform.position.z;
                    position.z = 0;
                }
                onBeginDrag.Invoke(position, eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !_isOnDrag) return;
            if (onEndDrag != null)
            {
                Vector3 position = new Vector3(eventData.position.x, eventData.position.y, 0);
                if (_rect)
                {
                    RectTransformUtility.ScreenPointToWorldPointInRectangle(_rect, eventData.position, eventData.pressEventCamera, out position);
                    position.z = 0;
                }
                onEndDrag.Invoke(position, eventData);
            }
            _isOnDrag = false;
        }
    }

    public class OnDragEvent : UnityEvent<Vector3, PointerEventData>
    {

    }
}
