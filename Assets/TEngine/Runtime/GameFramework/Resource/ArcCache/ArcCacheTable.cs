using System;
using System.Collections.Generic;
using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// Adaptive Replacement Cache缓存表。
    /// </summary>
    /// <typeparam name="TKey">Adaptive Replacement CacheKey。</typeparam>
    /// <typeparam name="TValue">Adaptive Replacement CacheValue</typeparam>
    /// <remarks>缓存替换策略（Cache Replacement Policy）- ARC（Adaptive Replacement Cache）。</remarks>
    public class ArcCacheTable<TKey, TValue>
    {
        /// <summary>
        /// ARC缓存表哈希表。
        /// <remarks>Dictionary 中存储 key 和 QueueNode 中节点的映射关系。</remarks>
        /// <remarks>算法优化的关键在于如何降低链表的删除操作的时间复杂度。不管是在插入、删除、查找缓存的时候，都可以通过这种联系来将时间复杂度降低到 O(1)。</remarks>
        /// </summary>
        protected readonly Dictionary<TKey, QueueNode<TKey, TValue>> CacheStorageMap;

        private readonly QueueNode<TKey, TValue> _t1Head;
        private readonly QueueNode<TKey, TValue> _t2Head;
        private readonly QueueNode<TKey, TValue> _b1Head;
        private readonly QueueNode<TKey, TValue> _b2Head;

        private int _adaptiveParameter;
        private int _t1Size;
        private int _t2Size;
        private int _b1Size;
        private int _b2Size;
        protected readonly int Capacity;

        public delegate void OnAdd(TValue data);

        public delegate void OnRemove(TValue data);

        /// <summary>
        /// ARC缓存表添加成功回调。
        /// </summary>
        public OnAdd OnAddCallback;

        /// <summary>
        /// ARC缓存表移除回调。
        /// </summary>
        public OnRemove OnRemoveCallback;

        /// <summary>
        /// ARC缓存表哈希表构造。
        /// </summary>
        /// <param name="capacity">容量。</param>
        /// <param name="onAdd">LRU缓存表添加成功回调。</param>
        /// <param name="onRemove">LRU缓存表移除回调。</param>
        public ArcCacheTable(int capacity, OnAdd onAdd = null, OnRemove onRemove = null)
        {
            this.Capacity = capacity;
            OnAddCallback = onAdd;
            OnRemoveCallback = onRemove;
            this.CacheStorageMap = new Dictionary<TKey, QueueNode<TKey, TValue>>();
            this._t1Head = new QueueNode<TKey, TValue>();
            this._t2Head = new QueueNode<TKey, TValue>();
            this._b1Head = new QueueNode<TKey, TValue>();
            this._b2Head = new QueueNode<TKey, TValue>();
        }

        /// <summary>
        /// 对象推入ARC缓存表。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="value">值。</param>
        public void PutCache(TKey key, TValue value)
        {
            CacheStorageMap.TryGetValue(key, out QueueNode<TKey, TValue> queueNode);

            if (queueNode == null)
            {
                OnMissOnAllQueue(key, value);
            }
            else if (queueNode.QueueType == QueueType.B1)
            {
                queueNode.Set(value);
                OnHitOnB1(queueNode);
            }
            else if (queueNode.QueueType == QueueType.B2)
            {
                queueNode.Set(value);
                OnHitOnB2(queueNode);
            }
            else
            {
                queueNode.Set(value);
                OnHitOnT1orT2(queueNode);
            }
        }

        /// <summary>
        /// 从ARC缓存表取出对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <returns>TValue from cache if exists or null。</returns>
        public TValue GetCache(TKey key)
        {
            CacheStorageMap.TryGetValue(key, out QueueNode<TKey, TValue> queueNode);

            if (queueNode == null)
            {
                return default;
            }
            else
            {
                return queueNode.Get();
            }
        }

        /// <summary>
        /// 在所有队列中未命中时执行任务(Case：Key不在(T1 u B1 u T2 u B2))。
        /// </summary>
        /// <param name="key">key cache key。</param>
        /// <param name="value">value to inset in cache。</param>
        private void OnMissOnAllQueue(TKey key, TValue value)
        {
            QueueNode<TKey, TValue> queueNode = new QueueNode<TKey, TValue>(key, value);
            queueNode.QueueType = QueueType.T1;

            int sizeL1 = (_t1Size + _b1Size);
            int sizeL2 = (_t2Size + _b2Size);
            if (sizeL1 == Capacity)
            {
                if (_t1Size < Capacity)
                {
                    QueueNode<TKey, TValue> queueNodeToBeRemoved = _b1Head.Next;
                    RemoveFromQueue(queueNodeToBeRemoved);
                    queueNodeToBeRemoved.Remove();
                    _b1Size--;

                    Replace(queueNode);
                }
                else
                {
                    QueueNode<TKey, TValue> queueNodeToBeRemoved = _t1Head.Next;
                    RemoveFromQueue(queueNodeToBeRemoved);
                    queueNodeToBeRemoved.Remove();
                    _t1Size--;
                }
            }
            else if ((sizeL1 < Capacity) && ((sizeL1 + sizeL2) >= Capacity))
            {
                if ((sizeL1 + sizeL2) >= (2 * Capacity))
                {
                    QueueNode<TKey, TValue> queueNodeToBeRemoved = _b2Head.Next;
                    RemoveFromQueue(queueNodeToBeRemoved);
                    queueNodeToBeRemoved.Remove();
                    _b2Size--;
                }

                Replace(queueNode);
            }

            _t1Size++;
            CacheStorageMap.Add(key, queueNode);
            queueNode.AddToLast(_t1Head);
        }

        /// <summary>
        /// 执行任务命中B1 (Case：Key在B1中)。
        /// </summary>
        /// <param name="queueNode">queueNode queue node。</param>
        private void OnHitOnB1(QueueNode<TKey, TValue> queueNode)
        {
            _adaptiveParameter = Math.Min(Capacity, _adaptiveParameter + Math.Max(_b2Size / _b1Size, 1));
            Replace(queueNode);

            _t2Size++;
            _b1Size--;
            queueNode.Remove();
            queueNode.QueueType = QueueType.T2;
            queueNode.AddToLast(_t2Head);
        }

        /// <summary>
        /// 执行任务命中B2 (Case：Key在B2中)。
        /// </summary>
        /// <param name="queueNode">queueNode queue node。</param>
        private void OnHitOnB2(QueueNode<TKey, TValue> queueNode)
        {
            _adaptiveParameter = Math.Max(0, _adaptiveParameter - Math.Max(_b1Size / _b2Size, 1));
            Replace(queueNode);

            _t2Size++;
            _b2Size--;
            queueNode.Remove();
            queueNode.QueueType = QueueType.T2;
            queueNode.AddToLast(_t2Head);
        }

        /// <summary>
        /// 执行任务命中T1或者T2 (Case：Key在T1或者T2中)。
        /// </summary>
        /// <param name="queueNode">queueNode queue node。</param>
        private void OnHitOnT1orT2(QueueNode<TKey, TValue> queueNode)
        {
            if (queueNode.QueueType == QueueType.T1)
            {
                _t1Size--;
                _t2Size++;
            }

            queueNode.Remove();
            queueNode.QueueType = QueueType.T2;
            queueNode.AddToLast(_t2Head);
        }

        /// <summary>
        /// 替换队列节点（情况：L1（T1 u B1）少于c个）
        /// </summary>
        /// <param name="queueNode">queueNode queue node。</param>
        private void Replace(QueueNode<TKey, TValue> queueNode)
        {
            if ((_t1Size >= 1) && (((queueNode.QueueType == QueueType.B2) && (_t1Size == _adaptiveParameter)) || (_t1Size > _adaptiveParameter)))
            {
                QueueNode<TKey, TValue> queueNodeToBeRemoved = _t1Head.Next;
                queueNodeToBeRemoved.Remove();
                queueNodeToBeRemoved.QueueType = QueueType.B1;
                queueNodeToBeRemoved.AddToLast(_b1Head);
                _t1Size--;
                _b1Size++;
            }
            else
            {
                QueueNode<TKey, TValue> queueNodeToBeRemoved = _t2Head.Next;
                queueNodeToBeRemoved.Remove();
                queueNodeToBeRemoved.QueueType = QueueType.B2;
                queueNodeToBeRemoved.AddToLast(_b2Head);
                _t2Size--;
                _b2Size++;
            }
        }


        /// <summary>
        /// Remove TValue data from queue and dispose it
        /// </summary>
        /// <param name="queueNodeToBeRemoved">queueNodeToBeRemoved queue node to be remove from queue</param>
        public void RemoveFromQueue(QueueNode<TKey, TValue> queueNodeToBeRemoved)
        {
            CacheStorageMap.Remove(queueNodeToBeRemoved.Key);
            TValue value = queueNodeToBeRemoved.Get();
            try
            {
                OnRemoveCallback?.Invoke(value);
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
        }

        public void PrintCacheIdsFromQueue()
        {
            String keys = "";
            foreach (var queueNode in CacheStorageMap)
            {
                var key = queueNode.Key;
                if (keys == "")
                    keys += key;
                else
                    keys += " | " + key;
            }

            Debug.Log("All Existing Keys in Cache are : " + keys);
        }
    }
}