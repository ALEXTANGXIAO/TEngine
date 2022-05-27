using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// 背景图片等比拉伸
    /// </summary>
    public class UIBackgroundImageStretch : MonoBehaviour
    {
        public bool m_noClip;

        private void Start()
        {
            var imageRect = GetComponent<RectTransform>();
            UIStretchUtil.Instance.DoStretch(imageRect, m_noClip);
        }
    }
}
