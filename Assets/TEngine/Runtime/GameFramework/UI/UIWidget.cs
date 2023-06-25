using System.Collections.Generic;
using UnityEngine;

namespace TEngine
{
    public abstract class UIWidget:UIBase,IUIBehaviour
    {
        /// <summary>
        /// 窗口组件的实例资源对象。
        /// </summary>
        public override GameObject gameObject { protected set; get; }
        
        /// <summary>
        /// 窗口组件矩阵位置组件。
        /// </summary>
        public override RectTransform rectTransform { protected set; get; }

        /// <summary>
        /// 窗口组件名称。
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string name { private set; get; } = nameof(UIWidget);
        
        /// <summary>
        /// UI类型。
        /// </summary>
        public override UIBaseType BaseType => UIBaseType.Widget;
        
        /// <summary>
        /// 所属的窗口。
        /// </summary>
        public UIWindow OwnerWindow
        {
            get
            {
                var parentUI = base.parent;
                while (parentUI != null)
                {
                    if (parentUI.BaseType == UIBaseType.Window)
                    {
                        return parentUI as UIWindow;
                    }

                    parentUI = parentUI.Parent;
                }

                return null;
            }
        }
        
        internal bool InternalUpdate()
        {
            if (!IsPrepare)
            {
                return false;
            }
            
            List<UIWidget> listNextUpdateChild = null;
            if (ListChild != null && ListChild.Count > 0)
            {
                listNextUpdateChild = m_listUpdateChild;
                var updateListValid = m_updateListValid;
                List<UIWidget> listChild = null;
                if (!updateListValid)
                {
                    if (listNextUpdateChild == null)
                    {
                        listNextUpdateChild = new List<UIWidget>();
                        m_listUpdateChild = listNextUpdateChild;
                    }
                    else
                    {
                        listNextUpdateChild.Clear();
                    }

                    listChild = ListChild;
                }
                else
                {
                    listChild = listNextUpdateChild;
                }

                for (int i = 0; i < listChild.Count; i++)
                {
                    var uiWidget = listChild[i];

                    TProfiler.BeginSample(uiWidget.name);
                    var needValid = uiWidget.InternalUpdate();
                    TProfiler.EndSample();

                    if (!updateListValid && needValid)
                    {
                        listNextUpdateChild.Add(uiWidget);
                    }
                }

                if (!updateListValid) 
                {
                    m_updateListValid = true;
                }
            }

            TProfiler.BeginSample("OnUpdate");

            bool needUpdate = false;
            if (listNextUpdateChild == null || listNextUpdateChild.Count <= 0)
            {
                HasOverrideUpdate = true;
                OnUpdate();
                needUpdate = HasOverrideUpdate;
            }
            else
            {
                OnUpdate();
                needUpdate = true;
            }
            TProfiler.EndSample();
                
            return needUpdate;
        }

        #region Create
        /// <summary>
        /// 创建窗口内嵌的界面。
        /// </summary>
        /// <param name="parentUI">父节点UI。</param>
        /// <param name="widgetRoot">组件根节点。</param>
        /// <param name="visible">是否可见。</param>
        /// <returns></returns>
        public bool Create(UIBase parentUI, GameObject widgetRoot, bool visible = true)
        {
            return CreateImp(parentUI, widgetRoot, false, visible);
        }
        
        /// <summary>
        /// 根据prefab或者模版来创建新的 widget。
        /// </summary>
        /// <param name="parentUI"></param>
        /// <param name="goPrefab"></param>
        /// <param name="parentTrans"></param>
        /// <param name="visible"></param>
        /// <returns></returns>
        public bool CreateByPrefab(UIBase parentUI, GameObject goPrefab, Transform parentTrans, bool visible = true)
        {
            if (parentTrans == null)
            {
                parentTrans = parentUI.rectTransform;
            }
            return CreateImp(parentUI, Object.Instantiate(goPrefab, parentTrans), true, visible);
        }

        private bool CreateImp(UIBase parentUI, GameObject widgetRoot, bool bindGo, bool visible = true)
        {
            if (!CreateBase(widgetRoot, bindGo))
            {
                return false;
            }
            AssetReference = AssetReference.BindAssetReference(widgetRoot,parent:parentUI.AssetReference);
            RestChildCanvas(parentUI);
            parent = parentUI;
            Parent.ListChild.Add(this);
            ScriptGenerator();
            BindMemberProperty();
            RegisterEvent();
            OnCreate();
            IsPrepare = true;
            
            if (!visible)
            {
                gameObject.SetActive(false);
            }
            return true;
        }
        
        protected bool CreateBase(GameObject go, bool bindGo)
        {
            if (go == null)
            {
                return false;
            }
            gameObject = go;
            rectTransform = go.GetComponent<RectTransform>();
            Log.Assert(rectTransform != null, $"{go.name} ui base element need to be RectTransform");
            return true;
        }
        
        protected void RestChildCanvas(UIBase parentUI)
        {
            Canvas parentCanvas = parentUI.gameObject.GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                return;
            }

            if (gameObject != null)
            {
                var listCanvas = gameObject.GetComponentsInChildren<Canvas>(true);
                for (var index = 0; index < listCanvas.Length; index++)
                {
                    var childCanvas = listCanvas[index];
                    childCanvas.sortingOrder = parentCanvas.sortingOrder + childCanvas.sortingOrder % UIModule.WINDOW_DEEP;
                }
            }
        }
        #endregion

        #region Destroy

        /// <summary>
        /// 组件被销毁调用。
        /// <remarks>请勿手动调用！</remarks>
        /// </summary>
        public void OnDestroyWidget()
        {
            RemoveAllUIEvent();
            if (Handle != null)
            {
                if (AssetReference != null && AssetReference.Parent != null)
                {
                    AssetReference.Parent.Release(Handle);
                }
            }
        }
        

        #endregion
    }
}