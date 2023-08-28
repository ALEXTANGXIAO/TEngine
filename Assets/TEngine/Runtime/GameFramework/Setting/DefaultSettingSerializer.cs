namespace TEngine
{
    /// <summary>
    /// 默认游戏配置序列化器。
    /// </summary>
    public sealed class DefaultSettingSerializer : GameFrameworkSerializer<DefaultSetting>
    {
        private static readonly byte[] Header = new byte[] { (byte)'T', (byte)'E', (byte)'S' };

        /// <summary>
        /// 初始化默认游戏配置序列化器的新实例。
        /// </summary>
        public DefaultSettingSerializer()
        {
        }

        /// <summary>
        /// 获取默认游戏配置头标识。
        /// </summary>
        /// <returns>默认游戏配置头标识。</returns>
        protected override byte[] GetHeader()
        {
            return Header;
        }
    }
}
