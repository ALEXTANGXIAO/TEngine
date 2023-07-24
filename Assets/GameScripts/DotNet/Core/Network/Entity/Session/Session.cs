using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TEngine.Core;
#pragma warning disable CS8603
#pragma warning disable CS8601
#pragma warning disable CS8618

namespace TEngine.Core.Network
{
    public class Session : Entity, INotSupportedPool, ISupportedMultiEntity
    {
        private uint _rpcId;
        public long NetworkId { get; private set; }
        public uint ChannelId { get; private set; }
        public long LastReceiveTime { get; private set; }
        public ANetworkMessageScheduler NetworkMessageScheduler { get; private set;}
        public static readonly Dictionary<long, Session> Sessions = new ();
        public readonly Dictionary<long, FTask<IResponse>> RequestCallback = new();

        public static void Create(ANetworkMessageScheduler networkMessageScheduler, ANetworkChannel channel, NetworkTarget networkTarget)
        {
#if TENGINE_DEVELOP
            if (ThreadSynchronizationContext.Main.ThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                throw new NotSupportedException("Session Create not in MainThread");
            }
#endif
            var session = Entity.Create<Session>(channel.Scene);
            session.ChannelId = channel.Id;
            session.NetworkId = channel.NetworkId;
            session.NetworkMessageScheduler = networkMessageScheduler;
            channel.OnDispose += session.Dispose;
            channel.OnReceiveMemoryStream += session.OnReceive;
#if TENGINE_NET
            if (networkTarget == NetworkTarget.Outer)
            {
                var interval = Define.SessionIdleCheckerInterval;
                var timeOut = Define.SessionIdleCheckerTimeout;
                session.AddComponent<SessionIdleCheckerComponent>().Start(interval, timeOut);
            }
#endif
            Sessions.Add(session.Id, session);
        }

        public static Session Create(AClientNetwork network)
        {
#if TENGINE_DEVELOP
            if (ThreadSynchronizationContext.Main.ThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                throw new NotSupportedException("Session Create not in MainThread");
            }
#endif
            var session = Entity.Create<Session>(network.Scene);
            session.ChannelId = network.ChannelId;
            session.NetworkId = network.Id;
            session.NetworkMessageScheduler = network.NetworkMessageScheduler;
            network.OnDispose += session.Dispose;
            network.OnChangeChannelId += session.OnChangeChannelId;
            network.OnReceiveMemoryStream += session.OnReceive;
            Sessions.Add(session.Id, session);
            return session;
        }
#if TENGINE_NET
        public static ServerInnerSession Create(ANetwork network)
        {
#if TENGINE_DEVELOP
            if (ThreadSynchronizationContext.Main.ThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                throw new NotSupportedException("Session Create not in MainThread");
            }
#endif
            var session = Entity.Create<ServerInnerSession>(network.Scene);
            session.NetworkMessageScheduler = network.NetworkMessageScheduler;
            Sessions.Add(session.Id, session);
            return session;
        }
        
        public static ServerInnerSession CreateServerInner(Scene scene)
        {
#if TENGINE_DEVELOP
            if (ThreadSynchronizationContext.Main.ThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                throw new NotSupportedException("Session Create not in MainThread");
            }
#endif
            var session = Entity.Create<ServerInnerSession>(scene, false);
            Sessions.Add(session.Id, session);
            return session;
        }
#endif

        public static bool TryGet(long id, out Session session)
        {
            return Sessions.TryGetValue(id, out session);
        }

        public override void Dispose()
        {
#if TENGINE_DEVELOP
            if (ThreadSynchronizationContext.Main.ThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                throw new NotSupportedException("Session Create not in MainThread");
            }
#endif
            if (IsDisposed)
            {
                return;
            }
            
            var id = Id;
            var networkId = NetworkId;
            var channelId = ChannelId;
            
            foreach (var requestCallback in RequestCallback.Values.ToArray())
            {
                requestCallback.SetException(new Exception($"session is dispose: {Id}"));
            }

            if (networkId != 0 && channelId != 0)
            {
                NetworkThread.Instance?.SynchronizationContext.Post(() =>
                {
                    NetworkThread.Instance?.RemoveChannel(networkId, channelId);
                });
            }
            
            this.NetworkId = 0;
            this.ChannelId = 0;
            base.Dispose();
            Sessions.Remove(id);
#if NETDEBUG
            Log.Debug($"Sessions Dispose Count:{Sessions.Count}");
#endif
        }

        public virtual void Send(object message, uint rpcId = 0, long routeId = 0)
        {
            if (IsDisposed)
            {
                return;
            }

            NetworkThread.Instance.Send(NetworkId, ChannelId, rpcId, 0, routeId, message);
        }

        public virtual void Send(IRouteMessage routeMessage, uint rpcId = 0, long routeId = 0)
        {
            if (IsDisposed)
            {
                return;
            }

            NetworkThread.Instance.Send(NetworkId, ChannelId, rpcId, routeMessage.RouteTypeOpCode(), routeId, routeMessage);
        }

        public virtual void Send(MemoryStream memoryStream, uint rpcId = 0, long routeTypeOpCode = 0, long routeId = 0)
        {
            if (IsDisposed)
            {
                return;
            }

            NetworkThread.Instance.SendStream(NetworkId, ChannelId, rpcId, routeTypeOpCode, routeId, memoryStream);
        }

        public virtual FTask<IResponse> Call(IRequest request, long routeId = 0)
        {
            if (IsDisposed)
            {
                return null;
            }
            
            var requestCallback = FTask<IResponse>.Create();
            
            unchecked
            {
                var rpcId = ++_rpcId;
                RequestCallback.Add(rpcId, requestCallback);
                
                if (request is IRouteMessage iRouteMessage)
                {
                    Send(iRouteMessage, rpcId, routeId);
                }
                else
                {
                    Send(request, rpcId, routeId);
                }
            }
            
            return requestCallback;
        }

        private void OnReceive(APackInfo packInfo)
        {
            if (IsDisposed)
            {
                return;
            }

            LastReceiveTime = TimeHelper.Now;

            try
            {
                NetworkMessageScheduler.Scheduler(this, packInfo).Coroutine();
            }
            catch (Exception e)
            {
                // 如果解析失败，只有一种可能，那就是有人恶意发包。
                // 所以这里强制关闭了当前连接。不让对方一直发包。
                Dispose();
                Log.Error(e);
            }
        }

        private void OnChangeChannelId(uint channelId)
        {
            ChannelId = channelId;
        }
    }
}