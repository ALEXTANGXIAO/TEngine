using TEngine.Runtime.HotUpdate;
namespace TEngine.Runtime
{
    public class OnlineVersionInfo
    {
        /// <summary>
        /// 最新的游戏版本号
        /// </summary>
        public string GameVersion
        {
            get;
            set;
        }

        /// <summary>
        /// 最新的游戏内部版本号
        /// </summary>
        public int InternalResourceVersion
        {
            get;
            set;
        }

        /// <summary>
        /// 最新的资源内部版本号
        /// </summary>
        public int ResourceVersion
        {
            get;
            set;
        }

        /// <summary>
        /// 资源更新下载地址
        /// </summary>
        public string UpdatePrefixUri
        {
            get;
            set;
        }

        public UpdateType UpdateType
        {
            get;
            set;
        }

        public UpdateStyle UpdateStyle
        {
            get;
            set;
        }

        public UpdateNotice UpdateNotice
        {
            get;
            set;
        }
    }
}