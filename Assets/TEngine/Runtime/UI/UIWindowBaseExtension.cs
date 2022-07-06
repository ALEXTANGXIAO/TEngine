using System;
using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;

namespace UI
{
    partial class UIWindowBase
    {
        /**
         * 创建子控件，
         * goPath 控件的gameobject相当于本window的位置
         */
        public T CreateWidget<T>(string goPath, bool visible = true) where T : UIWindowWidget, new()
        {
            var goRootTrans = FindChild(goPath);
            if (goRootTrans != null)
            {
                return CreateWidget<T>(goRootTrans.gameObject, visible);
            }

            TLogger.LogError("CreateWidget failed, path: {0}, widget type: {1}", goPath, typeof(T).FullName);
            return null;
        }

        public T CreateWidget<T>(Transform parent, string goPath, bool visible = true) where T : UIWindowWidget, new()
        {
            var goRootTrans = FindChild(parent, goPath);
            if (goRootTrans != null)
            {
                return CreateWidget<T>(goRootTrans.gameObject, visible);
            }

            return null;
        }

        public T[] CreateWidgets<T>(Transform parent) where T : UIWindowWidget, new()
        {
            T[] array = new T[parent.childCount];
            for (int i = 0; i < parent.childCount; ++i)
            {
                Transform child = parent.GetChild(i);
                array[i] = CreateWidget<T>(child.gameObject);
            }
            return array;
        }

        public T[] CreateWidgets<T>(string path) where T : UIWindowWidget, new()
        {
            var parent = FindChild(path);
            return CreateWidgets<T>(parent);
        }

        public T CreateWidget<T>(GameObject goRoot, bool visible = true) where T : UIWindowWidget, new()
        {
            var widget = new T();
            if (!widget.Create(this, goRoot, visible))
            {
                return null;
            }

            return widget;
        }

        //public UIWindowWidget CreateWidgetByPrefab(Type type, GameObject goPrefab, Transform parent, bool visible = true)
        //{
        //    var widget = Activator.CreateInstance(type) as UIWindowWidget;
        //    if (!widget.CreateByPrefab(this, goPrefab, parent, visible))
        //    {
        //        return null;
        //    }

        //    return widget;
        //}

        public T CreateWidgetByPrefab<T>(GameObject goPrefab, Transform parent, bool visible = true) where T : UIWindowWidget, new()
        {
            var widget = new T();
            if (!widget.CreateByPrefab(this, goPrefab, parent, visible))
            {
                return null;
            }

            return widget;
        }

        public T CreateWidgetByType<T>(Transform parent, string goPath, bool visible = true) where T : UIWindowWidget, new()
        {
            var goRootTrans = FindChild(parent, goPath);
            if (goRootTrans != null)
            {
                string resPath = string.Format("UI/{0}", typeof(T).Name);
                return CreateWidgetByResPath<T>(resPath, goRootTrans, visible);
            }
            return null;
        }

        public T CreateWidgetByType<T>(string goPath, bool visible = true) where T : UIWindowWidget, new()
        {
            return CreateWidgetByType<T>(transform, goPath, visible);
        }

        public T CreateWidgetByType<T>(Transform parent, bool visible = true) where T : UIWindowWidget, new()
        {
            string resPath = string.Format("UI/{0}", typeof(T).Name);
            return CreateWidgetByResPath<T>(resPath, parent, visible);
        }

        //public UIWindowWidget CreateWidgetByType(Type type, Transform parent, bool visible = true)
        //{
        //    string resPath = string.Format("UI/{0}", type.Name);
        //    return CreateWidgetByResPath(type, resPath, parent, visible);
        //}

        //public UIWindowWidget CreateWidgetByResPath(Type type, string resPath, Transform parent, bool visible = true)
        //{
        //    var widget = Activator.CreateInstance(type) as UIWindowWidget;
        //    if (!widget.CreateByPath(resPath, this, parent, visible))
        //    {
        //        return null;
        //    }
        //    return widget;
        //}

        public T CreateWidgetByResPath<T>(string resPath, Transform parent, bool visible = true) where T : UIWindowWidget, new()
        {
            var widget = new T();
            if (!widget.CreateByPath(resPath, this, parent, visible))
            {
                return null;
            }
            return widget;
        }

        /// <summary>
        /// 调整图标数量
        /// </summary>
        public void AdjustIconNum<T>(List<T> listIcon, int tarNum, Transform parent, GameObject prefab = null) where T : UIWindowWidget, new()
        {
            if (listIcon == null)
            {
                TLogger.LogError("List is null");
                return;
            }
            if (listIcon.Count < tarNum) // 不足则添加
            {
                T tmpT;
                int needNum = tarNum - listIcon.Count;
                for (int iconIdx = 0; iconIdx < needNum; iconIdx++)
                {
                    if (prefab == null)
                    {
                        tmpT = CreateWidgetByType<T>(parent);
                    }
                    else
                    {
                        tmpT = CreateWidgetByPrefab<T>(prefab, parent);
                    }
                    listIcon.Add(tmpT);
                }
            }
            else if (listIcon.Count > tarNum) // 多则删除
            {
                RemoveUnuseItem<T>(listIcon, tarNum);
            }
        }

        public void AsyncAdjustIconNum<T>(string name, List<T> listIcon, int tarNum, Transform parent, int maxNumPerFrame = 5,
            Action<T, int> updateAction = null, GameObject prefab = null) where T : UIWindowWidget, new()
        {
            StartCoroutine( AsyncAdjustIconNumIE(listIcon, tarNum, parent, maxNumPerFrame, updateAction, prefab));
        }

        /// <summary>
        /// 异步创建接口，maxNumPerFrame单帧最多的创建数量
        /// 注意disable的对象无法运行协程
        /// </summary>
        public IEnumerator AsyncAdjustIconNumIE<T>(List<T> listIcon, int tarNum, Transform parent, int maxNumPerFrame, Action<T, int> updateAction, GameObject prefab) where T : UIWindowWidget, new()
        {
            if (listIcon == null)
            {
                TLogger.LogError("List is null");
                yield break;
            }

            int createCnt = 0;

            for (int i = 0; i < tarNum; i++)
            {
                T tmpT;
                if (i < listIcon.Count)
                {
                    tmpT = listIcon[i];
                }
                else
                {
                    if (prefab == null)
                    {
                        tmpT = CreateWidgetByType<T>(parent);
                    }
                    else
                    {
                        tmpT = CreateWidgetByPrefab<T>(prefab, parent);
                    }
                    listIcon.Add(tmpT);
                }
                int index = i;
                if (updateAction != null)
                {
                    updateAction(tmpT, index);
                }

                createCnt++;
                if (createCnt >= maxNumPerFrame)
                {
                    createCnt = 0;
                    yield return null;
                }
            }
            if (listIcon.Count > tarNum) // 多则删除
            {
                RemoveUnuseItem(listIcon, tarNum);
            }
        }

        private void RemoveUnuseItem<T>(List<T> listIcon, int tarNum) where T : UIWindowWidget, new()
        {
            for (int i = 0; i < listIcon.Count; i++)
            {
                var icon = listIcon[i];
                if (i >= tarNum)
                {
                    listIcon.RemoveAt(i);
                    icon.Destroy();
                    --i;
                }
            }
        }

        public void SafeDestroyDelay(GameObject go, float time)
        {
            if (go != null)
            {
                AutoDestroyBehaviour w =
                    CreateWidget<AutoDestroyBehaviour>(go);
                if (w != null)
                {
                    w.Init(time);
                }
            }
        }
    }

    class AutoDestroyBehaviour : UIWindowWidget
    {
        private float m_timer;
        private bool m_isValid;
        public void Init(float time)
        {
            m_timer = time;
            m_isValid = true;
        }

        protected override void OnUpdate()
        {
            if (!m_isValid)
            {
                Destroy();
                return;
            }

            m_timer -= GameTime.deltaTime;
            if (m_timer <= 0)
            {
                Destroy();
            }
        }
    }
}