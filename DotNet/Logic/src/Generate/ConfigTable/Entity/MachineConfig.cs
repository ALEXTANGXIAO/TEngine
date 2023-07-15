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
    public sealed partial class MachineConfigData :  AProto, IConfigTable, IDisposable
    {
        [ProtoMember(1)]
        public List<MachineConfig> List { get; set; } = new List<MachineConfig>();
        [ProtoIgnore]
        private readonly Dictionary<uint, MachineConfig> _configs = new Dictionary<uint, MachineConfig>();
        private static MachineConfigData _instance;

        public static MachineConfigData Instance
        {
            get { return _instance ??= ConfigTableManage.Load<MachineConfigData>(); } 
            private set => _instance = value;
        }

        public MachineConfig Get(uint id, bool check = true)
        {
            if (_configs.ContainsKey(id))
            {
                return _configs[id];
            }
    
            if (check)
            {
                throw new Exception($"MachineConfig not find {id} Id");
            }
            
            return null;
        }
        public bool TryGet(uint id, out MachineConfig config)
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
                MachineConfig config = List[i];
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
    public sealed partial class MachineConfig : AProto
    {
		[ProtoMember(1, IsRequired  = true)]
		public uint Id { get; set; } // Id
		[ProtoMember(2, IsRequired  = true)]
		public string OuterIP { get; set; } // 外网IP
		[ProtoMember(3, IsRequired  = true)]
		public string OuterBindIP { get; set; } // 外网绑定IP
		[ProtoMember(4, IsRequired  = true)]
		public string InnerBindIP { get; set; } // 内网绑定IP
		[ProtoMember(5, IsRequired  = true)]
		public int ManagementPort { get; set; } // 管理端口        		     
    } 
}   