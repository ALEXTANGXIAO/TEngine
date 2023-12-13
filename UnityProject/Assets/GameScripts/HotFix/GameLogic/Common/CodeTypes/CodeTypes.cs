using System;
using System.Collections.Generic;
using System.Reflection;

namespace GameLogic
{
    public class CodeTypes
    {
        private static CodeTypes _instance;
        public static CodeTypes Instance => _instance ??= new CodeTypes();

        private readonly Dictionary<string, Type> _allTypes = new();
        private readonly UnOrderMultiMapSet<Type, Type> _types = new();
        
        public void Init(Assembly[] assemblies)
        {
            Dictionary<string, Type> addTypes = GetAssemblyTypes(assemblies);
            foreach ((string fullName, Type type) in addTypes)
            {
                _allTypes[fullName] = type;
                
                if (type.IsAbstract)
                {
                    continue;
                }
                
                // 记录所有的有BaseAttribute标记的的类型
                object[] objects = type.GetCustomAttributes(typeof(BaseAttribute), true);

                foreach (object o in objects)
                {
                    _types.Add(o.GetType(), type);
                }
            }
        }

        public HashSet<Type> GetTypes(Type systemAttributeType)
        {
            if (!_types.ContainsKey(systemAttributeType))
            {
                return new HashSet<Type>();
            }

            return _types[systemAttributeType];
        }

        public Dictionary<string, Type> GetTypes()
        {
            return _allTypes;
        }

        public Type GetType(string typeName)
        {
            return _allTypes[typeName];
        }
        
        public static Dictionary<string, Type> GetAssemblyTypes(params Assembly[] args)
        {
            Dictionary<string, Type> types = new Dictionary<string, Type>();

            foreach (Assembly ass in args)
            {
                foreach (Type type in ass.GetTypes())
                {
                    if (type.FullName != null)
                    {
                        types[type.FullName] = type;
                    }
                }
            }

            return types;
        }
    }
}