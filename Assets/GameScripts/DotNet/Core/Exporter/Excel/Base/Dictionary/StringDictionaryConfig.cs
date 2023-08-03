using System.Collections.Generic;
using ProtoBuf;

namespace TEngine.Core
{
    [ProtoContract]
    public sealed class StringDictionaryConfig
    {
        [ProtoMember(1, IsRequired = true)] 
        public Dictionary<int, string> Dic;
        
        public string this[int key] => GetValue(key);

        public bool TryGetValue(int key, out string value)
        {
            value = default;

            if (!Dic.ContainsKey(key))
            {
                return false;
            }
        
            value = Dic[key];
            return true;
        }

        private string GetValue(int key)
        {
            return Dic.TryGetValue(key, out var value) ? value : default;
        }
    }
}