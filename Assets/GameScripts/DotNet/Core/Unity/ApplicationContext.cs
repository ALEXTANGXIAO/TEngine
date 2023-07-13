#if TENGINE_UNITY
using System.Threading;
using TEngine.Core;

namespace TEngine
{
    public struct OnAppStart
    {
        public Scene ClientScene;
    }

    public struct OnAppClosed { }
    
    public static class ApplicationContext
    {
        public static void Initialize()
        {
            // 设置默认的线程的同步上下文
            SynchronizationContext.SetSynchronizationContext(ThreadSynchronizationContext.Main);
            // 初始化SingletonSystemCenter这个一定要放到最前面
            // 因为SingletonSystem会注册AssemblyManager的OnLoadAssemblyEvent和OnUnLoadAssemblyEvent的事件
            // 如果不这样、会无法把程序集的单例注册到SingletonManager中
            SingletonSystem.Initialize();
            // 加载核心程序集
            AssemblyManager.Initialize();
        }
        
        public static void Close()
        {
            SingletonSystem.Dispose();
            AssemblyManager.Dispose();
            Scene.DisposeAllScene();
        }
    }
}
#endif