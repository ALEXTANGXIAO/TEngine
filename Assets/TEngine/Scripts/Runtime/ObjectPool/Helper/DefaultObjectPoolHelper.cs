using System;
using System.Collections.Generic;
using UnityEngine;

namespace TEngine.Runtime
{
    /// <summary>
    /// 默认的对象池管理器助手
    /// </summary>
    public sealed class DefaultObjectPoolHelper : IObjectPoolHelper
    {
        /// <summary>
        /// 对象池默认上限
        /// </summary>
        private int _limit;

        /// <summary>
        /// 所有对象池
        /// </summary>
        public Dictionary<string, ObjectPool> ObjectPools { get; private set; } =
            new Dictionary<string, ObjectPool>();

        /// <summary>
        /// 初始化助手
        /// </summary>
        public DefaultObjectPoolHelper()
        {
            _limit = ObjectPoolManager.Instance.Limit;
        }

        /// <summary>
        /// ShutDown
        /// </summary>
        public void ShutDown()
        {
            ClearAll();
        }

        /// <summary>
        /// 注册对象池
        /// </summary>
        /// <param name="name">对象池名称</param>
        /// <param name="allocItem">对象模板</param>
        /// <param name="onAlloc">对象生成时初始化委托</param>
        /// <param name="onRelease">对象回收时处理委托</param>
        /// <param name="limit">对象池上限，等于0时，表示使用默认值</param>
        public void RegisterPool(string name, GameObject allocItem, Action<GameObject> onAlloc,
            Action<GameObject> onRelease, int limit)
        {
            if (allocItem == null)
                return;

            if (!ObjectPools.ContainsKey(name))
            {
                ObjectPools.Add(name, new ObjectPool(allocItem, limit <= 0 ? _limit : limit, onAlloc, onRelease));
            }
            else
            {
                Log.Error("注册对象池失败：已存在对象池 " + name + " ！");
            }
        }

        /// <summary>
        /// 是否存在指定名称的对象池
        /// </summary>
        /// <param name="name">对象池名称</param>
        /// <returns>是否存在</returns>
        public bool IsExistPool(string name)
        {
            return ObjectPools.ContainsKey(name);
        }

        /// <summary>
        /// 移除已注册的对象池
        /// </summary>
        /// <param name="name">对象池名称</param>
        public void UnRegisterPool(string name)
        {
            if (ObjectPools.ContainsKey(name))
            {
                ObjectPools[name].Clear();
                ObjectPools.Remove(name);
            }
            else
            {
                Log.Error("移除对象池失败：不存在对象池 " + name + " ！");
            }
        }

        /// <summary>
        /// 获取对象池中对象数量
        /// </summary>
        /// <param name="name">对象池名称</param>
        /// <returns>对象数量</returns>
        public int GetPoolCount(string name)
        {
            if (ObjectPools.ContainsKey(name))
            {
                return ObjectPools[name].Count;
            }
            else
            {
                Log.Warning("获取对象数量失败：不存在对象池 " + name + " ！");
                return 0;
            }
        }

        /// <summary>
        /// 生成对象
        /// </summary>
        /// <param name="name">对象池名称</param>
        /// <returns>对象</returns>
        public GameObject Alloc(string name)
        {
            if (ObjectPools.ContainsKey(name))
            {
                return ObjectPools[name].Alloc();
            }
            else
            {
                Log.Error("生成对象失败：不存在对象池 " + name + " ！");
                return null;
            }
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        /// <param name="name">对象池名称</param>
        /// <param name="target">对象</param>
        public void Release(string name, GameObject target)
        {
            if (target == null)
                return;

            if (ObjectPools.ContainsKey(name))
            {
                ObjectPools[name].Release(target);
            }
            else
            {
                Log.Error("回收对象失败：不存在对象池 " + name + " ！");
            }
        }

        /// <summary>
        /// 批量回收对象
        /// </summary>
        /// <param name="name">对象池名称</param>
        /// <param name="targets">对象数组</param>
        public void Releases(string name, GameObject[] targets)
        {
            if (targets == null)
                return;

            if (ObjectPools.ContainsKey(name))
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    ObjectPools[name].Release(targets[i]);
                }
            }
            else
            {
                Log.Error("回收对象失败：不存在对象池 " + name + " ！");
            }
        }

        /// <summary>
        /// 批量回收对象
        /// </summary>
        /// <param name="name">对象池名称</param>
        /// <param name="targets">对象集合</param>
        public void Releases(string name, List<GameObject> targets)
        {
            if (targets == null)
                return;

            if (ObjectPools.ContainsKey(name))
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    ObjectPools[name].Release(targets[i]);
                }

                targets.Clear();
            }
            else
            {
                Log.Error("回收对象失败：不存在对象池 " + name + " ！");
            }
        }

        /// <summary>
        /// 清空指定的对象池
        /// </summary>
        /// <param name="name">对象池名称</param>
        public void Clear(string name)
        {
            if (ObjectPools.ContainsKey(name))
            {
                ObjectPools[name].Clear();
            }
            else
            {
                Log.Error("清空对象池失败：不存在对象池 " + name + " ！");
            }
        }

        /// <summary>
        /// 清空所有对象池
        /// </summary>
        public void ClearAll()
        {
            foreach (var pool in ObjectPools)
            {
                pool.Value.Clear();
            }
        }
    }
}