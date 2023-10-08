using System;

namespace GameMain
{
    public class LoadUpdateLogic
    {
        private static LoadUpdateLogic _instance;

        public Action<int> DownloadCompleteAction = null;
        public Action<float> DownProgressAction = null;
        public Action<bool> UnpackedCompleteAction = null;
        public Action<float> UnpackedProgressAction = null;

        public static LoadUpdateLogic Instance => _instance ??= new LoadUpdateLogic();
    }
}