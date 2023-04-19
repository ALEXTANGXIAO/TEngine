namespace TEngine
{
    public class BuglyManager
    {
        private static BuglyManager _buglyManager;

        public static BuglyManager Instance => _buglyManager ??= new BuglyManager();

        public void Init(BuglyConfig config,string version = "")
        {
            if (config!= null)
            {
                ConfigDefault(config.channelId, string.IsNullOrEmpty(version) ? Version.GameVersion : version);
                BuglyAgent.ConfigDebugMode (false);
#if UNITY_IPHONE || UNITY_IOS
                BuglyAgent.InitWithAppId (config.iosId);
#elif UNITY_ANDROID
                BuglyAgent.InitWithAppId (config.androidId);
#endif
                BuglyAgent.EnableExceptionHandler();
                Log.Info($"Init Bugly Successes");
            }
            else
            {
                Log.Fatal("Init Bugly Fatal buglyConfig.asset is null!");
            }
        }
        
        /// <summary>
        /// 启动C#异常捕获上报，默认自动上报级别为LogError,那么LogError、LogException的异常日志都会自动捕获上报。
        /// </summary>
        public void EnableExceptionHandle()
        {
            BuglyAgent.EnableExceptionHandler();
        }

        /// <summary>
        /// 设置自动上报日志信息的级别，默认LogError，则>=LogError的日志都会自动捕获上报。
        /// </summary>
        /// <param name="logLevel"></param> 日志级别 
        public void SetReportLogLevel(LogSeverity logLevel)
        {
            BuglyAgent.ConfigAutoReportLogLevel(logLevel);
        }

        /// <summary>
        /// 设置上报的用户唯一标识，项目组可在收到服务器登录回调后调用。
        /// </summary>
        /// <param name="userId">用户唯一标识。</param>
        public void SetUserId(string userId)
        {
            BuglyAgent.SetUserId(userId);
        }

        /// <summary>
        /// 上报已捕获C#异常
        /// </summary>
        /// <param name="e">异常。</param>
        /// <param name="description">描述。</param> 
        public void ReportException(System.Exception e, string description)
        {
            BuglyAgent.ReportException(e, description);
        }

        /// <summary>
        /// 上报自定义错误信息
        /// </summary>
        /// <param name="name"></param>错误名称
        /// <param name="reason"></param>错误原因
        /// <param name="traceback"></param>错误堆栈
        public void ReportError(string name, string reason, string traceback)
        {
            BuglyAgent.ReportException(name, reason, traceback);
        }

        /// <summary>
        /// 修改默认配置
        /// </summary>
        /// <param name="channel"></param>渠道号
        /// <param name="version"></param>版本号
        /// <param name="userID"></param>用户唯一标识
        /// <param name="time"></param>初始化延时
        public void ConfigDefault(string channel, string version, string userID = "Unknow", long time = 0)
        {
            BuglyAgent.ConfigDefault(channel = null, version, userID = "Unknow", time = 0);
        }
    }
}