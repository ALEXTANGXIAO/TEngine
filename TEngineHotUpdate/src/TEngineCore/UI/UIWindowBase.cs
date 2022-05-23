using System.Collections;
using System.Collections.Generic;
using TEngineCore;
using UnityEngine;

namespace TEngineCore
{
    public class UIWindowBase : UIBase
    {
        /// <summary>
        /// 所属的window
        /// </summary>
        protected UIWindowBase m_parent = null;
        protected Canvas m_canvas;

        private List<UIWindowBase> m_listChild = null;
        private List<UIWindowBase> m_listUpdateChild = null;
        private bool m_updateListValid = false;

        /// <summary>
        /// 是否首次显示过了
        /// </summary>
        protected bool m_firstVisible = false;
        /// <summary>
        /// 当前是否显示出来了
        /// </summary>
        private bool m_visible = false;
        public bool Visible
        {
            get { return m_visible; }
        }

        public UIWindowBase Parent
        {
            get { return m_parent; }
        }

        protected UIManager m_ownUIManager = null;

        protected UIManager UIMgr
        {
            get
            {
                if (m_ownUIManager == null && m_parent != null)
                {
                    return m_parent.UIMgr;
                }

                return m_ownUIManager;
            }
        }

        public virtual UIWindowBaseType BaseType
        {
            get { return UIWindowBaseType.None; }
        }


        #region 最基础的接口

        /**
         * 创建对象
         *
         * bindGO 是否把GameObject和Window绑定在一起
         */
        protected bool CreateBase(GameObject go, bool bindGo)
        {
            ///has created
            if (!m_destroyed)
            {
                Debug.LogErrorFormat("UIBase has created: {0}", go.name);
                return false;
            }

            if (go == null)
            {
                return false;
            }

            m_destroyed = false;
            m_go = go;

            m_transform = go.GetComponent<RectTransform>();
            m_canvas = gameObject.GetComponent<Canvas>();

            var canvas = gameObject.GetComponentsInChildren<Canvas>(true);
            for (var i = 0; i < canvas.Length; i++)
            {
                var canva = canvas[i];
                canva.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;
            }

            if (m_transform == null)
            {
                Debug.LogErrorFormat("{0} ui base element need to be RectTransform", go.name);
            }
            return true;
        }

        protected void DestroyAllChild()
        {
            //销毁子对象
            if (m_listChild != null)
            {
                for (int i = 0; i < m_listChild.Count; i++)
                {
                    var child = m_listChild[i];
                    child.Destroy();
                }

                m_listChild.Clear();
            }
        }

        public void Destroy()
        {
            if (IsDestroyed)
            {
                return;
            }

            m_destroyed = true;

            OnDestroy();
            DestroyAllChild();

            if (m_parent != null)
            {
                m_parent.RmvChild(this);
                m_parent = null;
            }

            if (m_go != null)
            {
                Object.Destroy(m_go);
                m_go = null;
            }

            m_transform = null;
        }

        #endregion

        #region 子类扩展的接口

        /// <summary> 脚本生成的代码 </summary>
        protected virtual void ScriptGenerator()
        {

        }

        /// <summary>
        ///  绑定代码和prefab之间元素的关系
        /// </summary>
        protected virtual void BindMemberProperty()
        {
        }

        protected virtual void RegisterEvent()
        {
        }

        private bool m_hasOverrideUpdate = true;
        protected virtual void OnUpdate()
        {
            m_hasOverrideUpdate = false;

        }

        /// <summary>
        /// 界面创建出来的时候调用，被覆盖不可见不会重复触发
        /// </summary>
        protected virtual void OnCreate()
        {
        }

        protected virtual void OnDestroy()
        {
        }

        /// <summary>
        /// 创建出来首次visible
        /// 用来播放一些显示动画之类的
        /// </summary>
        protected virtual void OnFirstVisible()
        {
        }

        /// <summary>
        /// 当显示出来的时候调用
        /// 包括首次初始化后显示和上面的界面消失后重新恢复显示
        /// </summary>
        protected virtual void OnVisible()
        {
        }

        /// <summary>
        /// 界面不可见的时候调用
        /// 当被上层全屏界面覆盖后，也会触发一次隐藏
        /// </summary>
        protected virtual void OnHidden()
        {
        }

        protected void _OnSortingOrderChg()
        {
            if (m_listChild != null)
            {
                for (int i = 0; i < m_listChild.Count; i++)
                {
                    if (m_listChild[i].m_visible)
                    {
                        m_listChild[i]._OnSortingOrderChg();
                    }
                }
            }
            OnSortingOrderChg();
        }

        protected virtual void OnSortingOrderChg()
        {
        }
        #endregion

        public void AddChild(UIWindowBase child)
        {
            if (m_listChild == null)
            {
                m_listChild = new List<UIWindowBase>();
            }

            m_listChild.Add(child);
            MarkListChanged();
        }

        public void RmvChild(UIWindowBase child)
        {
            //如果已经销毁了或者销毁过程中，那么不掉用删除
            if (m_destroyed)
            {
                return;
            }

            if (m_listChild != null)
            {
                if (m_listChild.Remove(child))
                {
                    MarkListChanged();
                }
            }
        }

        /// <summary>
        /// 重新整理update和lateupdate的调用缓存
        /// </summary>
        private void MarkListChanged()
        {
            m_updateListValid = false;
            if (m_parent != null)
            {
                m_parent.MarkListChanged();
            }
        }

        #region 子类调用的接口

        protected Coroutine StartCoroutine(IEnumerator routine)
        {
            return MonoManager.Instance.StartCoroutine(routine);
        }

        protected void StopCoroutine(Coroutine cort)
        {
            MonoManager.Instance.StopCoroutine(cort);
        }

        #endregion

        #region 通用的操作接口

        public Transform FindChild(string path)
        {
            return UnityUtil.FindChild(transform, path);
        }

        public Transform FindChild(Transform _transform, string path)
        {
            return UnityUtil.FindChild(_transform, path);
        }

        public T FindChildComponent<T>(string path) where T : Component
        {
            return UnityUtil.FindChildComponent<T>(transform, path);
        }

        public T FindChildComponent<T>(Transform _transform, string path) where T : Component
        {
            return UnityUtil.FindChildComponent<T>(_transform, path);
        }


        public void Show(bool visible)
        {
            // 加个保护
            if (m_destroyed || gameObject == null)
            {
                return;
            }
            if (m_visible != visible)
            {
                m_visible = visible;
                if (visible)
                {
                    gameObject.SetActive(true);
                    _OnVisible();
                }
                else
                {
                    _OnHidden();

                    if (gameObject == null)
                    {
                        Debug.LogErrorFormat("ui bug, hiden destory gameobject: {0}", name);
                    }
                    else
                    {
                        gameObject.SetActive(false);
                    }
                }

                MarkListChanged();
            }
        }

        protected void _OnVisible()
        {
            if (m_listChild != null)
            {
                for (int i = 0; i < m_listChild.Count; i++)
                {
                    var child = m_listChild[i];
                    if (child.gameObject.activeInHierarchy)
                    {
                        child._OnVisible();
                    }
                }
            }
            if (!m_firstVisible)
            {
                m_firstVisible = true;
                OnFirstVisible();
            }
            OnVisible();
        }

        protected void _OnHidden()
        {
            if (m_listChild != null)
            {
                for (int i = 0; i < m_listChild.Count; i++)
                {
                    var child = m_listChild[i];
                    if (child.gameObject.activeInHierarchy)
                    {
                        child._OnHidden();
                    }
                }
            }
            OnHidden();
        }


        /// <summary>
        /// 返回是否有必要下一帧继续执行
        /// </summary>
        /// <returns></returns>
        public bool Update()
        {
            if (!m_visible || m_destroyed)
            {
                return false;
            }

            List<UIWindowBase> listNextUpdateChild = null;
            if (m_listChild != null && m_listChild.Count > 0)
            {
                listNextUpdateChild = m_listUpdateChild;
                var updateListValid = m_updateListValid;
                List<UIWindowBase> listChild = null;
                if (!updateListValid)
                {
                    if (listNextUpdateChild == null)
                    {
                        listNextUpdateChild = new List<UIWindowBase>();
                        m_listUpdateChild = listNextUpdateChild;
                    }
                    else
                    {
                        listNextUpdateChild.Clear();
                    }

                    listChild = m_listChild;
                }
                else
                {
                    listChild = listNextUpdateChild;
                }

                for (int i = 0; i < listChild.Count; i++)
                {
                    var window = listChild[i];

                    var needValid = window.Update();

                    if (!updateListValid && needValid)
                    {
                        listNextUpdateChild.Add(window);
                    }
                }

                if (!updateListValid)
                {
                    m_updateListValid = true;
                }
            }

            bool needUpdate = false;
            if (listNextUpdateChild == null || listNextUpdateChild.Count <= 0)
            {
                m_hasOverrideUpdate = true;
                OnUpdate();
                needUpdate = m_hasOverrideUpdate;
            }
            else
            {
                OnUpdate();
                needUpdate = true;
            }
            return needUpdate;
        }

        #endregion
    }

}