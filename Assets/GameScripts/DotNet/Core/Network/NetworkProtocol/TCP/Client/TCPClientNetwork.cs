using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TEngine.DataStructure;
#pragma warning disable CS8622
#pragma warning disable CS8625
#pragma warning disable CS8618

// ReSharper disable InconsistentNaming

namespace TEngine.Core.Network
{
    public sealed class TCPClientNetwork : AClientNetwork
    {
        #region 逻辑线程

        private bool _isInit;
        private Action _onConnectFail;
        private Action _onConnectComplete;
        private long _connectTimeoutId;
        public override event Action OnDispose;
        public override event Action<uint> OnChangeChannelId = channelId => { };
        public override event Action<APackInfo> OnReceiveMemoryStream;

        public TCPClientNetwork(Scene scene, NetworkTarget networkTarget) : base(scene, NetworkType.Client, NetworkProtocolType.TCP, networkTarget)
        {
            NetworkThread.Instance.AddNetwork(this);
        }

        public override uint Connect(IPEndPoint remoteEndPoint, Action onConnectComplete, Action onConnectFail, int connectTimeout = 5000)
        {
            if (_isInit)
            {
                throw new NotSupportedException($"KCPClientNetwork Id:{Id} Has already been initialized. If you want to call Connect again, please re instantiate it.");
            }
            
            _isInit = true;
            _onConnectFail = onConnectFail;
            _onConnectComplete = onConnectComplete;
            ChannelId = 0xC0000000 | (uint) new Random().Next();

            _sendAction = (rpcId, routeTypeOpCode, routeId, memoryStream, message) =>
            {
                if (IsDisposed)
                {
                    return;
                }

                _messageCache.Enqueue(new MessageCacheInfo()
                {
                    RpcId = rpcId,
                    RouteId = routeId,
                    RouteTypeOpCode = routeTypeOpCode,
                    Message = message,
                    MemoryStream = memoryStream
                });
            };
            
            _packetParser = APacketParser.CreatePacketParser(NetworkTarget);
            
            _outArgs.Completed += OnComplete;
            _innArgs.Completed += OnComplete;

            _connectTimeoutId = TimerScheduler.Instance.Core.OnceTimer(connectTimeout, () =>
            {
                _onConnectFail?.Invoke();
                Dispose();
            });
            
            NetworkThread.Instance.SynchronizationContext.Post(() =>
            {
                var outArgs = new SocketAsyncEventArgs
                {
                    RemoteEndPoint = remoteEndPoint
                };
                
                outArgs.Completed += OnComplete;
                
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) {NoDelay = true};
                _socket.SetSocketBufferToOsLimit();

                if (_socket.ConnectAsync(outArgs))
                {
                    return;
                }

                OnConnectComplete(outArgs);
            });

            return ChannelId;
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

                _outArgs?.Dispose();
                _innArgs?.Dispose();
                _sendBuffer?.Dispose();
                _receiveBuffer?.Dispose();
                _packetParser?.Dispose();

                _sendAction = null;
                _packetParser = null;
                _isSending = false;

                if (_messageCache != null)
                {
                    _messageCache.Clear();
                    _messageCache = null;
                }
                
                ThreadSynchronizationContext.Main.Post(OnDispose);
                base.Dispose();
            });
        }
        
        #endregion

        #region 网络主线程
        
        private Socket _socket;
        private bool _isSending;
        private APacketParser _packetParser;
        private Action<uint, long, long, MemoryStream, object> _sendAction;
        private readonly CircularBuffer _sendBuffer = new CircularBuffer();
        private readonly CircularBuffer _receiveBuffer = new CircularBuffer();
        private readonly SocketAsyncEventArgs _outArgs = new SocketAsyncEventArgs();
        private readonly SocketAsyncEventArgs _innArgs = new SocketAsyncEventArgs();
        private Queue<MessageCacheInfo> _messageCache = new Queue<MessageCacheInfo>();

        private void OnConnectComplete(SocketAsyncEventArgs asyncEventArgs)
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return;
            }
#endif
            if (IsDisposed)
            {
                return;
            }

            if (asyncEventArgs.SocketError != SocketError.Success)
            {
                Log.Error($"Unable to connect to the target server asyncEventArgs:{asyncEventArgs.SocketError}");
                
                if (_onConnectFail != null)
                {
                    ThreadSynchronizationContext.Main.Post(_onConnectFail);
                }
                
                Dispose();
                return;
            }
            
            Receive();
            ClearConnectTimeout(ref _connectTimeoutId);

            _sendAction = (rpcId, routeTypeOpCode, routeId, memoryStream, message) =>
            {
                if (IsDisposed)
                {
                    return;
                }

                memoryStream = Pack(rpcId, routeTypeOpCode, routeId, memoryStream, message);
                Send(memoryStream);
            };

            while (_messageCache.TryDequeue(out var messageCacheInfo))
            {
                _sendAction(
                    messageCacheInfo.RpcId, 
                    messageCacheInfo.RouteTypeOpCode, 
                    messageCacheInfo.RouteId,
                    messageCacheInfo.MemoryStream, 
                    messageCacheInfo.Message);
            }

            _messageCache.Clear();
            _messageCache = null;

            if (_onConnectComplete != null)
            {
                ThreadSynchronizationContext.Main.Post(_onConnectComplete);
            }
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
            if (channelId != ChannelId || IsDisposed)
            {
                return;
            }

            _sendAction(rpcId, routeTypeOpCode, routeId, null, message);
        }

        public override void Send(uint channelId, uint rpcId, long routeTypeOpCode, long routeId, MemoryStream memoryStream)
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return;
            }
#endif
            if (channelId != ChannelId || IsDisposed)
            {
                return;
            }

            _sendAction(rpcId, routeTypeOpCode, routeId, memoryStream, null);
        }

        private void Send(MemoryStream memoryStream)
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return;
            }
#endif
            _sendBuffer.Write(memoryStream);
            
            // 因为memoryStream对象池出来的、所以需要手动回收下
            
            memoryStream.Dispose();
            
            if (_isSending)
            {
                return;
            }
            
            Send();
        }
        
        private void Send()
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return;
            }
#endif
            if (_isSending || IsDisposed)
            {
                return;
            }

            for (;;)
            {
                try
                {
                    if (_sendBuffer.Length == 0)
                    {
                        _isSending = false;
                        return;
                    }
                    
                    _isSending = true;
                    
                    var sendSize = CircularBuffer.ChunkSize - _sendBuffer.FirstIndex;
                    
                    if (sendSize > _sendBuffer.Length)
                    {
                        sendSize = (int) _sendBuffer.Length;
                    }

                    _outArgs.SetBuffer(_sendBuffer.First, _sendBuffer.FirstIndex, sendSize);
                    
                    if (_socket.SendAsync(_outArgs))
                    {
                        return;
                    }
                    
                    SendCompletedHandler(_outArgs);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
        
        private void SendCompletedHandler(SocketAsyncEventArgs asyncEventArgs)
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return;
            }
#endif
            if (asyncEventArgs.SocketError != SocketError.Success || asyncEventArgs.BytesTransferred == 0)
            {
                return;
            }
            
            _sendBuffer.FirstIndex += asyncEventArgs.BytesTransferred;
        
            if (_sendBuffer.FirstIndex == CircularBuffer.ChunkSize)
            {
                _sendBuffer.FirstIndex = 0;
                _sendBuffer.RemoveFirst();
            }
        }

        private void OnSendComplete(SocketAsyncEventArgs asyncEventArgs)
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return;
            }
#endif
            if (IsDisposed)
            {
                return;
            }
            
            _isSending = false;
            SendCompletedHandler(asyncEventArgs);
            
            if (_sendBuffer.Length > 0)
            {
                Send();
            }
        }
        
        private void Receive()
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return;
            }
#endif
            for (;;)
            {
                try
                {
                    if (IsDisposed)
                    {
                        return;
                    }
                    
                    var size = CircularBuffer.ChunkSize - _receiveBuffer.LastIndex;
                    _innArgs.SetBuffer(_receiveBuffer.Last, _receiveBuffer.LastIndex, size);
                    
                    if (_socket.ReceiveAsync(_innArgs))
                    {
                        return;
                    }
                    
                    ReceiveCompletedHandler(_innArgs);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
        
        private void ReceiveCompletedHandler(SocketAsyncEventArgs asyncEventArgs)
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return;
            }
#endif
            if (asyncEventArgs.SocketError != SocketError.Success)
            {
                return;
            }

            if (asyncEventArgs.BytesTransferred == 0)
            {
                Dispose();
                return;
            }
            
            _receiveBuffer.LastIndex += asyncEventArgs.BytesTransferred;
                
            if (_receiveBuffer.LastIndex >= CircularBuffer.ChunkSize)
            {
                _receiveBuffer.AddLast();
                _receiveBuffer.LastIndex = 0;
            }

            for (;;)
            {
                try
                {
                    if (IsDisposed)
                    {
                        return;
                    }
                    
                    if (!_packetParser.UnPack(_receiveBuffer,out var packInfo))
                    {
                        break;
                    }

                    ThreadSynchronizationContext.Main.Post(() =>
                    {
                        if (IsDisposed)
                        {
                            return;
                        }
                        
                        // ReSharper disable once PossibleNullReferenceException
                        OnReceiveMemoryStream(packInfo);
                    });
                }
                catch (ScanException e)
                {
                    Log.Debug($"{e}");
                    Dispose();
                }
                catch (Exception e)
                {
                    Log.Error($"{e}");
                    Dispose();
                }
            }
        }
        
        private void OnReceiveComplete(SocketAsyncEventArgs asyncEventArgs)
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return;
            }
#endif
            ReceiveCompletedHandler(asyncEventArgs);
            Receive();
        }
        
        public override void RemoveChannel(uint channelId)
        {
            Dispose();
        }

        private void ClearConnectTimeout(ref long connectTimeoutId)
        {
            if (connectTimeoutId == 0)
            {
                return;
            }
            
            if (NetworkThread.Instance.ManagedThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                var timeoutId = connectTimeoutId;
                ThreadSynchronizationContext.Main.Post(() => { TimerScheduler.Instance.Core.Remove(timeoutId); });
                connectTimeoutId = 0;
                return;
            }
            
            TimerScheduler.Instance.Core.RemoveByRef(ref connectTimeoutId);
        }

        #endregion
        
        #region 网络线程（由Socket底层产生的线程）

        private void OnComplete(object sender, SocketAsyncEventArgs asyncEventArgs)
        {
            if (IsDisposed)
            {
                return;
            }

            switch (asyncEventArgs.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                {
                    NetworkThread.Instance.SynchronizationContext.Post(() => OnConnectComplete(asyncEventArgs));
                    break;
                }
                case SocketAsyncOperation.Receive:
                {
                    NetworkThread.Instance.SynchronizationContext.Post(() => OnReceiveComplete(asyncEventArgs));
                    break;
                }
                case SocketAsyncOperation.Send:
                {
                    NetworkThread.Instance.SynchronizationContext.Post(() => OnSendComplete(asyncEventArgs));
                    break;
                }
                case SocketAsyncOperation.Disconnect:
                {
                    NetworkThread.Instance.SynchronizationContext.Post(Dispose);
                    break;
                }
            }
        }

        #endregion
    }
}