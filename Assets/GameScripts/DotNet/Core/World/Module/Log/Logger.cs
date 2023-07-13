using System.Diagnostics;
using System.Text;

namespace ET
{
    public class Logger : Singleton<Logger>, ISingletonAwake
    {
        private ILog iLog;

        public ILog ILog
        {
            set { this.iLog = value; }
        }

        public enum LogLevel
        {
            INFO,
            SUCCESSED,
            ASSERT,
            WARNING,
            ERROR,
            EXCEPTION,
        }
        
        public void Awake()
        {
        }

        private LogLevel _filterLevel = LogLevel.INFO;
        private StringBuilder _stringBuilder = new StringBuilder();

        private const int TraceLevel = 1;
        private const int DebugLevel = 2;
        private const int InfoLevel = 3;
        private const int WarningLevel = 4;

        private bool CheckLogLevel(int level)
        {
            if (Options.Instance == null)
            {
                return true;
            }

            return Options.Instance.LogLevel <= level;
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
            this.Log(LogLevel.INFO, msg);
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

        public void Console(string message)
        {
            if (Options.Instance.Console == 1)
            {
                this.Log(LogLevel.SUCCESSED, message);
            }

            this.iLog.Debug(message);
        }

        public void Console(string message, params object[] args)
        {
            string s = string.Format(message, args);
            if (Options.Instance.Console == 1)
            {
                System.Console.WriteLine(s);
            }

            this.iLog.Debug(s);
        }

        private StringBuilder GetFormatString(LogLevel logLevel, string logString)
        {
            _stringBuilder.Clear();
            switch (logLevel)
            {
                case LogLevel.SUCCESSED:
                    _stringBuilder.AppendFormat(
                        "[TEngineServer][SUCCESSED][{0}] - {1}",
                        System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), logString);
                    break;
                case LogLevel.INFO:
                    _stringBuilder.AppendFormat(
                       "[TEngineServer][INFO][{0}] - {1}",
                        System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), logString);
                    break;
                case LogLevel.ASSERT:
                    _stringBuilder.AppendFormat(
                       "[TEngineServer][ASSERT][{0}] - {1}",
                        System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), logString);
                    break;
                case LogLevel.WARNING:
                    _stringBuilder.AppendFormat(
                        "[TEngineServer][WARNING][{0}] - {1}", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"),
                        logString);
                    break;
                case LogLevel.ERROR:
                    _stringBuilder.AppendFormat(
                       "[TEngineServer][ERROR][{0}] - {1}",
                        System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), logString);
                    break;
                case LogLevel.EXCEPTION:
                    _stringBuilder.AppendFormat(
                        "[TEngineServer][EXCEPTION][{0}] - {1}",
                        System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), logString);
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
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.WriteLine(logStr);
            }
            else if (type == LogLevel.SUCCESSED)
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine(logStr);
            }
            else if (type == LogLevel.WARNING)
            {
                System.Console.ForegroundColor = ConsoleColor.DarkYellow;
                System.Console.WriteLine(logStr);
            }
            else if (type == LogLevel.ASSERT)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine(logStr);
            }
            else if (type == LogLevel.ERROR)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine(logStr);
            }
            else if (type == LogLevel.EXCEPTION)
            {
                System.Console.ForegroundColor = ConsoleColor.Magenta;
                System.Console.WriteLine(logStr);
            }
        }
    }
}