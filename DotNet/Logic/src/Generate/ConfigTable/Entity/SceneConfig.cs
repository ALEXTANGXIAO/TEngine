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
    public sealed partial class SceneConfigData :  AProto, IConfigTable, IDisposable
    {
        [ProtoMember(1)]
        public List<SceneConfig> List { get; set; } = new List<SceneConfig>();
        [ProtoIgnore]
        private readonly Dictionary<uint, SceneConfig> _configs = new Dictionary<uint, SceneConfig>();
        private static SceneConfigData _instance;

        public static SceneConfigData Instance
        {
            get { return _instance ??= ConfigTableManage.Load<SceneConfigData>(); } 
            private set => _instance = value;
        }

        public SceneConfig Get(uint id, bool check = true)
        {
            if (_configs.ContainsKey(id))
            {
                return _configs[id];
            }
    
            if (check)
            {
                throw new Exception($"SceneConfig not find {id} Id");
            }
            
            return null;
        }
        public bool TryGet(uint id, out SceneConfig config)
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
                SceneConfig config = List[i];
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
    public sealed partial class SceneConfig : AProto
    {
		[ProtoMember(1, IsRequired  = true)]
		public uint Id { get; set; } // ID
		[ProtoMember(2, IsRequired  = true)]
		public long EntityId { get; set; } // 实体Id
		[ProtoMember(3, IsRequired  = true)]
		public uint RouteId { get; set; } // 路由Id
		[ProtoMember(4, IsRequired  = true)]
		public uint WorldId { get; set; } // 世界Id
		[ProtoMember(5, IsRequired  = true)]
		public string SceneType { get; set; } // Scene类型
		[ProtoMember(6, IsRequired  = true)]
		public string Name { get; set; } // 名称
		[ProtoMember(7, IsRequired  = true)]
		public string NetworkProtocol { get; set; } // 协议类型
		[ProtoMember(8, IsRequired  = true)]
		public int OuterPort { get; set; } // 外网端口        		     
    } 
}   