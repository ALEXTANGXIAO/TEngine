using System;
using System.Collections.Generic;
using System.Reflection;
#if TENGINE_NET
using System.Runtime.Loader;
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
#endif
#pragma warning disable CS8603
#pragma warning disable CS8618
namespace TEngine.Core
{
    public static class AssemblyManager
    {
        public static event Action<int> OnLoadAssemblyEvent;
        public static event Action<int> OnUnLoadAssemblyEvent;
        public static event Action<int> OnReLoadAssemblyEvent;
        private static readonly Dictionary<int, AssemblyInfo> AssemblyList = new Dictionary<int, AssemblyInfo>();

        public static void Initialize()
        {
            LoadAssembly(int.MaxValue, typeof(AssemblyManager).Assembly);
        }

        public static void LoadAssembly(int assemblyName, Assembly assembly)
        {
            var isReLoad = false;

            if (!AssemblyList.TryGetValue(assemblyName, out var assemblyInfo))
            {
                assemblyInfo = new AssemblyInfo();
                AssemblyList.Add(assemblyName, assemblyInfo);
            }
            else
            {
                isReLoad = true;
                assemblyInfo.Unload();

                if (OnUnLoadAssemblyEvent != null)
                {
                    OnUnLoadAssemblyEvent(assemblyName);
                }
            }

            assemblyInfo.Load(assembly);

            if (OnLoadAssemblyEvent != null)
            {
                OnLoadAssemblyEvent(assemblyName);
            }

            if (isReLoad && OnReLoadAssemblyEvent != null)
            {
                OnReLoadAssemblyEvent(assemblyName);
            }
        }
        
        public static void Load(int assemblyName, Assembly assembly)
        {
            if (int.MaxValue == assemblyName)
            {
                throw new NotSupportedException("AssemblyName cannot be 2147483647");
            }

            LoadAssembly(assemblyName, assembly);
        }
        
        public static IEnumerable<int> ForEachAssemblyName()
        {
            foreach (var (key, _) in AssemblyList)
            {
                yield return key;
            }
        }

        public static IEnumerable<Type> ForEach()
        {
            foreach (var (_, assemblyInfo) in AssemblyList)
            {
                foreach (var type in assemblyInfo.AssemblyTypeList)
                {
                    yield return type;
                }
            }
        }

        public static IEnumerable<Type> ForEach(int assemblyName)
        {
            if (!AssemblyList.TryGetValue(assemblyName, out var assemblyInfo))
            {
                yield break;
            }

            foreach (var type in assemblyInfo.AssemblyTypeList)
            {
                yield return type;
            }
        }

        public static IEnumerable<Type> ForEach(Type findType)
        {
            foreach (var (_, assemblyInfo) in AssemblyList)
            {
                if (!assemblyInfo.AssemblyTypeGroupList.TryGetValue(findType, out var assemblyLoad))
                {
                    yield break;
                }
                
                foreach (var type in assemblyLoad)
                {
                    yield return type;
                }
            }
        }

        public static IEnumerable<Type> ForEach(int assemblyName, Type findType)
        {
            if (!AssemblyList.TryGetValue(assemblyName, out var assemblyInfo))
            {
                yield break;
            }

            if (!assemblyInfo.AssemblyTypeGroupList.TryGetValue(findType, out var assemblyLoad))
            {
                yield break;
            }
            
            foreach (var type in assemblyLoad)
            {
                yield return type;
            }
        }

        public static Assembly GetAssembly(int assemblyName)
        {
            return !AssemblyList.TryGetValue(assemblyName, out var assemblyInfo) ? null : assemblyInfo.Assembly;
        }

        public static void Dispose()
        {
            foreach (var (_, assemblyInfo) in AssemblyList)
            {
                assemblyInfo.Unload();
            }
            
            AssemblyList.Clear();

            if (OnLoadAssemblyEvent != null)
            {
                foreach (var @delegate in OnLoadAssemblyEvent.GetInvocationList())
                {
                    OnLoadAssemblyEvent -= @delegate as Action<int>;
                }
            }
            
            if (OnUnLoadAssemblyEvent != null)
            {
                foreach (var @delegate in OnUnLoadAssemblyEvent.GetInvocationList())
                {
                    OnUnLoadAssemblyEvent -= @delegate as Action<int>;
                }
            }
            
            if (OnReLoadAssemblyEvent != null)
            {
                foreach (var @delegate in OnReLoadAssemblyEvent.GetInvocationList())
                {
                    OnReLoadAssemblyEvent -= @delegate as Action<int>;
                }
            }
        }
    }
}