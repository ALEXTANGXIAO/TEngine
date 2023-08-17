// ReSharper disable StaticMemberInGenericType

using System;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_WEBGL
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
using TEngine.DataStructure;
#pragma warning disable CS8601
#pragma warning disable CS8604
#pragma warning disable CS8600

namespace TEngine.Core
{
    public static class SingletonSystem
    {
        private static readonly Queue<IUpdateSingleton> Updates = new Queue<IUpdateSingleton>();
        private static readonly OneToManyQueue<int, ISingleton> Singletons = new OneToManyQueue<int, ISingleton>();

        public static void Initialize()
        {
            AssemblyManager.OnLoadAssemblyEvent += Load;
            AssemblyManager.OnUnLoadAssemblyEvent += UnLoad;
        }
        
        private static void Load(int assemblyName)
        {
            var count = 0;
#if !UNITY_WEBGL
           var task = new List<Task>();
#endif
            UnLoad(assemblyName);
            foreach (var singletonType in AssemblyManager.ForEach(assemblyName, typeof(ISingleton)))
            {
                var instance = (ISingleton) Activator.CreateInstance(singletonType);
                var registerMethodInfo = singletonType.BaseType?.GetMethod("RegisterSingleton", BindingFlags.Instance | BindingFlags.NonPublic);
                var initializeMethodInfo = singletonType.GetMethod("Initialize", BindingFlags.Instance | BindingFlags.Public);
                var onLoadMethodInfo = singletonType.GetMethod("OnLoad", BindingFlags.Instance | BindingFlags.NonPublic);
                
                if (initializeMethodInfo != null)
                {
#if !UNITY_WEBGL
                    task.Add((Task) initializeMethodInfo.Invoke(instance, null));
#else
                    initializeMethodInfo.Invoke(instance, null);
#endif
                }
                
                registerMethodInfo?.Invoke(instance, new object[] {instance});
                onLoadMethodInfo?.Invoke(instance, new object[] {assemblyName});

                count++;
                
                switch (instance)
                {
                    case IUpdateSingleton iUpdateSingleton:
                    {
                        Updates.Enqueue(iUpdateSingleton);
                        break;
                    }
                }
                
                Singletons.Enqueue(assemblyName, instance);
            }

#if !UNITY_WEBGL
            Task.WaitAll(task.ToArray());
#endif
            Log.Info($"assembly:{assemblyName} load Singleton count:{count}");
        }

        private static void UnLoad(Queue<ISingleton> singletons)
        {
            while (singletons.Count > 0)
            {
                try
                {
                    singletons.Dequeue().Dispose();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        private static void UnLoad(int assemblyName)
        {
            if (!Singletons.TryGetValue(assemblyName, out var singletons))
            {
                return;
            }

            var count = singletons.Count;
            UnLoad(singletons);
            Singletons.RemoveKey(assemblyName);
            // Log.Info($"assembly:{assemblyName} Unload Singleton count:{count}");
        }

        public static void Update()
        {
            var updatesCount = Updates.Count;

            while (updatesCount-- > 0)
            {
                var updateSingleton = Updates.Dequeue();

                if (updateSingleton.IsDisposed)
                {
                    continue;
                }
                
                Updates.Enqueue(updateSingleton);

                try
                {
                    updateSingleton.Update();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public static void Dispose()
        {
            foreach (var (_, singletons) in Singletons)
            {
                UnLoad(singletons);
            }

            Updates.Clear();
            Singletons.Clear();
            AssemblyManager.OnLoadAssemblyEvent -= Load;
            AssemblyManager.OnUnLoadAssemblyEvent -= UnLoad;
        }
    }
}