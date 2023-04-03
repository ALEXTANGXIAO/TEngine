using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// 默认界面辅助器。
    /// </summary>
    public class DefaultUIWindowHelper : UIWindowHelperBase
    {
        private ResourceComponent m_ResourceComponent = null;

        private Vector2 m_Half = new Vector2(0.5f,0.5f);

        private int m_UILayer;

        private void Start()
        {
            m_UILayer = LayerMask.NameToLayer("UI");
            m_ResourceComponent = GameEntry.GetComponent<ResourceComponent>();
            if (m_ResourceComponent == null)
            {
                Log.Fatal("Resource component is invalid.");
                return;
            }
        }

        /// <summary>
        /// 实例化界面。
        /// </summary>
        /// <param name="uiWindowAsset">要实例化的界面资源。</param>
        public override object InstantiateUIWindow(object uiWindowAsset)
        {
            return Instantiate((Object)uiWindowAsset);
        }

        /// <summary>
        /// 创建界面。
        /// </summary>
        /// <param name="uiWindowInstance">界面实例。</param>
        /// <param name="uiGroup">界面所属的界面组。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面。</returns>
        public override UIWindow CreateUIWindow(object uiWindowInstance, IUIGroup uiGroup, object userData)
        {
            GameObject obj = uiWindowInstance as GameObject;
            if (obj == null)
            {
                Log.Error("UI form instance is invalid.");
                return null;
            }

            Transform trans = obj.transform;
            trans.SetParent(((MonoBehaviour)uiGroup.Helper).transform);
            trans.localScale = Vector3.one;
            trans.localPosition = Vector3.zero;
            obj.layer = m_UILayer;
            
            RectTransform rectTransform = obj.GetComponent<RectTransform>();
            rectTransform.anchorMin = m_Half;
            rectTransform.anchorMax = m_Half;
            rectTransform.anchoredPosition = Vector2.zero;
            // return obj.GetOrAddComponent<UIWindow>();
            return null;
        }

        /// <summary>
        /// 释放界面。
        /// </summary>
        /// <param name="uiWindowAsset">要释放的界面资源。</param>
        /// <param name="uiWindowInstance">要释放的界面实例。</param>
        public override void ReleaseUIWindow(object uiWindowAsset, object uiWindowInstance)
        {
            // m_ResourceComponent.UnloadAsset(uiWindowAsset);
            Destroy((Object)uiWindowInstance);
        }
    }
}
