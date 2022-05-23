using System.Collections.Generic;

namespace TEngineCore
{
    public class StringId
    {
        private static Dictionary<string, int> m_eventTypeHashMap = new Dictionary<string, int>();
        private static Dictionary<int, string> m_eventHashToStringMap = new Dictionary<int, string>();
        private static int m_currentId = 0;

        public static int StringToHash(string val)
        {
            int hashId;
            if (m_eventTypeHashMap.TryGetValue(val, out hashId))
            {
                return hashId;
            }

            hashId = ++m_currentId;
            m_eventTypeHashMap[val] = hashId;
            m_eventHashToStringMap[hashId] = val;

            return hashId;
        }

        public static string HashToString(int hash)
        {
            string value;
            if (m_eventHashToStringMap.TryGetValue(hash, out value))
            {
                return value;
            }
            return string.Empty;
        }
    }

}