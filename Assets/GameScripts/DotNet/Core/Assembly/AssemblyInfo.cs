using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TEngine.DataStructure;

namespace TEngine.Core
{
    public sealed class AssemblyInfo
    {
        public Assembly Assembly { get; private set; }
        public readonly List<Type> AssemblyTypeList = new List<Type>();
        public readonly OneToManyList<Type, Type> AssemblyTypeGroupList = new OneToManyList<Type, Type>();

        public void Load(Assembly assembly)
        {
            Assembly = assembly;
            var assemblyTypes = assembly.GetTypes().ToList();

            foreach (var type in assemblyTypes)
            {
                if (type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                var interfaces = type.GetInterfaces();

                foreach (var interfaceType in interfaces)
                {
                    AssemblyTypeGroupList.Add(interfaceType, type);
                }
            }

            AssemblyTypeList.AddRange(assemblyTypes);
        }

        public void Unload()
        {
            AssemblyTypeList.Clear();
            AssemblyTypeGroupList.Clear();
        }
    }
}