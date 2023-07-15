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
    public sealed partial class WorldConfigData :  AProto, IConfigTable, IDisposable
    {
        [ProtoMember(1)]
        public List<WorldConfig> List { get; set; } = new List<WorldConfig>();
        [ProtoIgnore]
        private readonly Dictionary<uint, WorldConfig> _configs = new Dictionary<uint, WorldConfig>();
        private static WorldConfigData _instance;

        public static WorldConfigData Instance
        {
            get { return _instance ??= ConfigTableManage.Load<WorldConfigData>(); } 
            private set => _instance = value;
        }

        public WorldConfig Get(uint id, bool check = true)
        {
            if (_configs.ContainsKey(id))
            {
                return _configs[id];
            }
    
            if (check)
            {
                throw new Exception($"WorldConfig not find {id} Id");
            }
            
            return null;
        }
        public bool TryGet(uint id, out WorldConfig config)
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
                WorldConfig config = List[i];
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
    public sealed partial class WorldConfig : AProto
    {
		[ProtoMember(1, IsRequired  = true)]
		public uint Id { get; set; } // Id
		[ProtoMember(2, IsRequired  = true)]
		public string WorldName { get; set; } // 名称
		[ProtoMember(3, IsRequired  = true)]
		public string DbConnection { get; set; } // 连接字符串
		[ProtoMember(4, IsRequired  = true)]
		public string DbName { get; set; } // 数据库名称
		[ProtoMember(5, IsRequired  = true)]
		public string DbType { get; set; } // 数据库类型
		[ProtoMember(6, IsRequired  = true)]
		public bool IsGameWorld { get; set; } // 是否游戏服        		     
    } 
}   