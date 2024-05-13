using System.Diagnostics;

namespace GameBase
{
    /// <summary>
    /// 全局对象必须继承于此。
    /// </summary>
    /// <typeparam name="T">子类类型。</typeparam>
    public abstract class Singleton<T> : ISingleton where T : Singleton<T>, new()
    {
        protected static T _instance = default(T);

        public static T Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new T();
                    _instance.Init();
                    SingletonSystem.Retain(_instance);
                }

                return _instance;
            }
        }

        public static bool IsValid => _instance != null;

        protected Singleton()
        {
#if UNITY_EDITOR
            string st = new StackTrace().ToString();
            // using const string to compare simply
            if (!st.Contains("GameBase.Singleton`1[T].get_Instance"))
            {
                UnityEngine.Debug.LogError($"请必须通过Instance方法来实例化{typeof(T).FullName}类");
            }
#endif
        }

        protected virtual void Init()
        {
        }

        public virtual void Active()
        {
        }

        public virtual void Release()
        {
            if (_instance != null)
            {
                SingletonSystem.Release(_instance);
                _instance = null;
            }
        }
    }
}