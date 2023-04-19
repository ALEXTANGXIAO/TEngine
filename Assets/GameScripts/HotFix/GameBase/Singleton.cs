using TEngine;

namespace GameBase
{
    public class Singleton<T> where T:new()
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new T();
                    Log.Assert(_instance != null);
                }

                return _instance;
            }
        }
    }
}
