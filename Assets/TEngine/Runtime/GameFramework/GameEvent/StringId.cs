using System.Collections.Generic;

namespace TEngine
{
    /// <summary>
    /// 运行时字符串ID。
    /// </summary>
    public static class StringId
    {
        /// <summary>
        /// Key->字符串 | Value->Id (Table)
        /// </summary>
        private static readonly Dictionary<string, int> EventTypeHashMap = new Dictionary<string, int>();
        /// <summary>
        /// Key->Id    | Value->字符串 (Table)
        /// </summary>
        private static readonly Dictionary<int, string> EventHashToStringMap = new Dictionary<int, string>();
        /// <summary>
        /// 当前Id。
        /// </summary>
        private static int _currentId = 0;

        /// <summary>
        /// 字符串转HashId。
        /// </summary>
        /// <param name="val">字符串Value。</param>
        /// <returns>HashId。</returns>
        public static int StringToHash(string val)
        {
            if (EventTypeHashMap.TryGetValue(val, out var hashId))
            {
                return hashId;
            }

            hashId = ++_currentId;
            EventTypeHashMap[val] = hashId;
            EventHashToStringMap[hashId] = val;

            return hashId;
        }

        /// <summary>
        /// HashId转字符串。
        /// </summary>
        /// <param name="hash">HashId。</param>
        /// <returns>字符串。</returns>
        public static string HashToString(int hash)
        {
            if (EventHashToStringMap.TryGetValue(hash, out var value))
            {
                return value;
            }
            return string.Empty;
        }
    }
}