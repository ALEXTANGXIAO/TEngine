using System;
using System.IO;
using TEngine.Core;
#pragma warning disable CS8625
#pragma warning disable CS8618

namespace TEngine.Core.Network
{
    public struct MessageCacheInfo
    {
        public uint RpcId;
        public long RouteId;
        public long RouteTypeOpCode;
        public object Message;
        public MemoryStream MemoryStream;
    }

    public abstract class ANetwork : IDisposable
    {
        public long Id { get; protected set; }
        public Scene Scene { get; protected set; }
        public bool IsDisposed { get; protected set; }
        public NetworkType NetworkType { get; private set; }
        public NetworkTarget NetworkTarget { get; private set; }
        public NetworkProtocolType NetworkProtocolType { get; private set; }
        public ANetworkMessageScheduler NetworkMessageScheduler { get; protected set; }

        protected readonly Func<uint, long, long, MemoryStream, object, MemoryStream> Pack;
        private readonly LastMessageInfo _lastMessageInfo = new LastMessageInfo();

        protected ANetwork(Scene scene, NetworkType networkType, NetworkProtocolType networkProtocolType, NetworkTarget networkTarget)
        {
            Scene = scene;
            NetworkType = networkType;
            NetworkTarget = networkTarget;
            NetworkProtocolType = networkProtocolType;
            Id = IdFactory.NextRunTimeId();
#if TENGINE_NET
            if (networkTarget == NetworkTarget.Inner)
            {
                Pack = InnerPack;
                NetworkMessageScheduler = new InnerMessageScheduler();
                return;
            }
#endif
            Pack = OuterPack;
            
            switch (networkType)
            {
                case NetworkType.Client:
                {
                    NetworkMessageScheduler = new ClientMessageScheduler();
                    return;
                }
                case NetworkType.Server:
                {
                    NetworkMessageScheduler = new OuterMessageScheduler();
                    return;
                }
            }
        }
#if TENGINE_NET
        private MemoryStream InnerPack(uint rpcId, long routeTypeOpCode, long routeId, MemoryStream memoryStream, object message)
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return null;
            }
#endif
            if (memoryStream != null)
            {
                return InnerPacketParser.Pack(rpcId, routeId, memoryStream);
            }

            // 只针对服务器做缓存消息优化（例如群发消息等）、避免多次序列化
            if (ReferenceEquals(_lastMessageInfo.Message, message))
            {
                _lastMessageInfo.MemoryStream.Seek(0, SeekOrigin.Begin);
                return _lastMessageInfo.MemoryStream;
            }

            memoryStream = InnerPacketParser.Pack(rpcId, routeId, message);
            _lastMessageInfo.MemoryStream = memoryStream;
            _lastMessageInfo.Message = message;
            return memoryStream;
        }
#endif
        private MemoryStream OuterPack(uint rpcId, long routeTypeOpCode, long routeId, MemoryStream memoryStream, object message)
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return null;
            }
#endif
            if (memoryStream != null)
            {
                return OuterPacketParser.Pack(rpcId, routeTypeOpCode, memoryStream);
            }
            
            // 只针对服务器做缓存消息优化（例如群发消息等）、避免多次序列化
            // 客户端没有群发消息的功能、一般客户端都是自己缓存消息、如果这里做了缓存反而不好了
#if TENGINE_NET
            if (ReferenceEquals(_lastMessageInfo.Message, message))
            {
                _lastMessageInfo.MemoryStream.Seek(0, SeekOrigin.Begin);
                return _lastMessageInfo.MemoryStream;
            }
#endif
            memoryStream = OuterPacketParser.Pack(rpcId, routeTypeOpCode, message);
#if TENGINE_NET
            _lastMessageInfo.MemoryStream = memoryStream;
            _lastMessageInfo.Message = message;
#endif
            return memoryStream;
        }

        public abstract void Send(uint channelId, uint rpcId, long routeTypeOpCode, long routeId, object message);
        public abstract void Send(uint channelId, uint rpcId, long routeTypeOpCode, long routeId, MemoryStream memoryStream);
        public abstract void RemoveChannel(uint channelId);

        public virtual void Dispose()
        {
            NetworkThread.Instance?.RemoveNetwork(Id);

            Id = 0;
            Scene = null;
            IsDisposed = true;
            NetworkType = NetworkType.None;
            NetworkTarget = NetworkTarget.None;
            NetworkProtocolType = NetworkProtocolType.None;

            _lastMessageInfo.Dispose();
        }
    }
}