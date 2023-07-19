using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using TEngine.DataStructure;
using TEngine.Core;
// ReSharper disable InconsistentNaming
#pragma warning disable CS8601
#pragma warning disable CS8625
#pragma warning disable CS8618

namespace TEngine.Core.Network
{
    public class KCPServerNetwork : ANetwork, INetworkUpdate
    {
        #region 逻辑线程

        public KCPServerNetwork(Scene scene, NetworkTarget networkTarget, IPEndPoint address) : base(scene, NetworkType.Server, NetworkProtocolType.KCP, networkTarget)
        {
            _startTime = TimeHelper.Now;
            NetworkThread.Instance.AddNetwork(this);

            NetworkThread.Instance.SynchronizationContext.Post(() =>
            {
                KcpSettings = KCPSettings.Create(NetworkTarget);
                _rawReceiveBuffer = new byte[KcpSettings.Mtu + 5];
                
                _socket = new Socket(address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
                
                if (address.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    _socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
                }
                
                _socket.Bind(address);
                _socket.SetSocketBufferToOsLimit();
                NetworkHelper.SetSioUdpConnReset(_socket);
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
                    _socket.Disconnect(false);
                    _socket.Close();
                }

                var channels = new List<KCPServerNetworkChannel>();

                channels.AddRange(_connectionChannel.Values);
                channels.AddRange(_pendingConnection.Values);

                foreach (var channel in channels.Where(channel => !channel.IsDisposed))
                {
                    channel.Dispose();
                }

                _updateTimeOutTime.Clear();
                _updateChannels.Clear();
                _pendingTimeOutTime.Clear();
                _updateTimer.Clear();
                _pendingConnectionTimer.Clear();
                _pendingConnection.Clear();
                _connectionChannel.Clear();

                _socket = null;
                KcpSettings = null;
                _updateMinTime = 0;
                _pendingMinTime = 0;
                base.Dispose();
            });
        }

        #endregion

        #region 网络主线程

        private Socket _socket;
        private uint _updateMinTime;
        private uint _pendingMinTime;
        private byte[] _rawReceiveBuffer;
        private readonly long _startTime;
        private readonly byte[] _sendBuff = new byte[5];
        private EndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
        private readonly Queue<uint> _updateTimeOutTime = new Queue<uint>();
        private readonly HashSet<uint> _updateChannels = new HashSet<uint>();
        private readonly Queue<uint> _pendingTimeOutTime = new Queue<uint>();
        private readonly SortedOneToManyHashSet<uint, uint> _updateTimer = new SortedOneToManyHashSet<uint, uint>();
        private readonly SortedOneToManyList<uint, uint> _pendingConnectionTimer = new SortedOneToManyList<uint, uint>();
        private readonly Dictionary<uint, KCPServerNetworkChannel> _pendingConnection = new Dictionary<uint, KCPServerNetworkChannel>();
        private readonly Dictionary<uint, KCPServerNetworkChannel> _connectionChannel = new Dictionary<uint, KCPServerNetworkChannel>();
        public static readonly Dictionary<IntPtr, KCPServerNetworkChannel> ConnectionPtrChannel = new Dictionary<IntPtr, KCPServerNetworkChannel>();
        private KCPSettings KcpSettings { get; set; }
        private uint TimeNow => (uint) (TimeHelper.Now - _startTime);

        public override void Send(uint channelId, uint rpcId, long routeTypeOpCode, long routeId, MemoryStream memoryStream)
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return;
            }
#endif
            if (!_connectionChannel.TryGetValue(channelId, out var channel))
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
            if (!_connectionChannel.TryGetValue(channelId, out var channel))
            {
                return;
            }

            var memoryStream = Pack(rpcId, routeTypeOpCode, routeId, null, message);
            channel.Send(memoryStream);
        }

        private void SendToRepeatChannelId(uint channelId, EndPoint clientEndPoint)
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return;
            }
#endif
            _sendBuff.WriteTo(0, (byte) KcpHeader.RepeatChannelId);
            _sendBuff.WriteTo(1, channelId);
            _socket.SendTo(_sendBuff, 0, 5, SocketFlags.None, clientEndPoint);
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
            while (_socket != null && _socket.Available > 0)
            {
                try
                {
                    var receiveLength = _socket.ReceiveFrom(_rawReceiveBuffer, ref _clientEndPoint);
                    if (receiveLength < 1)
                    {
                        continue;
                    }
                    
                    var header = (KcpHeader) _rawReceiveBuffer[0];
                    var channelId = BitConverter.ToUInt32(_rawReceiveBuffer, 1);

                    switch (header)
                    {
                        case KcpHeader.RequestConnection:
                        {
                            // Log.Debug("KcpHeader.RequestConnection");
                            if (receiveLength != 5)
                            {
                                break;
                            }

                            if (_pendingConnection.TryGetValue(channelId, out var channel))
                            {
                                if (!_clientEndPoint.Equals(channel.RemoteEndPoint))
                                {
                                    SendToRepeatChannelId(channelId, _clientEndPoint);
                                }
                                
                                break;
                            }

                            if (_connectionChannel.ContainsKey(channelId))
                            {
                                SendToRepeatChannelId(channelId, _clientEndPoint);
                                break;
                            }
                            
                            var timeNow = TimeNow;
                            var tillTime = timeNow + 10 * 1000;
                            var pendingChannel = new KCPServerNetworkChannel(Scene, channelId, Id, _clientEndPoint, _socket, timeNow);
                            
                            if (tillTime < _pendingMinTime || _pendingMinTime == 0)
                            {
                                _pendingMinTime = tillTime;
                            }
                            
                            _pendingConnection.Add(channelId, pendingChannel);
                            _pendingConnectionTimer.Add(tillTime, channelId);
                            _sendBuff.WriteTo(0, (byte) KcpHeader.WaitConfirmConnection);
                            _sendBuff.WriteTo(1, channelId);
                            _socket.SendTo(_sendBuff, 0, 5, SocketFlags.None, _clientEndPoint);
                            break;
                        }
                        case KcpHeader.ConfirmConnection:
                        {
                            if (receiveLength != 5)
                            {
                                break;
                            }

                            if (!RemovePendingConnection(channelId, _clientEndPoint, out var channel))
                            {
                                break;
                            }

                            var kcpIntPtr = KCP.KcpCreate(channelId, new IntPtr(channelId));

                            KCP.KcpNodelay(kcpIntPtr, 1, 5, 2, 1);
                            KCP.KcpWndsize(kcpIntPtr, KcpSettings.SendWindowSize, KcpSettings.ReceiveWindowSize);
                            KCP.KcpSetmtu(kcpIntPtr, KcpSettings.Mtu);
                            KCP.KcpSetminrto(kcpIntPtr, 30);
                            KCP.KcpSetoutput(kcpIntPtr, KcpOutput);
                            
                            _connectionChannel.Add(channel.Id, channel);
                            ConnectionPtrChannel.Add(kcpIntPtr, channel);
                            channel.Connect(kcpIntPtr, AddToUpdate, KcpSettings.MaxSendWindowSize, NetworkTarget, NetworkMessageScheduler);
                            break;
                        }
                        case KcpHeader.ReceiveData:
                        {
                            var messageLength = receiveLength - 5;
                            
                            if (messageLength <= 0)
                            {
                                Log.Warning($"KCP Server KcpHeader.Data  messageLength <= 0");
                                break;
                            }

                            if (!_connectionChannel.TryGetValue(channelId, out var channel))
                            {
                                break;
                            }
                            
                            KCP.KcpInput(channel.KcpIntPtr, _rawReceiveBuffer, 5, messageLength);
                            AddToUpdate(0, channel.Id);
                            channel.Receive();
                            break;
                        }
                        case KcpHeader.Disconnect:
                        {
                            // Log.Debug("KcpHeader.Disconnect");
                            RemoveChannel(channelId);
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        private bool RemovePendingConnection(uint channelId, EndPoint remoteEndPoint, out KCPServerNetworkChannel channel)
        {
#if TENGINE_DEVELOP
            channel = null;
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return false;
            }
#endif
            if (!_pendingConnection.TryGetValue(channelId, out channel) || channel.IsDisposed)
            {
                return false;
            }

            if (remoteEndPoint != null && !remoteEndPoint.Equals(channel.RemoteEndPoint))
            {
                Log.Error($"KCPNetworkChannel syn address diff: {channelId} {channel.RemoteEndPoint} {remoteEndPoint}");
                return false;
            }

            _pendingConnection.Remove(channelId);
            _pendingConnectionTimer.RemoveValue(channel.CreateTime + 10 * 1000, channelId);
#if NETDEBUG
            Log.Debug($"KCPServerNetwork _pendingConnection:{_pendingConnection.Count} _pendingConnectionTimer:{_pendingConnectionTimer.Count}");
#endif
            return true;
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
            if (IsDisposed)
            {
                return;
            }

            if (_connectionChannel.Remove(channelId, out var channel))
            {
#if NETDEBUG
                Log.Debug($"KCPServerNetwork _connectionChannel:{_connectionChannel.Count}");
#endif
                channel.Dispose();
                return;
            }

            if (RemovePendingConnection(channelId, null, out channel) && !channel.IsDisposed)
            {
                channel.Dispose();
            }
        }

        public void Update()
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return;
            }
#endif
            Receive();
            
            var nowTime = TimeNow;

            if (nowTime >= _updateMinTime && _updateTimer.Count > 0)
            {
                foreach (var timeId in _updateTimer)
                {
                    var key = timeId.Key;
            
                    if (key > nowTime)
                    {
                        _updateMinTime = key;
                        break;
                    }
            
                    _updateTimeOutTime.Enqueue(key);
                }
                
                while (_updateTimeOutTime.TryDequeue(out var time))
                {
                    foreach (var channelId in _updateTimer[time])
                    {
                        _updateChannels.Add(channelId);
                    }
            
                    _updateTimer.RemoveKey(time);
                }
            }

            if (_updateChannels.Count > 0)
            {
                foreach (var channelId in _updateChannels)
                {
                    if (!_connectionChannel.TryGetValue(channelId, out var channel))
                    {
                        continue;
                    }

                    if (channel.IsDisposed)
                    {
                        continue;
                    }

                    try
                    {
                        KCP.KcpUpdate(channel.KcpIntPtr, nowTime); 
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                
                    if (channel.KcpIntPtr != IntPtr.Zero)
                    {
                        AddToUpdate(KCP.KcpCheck(channel.KcpIntPtr, nowTime), channelId);
                    }
                }
            
                _updateChannels.Clear();
            }

            if (_pendingConnection.Count <= 0 || nowTime < _pendingMinTime)
            {
                return;
            }
            
            foreach (var timeId in _pendingConnectionTimer)
            {
                var key = timeId.Key;
            
                if (key > nowTime)
                {
                    _pendingMinTime = key;
                    break;
                }
                    
                foreach (var channelId in timeId.Value)
                {
                    _pendingTimeOutTime.Enqueue(channelId);
                }
            }
                
            while (_pendingTimeOutTime.TryDequeue(out var channelId))
            {
                if (RemovePendingConnection(channelId, null, out var channel))
                {
                    channel.Dispose();
                }
            }
        }

        private void AddToUpdate(uint tillTime, uint channelId)
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
                _updateChannels.Add(channelId);
                return;
            }
        
            if (tillTime < _updateMinTime || _updateMinTime == 0)
            {
                _updateMinTime = tillTime;
            }
        
            _updateTimer.Add(tillTime, channelId);
        }
        
        
#if ENABLE_IL2CPP
        [AOT.MonoPInvokeCallback(typeof(KcpOutput))]
#endif
        private static int KcpOutput(IntPtr bytes, int count, IntPtr kcp, IntPtr user)
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return 0;
            }
#endif
            try
            {
                if (kcp == IntPtr.Zero || !ConnectionPtrChannel.TryGetValue(kcp, out var channel))
                {
                    return 0;
                }

                if (!channel.IsDisposed)
                {
                    channel.Output(bytes, count);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            return count;
        }

        #endregion
    }
}