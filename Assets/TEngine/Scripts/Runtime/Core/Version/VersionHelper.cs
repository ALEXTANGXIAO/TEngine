using UnityEngine;

namespace TEngine.Runtime
{
    public class VersionHelper : Version.IVersionHelper
    {
        public string GameVersion => GameConfig.Instance.AppId;

        public int InternalGameVersion => int.Parse(GameConfig.Instance.ResId);

        public string ApplicableGameVersion => GameConfig.Instance.ResId;

        public int InternalResourceVersion => int.Parse(GameConfig.Instance.BaseResId());
    }
}
