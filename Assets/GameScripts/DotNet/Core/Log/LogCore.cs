#if TENGINE_NET
using System.Diagnostics;
using System.Text;
using TEngine.Core;

namespace TEngine
{
    public class LogCore : Singleton<LogCore>
    {
        private ILog iLog;

        public ILog ILog
        {
            set => iLog = value;
        }

        public enum LogLevel
        {
            INFO,
            DEBUG,
            ASSERT,
            WARNING,
            ERROR,
            EXCEPTION,
        }

        private LogLevel _filterLevel = LogLevel.INFO;
        private StringBuilder _stringBuilder = new StringBuilder();

        private const int TraceLevel = 1;
        private const int DebugLevel = 2;
        private const int InfoLevel = 3;
        private const int WarningLevel = 4;

        private bool CheckLogLevel(int level)
        {
            if (AppDefine.Options == null)
            {
                return true;
            }

            return AppDefine.Options.LogLevel <= level;
        }

        public void Trace(string msg)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }

            StackTrace st = new StackTrace(2, true);
            string log = $"{msg}\n{st}";
            this.Log(LogLevel.INFO, log);
            this.iLog.Trace(log);
        }

        public void Debug(string msg)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }
            this.Log(LogLevel.DEBUG, msg);
            this.iLog.Debug(msg);
        }

        public void Info(string msg)
        {
            if (!CheckLogLevel(InfoLevel))
            {
                return;
            }
            this.Log(LogLevel.INFO, msg);
            this.iLog.Info(msg);
        }

        public void TraceInfo(string msg)
        {
            if (!CheckLogLevel(InfoLevel))
            {
                return;
            }

            StackTrace st = new StackTrace(2, true);
            string log = $"{msg}\n{st}";
            this.Log(LogLevel.INFO, msg);
            this.iLog.Trace(log);
        }

        public void Warning(string msg)
        {
            if (!CheckLogLevel(WarningLevel))
            {
                return;
            }
            this.Log(LogLevel.WARNING, msg);
            this.iLog.Warning(msg);
        }

        public void Error(string msg)
        {
            StackTrace st = new StackTrace(2, true);
            string log = $"{msg}\n{st}";
            this.Log(LogLevel.ERROR, log);
            this.iLog.Error(log);
        }

        public void Error(Exception e)
        {
            if (e.Data.Contains("StackTrace"))
            {
                this.iLog.Error($"{e.Data["StackTrace"]}\n{e}");
                return;
            }

            string str = e.ToString();
            this.Log(LogLevel.ERROR, str);
            this.iLog.Error(str);
        }

        public void Fatal(string msg)
        {
            StackTrace st = new StackTrace(2, true);
            this.iLog.Fatal($"{msg}\n{st}");
        }

        public void Trace(string message, params object[] args)
        {
            if (!CheckLogLevel(TraceLevel))
            {
                return;
            }

            StackTrace st = new StackTrace(2, true);
            this.iLog.Trace($"{string.Format(message, args)}\n{st}");
        }

        public void Warning(string message, params object[] args)
        {
            if (!CheckLogLevel(WarningLevel))
            {
                return;
            }

            this.iLog.Warning(string.Format(message, args));
        }

        public void Info(string message, params object[] args)
        {
            if (!CheckLogLevel(InfoLevel))
            {
                return;
            }

            this.iLog.Info(string.Format(message, args));
        }

        public void Debug(string message, params object[] args)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }

            this.iLog.Debug(string.Format(message, args));
        }

        public void Error(string message, params object[] args)
        {
            StackTrace st = new StackTrace(2, true);
            string s = string.Format(message, args) + '\n' + st;
            this.iLog.Error(s);
        }

        private StringBuilder GetFormatString(LogLevel logLevel, string logString)
        {
            _stringBuilder.Clear();
            switch (logLevel)
            {
                case LogLevel.DEBUG:
                    _stringBuilder.AppendFormat(
                        "[TEngine][DEBUG][{0:yyyy-MM-dd HH:mm:ss fff}] - {1}", DateTime.Now, logString);
                    break;
                case LogLevel.INFO:
                    _stringBuilder.AppendFormat(
                       "[TEngine][INFO][{0:yyyy-MM-dd HH:mm:ss fff}] - {1}", DateTime.Now, logString);
                    break;
                case LogLevel.ASSERT:
                    _stringBuilder.AppendFormat(
                       "[TEngine][ASSERT][{0:yyyy-MM-dd HH:mm:ss fff}] - {1}", DateTime.Now, logString);
                    break;
                case LogLevel.WARNING:
                    _stringBuilder.AppendFormat(
                        "[TEngine][WARNING][{0:yyyy-MM-dd HH:mm:ss fff}] - {1}", DateTime.Now,
                        logString);
                    break;
                case LogLevel.ERROR:
                    _stringBuilder.AppendFormat(
                       "[TEngine][ERROR][{0:yyyy-MM-dd HH:mm:ss fff}] - {1}", DateTime.Now, logString);
                    break;
                case LogLevel.EXCEPTION:
                    _stringBuilder.AppendFormat(
                        "[TEngine][EXCEPTION][{0:yyyy-MM-dd HH:mm:ss fff}] - {1}", DateTime.Now, logString);
                    break;
            }

            return _stringBuilder;
        }

        private void Log(LogLevel type, string logString)
        {
            if (type < _filterLevel)
            {
                return;
            }

            StringBuilder infoBuilder = GetFormatString(type, logString);

            if (type == LogLevel.ERROR || type == LogLevel.WARNING || type == LogLevel.EXCEPTION)
            {
                StackFrame[] sf = new StackTrace().GetFrames();
                for (int i = 0; i < sf.Length; i++)
                {
                    StackFrame frame = sf[i];
                    string declaringTypeName = frame.GetMethod()?.DeclaringType.FullName;
                    string methodName = frame.GetMethod()?.Name;
                    infoBuilder.AppendFormat("[{0}::{1}\n", declaringTypeName, methodName);
                }
            }

            string logStr = infoBuilder.ToString();

            if (type == LogLevel.INFO)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(logStr);
            }
            else if (type == LogLevel.DEBUG)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(logStr);
            }
            else if (type == LogLevel.WARNING)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(logStr);
            }
            else if (type == LogLevel.ASSERT)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(logStr);
            }
            else if (type == LogLevel.ERROR)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(logStr);
            }
            else if (type == LogLevel.EXCEPTION)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(logStr);
            }
        }
    }
}
#endif