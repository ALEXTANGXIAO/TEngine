namespace TEngine
{
    /// <summary>
    /// 资源相关常量。
    /// </summary>
    public static partial class Constant
    {
        /// <summary>
        /// 默认资源加载优先级。
        /// </summary>
        internal const int DefaultPriority = 0;
    }
    
    /// <summary>
    /// 常用设置相关常量。
    /// </summary>
    public static partial class Constant
    {
        public static class Setting
        {
            public const string Language = "Setting.Language";
            public const string SoundGroupMuted = "Setting.{0}Muted";
            public const string SoundGroupVolume = "Setting.{0}Volume";
            public const string MusicMuted = "Setting.MusicMuted";
            public const string MusicVolume = "Setting.MusicVolume";
            public const string SoundMuted = "Setting.SoundMuted";
            public const string SoundVolume = "Setting.SoundVolume";
            public const string UISoundMuted = "Setting.UISoundMuted";
            public const string UISoundVolume = "Setting.UISoundVolume";
        }
    }
}