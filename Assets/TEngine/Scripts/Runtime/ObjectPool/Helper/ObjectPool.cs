using System;
using System.Collections.Generic;
using UnityEngine;

namespace TEngine.Runtime
{
    /// <summary>
    /// 对象池
    /// </summary>
    public sealed class ObjectPool
    {
        private readonly GameObject _allocItem;
        private readonly int _limit = 100;
        private readonly Queue<GameObject> _objectQueue = new Queue<GameObject>();
        private readonly Action<GameObject> _onAlloc;
        private readonly Action<GameObject> _onRelease;

        /// <summary>
        /// 对象数量
        /// </summary>
        public int Count
        {
            get { return _objectQueue.Count; }
        }

        /// <summary>
        /// 构造函数对象池
        /// </summary>
        /// <param name="allocItem"></param>
        /// <param name="limit"></param>
        /// <param name="onAlloc"></param>
        /// <param name="onRelease"></param>
        public ObjectPool(GameObject allocItem, int limit, Action<GameObject> onAlloc,
            Action<GameObject> onRelease)
        {
            _allocItem = allocItem;
            _limit = limit;
            _onAlloc = onAlloc;
            _onRelease = onRelease;
        }

        /// <summary>
        /// 生成对象
        /// </summary>
        /// <returns>对象</returns>
        public GameObject Alloc()
        {
            GameObject obj;
            if (_objectQueue.Count > 0)
            {
                obj = _objectQueue.Dequeue();
            }
            else
            {
                obj = Utility.GameObjectUtils.CloneGameObject(_allocItem);
            }

            obj.SetActive(true);
            
            obj.transform.SetParent(null);

            _onAlloc?.Invoke(obj);

            return obj;
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        /// <param name="obj">对象</param>
        public void Release(GameObject obj)
        {
            if (_objectQueue.Count >= _limit)
            {
                _onRelease?.Invoke(obj);

                Utility.GameObjectUtils.Kill(obj);
            }
            else
            {
                obj.SetActive(false);

                _onRelease?.Invoke(obj);

                _objectQueue.Enqueue(obj);
                
                obj.transform.SetParent(ObjectPoolManager.Instance.gameObject.transform);
            }
        }

        /// <summary>
        /// 清空所有对象
        /// </summary>
        public void Clear()
        {
            while (_objectQueue.Count > 0)
            {
                GameObject obj = _objectQueue.Dequeue();
                if (obj)
                {
                    Utility.GameObjectUtils.Kill(obj);
                }
            }
        }
    }
}