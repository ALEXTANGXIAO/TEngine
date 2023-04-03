using System;
using YooAsset;

namespace TEngine
{
    public class YooAssetsLogger:ILogger
    {
        public void Log(string message)
        {
            TEngine.Log.Info(message);
        }

        public void Warning(string message)
        {
            TEngine.Log.Warning(message);
        }

        public void Error(string message)
        {
            TEngine.Log.Error(message);
        }

        public void Exception(Exception exception)
        {
            TEngine.Log.Fatal(exception.Message);
        }
    }
}