#if TENGINE_NET
using TEngine.Core.DataBase;
using TEngine.Core;
#pragma warning disable CS8603
#pragma warning disable CS8618

namespace TEngine.Core
{
    public static class ConfigTableManage
    {
        public static Func<uint, ServerConfigInfo> ServerConfig;
        public static Func<uint, MachineConfigInfo> MachineConfig;
        public static Func<uint, SceneConfigInfo> SceneConfig;
        public static Func<uint, WorldConfigInfo> WorldConfigInfo;
        
        public static Func<List<ServerConfigInfo>> AllServerConfig;
        public static Func<List<MachineConfigInfo>> AllMachineConfig;
        public static Func<List<SceneConfigInfo>> AllSceneConfig;
        
        private const string BinaryDirectory = "../../../Config/Binary/";
        private static readonly Dictionary<string, AProto> ConfigDic = new ();
        
        public static T Load<T>() where T : AProto
        {
            var dataConfig = typeof(T).Name;
            
            if (ConfigDic.TryGetValue(dataConfig, out var aProto))
            {
                return (T)aProto;
            }
            
            try
            {
                var configFile = GetConfigPath(dataConfig);
                var bytes = File.ReadAllBytes(configFile);
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
                var configFile = GetConfigPath(type.Name);
                var bytes = File.ReadAllBytes(configFile);
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

        private static string GetConfigPath(string name)
        {
            var configFile = Path.Combine(BinaryDirectory, $"{name}.bytes");

            if (File.Exists(configFile))
            {
                return configFile;
            }

            throw new FileNotFoundException($"{name}.byte not found: {configFile}");
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