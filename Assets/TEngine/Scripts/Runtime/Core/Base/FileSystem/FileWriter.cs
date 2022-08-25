using System;
using System.IO;

namespace TEngine.Runtime
{
    public class FileWriter 
    {
        private FileStream _fStream;
        private StreamWriter _writer;


        public bool OpenFile(string path)
        {
            try
            {
                _fStream = File.Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                _writer = new StreamWriter(_fStream);
                return true;
            }
            catch (Exception e)
            {
                TLogger.LogException(e.ToString());
                return false;
            }
        }

        public void Shutdown()
        {
            try
            {
                if (_writer != null)
                {
                    _writer.Close();
                }
                _writer = null;

                if (_fStream != null)
                {
                    _fStream.Close();
                }
                _fStream = null;
            }
            catch (Exception e)
            {
                TLogger.LogException(e.ToString());
                _writer = null;
                _fStream = null;
            }
        }

        public void Write(string msg)
        {
            if (_writer == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(msg))
            {
                return;
            }

            try
            {
                _writer.WriteLine(msg);
            }
            catch
            {
                Shutdown();
            }

            Flush();
        }

        public void Flush()
        {
            if (_writer == null)
            {
                return;
            }

            try
            {
                if (_writer.BaseStream != null)
                {
                    _writer.Flush();
                }
            }
            catch (Exception e)
            {
                Shutdown();
                TLogger.LogException( e.ToString());
            }
        }
    }
}