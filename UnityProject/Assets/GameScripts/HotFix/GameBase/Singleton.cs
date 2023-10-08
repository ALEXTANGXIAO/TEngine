using TEngine;

namespace GameBase
{
    /// <summary>
    /// 通用单例。
    /// </summary>
    /// <typeparam name="T">泛型T。</typeparam>
    public class Singleton<T> where T : new()
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