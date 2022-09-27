using System;
using System.Collections.Generic;
using UnityEngine;

namespace TEngine.Runtime
{
    public class PoolData
    {
        /// <summary>
        /// 父节点
        /// </summary>
        public readonly GameObject ParentObject;

        /// <summary>
        /// 数据结构栈
        /// </summary>
        private readonly Stack<GameObject> _stack;

        /// <summary>
        /// 容量 Capacity
        /// </summary>
        public static int Capacity = 100;

        /// <summary>
        /// 当前对象池的数目 Count
        /// </summary>
        public int Count => _stack.Count;

        /// <summary>
        /// 构造PoolData
        /// </summary>
        /// <param name="gameObject">GameObject</param>
        /// <param name="rootObject">对象池根物体</param>
        /// <param name="capacity">最大数目</param>
        public PoolData(GameObject gameObject, GameObject rootObject,int capacity = 100)
        {
            ParentObject = new GameObject(gameObject.name);

            ParentObject.transform.parent = rootObject.transform;

            _stack = new Stack<GameObject>();
            
            Capacity = capacity;

            Push(gameObject);
        }

        /// <summary>
        /// 从对象池获取 （弹出栈）
        /// </summary>
        /// <returns></returns>
        public GameObject Get()
        {
            GameObject obj = null;
            
            obj = _stack.Pop();
            
            obj.SetActive(true);
            
            obj.transform.parent = null;
            
            return obj;
        }

        /// <summary>
        /// 压入对象池 （压入栈）
        /// </summary>
        /// <param name="obj">GameObject</param>
        /// <exception cref="Exception">throw new Exception($"TEngine Pool:{obj.name} Over Max Count:{Count}");</exception>
        public void Push(GameObject obj)
        {
            if (Count > Capacity)
            {
                throw new Exception($"TEngine Pool:{obj.name} Over Max Count:{Count}");
            }
            
            obj.SetActive(false);
            
            _stack.Push(obj);
            
            obj.transform.parent = ParentObject.transform;
        }

        /// <summary>
        /// 释放池
        /// </summary>
        public void Release()
        {
            UnityEngine.Object.Destroy(ParentObject);
        }
    }
}