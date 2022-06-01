using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// 背景图片等比拉伸
    /// </summary>
    public class UIBackgroundImageStretch : MonoBehaviour
    {
        public bool NoClip;

        private void Start()
        {
            var imageRect = GetComponent<RectTransform>();
            UIStretchUtil.Instance.DoStretch(imageRect, NoClip);
        }
    }
}
