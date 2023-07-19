using System.Collections.Generic;
#pragma warning disable CS8601

namespace TEngine.DataStructure
{
    public static class DictionaryExtensions
    {
        public static bool TryRemove<T, TV>(this IDictionary<T, TV> self, T key, out TV value)
        {
            if (!self.TryGetValue(key, out value))
            {
                return false;
            }

            self.Remove(key);
            return true;
        }
    }
}