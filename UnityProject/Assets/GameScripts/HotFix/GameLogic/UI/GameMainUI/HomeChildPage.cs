using TEngine;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 首页子界面。
    /// </summary>
    public class HomeChildPage : UIWidget
    {
        public GameMainButton type;

        public int Index;

        private CanvasGroup _canvasGroup;

        /// <summary>
        /// 检查红点。
        /// </summary>
        /// <returns>红点状态。</returns>
        public virtual bool CheckRedNote()
        {
            return false;
        }

        /// <summary>
        /// 拖拽完成开始。
        /// </summary>
        /// <param name="isSelf"></param>
        public virtual void OnCenterOn(bool isSelf) { }
        
        /// <summary>
        /// 拖拽完成回调。
        /// </summary>
        /// <param name="isSelf"></param>
        public virtual void OnCenterOnEnd(bool isSelf) { }

        /// <summary>
        /// 激活界面回调。
        /// </summary>
        /// <param name="isActive">是否激活。</param>
        public virtual void OnActivePage(bool isActive)
        {
            SetAlpha(isActive ? 1 : 0);
        }

        /// <summary>
        /// 通过设置Alpha隐藏，减少SetActive的消耗。
        /// </summary>
        /// <param name="alpha">Alpha值。</param>
        protected void SetAlpha(float alpha)
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
            }

            _canvasGroup.alpha = alpha;
        }
    }
}
