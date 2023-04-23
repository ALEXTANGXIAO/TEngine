using System;
using ProtoBuf;
using TEngine;
using System.Collections.Generic;
namespace GameProto
{
	// 这个文件只放协议，和协议头
	// 消息协议
	[Serializable,global::ProtoBuf.ProtoContract(Name = @"CSPkg")]
	public partial class CSPkg
	{
		[global::ProtoBuf.ProtoMember(1)]
		public CSPkgHead Head { get; set; }

		[global::ProtoBuf.ProtoMember(2)]
		public CSPkgBody Body { get; set; }

	}

	// 消息协议头
	[Serializable,global::ProtoBuf.ProtoContract(Name = @"CSPkgHead")]
	public partial class CSPkgHead
	{
		[global::ProtoBuf.ProtoMember(1)]
		public uint MsgId { get; set; }

		[global::ProtoBuf.ProtoMember(2)]
		public uint MsgLength { get; set; }

		[global::ProtoBuf.ProtoMember(3)]
		public uint MsgVersion { get; set; }

		[global::ProtoBuf.ProtoMember(4)]
		public uint Echo { get; set; }

		[global::ProtoBuf.ProtoMember(5)]
		public uint SvrTime { get; set; }

	}

	// 消息协议体
	[Serializable,global::ProtoBuf.ProtoContract(Name = @"CSPkgBody")]
	public partial class CSPkgBody
	{
	}

	// 协议ID
	[global::ProtoBuf.ProtoContract()]
	public enum CSMsgID
	{
		CS_START		= 0,

		CS_HeartBeat    = 10001,

		CS_END          = 10000,

	}

}
