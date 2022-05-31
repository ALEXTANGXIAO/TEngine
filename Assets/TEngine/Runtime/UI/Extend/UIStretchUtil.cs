using UnityEngine;
using UnityEngine.UI;

namespace TEngine
{
    public class UIStretchUtil : TSingleton<UIStretchUtil>
    {
        private bool m_isInit;
        private Vector2 m_canvasSize;
        private float m_screenAspect;

        public bool Init()
        {
            var goRoot = GameObject.Find("UIRoot/Canvas");
            if (goRoot == null)
            {
                TLogger.LogError("找不到 goRoot");
                return false;
            }
            var canvasScaler = goRoot.GetComponent<CanvasScaler>();
            if (canvasScaler == null)
            {
                TLogger.LogError("找不到 CanvasScaler");
                return false;
            }

            m_screenAspect = Screen.width / (float)Screen.height;
            float designScale = canvasScaler.referenceResolution.x / canvasScaler.referenceResolution.y;
            if (m_screenAspect > designScale)
            {
                m_canvasSize.y = canvasScaler.referenceResolution.y;
                m_canvasSize.x = m_canvasSize.y * Screen.width / Screen.height;
            }
            else
            {
                m_canvasSize.x = canvasScaler.referenceResolution.x;
                m_canvasSize.y = m_canvasSize.x * Screen.height / Screen.width;
            }
            TLogger.LogWarning("referenceResolution = "+ canvasScaler.referenceResolution);
            TLogger.LogWarning("m_canvasSize = " + m_canvasSize);
            m_isInit = true;
            return true;
        }

        public void DoStretch(RectTransform rectTransform, bool noClip)
        {
            if (!m_isInit)
            {
                if (!Init())
                {
                    return;
                }
            }
            if (rectTransform != null && rectTransform.sizeDelta.x > 0 && rectTransform.sizeDelta.y > 0)
            {
                Vector2 rectSize = rectTransform.sizeDelta;
                var rectAspect = rectSize.x / rectSize.y;
                if (!noClip && rectAspect > m_screenAspect ||
                    noClip && rectAspect < m_screenAspect)
                {
                    //以高为标准
                    float scale = m_canvasSize.y / rectSize.y;
                    rectTransform.localScale = new Vector3(scale, scale, 1f);
                }
                else
                {
                    //以宽为标准
                    float scale = m_canvasSize.x / rectSize.x;
                    rectTransform.localScale = new Vector3(scale, scale, 1f);
                }
            }
        }
    }
}
