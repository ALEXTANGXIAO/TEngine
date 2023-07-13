using System;
using System.Threading;

namespace ET
{
    public static class Program
    {
        public static void Main()
        {
            Entry.Init();
            Init init = new();
            init.Start();
            while (true)
            {
                Thread.Sleep(1);
                try
                {
                    init.Update();
                    init.LateUpdate();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}