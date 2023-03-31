using System.Collections.Generic;

namespace TEngine
{
    public static class StringId
    {
        private static readonly Dictionary<string, int> eventTypeHashMap = new Dictionary<string, int>();
        private static readonly Dictionary<int, string> eventHashToStringMap = new Dictionary<int, string>();
        private static int _currentId = 0;

        public static int StringToHash(string val)
        {
            if (eventTypeHashMap.TryGetValue(val, out var hashId))
            {
                return hashId;
            }

            hashId = ++_currentId;
            eventTypeHashMap[val] = hashId;
            eventHashToStringMap[hashId] = val;

            return hashId;
        }

        public static string HashToString(int hash)
        {
            if (eventHashToStringMap.TryGetValue(hash, out var value))
            {
                return value;
            }
            return string.Empty;
        }
    }
}