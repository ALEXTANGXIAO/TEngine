using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace TEngine
{
    public class TypeUtility
    {
        public static Type GetType(string typeName)
        {
            return GetType("", typeName, false);
        }

        public static Type GetTypeInEditor(string typeName)
        {
            return GetType("", typeName, true);
        }

        public static Type GetNestedType(Type inDelareType, string inStrTypeName)
        {
            Type type = inDelareType.GetNestedType(inStrTypeName,
                BindingFlags.Public | BindingFlags.NonPublic);

            if (type == null)
            {
                TLogger.LogError("GetNestedType failed: {0}.{1}", inDelareType, inStrTypeName);
            }
            return type;
        }

        private static Dictionary<string, Dictionary<string, Type>> _s_type_with_name_cache = null;
        public static Type GetType(string inStrAssemblyName, string inStrTypeName, bool inIsEditor)
        {
            if (string.IsNullOrEmpty(inStrTypeName))
            {
                TLogger.LogError("GetType:typeName is null");
                return null;
            }

            Type type = null;

            if (_s_type_with_name_cache == null)
            {
                _s_type_with_name_cache = new Dictionary<string, Dictionary<string, Type>>();
            }

            if (_s_type_with_name_cache.ContainsKey(inStrAssemblyName) != true)
            {
                _s_type_with_name_cache.Add(inStrAssemblyName, new Dictionary<string, Type>());
            }
            else
            {
                if (_s_type_with_name_cache[inStrAssemblyName].ContainsKey(inStrTypeName))
                {
                    type = _s_type_with_name_cache[inStrAssemblyName][inStrTypeName];

                    return type;
                }
            }

            type = Type.GetType(inStrTypeName);

            if (null != type)
            {
                _s_type_with_name_cache[inStrAssemblyName][inStrTypeName] = type;
                return type;
            }

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!string.IsNullOrEmpty(inStrAssemblyName) && a.GetName().Name != inStrAssemblyName)
                    continue;

                type = a.GetType(inStrTypeName);
                if (type != null)
                {
                    _s_type_with_name_cache[inStrAssemblyName][inStrTypeName] = type;
                    return type;
                }

            }
            return null;
        }

        public static void ClearTypeCache()
        {
            if (_s_type_with_name_cache != null)
            {
                foreach (var kv in _s_type_with_name_cache)
                {
                    if (kv.Value != null)
                    {
                        kv.Value.Clear();
                    }
                }
                _s_type_with_name_cache.Clear();
                _s_type_with_name_cache = null;
            }
        }

        public class TypeComparer : IComparer<Type>
        {
            public int Compare(Type x, Type y)
            {
                if (object.ReferenceEquals(x, y))
                    return 0;
                else if (x == null)
                    return -1;
                else if (y == null)
                    return 1;
                else
                {
                    return string.Compare(x.FullName, y.FullName);
                }
            }
        }

        private static Dictionary<Type, Type[]> msDerivedTypes = new Dictionary<Type, Type[]>();


        public static bool InitStaticTypes(Type inBaseType, Type[] inTypes)
        {
            if (inTypes.Length == 0)
                return true;

            if (!msDerivedTypes.TryGetValue(inBaseType, out Type[] existingTypes))
            {
                existingTypes = inTypes;
                msDerivedTypes[inBaseType] = inTypes;
            }
            else
            {
                int numRedefinedTypes = 0;

                List<Type> tempTypes = new List<Type>(inTypes.Length + existingTypes.Length);
                tempTypes.AddRange(existingTypes);

#if UNITY_EDITOR
                TypeComparer comparer = new TypeUtility.TypeComparer();

                for (int i = 0; i < inTypes.Length; ++i)
                {
                    Type curType = inTypes[i];
                    int idx = Array.BinarySearch<Type>(existingTypes, 0, existingTypes.Length, curType, comparer);
                    if (idx < 0)
                        tempTypes.Add(curType);
                    else
                        numRedefinedTypes++;
                }

                if (numRedefinedTypes > 0)
                {
                    TLogger.LogError("{0} Redefined type in static type list.", numRedefinedTypes);
                }
                existingTypes = tempTypes.ToArray();
                Array.Sort(existingTypes, comparer);
#else
                tempTypes.AddRange(inTypes);
                existingTypes = tempTypes.ToArray();
#endif
                msDerivedTypes[inBaseType] = existingTypes;
            }
            return true;
        }



        [ThreadStatic]
        private static Dictionary<Type, Type[]> msDerivedTypesCurThread;

        public static void GetDerivedTypes(Type inType, string inStrAssembly, out Type[] outTypes)
        {
            GetTypesImpl(inType, inStrAssembly, true, IsDerivedClass, out outTypes);
        }

        public static void GetTypesImpInterfaceNonGenericNonAbstract(Type inInterface, string inStrAssembly, out Type[] outTypes)
        {
            GetTypesImpl(inInterface, inStrAssembly, true, IsImplInterfaceNonGeneric, out outTypes);
        }

        public static bool IsImplInterfaceNonGeneric(Type inInterface, Type inType)
        {
            return !inType.IsAbstract && !inType.IsGenericType && inType.GetInterface(inInterface.Name) != null;
        }

        public static bool IsSubclassOf(Type src, Type dst)
        {
            return src.IsSubclassOf(dst);
        }

        public static bool Equals(Type src, Type dst)
        {
            return src.Equals(dst);
        }

        public static bool IsDerivedClass(Type inBaseType, Type inType)
        {
            return (inBaseType == inType) || (!inType.IsAbstract && inType.IsSubclassOf(inBaseType));
        }

        public static void GetTypesImpl(Type inParamType, string inStrAssembly, bool inIsUseCache,
            Func<Type, Type, bool> inCriteria, out Type[] outTypes)
        {
            Type[] existingTypes = null;
            if (inIsUseCache && msDerivedTypes.TryGetValue(inParamType, out existingTypes) && existingTypes != null)
            {
#if !UNITY_EDITOR
                outTypes = existingTypes;
                return;
#endif
            }

            if (inIsUseCache)
            {
                if (msDerivedTypesCurThread == null)
                    msDerivedTypesCurThread = new Dictionary<Type, Type[]>();
                else
                {
                    if (msDerivedTypesCurThread.TryGetValue(inParamType, out Type[] threadExistingTypes)
                        && existingTypes != null)
                    {
                        outTypes = existingTypes;
                        return;
                    }
                }
            }

            if (existingTypes == null)
            {
                TLogger.LogWarning("TypeUtility: Getting {0} Derived Type by iterating assembly.", inParamType);
            }
            else
            {
                TLogger.LogInfo("TypeUtility: Checking {0} Derived Type with assembly iterating.", inParamType);
            }

            List<Type> derivedTypeList = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int k = 0; k < assemblies.Length; ++k)
            {
                Assembly tAssemble = assemblies[k];

                if (!string.IsNullOrEmpty(inStrAssembly)
                    && !tAssemble.GetName().Name.Equals(inStrAssembly))
                {
                    continue;
                }

                Type[] allTypes = null;

                try
                {
                    allTypes = tAssemble.GetTypes();
                }
                catch (Exception e)
                {
                    TLogger.LogError("GetDerivedTypes: {0} , GetTypes error -> {1}", e.ToString(), tAssemble);
                }

                if (allTypes == null)
                    continue;

                for (int i = 0; i < allTypes.Length; ++i)
                {
                    Type type = allTypes[i];
                    try
                    {
                        if (inCriteria(inParamType, type))
                        {
                            derivedTypeList.Add(type);
                        }
                    }
                    catch (Exception e)
                    {
                        TLogger.LogError("GetDerivedTypes with exception {0}, Type error -> {1} ", e.ToString(), type);
                    }
                }
            }

#if UNITY_EDITOR
            if (existingTypes != null)
            {
                TypeUtility.TypeComparer comparer = new TypeComparer();

                derivedTypeList.Sort(comparer);

                StringBuilder sb = new StringBuilder();
                int numDiff = 0;
                int numTypes;
                numTypes = derivedTypeList.Count;

                int i = 0;
                for (; i < numTypes; ++i)
                {
                    Type t = derivedTypeList[i];
                    int idx = Array.BinarySearch(existingTypes, t, comparer);
                    if (idx < 0)
                    {
                        if (t.Assembly.GetName().Name != "PMEditor")
                        {
                            sb.AppendFormat("{0}\n", t.Name);
                            numDiff++;
                        }
                    }
                }

                if (numDiff != 0)
                {

                    TLogger.LogWarning("GetDerivedTypes {0} consistency check failed with differences,\n runtime = {1}, static list = {2}, re-export code! Diff: {3}",
                    inParamType, derivedTypeList.Count, existingTypes.Length, sb.ToString());

                }
            }
            else
            {

                TLogger.LogWarning("GetDerivedTypes type \"{0}\" was not defined in generated code.", inParamType);

            }
#endif

            outTypes = derivedTypeList.ToArray();
            if (inIsUseCache)
            {
                msDerivedTypesCurThread[inParamType] = outTypes;
            }
        }

    }
}
