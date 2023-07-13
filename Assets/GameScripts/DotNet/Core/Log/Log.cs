#if TENGINE_NET
namespace TEngine
{
    public static class Log
    {
        static Log()
        {
            LogCore.Instance.ILog = new NLog("Server");
        }

        /// <summary>
        /// 打印追朔级别日志，用于记录追朔类日志信息。
        /// </summary>
        /// <param name="msg">日志内容。</param>
        public static void Trace(string msg)
        {
            LogCore.Instance.Trace(msg);
        }

        /// <summary>
        /// 打印调试级别日志，用于记录调试类日志信息。
        /// </summary>
        /// <param name="msg">日志内容。</param>
        public static void Debug(string msg)
        {
            LogCore.Instance.Debug(msg);
        }

        /// <summary>
        /// 打印信息级别日志，用于记录信息类日志信息。
        /// </summary>
        /// <param name="msg">日志内容。</param>
        public static void Info(string msg)
        {
            LogCore.Instance.Info(msg);
        }

        /// <summary>
        /// 打印追朔级别日志，用于记录追朔类日志信息。
        /// </summary>
        /// <param name="msg">日志内容。</param>
        public static void TraceInfo(string msg)
        {
            LogCore.Instance.Trace(msg);
        }

        /// <summary>
        /// 打印追警告日志，用于记录警告类日志信息。
        /// </summary>
        /// <param name="msg">日志内容。</param>
        public static void Warning(string msg)
        {
            LogCore.Instance.Warning(msg);
        }

        /// <summary>
        /// 打印错误级别日志，建议在发生功能逻辑错误，但尚不会导致游戏崩溃或异常时使用。
        /// </summary>
        /// <param name="msg">日志内容。</param>
        public static void Error(string msg)
        {
            LogCore.Instance.Error(msg);
        }

        /// <summary>
        /// 打印错误级别日志，建议在发生功能逻辑错误，但尚不会导致游戏崩溃或异常时使用。
        /// </summary>
        /// <param name="exception">异常内容。</param>
        public static void Error(Exception exception)
        {
            LogCore.Instance.Error(exception);
        }

        /// <summary>
        /// 打印严重错误级别日志，建议在发生严重错误，可能导致游戏崩溃或异常时使用，此时应尝试重启进程或重建服务框架。
        /// </summary>
        /// <param name="msg">日志内容。</param>
        public static void Fatal(string msg)
        {
            LogCore.Instance.Fatal(msg);
        }

        /// <summary>
        /// 打印严重错误级别日志，建议在发生严重错误，可能导致游戏崩溃或异常时使用，此时应尝试重启进程或重建服务框架。
        /// </summary>
        /// <param name="exception">常内容。</param>
        public static void Fatal(Exception exception)
        {
            LogCore.Instance.Fatal(exception.Message);
        }

        /// <summary>
        /// 断言严重错误级别日志，建议在发生严重错误。
        /// </summary>
        /// <param name="condition">断言条件</param>
        /// <param name="msg"></param>
        public static void Assert(bool condition, string msg = "")
        {
            if (!condition)
            {
                if (string.IsNullOrEmpty(msg))
                {
                    msg = AssertError;
                }

                LogCore.Instance.Error(msg);
            }
        }

        /// <summary>
        /// 断言默认日志。
        /// </summary>
        public const string AssertError = "ASSERT FAILD";

        public static void Trace(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            LogCore.Instance.Trace(message.ToStringAndClear());
        }

        public static void Warning(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            LogCore.Instance.Warning(message.ToStringAndClear());
        }

        public static void Info(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            LogCore.Instance.Info(message.ToStringAndClear());
        }

        public static void Debug(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            LogCore.Instance.Debug(message.ToStringAndClear());
        }

        public static void Error(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            LogCore.Instance.Error(message.ToStringAndClear());
        }
    }
}
#endif
