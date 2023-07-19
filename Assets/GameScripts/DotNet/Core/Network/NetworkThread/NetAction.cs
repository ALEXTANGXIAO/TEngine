using System;
using System.IO;
#pragma warning disable CS8625
#pragma warning disable CS8618

namespace TEngine.Core.Network
{
    public enum NetActionType
    {
        None = 0,
        Send = 1,
        SendMemoryStream = 2,
        RemoveChannel = 3,
    }
    
    public struct NetAction : IDisposable
    {
        public object Obj;
        public uint RpcId;
        public long EntityId;
        public long NetworkId;
        public uint ChannelId;
        public long RouteTypeOpCode;
        public MemoryStream MemoryStream;
        public NetActionType NetActionType;

        public NetAction(long networkId, uint channelId, uint rpcId, long routeTypeOpCode, long entityId, NetActionType netActionType, MemoryStream memoryStream)
        {
            Obj = null;
            RpcId = rpcId;
            EntityId = entityId;
            NetworkId = networkId;
            ChannelId = channelId;
            RouteTypeOpCode = routeTypeOpCode;
            MemoryStream = memoryStream;
            NetActionType = netActionType;
        }

        public NetAction(long networkId, uint channelId, uint rpcId, long routeTypeOpCode, long entityId, NetActionType netActionType, object obj)
        {
            Obj = obj;
            RpcId = rpcId;
            EntityId = entityId;
            NetworkId = networkId;
            ChannelId = channelId;
            MemoryStream = null;
            RouteTypeOpCode = routeTypeOpCode;
            NetActionType = netActionType;
        }

        public void Dispose()
        {
            Obj = null;
            MemoryStream = null;
            RpcId = 0;
            EntityId = 0;
            NetworkId = 0;
            ChannelId = 0;
            RouteTypeOpCode = 0;
            NetActionType = NetActionType.None;
        }
    }
}