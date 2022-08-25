namespace TEngine.Runtime
{
    public static partial class Version
    {
        /// <summary>
        /// 版本号辅助器接口。
        /// </summary>
        public interface IVersionHelper
        {
            /// <summary>
            /// 获取游戏版本号。
            /// </summary>
            string GameVersion
            {
                get;
            }

            /// <summary>
            /// 获取内部游戏版本号。
            /// </summary>
            int InternalGameVersion
            {
                get;
            }

            /// <summary>
            /// 获取当前资源适用的游戏版本号。
            /// </summary>
            string ApplicableGameVersion
            {
                get;
            }

            /// <summary>
            /// 获取当前内部资源版本号。
            /// </summary>
            int InternalResourceVersion
            {
                get;
            }
        }
    }
}