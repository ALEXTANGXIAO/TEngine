using System;
using System.Collections.Generic;
using UnityEngine;

namespace TEngine.Runtime
{
    /// <summary>
    /// 对象池管理器的助手接口
    /// </summary>
    public interface IObjectPoolHelper
    {
        /// <summary>
        /// 所有对象池
        /// </summary>
        Dictionary<string, ObjectPool> ObjectPools { get; }

        /// <summary>
        /// 注册对象池
        /// </summary>
        /// <param name="name">对象池名称</param>
        /// <param name="allocItem">对象模板</param>
        /// <param name="onAlloc">对象生成时初始化委托</param>
        /// <param name="onRelease">对象回收时处理委托</param>
        /// <param name="limit">对象池上限，等于0时，表示使用默认值</param>
        void RegisterPool(string name, GameObject allocItem, Action<GameObject> onAlloc,
            Action<GameObject> onRelease, int limit);

        /// <summary>
        /// 是否存在指定名称的对象池
        /// </summary>
        /// <param name="name">对象池名称</param>
        /// <returns>是否存在</returns>
        bool IsExistPool(string name);

        /// <summary>
        /// 移除已注册的对象池
        /// </summary>
        /// <param name="name">对象池名称</param>
        void UnRegisterPool(string name);

        /// <summary>
        /// 获取对象池中对象数量
        /// </summary>
        /// <param name="name">对象池名称</param>
        /// <returns>对象数量</returns>
        int GetPoolCount(string name);

        /// <summary>
        /// 生成对象
        /// </summary>
        /// <param name="name">对象池名称</param>
        /// <returns>对象</returns>
        GameObject Alloc(string name);

        /// <summary>
        /// 回收对象
        /// </summary>
        /// <param name="name">对象池名称</param>
        /// <param name="target">对象</param>
        void Release(string name, GameObject target);

        /// <summary>
        /// 批量回收对象
        /// </summary>
        /// <param name="name">对象池名称</param>
        /// <param name="targets">对象数组</param>
        void Releases(string name, GameObject[] targets);

        /// <summary>
        /// 批量回收对象
        /// </summary>
        /// <param name="name">对象池名称</param>
        /// <param name="targets">对象集合</param>
        void Releases(string name, List<GameObject> targets);

        /// <summary>
        /// 清空指定的对象池
        /// </summary>
        /// <param name="name">对象池名称</param>
        void Clear(string name);

        /// <summary>
        /// 清空所有对象池
        /// </summary>
        void ClearAll();
    }
}