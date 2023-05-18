using System;
using System.Net;
using System.Net.Sockets;

namespace TEngine
{
    internal sealed partial class NetworkManager
    {
        /// <summary>
        /// 使用同步接收的 TCP 网络频道。
        /// </summary>
        private sealed class TcpWithSyncReceiveNetworkChannel : NetworkChannelBase
        {
            private readonly AsyncCallback _connectCallback;
            private readonly AsyncCallback _sendCallback;

            /// <summary>
            /// 初始化网络频道的新实例。
            /// </summary>
            /// <param name="name">网络频道名称。</param>
            /// <param name="networkChannelHelper">网络频道辅助器。</param>
            public TcpWithSyncReceiveNetworkChannel(string name, INetworkChannelHelper networkChannelHelper)
                : base(name, networkChannelHelper)
            {
                _connectCallback = ConnectCallback;
                _sendCallback = SendCallback;
            }

            /// <summary>
            /// 获取网络服务类型。
            /// </summary>
            public override ServiceType ServiceType
            {
                get
                {
                    return ServiceType.TcpWithSyncReceive;
                }
            }

            /// <summary>
            /// 连接到远程主机。
            /// </summary>
            /// <param name="ipAddress">远程主机的 IP 地址。</param>
            /// <param name="port">远程主机的端口号。</param>
            /// <param name="userData">用户自定义数据。</param>
            public override void Connect(IPAddress ipAddress, int port, object userData)
            {
                base.Connect(ipAddress, port, userData);
                MSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                if (MSocket == null)
                {
                    string errorMessage = "Initialize network channel failure.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SocketError, SocketError.Success, errorMessage);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                NetworkChannelHelper.PrepareForConnecting();
                ConnectAsync(ipAddress, port, userData);
            }

            protected override bool ProcessSend()
            {
                if (base.ProcessSend())
                {
                    SendAsync();
                    return true;
                }

                return false;
            }

            protected override void ProcessReceive()
            {
                base.ProcessReceive();
                while (MSocket.Available > 0)
                {
                    if (!ReceiveSync())
                    {
                        break;
                    }
                }
            }

            private void ConnectAsync(IPAddress ipAddress, int port, object userData)
            {
                try
                {
                    MSocket.BeginConnect(ipAddress, port, _connectCallback, new ConnectState(MSocket, userData));
                }
                catch (Exception exception)
                {
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ConnectError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }
            }

            private void ConnectCallback(IAsyncResult ar)
            {
                ConnectState socketUserData = (ConnectState)ar.AsyncState;
                try
                {
                    socketUserData.Socket.EndConnect(ar);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (Exception exception)
                {
                    Active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ConnectError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }

                MSentPacketCount = 0;
                MReceivedPacketCount = 0;

                lock (SendPacketPool)
                {
                    SendPacketPool.Clear();
                }

                lock (MHeartBeatState)
                {
                    MHeartBeatState.Reset(true);
                }

                if (NetworkChannelConnected != null)
                {
                    NetworkChannelConnected(this, socketUserData.UserData);
                }

                Active = true;
            }

            private void SendAsync()
            {
                try
                {
                    MSocket.BeginSend(MSendState.Stream.GetBuffer(), (int)MSendState.Stream.Position, (int)(MSendState.Stream.Length - MSendState.Stream.Position), SocketFlags.None, _sendCallback, MSocket);
                }
                catch (Exception exception)
                {
                    Active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.SendError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }
            }

            private void SendCallback(IAsyncResult ar)
            {
                Socket socket = (Socket)ar.AsyncState;
                if (!socket.Connected)
                {
                    return;
                }

                int bytesSent = 0;
                try
                {
                    bytesSent = socket.EndSend(ar);
                }
                catch (Exception exception)
                {
                    Active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.SendError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }

                MSendState.Stream.Position += bytesSent;
                if (MSendState.Stream.Position < MSendState.Stream.Length)
                {
                    SendAsync();
                    return;
                }

                MSentPacketCount++;
                MSendState.Reset();
            }

            private bool ReceiveSync()
            {
                try
                {
                    int bytesReceived = MSocket.Receive(MReceiveState.Stream.GetBuffer(), (int)MReceiveState.Stream.Position, (int)(MReceiveState.Stream.Length - MReceiveState.Stream.Position), SocketFlags.None);
                    if (bytesReceived <= 0)
                    {
                        Close();
                        return false;
                    }

                    MReceiveState.Stream.Position += bytesReceived;
                    if (MReceiveState.Stream.Position < MReceiveState.Stream.Length)
                    {
                        return false;
                    }

                    MReceiveState.Stream.Position = 0L;

                    bool processSuccess = false;
                    if (MReceiveState.PacketHeader != null)
                    {
                        processSuccess = ProcessPacket();
                        MReceivedPacketCount++;
                    }
                    else
                    {
                        processSuccess = ProcessPacketHeader();
                    }

                    return processSuccess;
                }
                catch (Exception exception)
                {
                    Active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ReceiveError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return false;
                    }

                    throw;
                }
            }
        }
    }
}
