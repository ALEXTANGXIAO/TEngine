using UnityEngine;
using TEngine;

namespace TEngine
{
    public class SafeTop : MonoBehaviour
    {
        void Start()
        {
            var topRect = gameObject.transform as RectTransform;
            CheckNotch(true);
            if (topRect != null)
            {
                var anchoredPosition = topRect.anchoredPosition;
                anchoredPosition = new Vector2(anchoredPosition.x, anchoredPosition.y - _notchHeight);
                topRect.anchoredPosition = anchoredPosition;
            }
        }

        private static float _notchHeight;

        public static void CheckNotch(bool applyEditorNotch = true)
        {
#if UNITY_EDITOR
            _notchHeight = applyEditorNotch ? Screen.safeArea.y > 0f ? Screen.safeArea.y : Screen.currentResolution.height - Screen.safeArea.height : 0f;
            if (_notchHeight < 0)
            {
                _notchHeight = 0;
            }
#else
            _notchHeight = Screen.safeArea.y > 0f ? Screen.safeArea.y : Screen.currentResolution.height - Screen.currentResolution.height;
#endif
            Debug.Log($"CheckNotch :{_notchHeight}");
        }
    }
}