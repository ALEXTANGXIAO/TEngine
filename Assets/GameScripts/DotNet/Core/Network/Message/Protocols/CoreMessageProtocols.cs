using TEngine.Core.Network;
using ProtoBuf;

namespace TEngine
{
    [ProtoContract]
    public sealed class Response : AProto, IResponse
    {
        public uint OpCode()
        {
            return Opcode.DefaultResponse;
        }

        [ProtoMember(90)] public long RpcId { get; set; }
        [ProtoMember(91, IsRequired = true)] public uint ErrorCode { get; set; }
    }

    [ProtoContract]
    public sealed class RouteResponse : AProto, IRouteResponse
    {
        public uint OpCode()
        {
            return Opcode.DefaultRouteResponse;
        }
        
        [ProtoMember(90)] public long RpcId { get; set; }
        [ProtoMember(91, IsRequired = true)] public uint ErrorCode { get; set; }
    }
    [ProtoContract]
    public class PingRequest : AProto, IRequest
    {
        public uint OpCode()
        {
            return Opcode.PingRequest;
        }

        [ProtoIgnore] public PingResponse ResponseType { get; set; }
        [ProtoMember(90)] public long RpcId { get; set; }
    }
    [ProtoContract]
    public class PingResponse : AProto, IResponse
    {
        public uint OpCode()
        {
            return Opcode.PingResponse;
        }

        [ProtoMember(90)] public long RpcId { get; set; }
        [ProtoMember(91, IsRequired = true)] public uint ErrorCode { get; set; }
        [ProtoMember(1)] public long Now;
    }
    /// <summary>
    /// 添加一个可寻址地址
    /// </summary>
    [ProtoContract]
    public partial class I_AddressableAdd_Request : AProto, IRouteRequest
    {
        [ProtoIgnore]
        public I_AddressableAdd_Response ResponseType { get; set; }
        public uint OpCode() { return Opcode.AddressableAddRequest; }
        public long RouteTypeOpCode() { return 1; }
        [ProtoMember(1)]
        public long AddressableId { get; set; }
        [ProtoMember(2)]
        public long RouteId { get; set; }
    }
    [ProtoContract]
    public partial class I_AddressableAdd_Response : AProto, IRouteResponse
    {
        public uint OpCode() { return Opcode.AddressableAddResponse; }
        [ProtoMember(91, IsRequired = true)]
        public uint ErrorCode { get; set; }
    }
    /// <summary>
    /// 查询一个可寻址
    /// </summary>
    [ProtoContract]
    public partial class I_AddressableGet_Request : AProto, IRouteRequest
    {
        [ProtoIgnore]
        public I_AddressableGet_Response ResponseType { get; set; }
        public uint OpCode() { return Opcode.AddressableGetRequest; }
        public long RouteTypeOpCode() { return 1; }
        [ProtoMember(1)]
        public long AddressableId { get; set; }
    }
    [ProtoContract]
    public partial class I_AddressableGet_Response : AProto, IRouteResponse
    {
        public uint OpCode() { return Opcode.AddressableGetResponse; }
        [ProtoMember(91, IsRequired = true)]
        public uint ErrorCode { get; set; }
        [ProtoMember(1)]
        public long RouteId { get; set; }
    }
    /// <summary>
    /// 删除一个可寻址
    /// </summary>
    [ProtoContract]
    public partial class I_AddressableRemove_Request : AProto, IRouteRequest
    {
        [ProtoIgnore]
        public I_AddressableRemove_Response ResponseType { get; set; }
        public uint OpCode() { return Opcode.AddressableRemoveRequest; }
        public long RouteTypeOpCode() { return 1; }
        [ProtoMember(1)]
        public long AddressableId { get; set; }
    }
    [ProtoContract]
    public partial class I_AddressableRemove_Response : AProto, IRouteResponse
    {
        public uint OpCode() { return Opcode.AddressableRemoveResponse; }
        [ProtoMember(91, IsRequired = true)]
        public uint ErrorCode { get; set; }
    }
    /// <summary>
    /// 锁定一个可寻址
    /// </summary>
    [ProtoContract]
    public partial class I_AddressableLock_Request : AProto, IRouteRequest
    {
        [ProtoIgnore]
        public I_AddressableLock_Response ResponseType { get; set; }
        public uint OpCode() { return Opcode.AddressableLockRequest; }
        public long RouteTypeOpCode() { return 1; }
        [ProtoMember(1)]
        public long AddressableId { get; set; }
    }
    [ProtoContract]
    public partial class I_AddressableLock_Response : AProto, IRouteResponse
    {
        public uint OpCode() { return Opcode.AddressableLockResponse; }
        [ProtoMember(91, IsRequired = true)]
        public uint ErrorCode { get; set; }
    }
    /// <summary>
    /// 解锁一个可寻址
    /// </summary>
    [ProtoContract]
    public partial class I_AddressableUnLock_Request : AProto, IRouteRequest
    {
        [ProtoIgnore]
        public I_AddressableUnLock_Response ResponseType { get; set; }
        public uint OpCode() { return Opcode.AddressableUnLockRequest; }
        public long RouteTypeOpCode() { return 1; }
        [ProtoMember(1)]
        public long AddressableId { get; set; }
        [ProtoMember(2)]
        public long RouteId { get; set; }
        [ProtoMember(3)]
        public string Source { get; set; }
    }
    [ProtoContract]
    public partial class I_AddressableUnLock_Response : AProto, IRouteResponse
    {
        public uint OpCode() { return Opcode.AddressableUnLockResponse; }
        [ProtoMember(91, IsRequired = true)]
        public uint ErrorCode { get; set; }
    }
}