using System.Collections;
using System.ComponentModel;
using TEngine;
using UnityEngine;
using UnityEngine.Events;

namespace TEngine
{
    public class MonoManager : TSingleton<MonoManager>
    {
        private MonoController controller;

        public override void Release()
        {
            StopAllCoroutine();
            controller.Release();
            controller = null;
            base.Release();
        }

        public MonoManager()
        {
            GameObject obj = new GameObject("MonoManager");

            controller = obj.AddComponent<MonoController>();

#if UNITY_EDITOR
            GameObject tEngine = SingletonMgr.Root;
            if (tEngine != null)
            {
                obj.transform.SetParent(tEngine.transform);
            }
#endif
        }

        ~MonoManager()
        {
            StopAllCoroutine();
            controller.Release();
            controller = null;
        }

        #region 注入UnityUpdate/FixedUpdate
        /// <summary>
        /// 为给外部提供的 添加帧更新事件
        /// </summary>
        /// <param name="fun"></param>
        public void AddUpdateListener(UnityAction fun)
        {
            controller.AddUpdateListener(fun);
        }

        /// <summary>
        /// 为给外部提供的 添加物理帧更新事件
        /// </summary>
        /// <param name="fun"></param>
        public void AddFixedUpdateListener(UnityAction fun)
        {
            controller.AddFixedUpdateListener(fun);
        }

        /// <summary>
        /// 移除帧更新事件
        /// </summary>
        /// <param name="fun"></param>
        public void RemoveUpdateListener(UnityAction fun)
        {
            controller.RemoveUpdateListener(fun);
        }
        #endregion

        #region 控制协程Coroutine
        public Coroutine StartCoroutine(string methodName)
        {
            if (controller == null)
            {
                return null;
            }
            return controller.StartCoroutine(methodName);
        }

        public Coroutine StartCoroutine(IEnumerator routine)
        {
            if (controller == null)
            {
                return null;
            }
            return controller.StartCoroutine(routine);
        }

        public Coroutine StartCoroutine(string methodName, [DefaultValue("null")] object value)
        {
            if (controller == null)
            {
                return null;
            }
            return controller.StartCoroutine(methodName, value);
        }

        public void StopCoroutine(string methodName)
        {
            if (controller == null)
            {
                return;
            }
            controller.StopCoroutine(methodName);
        }

        public void StopCoroutine(IEnumerator routine)
        {
            if (controller == null)
            {
                return;
            }
            controller.StopCoroutine(routine);
        }

        public void StopCoroutine(Coroutine routine)
        {
            if (controller == null)
            {
                return;
            }
            controller.StopCoroutine(routine);
        }

        public void StopAllCoroutine()
        {
            if (controller != null)
            {
                controller.StopAllCoroutines();
            }
        }
        #endregion

        #region GC
        public void GC()
        {
            System.GC.Collect();
        }
        #endregion
    }

}