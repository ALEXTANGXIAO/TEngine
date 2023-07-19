using System.Threading.Tasks;
#pragma warning disable CS8601
#pragma warning disable CS8618

namespace TEngine.Core
{
    public abstract class Singleton<T> : ISingleton where T : ISingleton, new()
    {
        public bool IsDisposed { get; set; }
        
        public static T Instance { get; private set; }

        private void RegisterSingleton(ISingleton singleton)
        {
            Instance = (T) singleton;
            AssemblyManager.OnLoadAssemblyEvent += OnLoad;
            AssemblyManager.OnUnLoadAssemblyEvent += OnUnLoad;
        }

        /// <summary>
        /// 特殊情况调用设置。
        /// </summary>
        public static T TempInstance
        {
            get
            {
                return Instance;
            }
            set
            {
                if (Instance != null)
                {
                    return;
                }

                Instance = value;
            }
        }

        public virtual Task Initialize()
        {
            return Task.CompletedTask;
        }

        protected virtual void OnLoad(int assemblyName) { }

        protected virtual void OnUnLoad(int assemblyName) { }

        public virtual void Dispose()
        {
            IsDisposed = true;
            Instance = default;
            AssemblyManager.OnLoadAssemblyEvent -= OnLoad;
            AssemblyManager.OnUnLoadAssemblyEvent -= OnUnLoad;
        }
    }
}