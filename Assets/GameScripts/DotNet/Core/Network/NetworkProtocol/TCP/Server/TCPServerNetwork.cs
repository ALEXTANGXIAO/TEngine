using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
// ReSharper disable InconsistentNaming
#pragma warning disable CS8625
#pragma warning disable CS8622
#pragma warning disable CS8618

namespace TEngine.Core.Network
{
    public sealed class TCPServerNetwork : ANetwork
    {
        #region 逻辑线程

        public TCPServerNetwork(Scene scene, NetworkTarget networkTarget, IPEndPoint address) : base(scene, NetworkType.Server, NetworkProtocolType.TCP, networkTarget)
        {
            _acceptAsync = new SocketAsyncEventArgs();
            NetworkThread.Instance.AddNetwork(this);
            NetworkThread.Instance.SynchronizationContext.Post(() =>
            {
                _random = new Random();
                _socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
            
                if (address.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    _socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
                }
            
                _socket.Bind(address);
                _socket.Listen(int.MaxValue);
                _socket.SetSocketBufferToOsLimit();
                _acceptAsync.Completed += OnCompleted;
                AcceptAsync();
            });
        }
        
        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }
            
            IsDisposed = true;
            
            NetworkThread.Instance.SynchronizationContext.Post(() =>
            {
                if (_socket.Connected)
                {
                    _socket.Disconnect(true);
                    _socket.Close();
                }

                _socket = null;
                _random = null;
                _acceptAsync = null;

                var channels = new List<TCPServerNetworkChannel>(_connectionChannel.Values);

                foreach (var tcpServerNetworkChannel in channels)
                {
                    tcpServerNetworkChannel.Dispose();
                }
                
                channels.Clear();
                _connectionChannel.Clear();
                base.Dispose();
            });
        }

        #endregion

        #region 网络主线程

        private Socket _socket;
        private Random _random;
        private SocketAsyncEventArgs _acceptAsync;
        private readonly Dictionary<long, TCPServerNetworkChannel> _connectionChannel = new Dictionary<long, TCPServerNetworkChannel>();

        public override void Send(uint channelId, uint rpcId, long routeTypeOpCode, long routeId, MemoryStream memoryStream)
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return;
            }
#endif
            if (!_connectionChannel.TryGetValue(channelId, out var channel) || channel.IsDisposed)
            {
                return;
            }

            var sendMemoryStream = Pack(rpcId, routeTypeOpCode, routeId, memoryStream, null);
            channel.Send(sendMemoryStream);
        }

        public override void Send(uint channelId, uint rpcId, long routeTypeOpCode, long routeId, object message)
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return;
            }
#endif
            if (!_connectionChannel.TryGetValue(channelId, out var channel) || channel.IsDisposed)
            {
                return;
            }

            var memoryStream = Pack(rpcId, routeTypeOpCode, routeId, null, message);
            channel.Send(memoryStream);
        }

        private void AcceptAsync()
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return;
            }
#endif
            _acceptAsync.AcceptSocket = null;

            if (_socket.AcceptAsync(_acceptAsync))
            {
                return;
            }

            OnAcceptComplete(_acceptAsync);
        }

        private void OnAcceptComplete(SocketAsyncEventArgs asyncEventArgs)
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return;
            }
#endif
            if (_socket == null || asyncEventArgs.AcceptSocket == null)
            {
                return;
            }

            if (asyncEventArgs.SocketError != SocketError.Success)
            {
                Log.Error($"Socket Accept Error: {_acceptAsync.SocketError}");
                return;
            }

            try
            {
                var channelId = 0xC0000000 | (uint) _random.Next();

                while (_connectionChannel.ContainsKey(channelId))
                {
                    channelId = 0xC0000000 | (uint) _random.Next();
                }

                var channel = new TCPServerNetworkChannel(channelId, asyncEventArgs.AcceptSocket, this);
                
                ThreadSynchronizationContext.Main.Post(() =>
                {
                    if (channel.IsDisposed)
                    {
                        return;
                    }

                    Session.Create(NetworkMessageScheduler, channel);
                });
                
                _connectionChannel.Add(channelId, channel);
                channel.Receive();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                AcceptAsync();
            }
        }

        public override void RemoveChannel(uint channelId)
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return;
            }
#endif
            if (IsDisposed || !_connectionChannel.Remove(channelId, out var channel))
            {
                return;
            }
#if NETDEBUG
            Log.Debug($"TCPServerNetwork _connectionChannel:{_connectionChannel.Count}");
#endif
            if (channel.IsDisposed)
            {
                return;
            }

            channel.Dispose();
        }

        #endregion

        #region 网络线程（由Socket底层产生的线程）

        private void OnCompleted(object sender, SocketAsyncEventArgs asyncEventArgs)
        {
            switch (asyncEventArgs.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                {
                    NetworkThread.Instance.SynchronizationContext.Post(() => OnAcceptComplete(asyncEventArgs));
                    break;
                }
                default:
                {
                    throw new Exception($"Socket Accept Error: {asyncEventArgs.LastOperation}");
                }
            }
        }

        #endregion
    }
}