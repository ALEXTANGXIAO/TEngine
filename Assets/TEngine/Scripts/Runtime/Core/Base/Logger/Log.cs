namespace TEngine.Runtime
{
    public class Log
    {
        public static void Debug(string logStr)
        {
            TLogger.LogInfo(logStr);
        }

        public static void Debug(string format, params System.Object[] args)
        {
            TLogger.LogInfo(format, args);
        }

        public static void Info(string logStr)
        {
            TLogger.LogInfoSuccessd(logStr);
        }

        public static void Info(string format, params System.Object[] args)
        {
            TLogger.LogInfoSuccessd(format, args);
        }

        public static void Warning(string logStr)
        {
            TLogger.LogWarning(logStr);
        }

        public static void Warning(string format, params System.Object[] args)
        {
            TLogger.LogWarning(format, args);
        }

        public static void Error(string logStr)
        {
            TLogger.LogError(logStr);
        }

        public static void Error(string format, params System.Object[] args)
        {
            TLogger.LogError(format, args);
        }

        public static void Fatal(string logStr)
        {
            TLogger.LogException(logStr);
        }

        public static void Fatal(string format, params System.Object[] args)
        {
            TLogger.LogException(format, args);
        }

        public static void Assert(bool condition, string logStr = "")
        {
            TLogger.LogAssert(condition, logStr);
        }

        public static void Assert(bool condition, string format, params System.Object[] args)
        {
            TLogger.LogAssert(condition, format, args);
        }
    }
}