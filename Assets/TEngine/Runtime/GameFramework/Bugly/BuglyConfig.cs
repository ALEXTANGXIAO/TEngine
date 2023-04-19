using UnityEngine;

namespace TEngine
{
    [CreateAssetMenu]
    public class BuglyConfig : ScriptableObject
    {
        public string channelId;
        public string androidId;
        public string androidKey;
        public string iosId;
        public string iosKey;
    }
}