using System;
using System.Text;

namespace TEngine
{
    /// <summary>
    /// 字符串构建器缓存。
    /// <remarks>常用于临时构筑。</remarks>
    /// </summary>
    public static class StringBuilderCache
    {
        [ThreadStatic] private static StringBuilder _cacheStringBuilder;

        private const int MaxBuilderSize = 512;

        /// <summary>
        /// 获取缓存中的字符串构建器。
        /// </summary>
        /// <param name="capacity">字符串构建器容量。</param>
        /// <returns>字符串构建器。</returns>
        public static StringBuilder Acquire(int capacity = 256)
        {
            StringBuilder stringBuilder = _cacheStringBuilder;

            if (stringBuilder != null && stringBuilder.Capacity >= capacity)
            {
                _cacheStringBuilder = null;
                stringBuilder.Clear();
                return stringBuilder;
            }

            return new StringBuilder(capacity);
        }

        /// <summary>
        /// 获取文本并释放字符串构建器。
        /// </summary>
        /// <param name="stringBuilder">字符串构建器。</param>
        /// <returns>文本实例。</returns>
        public static string GetStringAndRelease(StringBuilder stringBuilder)
        {
            string result = stringBuilder.ToString();
            Release(stringBuilder);
            return result;
        }

        /// <summary>
        /// 释放字符串构建器。
        /// </summary>
        /// <param name="stringBuilder">字符串构建器。</param>
        public static void Release(StringBuilder stringBuilder)
        {
            if (stringBuilder.Capacity <= MaxBuilderSize)
            {
                _cacheStringBuilder = stringBuilder;
            }
        }
    }
}