using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace TEngine
{
    internal sealed partial class NetworkManager
    {
        /// <summary>
        /// 网络频道基类。
        /// </summary>
        private abstract class NetworkChannelBase : INetworkChannel, IDisposable
        {
            private const float DefaultHeartBeatInterval = 30f;

            private readonly string _name;
            protected readonly Queue<Packet> SendPacketPool;
            protected readonly INetworkChannelHelper NetworkChannelHelper;
            protected AddressFamily MAddressFamily;
            protected bool MResetHeartBeatElapseSecondsWhenReceivePacket;
            protected float MHeartBeatInterval;
            protected Socket MSocket;
            protected readonly SendState MSendState;
            protected readonly ReceiveState MReceiveState;
            protected readonly HeartBeatState MHeartBeatState;
            protected int MSentPacketCount;
            protected int MReceivedPacketCount;
            protected bool Active;
            private bool _disposed;

            public Action<NetworkChannelBase, object> NetworkChannelConnected;
            public Action<NetworkChannelBase> NetworkChannelClosed;
            public Action<NetworkChannelBase, int> NetworkChannelMissHeartBeat;
            public Action<NetworkChannelBase, NetworkErrorCode, SocketError, string> NetworkChannelError;
            public Action<NetworkChannelBase, object> NetworkChannelCustomError;

            /// <summary>
            /// 消息注册Map。
            /// </summary>
            private readonly Dictionary<int, List<CsMsgDelegate>> _msgHandlerMap = new Dictionary<int, List<CsMsgDelegate>>();
            
            /// <summary>
            /// 委托缓存堆栈。
            /// </summary>
            private readonly Queue<List<CsMsgDelegate>> _cacheHandlerQueue = new Queue<List<CsMsgDelegate>>();

            /// <summary>
            /// 消息包缓存堆栈。
            /// </summary>
            private readonly Queue<Packet> _packsQueue = new Queue<Packet>();
            
            /// <summary>
            /// 初始化网络频道基类的新实例。
            /// </summary>
            /// <param name="name">网络频道名称。</param>
            /// <param name="networkChannelHelper">网络频道辅助器。</param>
            public NetworkChannelBase(string name, INetworkChannelHelper networkChannelHelper)
            {
                _name = name ?? string.Empty;
                SendPacketPool = new Queue<Packet>();
                NetworkChannelHelper = networkChannelHelper;
                MAddressFamily = AddressFamily.Unknown;
                MResetHeartBeatElapseSecondsWhenReceivePacket = false;
                MHeartBeatInterval = DefaultHeartBeatInterval;
                MSocket = null;
                MSendState = new SendState();
                MReceiveState = new ReceiveState();
                MHeartBeatState = new HeartBeatState();
                MSentPacketCount = 0;
                MReceivedPacketCount = 0;
                Active = false;
                _disposed = false;

                NetworkChannelConnected = null;
                NetworkChannelClosed = null;
                NetworkChannelMissHeartBeat = null;
                NetworkChannelError = null;
                NetworkChannelCustomError = null;

                networkChannelHelper.Initialize(this);
            }

            /// <summary>
            /// 获取网络频道名称。
            /// </summary>
            public string Name => _name;

            /// <summary>
            /// 获取网络频道所使用的 Socket。
            /// </summary>
            public Socket Socket => MSocket;

            /// <summary>
            /// 获取是否已连接。
            /// </summary>
            public bool Connected
            {
                get
                {
                    if (MSocket != null)
                    {
                        return MSocket.Connected;
                    }

                    return false;
                }
            }

            /// <summary>
            /// 获取网络服务类型。
            /// </summary>
            public abstract ServiceType ServiceType { get; }

            /// <summary>
            /// 获取网络地址类型。
            /// </summary>
            public AddressFamily AddressFamily => MAddressFamily;

            /// <summary>
            /// 获取要发送的消息包数量。
            /// </summary>
            public int SendPacketCount => SendPacketPool.Count;

            /// <summary>
            /// 获取累计发送的消息包数量。
            /// </summary>
            public int SentPacketCount => MSentPacketCount;

            /// <summary>
            /// 获取已接收未处理的消息包数量。
            /// </summary>
            public int ReceivePacketCount => _cacheHandlerQueue.Count;

            /// <summary>
            /// 获取累计已接收的消息包数量。
            /// </summary>
            public int ReceivedPacketCount => MReceivedPacketCount;

            /// <summary>
            /// 获取或设置当收到消息包时是否重置心跳流逝时间。
            /// </summary>
            public bool ResetHeartBeatElapseSecondsWhenReceivePacket
            {
                get => MResetHeartBeatElapseSecondsWhenReceivePacket;
                set => MResetHeartBeatElapseSecondsWhenReceivePacket = value;
            }

            /// <summary>
            /// 获取丢失心跳的次数。
            /// </summary>
            public int MissHeartBeatCount => MHeartBeatState.MissHeartBeatCount;

            /// <summary>
            /// 获取或设置心跳间隔时长，以秒为单位。
            /// </summary>
            public float HeartBeatInterval
            {
                get => MHeartBeatInterval;
                set => MHeartBeatInterval = value;
            }

            /// <summary>
            /// 获取心跳等待时长，以秒为单位。
            /// </summary>
            public float HeartBeatElapseSeconds => MHeartBeatState.HeartBeatElapseSeconds;

            /// <summary>
            /// 网络频道轮询。
            /// </summary>
            /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
            /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
            public virtual void Update(float elapseSeconds, float realElapseSeconds)
            {
                if (MSocket == null || !Active)
                {
                    return;
                }

                ProcessSend();
                ProcessReceive();
                if (MSocket == null || !Active)
                {
                    return;
                }

                HandleCsMsgOnUpdate();

                if (MHeartBeatInterval > 0f)
                {
                    bool sendHeartBeat = false;
                    int missHeartBeatCount = 0;
                    lock (MHeartBeatState)
                    {
                        if (MSocket == null || !Active)
                        {
                            return;
                        }

                        MHeartBeatState.HeartBeatElapseSeconds += realElapseSeconds;
                        if (MHeartBeatState.HeartBeatElapseSeconds >= MHeartBeatInterval)
                        {
                            sendHeartBeat = true;
                            missHeartBeatCount = MHeartBeatState.MissHeartBeatCount;
                            MHeartBeatState.HeartBeatElapseSeconds = 0f;
                            MHeartBeatState.MissHeartBeatCount++;
                        }
                    }

                    if (sendHeartBeat && NetworkChannelHelper.SendHeartBeat())
                    {
                        if (missHeartBeatCount > 0 && NetworkChannelMissHeartBeat != null)
                        {
                            NetworkChannelMissHeartBeat(this, missHeartBeatCount);
                        }
                    }
                }
            }

            /// <summary>
            /// 关闭网络频道。
            /// </summary>
            public virtual void Shutdown()
            {
                Close();
                NetworkChannelHelper.Shutdown();
            }

            /// <summary>
            /// 注册网络消息包处理函数。
            /// </summary>
            /// <param name="msgId">网络消息包id。</param>
            /// <param name="msgDelegate">要注册的网络消息包处理函数。</param>
            public void RegisterMsgHandler(int msgId, CsMsgDelegate msgDelegate)
            {
                if (msgDelegate == null)
                {
                    throw new GameFrameworkException("Msg handler is invalid.");
                }

                if (!_msgHandlerMap.TryGetValue(msgId, out var listHandle))
                {
                    listHandle = new List<CsMsgDelegate>();
                    _msgHandlerMap[msgId] = listHandle;
                }

                if (listHandle != null)
                {
                    if (!listHandle.Contains(msgDelegate))
                    {
                        listHandle.Add(msgDelegate);
                    }
                    else
                    {
                        Log.Error("-------------repeat RegCmdHandle MsgId:{0}-----------",msgId);
                    }
                }
            }

            /// <summary>
            /// 移除网络消息包处理函数。
            /// </summary>
            /// <param name="msgId">网络消息包id。</param>
            /// <param name="msgDelegate">要注册的网络消息包处理函数。</param>
            public void RemoveMsgHandler(int msgId, CsMsgDelegate msgDelegate)
            {
                if (!_msgHandlerMap.TryGetValue(msgId, out List<CsMsgDelegate> listHandle))
                {
                    return;
                }

                if (listHandle != null)
                {
                    listHandle.Remove(msgDelegate);
                }
            }
            
            /// <summary>
            /// 连接到远程主机。
            /// </summary>
            /// <param name="ipAddress">远程主机的 IP 地址。</param>
            /// <param name="port">远程主机的端口号。</param>
            public void Connect(string ipAddress, int port)
            {
                IPAddress address = IPAddress.Parse(ipAddress);
                Connect(address, port, null);
            }

            /// <summary>
            /// 连接到远程主机。
            /// </summary>
            /// <param name="ipAddress">远程主机的 IP 地址。</param>
            /// <param name="port">远程主机的端口号。</param>
            public void Connect(IPAddress ipAddress, int port)
            {
                Connect(ipAddress, port, null);
            }

            /// <summary>
            /// 连接到远程主机。
            /// </summary>
            /// <param name="ipAddress">远程主机的 IP 地址。</param>
            /// <param name="port">远程主机的端口号。</param>
            /// <param name="userData">用户自定义数据。</param>
            public virtual void Connect(IPAddress ipAddress, int port, object userData)
            {
                if (MSocket != null)
                {
                    Close();
                    MSocket = null;
                }

                switch (ipAddress.AddressFamily)
                {
                    case System.Net.Sockets.AddressFamily.InterNetwork:
                        MAddressFamily = AddressFamily.IPv4;
                        break;

                    case System.Net.Sockets.AddressFamily.InterNetworkV6:
                        MAddressFamily = AddressFamily.IPv6;
                        break;

                    default:
                        string errorMessage = Utility.Text.Format("Not supported address family '{0}'.", ipAddress.AddressFamily);
                        if (NetworkChannelError != null)
                        {
                            NetworkChannelError(this, NetworkErrorCode.AddressFamilyError, SocketError.Success, errorMessage);
                            return;
                        }

                        throw new GameFrameworkException(errorMessage);
                }

                MSendState.Reset();
                MReceiveState.PrepareForPacketHeader(NetworkChannelHelper.PacketHeaderLength);
            }

            /// <summary>
            /// 关闭连接并释放所有相关资源。
            /// </summary>
            public void Close()
            {
                lock (this)
                {
                    if (MSocket == null)
                    {
                        return;
                    }

                    Active = false;

                    try
                    {
                        MSocket.Shutdown(SocketShutdown.Both);
                    }
                    catch
                    {
                        // ignored
                    }
                    finally
                    {
                        MSocket.Close();
                        MSocket = null;

                        if (NetworkChannelClosed != null)
                        {
                            NetworkChannelClosed(this);
                        }
                    }

                    MSentPacketCount = 0;
                    MReceivedPacketCount = 0;

                    lock (SendPacketPool)
                    {
                        SendPacketPool.Clear();
                    }

                    lock (_packsQueue)
                    {
                        _packsQueue.Clear();
                    }

                    lock (_msgHandlerMap)
                    {
                        _msgHandlerMap.Clear();
                    }

                    lock (_cacheHandlerQueue)
                    {
                        _cacheHandlerQueue.Clear();
                    }

                    lock (MHeartBeatState)
                    {
                        MHeartBeatState.Reset(true);
                    }
                }
            }

            /// <summary>
            /// 向远程主机发送消息包。
            /// </summary>
            /// <typeparam name="T">消息包类型。</typeparam>
            /// <param name="packet">要发送的消息包。</param>
            public bool Send<T>(T packet) where T : Packet
            {
                if (MSocket == null)
                {
                    string errorMessage = "You must connect first.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SendError, SocketError.Success, errorMessage);
                        return false;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                if (!Active)
                {
                    string errorMessage = "Socket is not active.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SendError, SocketError.Success, errorMessage);
                        return false;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                if (packet == null)
                {
                    string errorMessage = "Packet is invalid.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SendError, SocketError.Success, errorMessage);
                        return false;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                lock (SendPacketPool)
                {
                    SendPacketPool.Enqueue(packet);
                }
                return true;
            }

            /// <summary>
            /// 向远程主机发送消息包并注册消息回调。
            /// </summary>
            /// <typeparam name="T">消息包类型。</typeparam>
            /// <param name="pack">要发送的消息包。</param>
            /// <param name="resHandler">要注册的回调。</param>
            /// <param name="needShowWaitUI">是否需要等待UI。</param>
            /// <returns>消息包是否发送成功。</returns>
            public bool Send<T>(T pack, CsMsgDelegate resHandler, bool needShowWaitUI = false) where T : Packet
            {
                //TODO
                return true;
            }

            /// <summary>
            /// 释放资源。
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// 释放资源。
            /// </summary>
            /// <param name="disposing">释放资源标记。</param>
            private void Dispose(bool disposing)
            {
                if (_disposed)
                {
                    return;
                }

                if (disposing)
                {
                    Close();
                    MSendState.Dispose();
                    MReceiveState.Dispose();
                }

                _disposed = true;
            }

            protected virtual bool ProcessSend()
            {
                if (MSendState.Stream.Length > 0 || SendPacketPool.Count <= 0)
                {
                    return false;
                }

                while (SendPacketPool.Count > 0)
                {
                    Packet packet = null;
                    lock (SendPacketPool)
                    {
                        packet = SendPacketPool.Dequeue();
                    }

                    bool serializeResult = false;
                    try
                    {
                        serializeResult = NetworkChannelHelper.Serialize(packet, MSendState.Stream);
                    }
                    catch (Exception exception)
                    {
                        Active = false;
                        if (NetworkChannelError != null)
                        {
                            SocketException socketException = exception as SocketException;
                            NetworkChannelError(this, NetworkErrorCode.SerializeError, socketException?.SocketErrorCode ?? SocketError.Success,
                                exception.ToString());
                            return false;
                        }

                        throw;
                    }

                    if (!serializeResult)
                    {
                        string errorMessage = "Serialized packet failure.";
                        if (NetworkChannelError != null)
                        {
                            NetworkChannelError(this, NetworkErrorCode.SerializeError, SocketError.Success, errorMessage);
                            return false;
                        }

                        throw new GameFrameworkException(errorMessage);
                    }
                }

                MSendState.Stream.Position = 0L;
                return true;
            }

            protected virtual void ProcessReceive()
            {
            }

            protected virtual bool ProcessPacketHeader()
            {
                try
                {
                    IPacketHeader packetHeader = NetworkChannelHelper.DeserializePacketHeader(MReceiveState.Stream, out var customErrorData);

                    if (customErrorData != null && NetworkChannelCustomError != null)
                    {
                        NetworkChannelCustomError(this, customErrorData);
                    }

                    if (packetHeader == null)
                    {
                        string errorMessage = "Packet header is invalid.";
                        if (NetworkChannelError != null)
                        {
                            NetworkChannelError(this, NetworkErrorCode.DeserializePacketHeaderError, SocketError.Success, errorMessage);
                            return false;
                        }

                        throw new GameFrameworkException(errorMessage);
                    }

                    MReceiveState.PrepareForPacket(packetHeader);
                    if (packetHeader.PacketLength <= 0)
                    {
                        bool processSuccess = ProcessPacket();
                        MReceivedPacketCount++;
                        return processSuccess;
                    }
                }
                catch (Exception exception)
                {
                    Active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.DeserializePacketHeaderError, socketException?.SocketErrorCode ?? SocketError.Success,
                            exception.ToString());
                        return false;
                    }

                    throw;
                }

                return true;
            }

            protected virtual bool ProcessPacket()
            {
                lock (MHeartBeatState)
                {
                    MHeartBeatState.Reset(MResetHeartBeatElapseSecondsWhenReceivePacket);
                }

                try
                {
                    Packet packet = NetworkChannelHelper.DeserializePacket(MReceiveState.PacketHeader, MReceiveState.Stream, out var customErrorData);

                    if (customErrorData != null && NetworkChannelCustomError != null)
                    {
                        NetworkChannelCustomError(this, customErrorData);
                    }

                    if (packet != null)
                    {
                        lock (_cacheHandlerQueue)
                        {
                            if (_msgHandlerMap.TryGetValue((int)packet.Id, out var listHandle))
                            {
                                _cacheHandlerQueue.Enqueue(listHandle);

                                _packsQueue.Enqueue(packet);
                            }
                        }
                    }

                    MReceiveState.PrepareForPacketHeader(NetworkChannelHelper.PacketHeaderLength);
                }
                catch (Exception exception)
                {
                    Active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.DeserializePacketError, socketException?.SocketErrorCode ?? SocketError.Success,
                            exception.ToString());
                        return false;
                    }

                    throw;
                }
                return true;
            }
            
            /// <summary>
            /// 主线程从消息包缓存堆栈/委托缓存堆栈中出列。
            /// </summary>
            private void HandleCsMsgOnUpdate()
            {
                if (_cacheHandlerQueue.Count <= 0 || _packsQueue.Count <= 0)
                {
                    return;
                }
                try
                {
                    foreach (CsMsgDelegate handle in _cacheHandlerQueue.Dequeue())
                    {
                        var pack = _packsQueue.Peek();

                        if (pack != null)
                        {
                            handle(pack);
                        }
                    }
                    _packsQueue.Dequeue();
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                }
            }
        }
    }
}