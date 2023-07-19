#if TENGINE_UNITY
using System;
using System.Collections.Generic;
using TEngine.Core;
using UnityEngine;

namespace TEngine.Core
{
    public static class ConfigTableManage
    {
        private static readonly string ConfigBundle = "Config".ToLower();
        private static readonly Dictionary<string, AProto> ConfigDic = new ();

        static ConfigTableManage()
        {

        }
        
        public static T Load<T>() where T : AProto
        {
            var dataConfig = typeof(T).Name;
            
            if (ConfigDic.TryGetValue(dataConfig, out var aProto))
            {
                return (T)aProto;
            }
            
            try
            {
                var bytes = GameModule.Resource.LoadAsset<TextAsset>($"{ConfigBundle}-{dataConfig}").bytes;
                var data = (AProto) ProtoBufHelper.FromBytes(typeof(T), bytes, 0, bytes.Length);
                data.AfterDeserialization();
                ConfigDic[dataConfig] = data;
                return (T)data;
            }
            catch (Exception ex)
            {
                throw new Exception($"ConfigTableManage:{typeof(T).Name} 数据表加载之后反序列化时出错:{ex}");
            }
        }
        
        private static AProto Load(string dataConfig, int assemblyName)
        {
            if (ConfigDic.TryGetValue(dataConfig, out var aProto))
            {
                return aProto;
            }
            
            var fullName = $"TEngine.{dataConfig}";
            var assembly = AssemblyManager.GetAssembly(assemblyName);
            var type = assembly.GetType(fullName);

            if (type == null)
            {
                Log.Error($"not find {fullName} in assembly");
                return null;
            }
            
            try
            {
                var bytes = GameModule.Resource.LoadAsset<TextAsset>($"{ConfigBundle}-{dataConfig}").bytes;
                var data = (AProto) ProtoBufHelper.FromBytes(type, bytes, 0, bytes.Length);
                data.AfterDeserialization();
                ConfigDic[dataConfig] = data;
                return data;
            }
            catch (Exception ex)
            {
                throw new Exception($"ConfigTableManage:{type.Name} 数据表加载之后反序列化时出错:{ex}");
            }
        }

        private static void Reload()
        {
            foreach (var (_, aProto) in ConfigDic)
            {
                ((IDisposable) aProto).Dispose();
            }

            ConfigDic.Clear();
        }
    }
}
#endif