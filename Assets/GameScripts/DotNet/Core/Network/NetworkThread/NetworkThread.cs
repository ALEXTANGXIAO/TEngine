using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TEngine.Core;
#pragma warning disable CS8625
#pragma warning disable CS8618

namespace TEngine.Core.Network
{
    public sealed class NetworkThread : Singleton<NetworkThread>
    {
        #region 逻辑线程
        
        private Thread _netWorkThread;
        public ThreadSynchronizationContext SynchronizationContext;
        private readonly ConcurrentQueue<NetAction> _actions = new ConcurrentQueue<NetAction>();
        
        public int ManagedThreadId => _netWorkThread.ManagedThreadId;

        public override async Task Initialize()
        {
            _netWorkThread = new Thread(Update);
            SynchronizationContext = new ThreadSynchronizationContext(_netWorkThread.ManagedThreadId);
            _netWorkThread.Start();
            await Task.CompletedTask;
        }

        public void Send(long networkId, uint channelId, uint rpcId, long routeTypeOpCode, long entityId, object message)
        {
            if (IsDisposed)
            {
                return;
            }

            _actions.Enqueue(new NetAction(networkId, channelId, rpcId, routeTypeOpCode, entityId, NetActionType.Send, message));
        }

        public void SendStream(long networkId, uint channelId, uint rpcId, long routeTypeOpCode, long routeId, MemoryStream memoryStream)
        {
            if (IsDisposed)
            {
                return;
            }

            _actions.Enqueue(new NetAction(networkId, channelId, rpcId, routeTypeOpCode, routeId, NetActionType.SendMemoryStream, memoryStream));
        }

        public void RemoveChannel(long networkId, uint channelId)
        {
            if (IsDisposed)
            {
                return;
            }

            _actions.Enqueue(new NetAction(networkId, channelId, 0, 0, 0, NetActionType.RemoveChannel, null));
        }

        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }
            
            SynchronizationContext.Post(() =>
            {
                if (IsDisposed)
                {
                    return;
                }

                IsDisposed = true;
                var removeList = new Queue<ANetwork>(_networks.Values);

                foreach (var aNetwork in removeList)
                {
                    if (aNetwork.IsDisposed)
                    {
                        continue;
                    }
                
                    aNetwork.Dispose();
                }
                
                foreach (var netAction in _actions)
                {
                    netAction.Dispose();
                }
            
                _actions.Clear();
                _networks.Clear();
                removeList.Clear();
                
                _netWorkThread = null;
                SynchronizationContext = null;
                base.Dispose();
            });
        }

        #endregion
        
        #region 网络线程
        
        private readonly Dictionary<long, ANetwork> _networks = new Dictionary<long, ANetwork>();
        private readonly Dictionary<long, INetworkUpdate> _updates = new Dictionary<long, INetworkUpdate>();

        private void Update()
        {
            System.Threading.SynchronizationContext.SetSynchronizationContext(SynchronizationContext);

            while (!IsDisposed)
            {
                Thread.Sleep(1);
                
                foreach (var (_, aNetwork) in _updates)
                {
                    try
                    {
                        aNetwork.Update();
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
                
                SynchronizationContext.Update();

                while (_actions.TryDequeue(out var action))
                {
                    if (!_networks.TryGetValue(action.NetworkId, out var network) || network.IsDisposed)
                    {
                        continue;
                    }

                    try
                    {
                        switch (action.NetActionType)
                        {
                            case NetActionType.Send:
                            {
                                network.Send(action.ChannelId, action.RpcId, action.RouteTypeOpCode, action.EntityId, action.Obj);
                                break;
                            }
                            case NetActionType.SendMemoryStream:
                            {
                                network.Send(action.ChannelId, action.RpcId, action.RouteTypeOpCode, action.EntityId, action.MemoryStream);
                                break;
                            }
                            case NetActionType.RemoveChannel:
                            {
                                network.RemoveChannel(action.ChannelId);
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                    finally
                    {
                        action.Dispose();
                    }
                }
            }
        }
        
        public void AddNetwork(ANetwork aNetwork)
        {
            if (IsDisposed)
            {
                return;
            }
            
            SynchronizationContext.Post(() =>
            {
                if (IsDisposed || aNetwork.IsDisposed)
                {
                    return;
                }

                _networks.Add(aNetwork.Id, aNetwork);

                if (aNetwork is INetworkUpdate iNetworkUpdate)
                {
                    _updates.Add(aNetwork.Id, iNetworkUpdate);
                }
            });
        }
        
        public void RemoveNetwork(long networkId)
        {
            if (IsDisposed)
            {
                return;
            }
            
            SynchronizationContext.Post(() =>
            {
                if (IsDisposed || !_networks.Remove(networkId, out var network))
                {
                    return;
                }

                if (network is INetworkUpdate)
                {
                    _updates.Remove(networkId);
                }
            });
        }

        #endregion
    }
}