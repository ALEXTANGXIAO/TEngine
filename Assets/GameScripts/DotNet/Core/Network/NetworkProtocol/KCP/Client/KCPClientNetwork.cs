#if !UNITY_WEBGL
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using kcp2k;
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
// ReSharper disable PossibleNullReferenceException
// ReSharper disable InconsistentNaming

namespace TEngine.Core.Network
{
    public sealed class KCPClientNetwork : AClientNetwork, INetworkUpdate
    {
        #region 逻辑线程

        private bool _isInit;
        private long _connectTimeoutId;
        public override event Action OnDispose;
        public override event Action OnConnectFail;
        public override event Action OnConnectComplete;
        public override event Action OnConnectDisconnect;
        public override event Action<uint> OnChangeChannelId;
        public override event Action<APackInfo> OnReceiveMemoryStream;

        public KCPClientNetwork(Scene scene, NetworkTarget networkTarget) : base(scene, NetworkType.Client, NetworkProtocolType.KCP, networkTarget)
        {
            _startTime = TimeHelper.Now;
            NetworkThread.Instance.AddNetwork(this);
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
                if (!_isDisconnect)
                {
                    SendHeader(KcpHeader.Disconnect);
                }
                
                if (_socket.Connected)
                {
                    if (OnConnectDisconnect != null)
                    {
                        ThreadSynchronizationContext.Main.Post(OnConnectDisconnect);
                    }
                    
                    _socket.Close();
                }

                _kcp = null;
                _maxSndWnd = 0;
                _updateMinTime = 0;
                _memoryPool.Dispose();
                _memoryPool = null;
                _sendAction = null;
                _rawSendBuffer = null;
                _rawReceiveBuffer = null;
                
                _packetParser?.Dispose();

                ClearConnectTimeout(ref _connectTimeoutId);

                if (_messageCache != null)
                {
                    _messageCache.Clear();
                    _messageCache = null;
                }
#if NETDEBUG
                Log.Debug($"KCPClientNetwork ConnectionPtrChannel:{ConnectionPtrChannel.Count}");
#endif
                _updateTimer.Clear();
                _updateTimeOutTime.Clear();
                ThreadSynchronizationContext.Main.Post(OnDispose);
                base.Dispose();
            });
        }

        public override uint Connect(IPEndPoint remoteEndPoint, Action onConnectComplete, Action onConnectFail, Action onConnectDisconnect, int connectTimeout = 5000)
        {
            if (_isInit)
            {
                throw new NotSupportedException($"KCPClientNetwork Id:{Id} Has already been initialized. If you want to call Connect again, please re instantiate it.");
            }
            
            _isInit = true;
            OnConnectFail = onConnectFail;
            OnConnectComplete = onConnectComplete;
            OnConnectDisconnect = onConnectDisconnect;
            ChannelId = CreateChannelId();
            _kcpSettings = KCPSettings.Create(NetworkTarget);
            _maxSndWnd = _kcpSettings.MaxSendWindowSize;
            _messageCache = new Queue<MessageCacheInfo>();
            _rawReceiveBuffer = new byte[_kcpSettings.Mtu + 5];
            _memoryPool = MemoryPool<byte>.Shared;

            _sendAction = (rpcId, routeTypeOpCode, routeId, memoryStream, message) =>
            {
                _messageCache.Enqueue(new MessageCacheInfo()
                {
                    RpcId = rpcId,
                    RouteId = routeId,
                    RouteTypeOpCode = routeTypeOpCode,
                    Message = message,
                    MemoryStream = memoryStream
                });
            };

            _connectTimeoutId = TimerScheduler.Instance.Core.OnceTimer(connectTimeout, () =>
            {
                if (OnConnectFail == null)
                {
                    return;
                }
                OnConnectFail();
                Dispose();
            });

            NetworkThread.Instance.SynchronizationContext.Post(() =>
            {
                _socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                _socket.SetSocketBufferToOsLimit();
                NetworkHelper.SetSioUdpConnReset(_socket);
                _socket.Connect(remoteEndPoint);
                SendHeader(KcpHeader.RequestConnection);
            });

            return ChannelId;
        }

        #endregion

        #region 网络主线程
        
        private Socket _socket;
        private int _maxSndWnd;
        private Kcp _kcp;
        private bool _isDisconnect;
        private long _updateMinTime;
        private byte[] _rawSendBuffer;
        private readonly long _startTime;
        private byte[] _rawReceiveBuffer;
        private KCPSettings _kcpSettings;
        private APacketParser _packetParser;
        private MemoryPool<byte> _memoryPool;
        private Queue<MessageCacheInfo> _messageCache;
        private Action<uint, long, long, MemoryStream, object> _sendAction;
        private readonly Queue<uint> _updateTimeOutTime = new Queue<uint>();
        private EndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
        private readonly SortedSet<uint> _updateTimer = new SortedSet<uint>();
        private uint TimeNow => (uint) (TimeHelper.Now - _startTime);

        private void Receive()
        {
            while (_socket != null && _socket.Poll(0, SelectMode.SelectRead))
            {
                try
                {
                    var receiveLength = _socket.ReceiveFrom(_rawReceiveBuffer, ref _clientEndPoint);
                    
                    if (receiveLength > _rawReceiveBuffer.Length)
                    {
                        Log.Error($"KCP ClientConnection: message of size {receiveLength} does not fit into buffer of size {_rawReceiveBuffer.Length}. The excess was silently dropped. Disconnecting.");
                        Dispose();
                        return;
                    }
                    
                    var header = (KcpHeader) _rawReceiveBuffer[0];
                    var channelId = BitConverter.ToUInt32(_rawReceiveBuffer, 1);

                    switch (header)
                    {
                        case KcpHeader.RepeatChannelId:
                        {
                            if (receiveLength != 5 || channelId != ChannelId)
                            {
                                break;
                            }
                            
                            // 到这里是客户端的channelId再服务器上已经存在、需要重新生成一个再次尝试连接
                            ChannelId = CreateChannelId();
                            SendHeader(KcpHeader.RequestConnection);
                            // 这里要处理入如果有发送的消息的问题、后面处理
                            break;
                        }
                        case KcpHeader.WaitConfirmConnection:
                        {
                            if (receiveLength != 5 || channelId != ChannelId)
                            {
                                break;
                            }
        
                            SendHeader(KcpHeader.ConfirmConnection);
                            ClearConnectTimeout(ref _connectTimeoutId);
                            // 创建KCP和相关的初始化
                            _kcp = new Kcp(channelId, Output);
                            _kcp.SetNoDelay(1, 5, 2, true);
                            _kcp.SetWindowSize(_kcpSettings.SendWindowSize, _kcpSettings.ReceiveWindowSize);
                            _kcp.SetMtu(_kcpSettings.Mtu);
                            _rawSendBuffer = new byte[ushort.MaxValue];
                            _packetParser = APacketParser.CreatePacketParser(NetworkTarget);
                            
                            // 把缓存的消息全部发送给服务器

                            _sendAction = (rpcId, routeTypeOpCode, routeId, memoryStream, message) =>
                            {
                                if (IsDisposed)
                                {
                                    return;
                                }

                                memoryStream = Pack(rpcId, routeTypeOpCode, routeId, memoryStream, message);
                                Send(memoryStream);
                            };

                            while (_messageCache.TryDequeue(out var messageCache))
                            {
                                _sendAction(
                                    messageCache.RpcId, 
                                    messageCache.RouteTypeOpCode, 
                                    messageCache.RouteId,
                                    messageCache.MemoryStream, 
                                    messageCache.Message);
                            }

                            _messageCache.Clear();
                            _messageCache = null;
                            // 调用ChannelId改变事件、就算没有改变也要发下、接收事件的地方会判定下
                            ThreadSynchronizationContext.Main.Post(() =>
                            {
                                OnChangeChannelId(ChannelId);
                                OnConnectComplete?.Invoke();
                            });
                            // 到这里正确创建上连接了、可以正常发送消息了
                            break;
                        }
                        case KcpHeader.ReceiveData:
                        {
                            var messageLength = receiveLength - 5;
                            
                            if (messageLength <= 0)
                            {
                                Log.Warning($"KCPClient KcpHeader.Data  messageLength <= 0");
                                break;
                            }

                            if (channelId != ChannelId)
                            {
                                break;
                            }

                            _kcp.Input(_rawReceiveBuffer, 5, messageLength);
                            AddToUpdate(0);
                            KcpReceive();
                            break;
                        }
                        case KcpHeader.Disconnect:
                        {
                            if (channelId != ChannelId)
                            {
                                break;
                            }
                            
                            _isDisconnect = true;
                            Dispose();
                            break;
                        }
                    }
                }
                // this is fine, the socket might have been closed in the other end
                catch (SocketException) { }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
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
            // 检查等待发送的消息，如果超出两倍窗口大小，KCP作者给的建议是要断开连接
            
            var waitSendSize = _kcp.WaitSnd;

            if (waitSendSize > _maxSndWnd)
            {
                Log.Warning($"ERR_KcpWaitSendSizeTooLarge {waitSendSize} > {_maxSndWnd}");
                Dispose();
                return;
            }
            
            _kcp.Send(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
            memoryStream.Dispose();
            AddToUpdate(0);
        }

        public override void Send(uint channelId, uint rpcId, long routeTypeOpCode, long entityId, MemoryStream memoryStream)
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

            _sendAction(rpcId, routeTypeOpCode, entityId, memoryStream, null);
        }

        public override void Send(uint channelId, uint rpcId, long routeTypeOpCode, long entityId, object message)
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

            _sendAction(rpcId, routeTypeOpCode, entityId, null, message);
        }

        private void Output(byte[] bytes, int count)
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
            
            try
            {
                if (count == 0)
                {
                    throw new Exception("KcpOutput count 0");
                }

                _rawSendBuffer.WriteTo(0, (byte) KcpHeader.ReceiveData);
                _rawSendBuffer.WriteTo(1, ChannelId);
                Buffer.BlockCopy(bytes, 0, _rawSendBuffer, 5, count);
                _socket.Send(_rawSendBuffer, 0, count + 5, SocketFlags.None);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private void KcpReceive()
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

            for (;;)
            {
                try
                {
                    // 获得一个完整消息的长度
                    
                    var peekSize = _kcp.PeekSize();

                    // 如果没有接收的消息那就跳出当前循环。

                    if (peekSize < 0)
                    {
                        return;
                    }

                    // 如果为0，表示当前消息发生错误。提示下、一般情况不会发生的

                    if (peekSize == 0)
                    {
                        throw new Exception("SocketError.NetworkReset");
                    }

                    var receiveMemoryOwner = _memoryPool.Rent(Packet.OuterPacketMaxLength);
                    var receiveCount = _kcp.Receive(receiveMemoryOwner.Memory, peekSize);

                    // 如果接收的长度跟peekSize不一样，不需要处理，因为消息肯定有问题的(虽然不可能出现)。

                    if (receiveCount != peekSize)
                    {
                        Log.Error($"receiveCount != peekSize receiveCount:{receiveCount} peekSize:{peekSize}");
                        break;
                    }

                    if (!_packetParser.UnPack(receiveMemoryOwner, out var packInfo))
                    {
                        break;
                    }

                    ThreadSynchronizationContext.Main.Post(() =>
                    {
                        if (IsDisposed)
                        {
                            return;
                        }
                        
                        OnReceiveMemoryStream(packInfo);
                    });
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
        
        public void Update()
        {
            Receive();
            
            var nowTime = TimeNow;
            
            if (_updateTimer.Count == 0 || nowTime < _updateMinTime)
            {
                return;
            }
            
            foreach (var timeId in _updateTimer)
            {
                if (timeId > nowTime)
                {
                    _updateMinTime = timeId;
                    break;
                }
            
                _updateTimeOutTime.Enqueue(timeId);
            }
            
            while (_updateTimeOutTime.TryDequeue(out var time))
            {
                KcpUpdate();
                _updateTimer.Remove(time);
            }
        }

        private void AddToUpdate(uint tillTime)
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return;
            }
#endif
            if (tillTime == 0)
            {
                KcpUpdate();
                return;
            }
        
            if (tillTime < _updateMinTime || _updateMinTime == 0)
            {
                _updateMinTime = tillTime;
            }

            _updateTimer.Add(tillTime);
        }

        private void KcpUpdate()
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return;
            }
#endif
            var nowTime = TimeNow;
            
            try
            {
                _kcp.Update(nowTime);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
                
            AddToUpdate(_kcp.Check(nowTime));
        }

        public override void RemoveChannel(uint channelId)
        {
            Dispose();
        }

        private uint CreateChannelId()
        {
            return 0xC0000000 | (uint) new Random().Next();
        }

        private void SendHeader(KcpHeader kcpHeader)
        {
            if (_socket == null || !_socket.Connected)
            {
                return;
            }
            
            var buff = new byte[5];
            buff.WriteTo(0, (byte) kcpHeader);
            buff.WriteTo(1, ChannelId);
            _socket.Send(buff, 5, SocketFlags.None);
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
    }
}
#endif