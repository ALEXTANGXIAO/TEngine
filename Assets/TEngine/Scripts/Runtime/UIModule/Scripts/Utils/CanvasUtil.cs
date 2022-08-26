using UnityEngine;

namespace TEngine.Runtime.UIModule
{
    public static class CanvasUtil
    {
        public static void SetMax(this RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return;
            }
            rectTransform.localPosition = new Vector3(0, 0, 0);
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;

            rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
            rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
        }

        /// <summary>
        /// 调整 RectTransform 组件中的 Left、Bottom 属性
        /// </summary>
        /// <param name="rt">引用目标 RectTransform 对象</param>
        /// <param name="left">Left值</param>
        /// <param name="bottom">Bottom值</param>
        public static void LeftBottom(RectTransform rectTransform, float left, float bottom)
        {
            Vector2 size = rectTransform.rect.size;
            rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, left, size.x);
            rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, bottom, size.y);
        }
        /// <summary>
        /// 调整 RectTransform 组件中的 Left、Top 属性
        /// </summary>
        /// <param name="rt"></param>
        /// <param name="left">Left值</param>
        /// <param name="top">Top值</param>
        public static void LeftTop(RectTransform rectTransform, float left, float top)
        {
            Vector2 size = rectTransform.rect.size;
            rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, left, size.x);
            rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, top, size.y);
        }
        /// <summary>
        /// 调整 RectTransform 组件中的 Right、Bottom 属性
        /// </summary>
        /// <param name="rt"></param>
        /// <param name="right">Right值</param>
        /// <param name="bottom">Bottom值</param>
        public static void RightBottom(RectTransform rectTransform, float right, float bottom)
        {
            Vector2 size = rectTransform.rect.size;
            rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, right, size.x);
            rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, bottom, size.y);
        }
        /// <summary>
        /// 调整 RectTransform 组件中的 Right、Top 属性
        /// </summary>
        /// <param name="rt"></param>
        /// <param name="right">Right值</param>
        /// <param name="top">Top值</param>
        public static void RightTop(RectTransform rectTransform, float right, float top)
        {
            Vector2 size = rectTransform.rect.size;
            rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, right, size.x);
            rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, top, size.y);
        }
        public static void SetCenter(this RectTransform rectTransform, float x = 0, float y = 0)
        {
            rectTransform.localPosition = new Vector3(0, 0, 0);
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;

            Vector2 size = rectTransform.rect.size;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            rectTransform.localPosition = new Vector2(x, y);
        }
    }
}