using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace TEngine
{
    /// <summary>
    /// UI组件。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed partial class UIComponent : GameFrameworkComponent
    {
        private const int DefaultPriority = 0;
        
        [SerializeField]
        private Transform m_InstanceRoot = null;
        
        [SerializeField]
        private Camera m_UICamera = null;
        
        [SerializeField]
        private string m_UIWindowHelperTypeName = "TEngine.DefaultUIWindowHelper";

        [SerializeField]
        private UIWindowHelperBase mCustomUIWindowHelper = null;

        [SerializeField]
        private string m_UIGroupHelperTypeName = "TEngine.DefaultUIGroupHelper";

        [SerializeField]
        private UIGroupHelperBase m_CustomUIGroupHelper = null;
        
        [SerializeField]
        private UIGroup[] m_UIGroups = null;
        
        public const int GROUP_DEEP = 10000;
        public const int WINDOWS_DEEP = 100;
        
        /// <summary>
        /// UI根节点。
        /// </summary>
        public Transform UIRoot => m_InstanceRoot;

        /// <summary>
        /// UI根节点。
        /// </summary>
        public Camera UICamera => m_UICamera;
        
        /// <summary>
        /// 获取界面组数量。
        /// </summary>
        public int UIGroupCount => m_UIGroups?.Length ?? 0;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            RootComponent rootComponent = GameEntry.GetComponent<RootComponent>();
            if (rootComponent == null)
            {
                Log.Fatal("Base component is invalid.");
                return;
            }
            
            UIWindowHelperBase uiWindowHelper = Helper.CreateHelper(m_UIWindowHelperTypeName, mCustomUIWindowHelper);
            if (uiWindowHelper == null)
            {
                Log.Error("Can not create UI form helper.");
                return;
            }

            uiWindowHelper.name = "UI Form Helper";
            Transform transform = uiWindowHelper.transform;
            transform.SetParent(this.transform);
            transform.localScale = Vector3.one;
            
            if (m_InstanceRoot == null)
            {
                m_InstanceRoot = new GameObject("UI Form Instances").transform;
                m_InstanceRoot.SetParent(gameObject.transform);
                m_InstanceRoot.localScale = Vector3.one;
            }

            m_InstanceRoot.gameObject.layer = LayerMask.NameToLayer("UI");

            for (int i = 0; i < m_UIGroups.Length; i++)
            {
                if (!AddUIGroup(m_UIGroups[i].Name, m_UIGroups[i].Depth))
                {
                    Log.Warning("Add UI group '{0}' failure.", m_UIGroups[i].Name);
                    continue;
                }
            }
        }
        
        /// <summary>
        /// 增加界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="depth">界面组深度。</param>
        /// <returns>是否增加界面组成功。</returns>
        public bool AddUIGroup(string uiGroupName, int depth)
        {
            UIGroupHelperBase uiGroupHelper = Helper.CreateHelper(m_UIGroupHelperTypeName, m_CustomUIGroupHelper, m_UIGroups.Length);
            if (uiGroupHelper == null)
            {
                Log.Error("Can not create UI group helper.");
                return false;
            }

            uiGroupHelper.name = Utility.Text.Format("UI Group - {0}", uiGroupName);
            uiGroupHelper.gameObject.layer = LayerMask.NameToLayer("UI");
            Transform transform = uiGroupHelper.transform;
            transform.SetParent(m_InstanceRoot);
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;

            return true;
        }
    }
}