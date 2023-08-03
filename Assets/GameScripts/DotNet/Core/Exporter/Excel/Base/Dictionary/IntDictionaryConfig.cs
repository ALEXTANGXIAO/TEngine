using ProtoBuf;

namespace TEngine.Core
{
    [ProtoContract]
    public class IntDictionaryConfig
    {
        [ProtoMember(1, IsRequired = true)] 
        public Dictionary<int, int> Dic;
        
        public int this[int key] => GetValue(key);

        public bool TryGetValue(int key, out int value)
        {
            value = default;

            if (!Dic.ContainsKey(key))
            {
                return false;
            }
        
            value = Dic[key];
            return true;
        }

        private int GetValue(int key)
        {
            return Dic.TryGetValue(key, out var value) ? value : default;
        }
    }
}