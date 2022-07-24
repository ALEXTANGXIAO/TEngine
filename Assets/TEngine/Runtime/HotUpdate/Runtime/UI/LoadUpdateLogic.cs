using System;

namespace TEngine.UI
{
    public class LoadUpdateLogic
    {
        private static LoadUpdateLogic _instance;

        public Action<int> Download_Complete_Action    = null;        
        public Action<long> Down_Progress_Action      = null;
        public Action<bool,GameStatus> _Unpacked_Complete_Action   = null;
        public Action<float,GameStatus> _Unpacked_Progress_Action = null;

        public static LoadUpdateLogic Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LoadUpdateLogic();
                return _instance;
            }
        }
    }
}