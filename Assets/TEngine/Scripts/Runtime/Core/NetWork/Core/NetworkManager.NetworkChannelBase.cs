using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TEngineProto;

namespace TEngine.Runtime
{
    public delegate void CsMsgDelegate(MainPack mainPack);
    
    public sealed partial class NetworkManager
    {
        /// <summary>
        /// 网络频道基类。
        /// </summary>
        private abstract class NetworkChannelBase : INetworkChannel, IDisposable
        {
            private const float DefaultHeartBeatInterval = 3;
            private const int MAX_MSG_HANDLE = 256;

            private readonly string m_Name;
            protected readonly Queue<MainPack> m_SendPacketPool;
            protected readonly INetworkChannelHelper m_NetworkChannelHelper;
            protected AddressFamily m_AddressFamily;
            protected bool m_ResetHeartBeatElapseSecondsWhenReceivePacket;
            protected float m_HeartBeatInterval;
            protected Socket m_Socket;
            protected readonly SendState m_SendState;
            protected readonly ReceiveState m_ReceiveState;
            protected readonly HeartBeatState m_HeartBeatState;
            protected int m_SentPacketCount;
            protected int m_ReceivedPacketCount;
            protected bool m_Active;
            private bool m_Disposed;

            public Action<NetworkChannelBase, object> NetworkChannelConnected;
            public Action<NetworkChannelBase> NetworkChannelClosed;
            public Action<NetworkChannelBase, int> NetworkChannelMissHeartBeat;
            public Action<NetworkChannelBase, NetworkErrorCode, SocketError, string> NetworkChannelError;
            public Action<NetworkChannelBase, object> NetworkChannelCustomError;
            
            /// <summary>
            /// 委托注册队列
            /// </summary>
            protected Dictionary<int, List<CsMsgDelegate>> m_MapCmdHandle = new Dictionary<int, List<CsMsgDelegate>>();
            /// <summary>
            /// 委托缓存堆栈
            /// </summary>
            protected Queue<List<CsMsgDelegate>> m_CachelistHandle = new Queue<List<CsMsgDelegate>>();

            /// <summary>
            /// 消息包缓存堆栈
            /// </summary>
            protected Queue<MainPack> m_QueuepPacks = new Queue<MainPack>();

            /// <summary>
            /// 初始化网络频道基类的新实例。
            /// </summary>
            /// <param name="name">网络频道名称。</param>
            /// <param name="networkChannelHelper">网络频道辅助器。</param>
            public NetworkChannelBase(string name, INetworkChannelHelper networkChannelHelper)
            {
                m_Name = name ?? string.Empty;
                m_SendPacketPool = new Queue<MainPack>();
                m_NetworkChannelHelper = networkChannelHelper;
                m_AddressFamily = AddressFamily.Unknown;
                m_ResetHeartBeatElapseSecondsWhenReceivePacket = false;
                m_HeartBeatInterval = DefaultHeartBeatInterval;
                m_Socket = null;
                m_SendState = new SendState();
                m_ReceiveState = new ReceiveState();
                m_HeartBeatState = new HeartBeatState();
                m_SentPacketCount = 0;
                m_ReceivedPacketCount = 0;
                m_Active = false;
                m_Disposed = false;

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
            public string Name
            {
                get
                {
                    return m_Name;
                }
            }

            /// <summary>
            /// 获取网络频道所使用的 Socket。
            /// </summary>
            public Socket Socket
            {
                get
                {
                    return m_Socket;
                }
            }

            /// <summary>
            /// 获取是否已连接。
            /// </summary>
            public bool Connected
            {
                get
                {
                    if (m_Socket != null)
                    {
                        return m_Socket.Connected;
                    }

                    return false;
                }
            }

            /// <summary>
            /// 获取网络服务类型。
            /// </summary>
            public abstract ServiceType ServiceType
            {
                get;
            }

            /// <summary>
            /// 获取网络地址类型。
            /// </summary>
            public AddressFamily AddressFamily
            {
                get
                {
                    return m_AddressFamily;
                }
            }

            /// <summary>
            /// 获取要发送的消息包数量。
            /// </summary>
            public int SendPacketCount
            {
                get
                {
                    return m_SendPacketPool.Count;
                }
            }

            /// <summary>
            /// 获取累计发送的消息包数量。
            /// </summary>
            public int SentPacketCount
            {
                get
                {
                    return m_SentPacketCount;
                }
            }

            /// <summary>
            /// 获取累计已接收的消息包数量。
            /// </summary>
            public int ReceivedPacketCount
            {
                get
                {
                    return m_ReceivedPacketCount;
                }
            }

            /// <summary>
            /// 获取或设置当收到消息包时是否重置心跳流逝时间。
            /// </summary>
            public bool ResetHeartBeatElapseSecondsWhenReceivePacket
            {
                get
                {
                    return m_ResetHeartBeatElapseSecondsWhenReceivePacket;
                }
                set
                {
                    m_ResetHeartBeatElapseSecondsWhenReceivePacket = value;
                }
            }

            /// <summary>
            /// 获取丢失心跳的次数。
            /// </summary>
            public int MissHeartBeatCount
            {
                get
                {
                    return m_HeartBeatState.MissHeartBeatCount;
                }
            }

            /// <summary>
            /// 获取或设置心跳间隔时长，以秒为单位。
            /// </summary>
            public float HeartBeatInterval
            {
                get
                {
                    return m_HeartBeatInterval;
                }
                set
                {
                    m_HeartBeatInterval = value;
                }
            }

            /// <summary>
            /// 获取心跳等待时长，以秒为单位。
            /// </summary>
            public float HeartBeatElapseSeconds
            {
                get
                {
                    return m_HeartBeatState.HeartBeatElapseSeconds;
                }
            }

            /// <summary>
            /// 网络频道轮询。
            /// </summary>
            /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
            /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
            public virtual void Update(float elapseSeconds, float realElapseSeconds)
            {
                if (m_Socket == null || !m_Active)
                {
                    return;
                }

                HandleCsMsgOnUpdate();
                CheckCsMsgTimeOut();
                ProcessSend();
                ProcessReceive();
                if (m_Socket == null || !m_Active)
                {
                    return;
                }

                if (m_HeartBeatInterval > 0f)
                {
                    bool sendHeartBeat = false;
                    int missHeartBeatCount = 0;
                    lock (m_HeartBeatState)
                    {
                        if (m_Socket == null || !m_Active)
                        {
                            return;
                        }

                        m_HeartBeatState.HeartBeatElapseSeconds += realElapseSeconds;
                        if (m_HeartBeatState.HeartBeatElapseSeconds >= m_HeartBeatInterval)
                        {
                            sendHeartBeat = true;
                            missHeartBeatCount = m_HeartBeatState.MissHeartBeatCount;
                            m_HeartBeatState.HeartBeatElapseSeconds = 0f;
                            m_HeartBeatState.MissHeartBeatCount++;
                        }
                    }

                    if (sendHeartBeat && m_NetworkChannelHelper.SendHeartBeat())
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
                m_NetworkChannelHelper.Shutdown();
            }

            /// <summary>
            /// 注册网络消息包处理函数。
            /// </summary>
            /// <param name="actionId"></param>
            /// <param name="msgDelegate"></param>
            /// <param name="checkRepeat"></param>
            /// <exception cref="Exception"></exception>
            public void RegisterHandler(int actionId, CsMsgDelegate msgDelegate,bool checkRepeat = true)
            {
                if (msgDelegate == null)
                {
                    throw new Exception("Packet handler is invalid.");
                }
                
                List<CsMsgDelegate> listHandle;
                if (!m_MapCmdHandle.TryGetValue(actionId, out listHandle))
                {
                    listHandle = new List<CsMsgDelegate>();
                    m_MapCmdHandle[actionId] = listHandle;
                }

                if (listHandle != null)
                {
                    if (!listHandle.Contains(msgDelegate))
                    {
                        listHandle.Add(msgDelegate);
                    }
                    else
                    {
                        if (checkRepeat)
                        {
                            Log.Warning("-------------repeat RegCmdHandle ActionCode:{0}-----------", (ActionCode)actionId);
                        }
                    }
                }
            }
            
            /// <summary>
            /// 移除消息处理函数
            /// </summary>
            /// <param name="actionId"></param>
            /// <param name="msgDelegate"></param>
            public void RmvHandler(int actionId, CsMsgDelegate msgDelegate)
            {
                List<CsMsgDelegate> listHandle;
                if (!m_MapCmdHandle.TryGetValue(actionId, out listHandle))
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
                if (m_Socket != null)
                {
                    Close();
                    m_Socket = null;
                }

                switch (ipAddress.AddressFamily)
                {
                    case System.Net.Sockets.AddressFamily.InterNetwork:
                        m_AddressFamily = AddressFamily.IPv4;
                        break;

                    case System.Net.Sockets.AddressFamily.InterNetworkV6:
                        m_AddressFamily = AddressFamily.IPv6;
                        break;

                    default:
                        string errorMessage = Utility.Text.Format("Not supported address family '{0}'.", ipAddress.AddressFamily);
                        if (NetworkChannelError != null)
                        {
                            NetworkChannelError(this, NetworkErrorCode.AddressFamilyError, SocketError.Success, errorMessage);
                            return;
                        }

                        throw new Exception(errorMessage);
                }

                m_SendState.Reset();
                m_ReceiveState.PrepareForPacketHeader(m_NetworkChannelHelper.PacketHeaderLength);
            }

            /// <summary>
            /// 关闭连接并释放所有相关资源。
            /// </summary>
            public void Close()
            {
                lock (this)
                {
                    if (m_Socket == null)
                    {
                        return;
                    }

                    m_Active = false;

                    try
                    {
                        m_Socket.Shutdown(SocketShutdown.Both);
                    }
                    catch
                    {
                    }
                    finally
                    {
                        m_Socket.Close();
                        m_Socket = null;

                        if (NetworkChannelClosed != null)
                        {
                            NetworkChannelClosed(this);
                        }
                    }

                    m_SentPacketCount = 0;
                    m_ReceivedPacketCount = 0;

                    lock (m_SendPacketPool)
                    {
                        m_SendPacketPool.Clear();
                    }

                    lock (m_HeartBeatState)
                    {
                        m_HeartBeatState.Reset(true);
                    }
                }
            }
            
            /// <summary>
            /// 发送消息包并注册回调
            /// </summary>
            /// <param name="pack"></param>
            /// <param name="resHandler"></param>
            /// <param name="needShowWaitUI"></param>
            /// <returns></returns>
            public bool SendCsMsg(MainPack pack, CsMsgDelegate resHandler = null)
            {
                var ret = Send(pack);
                if (!ret)
                {
                    TLogger.LogError("SendCSMsg Error");
                }
                else
                {
                    if (resHandler != null)
                    {
                        RegTimeOutHandle((uint)pack.actioncode, resHandler);
                        RegisterHandler((int)pack.actioncode, resHandler, false);
                    }
                }

                return ret;
            }

            /// <summary>
            /// 向远程主机发送消息包。
            /// </summary>
            /// <typeparam name="T">消息包类型。</typeparam>
            /// <param name="packet">要发送的消息包。</param>
            public bool Send(MainPack packet)
            {
                if (m_Socket == null)
                {
                    string errorMessage = "You must connect first.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SendError, SocketError.Success, errorMessage);
                        return false;
                    }

                    throw new Exception(errorMessage);
                }

                if (!m_Active)
                {
                    string errorMessage = "Socket is not active.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SendError, SocketError.Success, errorMessage);
                        return false;
                    }

                    throw new Exception(errorMessage);
                }

                if (packet == null)
                {
                    string errorMessage = "Packet is invalid.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SendError, SocketError.Success, errorMessage);
                        return false;
                    }

                    throw new Exception(errorMessage);
                }

                lock (m_SendPacketPool)
                {
                    m_SendPacketPool.Enqueue(packet);
                }

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
                if (m_Disposed)
                {
                    return;
                }

                if (disposing)
                {
                    Close();
                    m_SendState.Dispose();
                    m_ReceiveState.Dispose();
                }

                m_Disposed = true;
            }

            protected virtual bool ProcessSend()
            {
                if (m_SendState.Stream.Length > 0 || m_SendPacketPool.Count <= 0)
                {
                    return false;
                }

                while (m_SendPacketPool.Count > 0)
                {
                    MainPack packet = null;
                    lock (m_SendPacketPool)
                    {
                        packet = m_SendPacketPool.Dequeue();
                    }

                    bool serializeResult = false;
                    try
                    {
                        serializeResult = m_NetworkChannelHelper.Serialize(packet, m_SendState.Stream);
                    }
                    catch (Exception exception)
                    {
                        m_Active = false;
                        if (NetworkChannelError != null)
                        {
                            SocketException socketException = exception as SocketException;
                            NetworkChannelError(this, NetworkErrorCode.SerializeError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
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

                        throw new Exception(errorMessage);
                    }
                }

                m_SendState.Stream.Position = 0L;
                return true;
            }

            protected virtual void ProcessReceive()
            {
            }

            protected virtual bool ProcessPacketHeader()
            {
                try
                {
                    object customErrorData = null;
                    IPacketHeader packetHeader = m_NetworkChannelHelper.DeserializePacketHeader(m_ReceiveState.Stream, out customErrorData);

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

                        throw new Exception(errorMessage);
                    }

                    m_ReceiveState.PrepareForPacket(packetHeader);
                    if (packetHeader.PacketLength <= 0)
                    {
                        bool processSuccess = ProcessPacket();
                        m_ReceivedPacketCount++;
                        return processSuccess;
                    }
                }
                catch (Exception exception)
                {
                    m_Active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.DeserializePacketHeaderError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return false;
                    }

                    throw;
                }

                return true;
            }

            protected virtual bool ProcessPacket()
            {
                lock (m_HeartBeatState)
                {
                    m_HeartBeatState.Reset(m_ResetHeartBeatElapseSecondsWhenReceivePacket);
                }

                try
                {
                    object customErrorData = null;
                    MainPack packet = m_NetworkChannelHelper.DeserializePacket(m_ReceiveState.PacketHeader, m_ReceiveState.Stream, out customErrorData);

                    if (customErrorData != null && NetworkChannelCustomError != null)
                    {
                        NetworkChannelCustomError(this, customErrorData);
                    }

                    if (packet != null)
                    {
                        HandleResponse(packet);
                    }

                    m_ReceiveState.PrepareForPacketHeader(m_NetworkChannelHelper.PacketHeaderLength);
                }
                catch (Exception exception)
                {
                    m_Active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.DeserializePacketError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return false;
                    }

                    throw;
                }

                return true;
            }
            
            /// <summary>
            /// 网络消息回调，非主线程
            /// </summary>
            /// <param name="pack"></param>
            public void HandleResponse(MainPack pack)
            {
                lock (m_CachelistHandle)
                {
                    List<CsMsgDelegate> listHandle;

                    if (m_MapCmdHandle.TryGetValue((int)pack.actioncode, out listHandle))
                    {
                        m_CachelistHandle.Enqueue(listHandle);

                        m_QueuepPacks.Enqueue(pack);
                    }
                }
            }
            
            /// <summary>
            /// 主线程从消息包缓存堆栈/委托缓存堆栈中出列
            /// </summary>
            private void HandleCsMsgOnUpdate()
            {
                if (m_CachelistHandle.Count <= 0 || m_QueuepPacks.Count <= 0)
                {
                    return;
                }
                try
                {
                    foreach (CsMsgDelegate handle in m_CachelistHandle.Dequeue())
                    {
                        var pack = m_QueuepPacks.Peek();

                        if (pack != null)
                        {
                            handle(pack);

                            UInt32 hashIndex = (uint)pack.actioncode % MAX_MSG_HANDLE;

                            RmvCheckCsMsg((int)hashIndex);
                        }
                    }
                    m_QueuepPacks.Dequeue();
                }
                catch (Exception e)
                {
                    TLogger.LogError(e.Message);
                }
            }

            #region 超时检测
            private const int CHECK_TIMEOUT_PERFRAME = 10;
            UInt32 m_dwLastCheckIndex = 0;
            CsMsgDelegate[] m_aMsgHandles = new CsMsgDelegate[MAX_MSG_HANDLE];
            float[] m_fMsgRegTime = new float[MAX_MSG_HANDLE];
            private float m_timeout = 15;
            private readonly MainPack _timeOutPack = new MainPack { returncode = ReturnCode.MsgTimeOut };
            public void RmvCheckCsMsg(int index)
            {
                m_aMsgHandles[index] = null;
                m_fMsgRegTime[index] = 0;
            }
            private void RegTimeOutHandle(uint actionCode, CsMsgDelegate resHandler)
            {
                uint hashIndex = actionCode % MAX_MSG_HANDLE;
                if (m_aMsgHandles[hashIndex] != null)
                {
                    //NotifyTimeout(m_aMsgHandles[hashIndex]);
                    RmvCheckCsMsg((int)hashIndex);
                }
                m_aMsgHandles[hashIndex] = resHandler;
                m_fMsgRegTime[hashIndex] = UnityEngine.Time.time;
            }

            protected void NotifyTimeout(CsMsgDelegate msgHandler)
            {
                msgHandler(_timeOutPack);
            }
            
            private void CheckCsMsgTimeOut()
            {
                float nowTime = UnityEngine.Time.time;
                for (int i = 0; i < CHECK_TIMEOUT_PERFRAME; i++)
                {
                    m_dwLastCheckIndex = (m_dwLastCheckIndex + 1) % MAX_MSG_HANDLE;
                    if (m_aMsgHandles[m_dwLastCheckIndex] != null)
                    {
                        if (m_fMsgRegTime[m_dwLastCheckIndex] + m_timeout < nowTime)
                        {
                            TLogger.LogError("msg timeout, resCmdID[{0}]", m_aMsgHandles[m_dwLastCheckIndex]);

                            NotifyTimeout(m_aMsgHandles[m_dwLastCheckIndex]);

                            RmvCheckCsMsg((int)m_dwLastCheckIndex);
                        }
                    }
                }
            }

            #endregion
        }
    }
}
