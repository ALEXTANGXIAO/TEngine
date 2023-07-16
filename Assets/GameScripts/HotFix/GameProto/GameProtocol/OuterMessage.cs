using ProtoBuf;
using Unity.Mathematics;
using System.Collections.Generic;
using TEngine.Core.Network;
#pragma warning disable CS8618

namespace TEngine
{
	/// <summary>
	///  发送一个消息到Gate服务器
	/// </summary>
	[ProtoContract]
	public partial class H_C2G_Message : AProto, IMessage
	{
		public uint OpCode() { return OuterOpcode.H_C2G_Message; }
		[ProtoMember(1)]
		public string Message { get; set; }
	}
	/// <summary>
	///  发送一个RPC消息到Gate服务器
	/// </summary>
	[ProtoContract]
	public partial class H_C2G_MessageRequest : AProto, IRequest
	{
		[ProtoIgnore]
		public H_G2C_MessageResponse ResponseType { get; set; }
		public uint OpCode() { return OuterOpcode.H_C2G_MessageRequest; }
		[ProtoMember(1)]
		public string Message { get; set; }
	}
	[ProtoContract]
	public partial class H_G2C_MessageResponse : AProto, IResponse
	{
		public uint OpCode() { return OuterOpcode.H_G2C_MessageResponse; }
		[ProtoMember(91, IsRequired = true)]
		public uint ErrorCode { get; set; }
		[ProtoMember(1)]
		public string Message { get; set; }
	}
	/// <summary>
	///  发送一个消息通知服务器给客户端推送一个消息
	/// </summary>
	[ProtoContract]
	public partial class H_C2G_PushMessageToClient : AProto, IMessage
	{
		public uint OpCode() { return OuterOpcode.H_C2G_PushMessageToClient; }
		[ProtoMember(1)]
		public string Message { get; set; }
	}
	/// <summary>
	///  客户端接收服务器推送的一条消息
	/// </summary>
	[ProtoContract]
	public partial class H_G2C_ReceiveMessageToServer : AProto, IMessage
	{
		public uint OpCode() { return OuterOpcode.H_G2C_ReceiveMessageToServer; }
		[ProtoMember(1)]
		public string Message { get; set; }
	}
	/// <summary>
	///  注册Address消息
	/// </summary>
	[ProtoContract]
	public partial class H_C2G_LoginAddressRequest : AProto, IRequest
	{
		[ProtoIgnore]
		public H_G2C_LoginAddressResponse ResponseType { get; set; }
		public uint OpCode() { return OuterOpcode.H_C2G_LoginAddressRequest; }
		[ProtoMember(1)]
		public string Message { get; set; }
	}
	[ProtoContract]
	public partial class H_G2C_LoginAddressResponse : AProto, IResponse
	{
		public uint OpCode() { return OuterOpcode.H_G2C_LoginAddressResponse; }
		[ProtoMember(91, IsRequired = true)]
		public uint ErrorCode { get; set; }
	}
	/// <summary>
	///  发送一个Address消息给Map
	/// </summary>
	[ProtoContract]
	public partial class H_C2M_Message : AProto, IAddressableRouteMessage
	{
		public uint OpCode() { return OuterOpcode.H_C2M_Message; }
		public long RouteTypeOpCode() { return CoreRouteType.Addressable; }
		[ProtoMember(1)]
		public string Message { get; set; }
	}
	/// <summary>
	///  发送一个AddressRPC消息给Map
	/// </summary>
	[ProtoContract]
	public partial class H_C2M_MessageRequest : AProto, IAddressableRouteRequest
	{
		[ProtoIgnore]
		public H_M2C_MessageResponse ResponseType { get; set; }
		public uint OpCode() { return OuterOpcode.H_C2M_MessageRequest; }
		public long RouteTypeOpCode() { return CoreRouteType.Addressable; }
		[ProtoMember(1)]
		public string Message { get; set; }
	}
	[ProtoContract]
	public partial class H_M2C_MessageResponse : AProto, IAddressableRouteResponse
	{
		public uint OpCode() { return OuterOpcode.H_M2C_MessageResponse; }
		[ProtoMember(91, IsRequired = true)]
		public uint ErrorCode { get; set; }
		[ProtoMember(1)]
		public string Message { get; set; }
	}
	/// <summary>
	///  发送一个消息通知服务器给客户端推送一个Address消息
	/// </summary>
	[ProtoContract]
	public partial class H_C2M_PushAddressMessageToClient : AProto, IAddressableRouteMessage
	{
		public uint OpCode() { return OuterOpcode.H_C2M_PushAddressMessageToClient; }
		public long RouteTypeOpCode() { return CoreRouteType.Addressable; }
		[ProtoMember(1)]
		public string Message { get; set; }
	}
	/// <summary>
	///  客户端接收服务器推送的一条Address消息
	/// </summary>
	[ProtoContract]
	public partial class H_M2C_ReceiveAddressMessageToServer : AProto, IAddressableRouteMessage
	{
		public uint OpCode() { return OuterOpcode.H_M2C_ReceiveAddressMessageToServer; }
		public long RouteTypeOpCode() { return CoreRouteType.Addressable; }
		[ProtoMember(1)]
		public string Message { get; set; }
	}
	/// <summary>
	///  客户端发送消息请求登录服务器
	/// </summary>
	[ProtoContract]
	public partial class H_C2G_LoginRequest : AProto, IRequest
	{
		[ProtoIgnore]
		public H_G2C_LoginResponse ResponseType { get; set; }
		public uint OpCode() { return OuterOpcode.H_C2G_LoginRequest; }
		[ProtoMember(1)]
		public string UserName { get; set; }
		[ProtoMember(2)]
		public string Password { get; set; }
	}
	[ProtoContract]
	public partial class H_G2C_LoginResponse : AProto, IResponse
	{
		public uint OpCode() { return OuterOpcode.H_G2C_LoginResponse; }
		[ProtoMember(91, IsRequired = true)]
		public uint ErrorCode { get; set; }
		[ProtoMember(1)]
		public uint UID { get; set; }
		[ProtoMember(2)]
		public string Text { get; set; }
	}
	/// <summary>
	///  客户端发送消息请求注册账户
	/// </summary>
	[ProtoContract]
	public partial class H_C2G_RegisterRequest : AProto, IRequest
	{
		[ProtoIgnore]
		public H_G2C_RegisterResponse ResponseType { get; set; }
		public uint OpCode() { return OuterOpcode.H_C2G_RegisterRequest; }
		[ProtoMember(1)]
		public string UserName { get; set; }
		[ProtoMember(2)]
		public string Password { get; set; }
		[ProtoMember(3)]
		public uint SDKUID { get; set; }
	}
	[ProtoContract]
	public partial class H_G2C_RegisterResponse : AProto, IResponse
	{
		public uint OpCode() { return OuterOpcode.H_G2C_RegisterResponse; }
		[ProtoMember(91, IsRequired = true)]
		public uint ErrorCode { get; set; }
		[ProtoMember(1)]
		public uint UID { get; set; }
	}
}
