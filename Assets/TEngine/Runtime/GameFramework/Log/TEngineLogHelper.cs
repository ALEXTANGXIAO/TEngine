using System.Diagnostics;
using System.Text;
using Debug = UnityEngine.Debug;

namespace TEngine
{
    public class TEngineLogHelper : GameFrameworkLog.ILogHelper
    {
        public void Log(GameFrameworkLogLevel level, object message)
        {
            switch (level)
            {
                case GameFrameworkLogLevel.Debug:
                    TEngineLogger.LogInfo(Utility.Text.Format("<color=#888888>{0}</color>", message));
                    break;
                case GameFrameworkLogLevel.Info:
                    TEngineLogger.LogInfo(message.ToString());
                    break;

                case GameFrameworkLogLevel.Warning:
                    TEngineLogger.LogWarning(message.ToString());
                    break;

                case GameFrameworkLogLevel.Error:
                    TEngineLogger.LogError(message.ToString());
                    break;
                case GameFrameworkLogLevel.Fatal:
                    TEngineLogger.LogException(message.ToString());
                    break;
                default:
                    throw new GameFrameworkException(message.ToString());
            }
        }
    }

    public class TEngineLogger
    {
        public static void LogInfo(string logStr)
        {
            Log(LogLevel.INFO, logStr);
        }

        public static void LogWarning(string logStr)
        {
            Log(LogLevel.WARNING, logStr);
        }

        public static void LogError(string logStr)
        {
            Log(LogLevel.ERROR, logStr);
        }

        public static void LogException(string logStr)
        {
            Log(LogLevel.EXCEPTION, logStr);
        }

        private static StringBuilder GetFormatString(LogLevel logLevel, string logString, bool bColor)
        {
            _stringBuilder.Clear();
#if UNITY_EDITOR
            string timeNow = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff");
#else
            string timeNow = string.Empty;
#endif
            switch (logLevel)
            {
                case LogLevel.Successd:
                    _stringBuilder.AppendFormat(
                        bColor
                            ? "<color=#0099bc><b>[TEngine] ► </b></color><color=gray><b>[INFO] ► </b></color>[{0}] - <color=#00FF18>{1}</color>"
                            : "<color=#0099bc><b>[TEngine] ► </b></color><color=#00FF18><b>[SUCCESSED] ► </b></color>[{0}] - {1}",
                        timeNow, logString);
                    break;
                case LogLevel.INFO:
                    _stringBuilder.AppendFormat(
                        bColor
                            ? "<color=#0099bc><b>[TEngine] ► </b></color><color=gray><b>[INFO] ► </b></color>[{0}] - <color=gray>{1}</color>"
                            : "<color=#0099bc><b>[TEngine] ► </b></color><color=gray><b>[INFO] ► </b></color>[{0}] - {1}",
                        timeNow, logString);
                    break;
                case LogLevel.ASSERT:
                    _stringBuilder.AppendFormat(
                        bColor
                            ? "<color=#0099bc><b>[TEngine] ► </b></color><color=#FF00BD><b>[ASSERT] ► </b></color>[{0}] - <color=green>{1}</color>"
                            : "<color=#0099bc><b>[TEngine] ► </b></color><color=#FF00BD><b>[ASSERT] ► </b></color>[{0}] - {1}",
                        timeNow, logString);
                    break;
                case LogLevel.WARNING:
                    _stringBuilder.AppendFormat(
                        bColor
                            ? "<color=#0099bc><b>[TEngine] ► </b></color><color=#FF9400><b>[WARNING] ► </b></color>[{0}] - <color=yellow>{1}</color>"
                            : "<color=#0099bc><b>[TEngine] ► </b></color><color=#FF9400><b>[WARNING] ► </b></color>[{0}] - {1}", timeNow,
                        logString);
                    break;
                case LogLevel.ERROR:
                    _stringBuilder.AppendFormat(
                        bColor
                            ? "<color=#0099bc><b>[TEngine] ► </b></color><color=red><b>[ERROR] ► </b></color>[{0}] - <color=red>{1}</color>"
                            : "<color=#0099bc><b>[TEngine] ► </b></color><color=red><b>[ERROR] ► </b></color>[{0}] - {1}",
                        timeNow, logString);
                    break;
                case LogLevel.EXCEPTION:
                    _stringBuilder.AppendFormat(
                        bColor
                            ? "<color=#0099bc><b>[TEngine] ► </b></color><color=red><b>[EXCEPTION] ► </b></color>[{0}] - <color=red>{1}</color>"
                            : "<color=#0099bc><b>[TEngine] ► </b></color><color=red><b>[EXCEPTION] ► </b></color>[{0}] - {1}",
                        timeNow, logString);
                    break;
            }

            return _stringBuilder;
        }

        private static void Log(LogLevel type, string logString)
        {
            if (_outputType == OutputType.NONE)
            {
                return;
            }

            if (type < _filterLevel)
            {
                return;
            }

            StringBuilder infoBuilder = GetFormatString(type, logString, true);
            string logStr = infoBuilder.ToString();

            //获取C#堆栈,Warning以上级别日志才获取堆栈
            if (type == LogLevel.ERROR || type == LogLevel.WARNING || type == LogLevel.EXCEPTION)
            {
                StackFrame[] stackFrames = new StackTrace().GetFrames();
                for (int i = 0; i < stackFrames.Length; i++)
                {
                    StackFrame frame = stackFrames[i];
                    string declaringTypeName = frame.GetMethod().DeclaringType.FullName;
                    string methodName = stackFrames[i].GetMethod().Name;

                    infoBuilder.AppendFormat("[{0}::{1}\n", declaringTypeName, methodName);
                }
            }

            if (type == LogLevel.INFO || type == LogLevel.Successd)
            {
                Debug.Log(logStr);
            }
            else if (type == LogLevel.WARNING)
            {
                Debug.LogWarning(logStr);
            }
            else if (type == LogLevel.ASSERT)
            {
                Debug.LogAssertion(logStr);
            }
            else if (type == LogLevel.ERROR)
            {
                Debug.LogError(logStr);
            }
            else if (type == LogLevel.EXCEPTION)
            {
                Debug.LogError(logStr);
            }
        }

        #region Properties

        public enum LogLevel
        {
            INFO,
            Successd,
            ASSERT,
            WARNING,
            ERROR,
            EXCEPTION,
        }

        [System.Flags]
        public enum OutputType
        {
            NONE = 0,
            EDITOR = 0x1,
            GUI = 0x2,
            FILE = 0x4
        }

        private static LogLevel _filterLevel = LogLevel.INFO;
        private static OutputType _outputType = OutputType.EDITOR;
        private static StringBuilder _stringBuilder = new StringBuilder();

        #endregion
    }
}