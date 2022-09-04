using System;
using System.Collections.Generic;
using UnityEngine;

namespace TEngine.Runtime
{
    /// <summary>
    /// 对象池管理器
    /// </summary>
    public sealed class ObjectPoolManager : UnitySingleton<ObjectPoolManager>
    {
        private IObjectPoolHelper _helper;

        public IObjectPoolHelper Helper => _helper;

        public override int Priority => -1;

        public override void Awake()
        {
            base.Awake();
            _helper = new DefaultObjectPoolHelper();
        }

        /// <summary>
        /// 单个对象池上限
        /// </summary>
        [SerializeField] internal int Limit = 100;

        /// <summary>
        /// 注册对象池
        /// </summary>
        /// <param name="name">对象池名称</param>
        /// <param name="allocItem">对象模板</param>
        /// <param name="onAlloc">对象生成时初始化委托</param>
        /// <param name="onRelease">对象回收时处理委托</param>
        /// <param name="limit">对象池上限，等于0时，表示使用默认值</param>
        public void RegisterPool(string name, GameObject allocItem, Action<GameObject> onAlloc = null,
            Action<GameObject> onRelease = null, int limit = 0)
        {
            _helper.RegisterPool(name, allocItem, onAlloc, onRelease, limit);
        }

        /// <summary>
        /// 是否存在指定名称的对象池
        /// </summary>
        /// <param name="name">对象池名称</param>
        /// <returns>是否存在</returns>
        public bool IsExistPool(string name)
        {
            return _helper.IsExistPool(name);
        }

        /// <summary>
        /// 移除已注册的对象池
        /// </summary>
        /// <param name="name">对象池名称</param>
        public void UnRegisterPool(string name)
        {
            _helper.UnRegisterPool(name);
        }

        /// <summary>
        /// 获取对象池中对象数量
        /// </summary>
        /// <param name="name">对象池名称</param>
        /// <returns>对象数量</returns>
        public int GetPoolCount(string name)
        {
            return _helper.GetPoolCount(name);
        }

        /// <summary>
        /// 生成对象
        /// </summary>
        /// <param name="name">对象池名称</param>
        /// <returns>对象</returns>
        public GameObject Alloc(string name)
        {
            return _helper.Alloc(name);
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        /// <param name="name">对象池名称</param>
        /// <param name="target">对象</param>
        public void Release(string name, GameObject target)
        {
            _helper.Release(name, target);
        }

        /// <summary>
        /// 批量回收对象
        /// </summary>
        /// <param name="name">对象池名称</param>
        /// <param name="targets">对象数组</param>
        public void Releases(string name, GameObject[] targets)
        {
            _helper.Releases(name, targets);
        }

        /// <summary>
        /// 批量回收对象
        /// </summary>
        /// <param name="name">对象池名称</param>
        /// <param name="targets">对象集合</param>
        public void Releases(string name, List<GameObject> targets)
        {
            _helper.Releases(name, targets);
        }

        /// <summary>
        /// 清空指定的对象池
        /// </summary>
        /// <param name="name">对象池名称</param>
        public void Clear(string name)
        {
            _helper.Clear(name);
        }

        /// <summary>
        /// 清空所有对象池
        /// </summary>
        public void ClearAll()
        {
            _helper.ClearAll();
        }
    }
}