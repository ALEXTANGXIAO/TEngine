using System;
using System.Collections.Generic;

namespace TEngine
{
    public abstract class PoolCore<T>
    {
        private readonly Action<T> _reset;
        private readonly Func<T> _generator;
        private readonly Stack<T> _objects = new Stack<T>();
        public int Count => _objects.Count;
        
        /// <summary>
        /// 初始化一个池子
        /// </summary>
        /// <param name="generator">某些类型的构造函数中可能需要额外的参数，所以使用Func<T>生成器</param>
        /// <param name="reset">某些类型可能需要对返回的对象进行额外清理</param>
        /// <param name="initialCapacity">池子的初始大小、可以预先分配</param>
        protected PoolCore(Func<T> generator, Action<T> reset, int initialCapacity = 0)
        {
            _generator = generator;
            _reset = reset;

            for (var i = 0; i < initialCapacity; ++i)
            {
                _objects.Push(generator());
            }
        }
        
        public T Rent()
        {
            return _objects.Count > 0 ? _objects.Pop() : _generator();
        }
        
        public void Return(T item)
        {
            _reset(item);
            _objects.Push(item);
        }

        public void Clear()
        {
            _objects.Clear();
        }
    }
}