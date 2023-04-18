using UnityEngine;
using TEngine;

namespace GameLogic
{
    public class SafeTop : MonoBehaviour
    {
        void Start()
        {
            var topRect = (gameObject.transform as RectTransform);
            var safeArea = UnityEngine.Screen.safeArea;
            if (topRect != null)
            {
                var anchoredPosition = topRect.anchoredPosition;
                anchoredPosition = new Vector2(anchoredPosition.x,anchoredPosition.y - safeArea.y);
                topRect.anchoredPosition = anchoredPosition;
            }
            Log.Debug(safeArea);
        }
    }
}
