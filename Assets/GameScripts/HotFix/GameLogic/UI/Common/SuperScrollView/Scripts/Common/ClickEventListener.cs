using UnityEngine;
using UnityEngine.EventSystems;

namespace GameLogic
{
    public class ClickEventListener : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public static ClickEventListener Get(GameObject obj)
        {
            ClickEventListener listener = obj.GetComponent<ClickEventListener>();
            if (listener == null)
            {
                listener = obj.AddComponent<ClickEventListener>();
            }

            return listener;
        }

        private System.Action<GameObject> _clickedHandler = null;
        private System.Action<GameObject> _doubleClickedHandler = null;
        private System.Action<GameObject> _onPointerDownHandler = null;
        private System.Action<GameObject> _onPointerUpHandler = null;
        bool _isPressed = false;

        public bool IsPressed => _isPressed;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2)
            {
                if (_doubleClickedHandler != null)
                {
                    _doubleClickedHandler(gameObject);
                }
            }
            else
            {
                if (_clickedHandler != null)
                {
                    _clickedHandler(gameObject);
                }
            }
        }

        public void SetClickEventHandler(System.Action<GameObject> handler)
        {
            _clickedHandler = handler;
        }

        public void SetDoubleClickEventHandler(System.Action<GameObject> handler)
        {
            _doubleClickedHandler = handler;
        }

        public void SetPointerDownHandler(System.Action<GameObject> handler)
        {
            _onPointerDownHandler = handler;
        }

        public void SetPointerUpHandler(System.Action<GameObject> handler)
        {
            _onPointerUpHandler = handler;
        }


        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            if (_onPointerDownHandler != null)
            {
                _onPointerDownHandler(gameObject);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            if (_onPointerUpHandler != null)
            {
                _onPointerUpHandler(gameObject);
            }
        }
    }
}