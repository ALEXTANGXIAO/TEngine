using System.Collections.Generic;
using TEngineCore;
using UnityEngine;

namespace TEngineCore
{
    public class UISys : BaseLogicSys<UISys>
    {
        public static int DesginWidth
        {
            get
            {
                return 750;
            }
        }

        public static int DesginHeight
        {
            get
            {
                return 1624;
            }
        }

        public static int ScreenWidth;
        public static int ScreenHeight;
        public bool IsLandScape { private set; get; }
        private List<IUIController> m_listController = new List<IUIController>();

        public static UIManager Mgr
        {
            get { return UIManager.Instance; }
        }

        public override void OnUpdate()
        {
            UIManager.Instance.Update();
        }

        public override bool OnInit()
        {
            base.OnInit();

            ScreenWidth = Screen.width;

            ScreenHeight = Screen.height;

            IsLandScape = ScreenWidth > ScreenHeight;

            RegistAllController();

            return true;
        }

        private void RegistAllController()
        {
            //AddController<LoadingUIController>();
        }

        private void AddController<T>() where T : IUIController, new()
        {
            for (int i = 0; i < m_listController.Count; i++)
            {
                var type = m_listController[i].GetType();

                if (type == typeof(T))
                {
                    Debug.LogError(string.Format("repeat controller type: {0}", typeof(T).Name));

                    return;
                }
            }

            var controller = new T();

            m_listController.Add(controller);

            controller.ResigterUIEvent();
        }

        public static void ShowTipMsg(string str)
        {

        }
    }

    public static class CanvasUtils
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