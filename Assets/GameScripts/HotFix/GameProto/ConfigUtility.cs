using System;
using System.Collections.Generic;
using TEngine;

namespace GameProto
{
    /// <summary>
    /// 指定Key委托。
    /// </summary>
    /// <typeparam name="TKey">键。</typeparam>
    /// <typeparam name="TValue">值。</typeparam>
    public delegate TKey ConvertDictionaryKey<out TKey, in TValue>(TValue val);

    /// <summary>
    /// 配置表辅助工具。
    /// </summary>
    public static class ConfigUtility
    {
        /// <summary>
        /// 生成64long的主键。
        /// </summary>
        /// <param name="key1">键1。</param>
        /// <param name="key2">键2。</param>
        /// <returns>64long的主键。</returns>
        public static UInt64 Make64Key(uint key1, uint key2)
        {
            return ((UInt64)key1 << 32) | key2;
        }

        /// <summary>
        /// 拷贝配置表字典。
        /// </summary>
        /// <param name="dict">拷贝地址。</param>
        /// <param name="source">拷贝源。</param>
        /// <param name="convKey">指定主键。</param>
        /// <typeparam name="TKey">键。</typeparam>
        /// <typeparam name="TValue">值。</typeparam>
        /// <returns>是否拷贝成功。</returns>
        public static bool CopyConfigDict<TKey, TValue>(ref Dictionary<TKey, TValue> dict,List<TValue> source, ConvertDictionaryKey<TKey, TValue> convKey)
        {
            if (source == null)
            {
                return false;
            }
            
            dict.Clear();
            
            bool failed = false;
            for (int i = 0; i < source.Count; i++)
            {
                var data = source[i];
                TKey key = convKey(data);
                if (dict.ContainsKey(key))
                {
                    Log.Fatal("Copy Config Failed: {0} IndexOf {1} Had Repeated Key: {2} ", typeof(TValue).Name, i + 1, key);
                    failed = true;
                    break;
                }

                dict.Add(key, data);
            }
            return !failed;
        }
    }
}