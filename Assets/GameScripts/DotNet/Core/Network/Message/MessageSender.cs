using System;
using TEngine.Core;
#pragma warning disable CS8625
#pragma warning disable CS8618

namespace TEngine.Core.Network
{
    public sealed class MessageSender : IDisposable
    {
        public uint RpcId { get; private set; }
        public long RouteId { get; private set; }
        public long CreateTime { get; private set; }
        public Type MessageType { get; private set; }
        public IMessage Request { get; private set; }
        public FTask<IResponse> Tcs { get; private set; }

        public void Dispose()
        {
            RpcId = 0;
            RouteId = 0;
            CreateTime = 0;
            Tcs = null;
            Request = null;
            MessageType = null;
            Pool<MessageSender>.Return(this);
        }

        public static MessageSender Create(uint rpcId, Type requestType, FTask<IResponse> tcs)
        {
            var routeMessageSender = Pool<MessageSender>.Rent();
            routeMessageSender.Tcs = tcs;
            routeMessageSender.RpcId = rpcId;
            routeMessageSender.MessageType = requestType;
            routeMessageSender.CreateTime = TimeHelper.Now;
            return routeMessageSender;
        }

        public static MessageSender Create(uint rpcId, IRequest request, FTask<IResponse> tcs)
        {
            var routeMessageSender = Pool<MessageSender>.Rent();
            routeMessageSender.Tcs = tcs;
            routeMessageSender.RpcId = rpcId;
            routeMessageSender.Request = request;
            routeMessageSender.CreateTime = TimeHelper.Now;
            return routeMessageSender;
        }

        public static MessageSender Create(uint rpcId, long routeId, IRouteMessage request, FTask<IResponse> tcs)
        {
            var routeMessageSender = Pool<MessageSender>.Rent();
            routeMessageSender.Tcs = tcs;
            routeMessageSender.RpcId = rpcId;
            routeMessageSender.RouteId = routeId;
            routeMessageSender.Request = request;
            routeMessageSender.CreateTime = TimeHelper.Now;
            return routeMessageSender;
        }
    }
}