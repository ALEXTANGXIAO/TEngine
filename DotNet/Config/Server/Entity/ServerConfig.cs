using System;
using ProtoBuf;
using TEngine.Core;
using System.Linq;
using System.Collections.Generic;
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS0169
#pragma warning disable CS8618
#pragma warning disable CS8625
#pragma warning disable CS8603

namespace TEngine
{
    [ProtoContract]
    public sealed partial class ServerConfigData :  AProto, IConfigTable, IDisposable
    {
        [ProtoMember(1)]
        public List<ServerConfig> List { get; set; } = new List<ServerConfig>();
        [ProtoIgnore]
        private readonly Dictionary<uint, ServerConfig> _configs = new Dictionary<uint, ServerConfig>();
        private static ServerConfigData _instance;

        public static ServerConfigData Instance
        {
            get { return _instance ??= ConfigTableManage.Load<ServerConfigData>(); } 
            private set => _instance = value;
        }

        public ServerConfig Get(uint id, bool check = true)
        {
            if (_configs.ContainsKey(id))
            {
                return _configs[id];
            }
    
            if (check)
            {
                throw new Exception($"ServerConfig not find {id} Id");
            }
            
            return null;
        }
        public bool TryGet(uint id, out ServerConfig config)
        {
            config = null;
            
            if (!_configs.ContainsKey(id))
            {
                return false;
            }
                
            config = _configs[id];
            return true;
        }
        public override void AfterDeserialization()
        {
            for (var i = 0; i < List.Count; i++)
            {
                ServerConfig config = List[i];
                _configs.Add(config.Id, config);
                config.AfterDeserialization();
            }
    
            base.AfterDeserialization();
        }
        
        public void Dispose()
        {
            Instance = null;
        }
    }
    
    [ProtoContract]
    public sealed partial class ServerConfig : AProto
    {
		[ProtoMember(1, IsRequired  = true)]
		public uint Id { get; set; } // 路由Id
		[ProtoMember(2, IsRequired  = true)]
		public uint MachineId { get; set; } // 机器ID
		[ProtoMember(3, IsRequired  = true)]
		public int InnerPort { get; set; } // 内网端口
		[ProtoMember(4, IsRequired  = true)]
		public bool ReleaseMode { get; set; } // Release下运行        		     
    } 
}   