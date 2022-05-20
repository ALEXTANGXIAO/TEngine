using System;
using System.Collections.Generic;
using System.IO;

namespace TEngine
{
    public class LogToFile
    {
        private FileWriter _fileWriter = new FileWriter();
        private static readonly string mLogRootPath = Path.Combine(FileSystem.PersistentDataPath, "Log").FixPath();
        private static int MaxLogDays = 3;
        private static string _currentLogName = string.Empty;

        private string GetLogPath(DateTime dt)
        {
            string dataDir = dt.ToString("yyyy_MM_dd");
            string logPath = Path.Combine(mLogRootPath, dataDir);
            return logPath.FixPath();
        }

        public void DeInit()
        {
            if (_fileWriter == null)
            {
                return;
            }
            _fileWriter.Flush();
            _fileWriter.Shutdown();
        }

        public void Init()
        {
            DateTime currentTime = DateTime.Now;
            RemoveOldLogs(currentTime);
            string logDir = GetLogPath(currentTime);
            string logFileName = string.Format("Log_{0}.log", currentTime.ToString("yyyy_MM_dd-HH_mm_ss"));
            _currentLogName = logFileName;
            string fullPath = Path.Combine(logDir, logFileName).FixPath();
            try
            {
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                _fileWriter.OpenFile(fullPath);
            }
            catch (Exception e)
            {
                TLogger.LogException( e.ToString());
            }
        }

        public void Write(string msg)
        {
            if (_fileWriter == null)
            {
                return;
            }
            _fileWriter.Write(msg);
        }

        private void RemoveOldLogs(DateTime now)
        {
            HashSet<string> foldersToKeep = new HashSet<string>();
            for (int i = 0; i < MaxLogDays; i++)
            {
                DateTime current = now.AddDays(-i);
                string folder = GetLogPath(current);
                foldersToKeep.Add(folder);
            }
            if (Directory.Exists(mLogRootPath))
            {
                try
                {
                    string[] allLogDir = Directory.GetDirectories(mLogRootPath);
                    foreach (string dir in allLogDir)
                    {
                        string fixedDir = dir.FixPath();
                        if (!foldersToKeep.Contains(fixedDir))
                        {
                            try
                            {
                                Directory.Delete(fixedDir, true);
                            }
                            catch (Exception e)
                            {
                                TLogger.LogException(e.ToString());
                            }

                        }
                    }
                }
                catch (Exception e)
                {
                    TLogger.LogException(e.ToString());
                }
            }
        }
    }
}