namespace TEngine
{
    /// <summary>
    /// 通用单例，无需释放和销毁
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> where T : new()
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new T();
                }
                return _instance;
            }
        }

        public static T Active()
        {
            return Instance;
        }
    }
}