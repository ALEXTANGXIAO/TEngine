using System;
using System.Net;
using System.Net.Sockets;

namespace TEngine
{
    internal sealed partial class NetworkManager
    {
        /// <summary>
        /// Kcp 网络频道。
        /// </summary>
        private sealed class KcpNetworkChannel : NetworkChannelBase
        {
            private readonly AsyncCallback _connectCallback;
            private readonly AsyncCallback _sendCallback;
            private readonly AsyncCallback _receiveCallback;

            /// <summary>
            /// 获取网络服务类型。
            /// </summary>
            public override ServiceType ServiceType => ServiceType.Kcp;


            public KcpNetworkChannel(string name, INetworkChannelHelper networkChannelHelper)
                : base(name, networkChannelHelper)
            {
                _connectCallback = ConnectCallback;
                _sendCallback = SendCallback;
                _receiveCallback = ReceiveCallback;
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
                MSocket = new Socket(ipAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
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
                        NetworkChannelError(this, NetworkErrorCode.ConnectError,
                            socketException?.SocketErrorCode ?? SocketError.Success,
                            exception.ToString());
                        return;
                    }

                    throw;
                }
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
                        NetworkChannelError(this, NetworkErrorCode.ConnectError,
                            socketException?.SocketErrorCode ?? SocketError.Success,
                            exception.ToString());
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
                ReceiveAsync();
            }

            private void SendAsync()
            {
                try
                {
                    MSocket.BeginSend(MSendState.Stream.GetBuffer(), (int)MSendState.Stream.Position,
                        (int)(MSendState.Stream.Length - MSendState.Stream.Position), SocketFlags.None, _sendCallback,
                        MSocket);
                }
                catch (Exception exception)
                {
                    Active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.SendError,
                            socketException?.SocketErrorCode ?? SocketError.Success,
                            exception.ToString());
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
                        NetworkChannelError(this, NetworkErrorCode.SendError,
                            socketException?.SocketErrorCode ?? SocketError.Success,
                            exception.ToString());
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

            private void ReceiveAsync()
            {
                try
                {
                    MSocket.BeginReceive(MReceiveState.Stream.GetBuffer(), (int)MReceiveState.Stream.Position,
                        (int)(MReceiveState.Stream.Length - MReceiveState.Stream.Position), SocketFlags.None,
                        _receiveCallback, MSocket);
                }
                catch (Exception exception)
                {
                    Active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ReceiveError,
                            socketException?.SocketErrorCode ?? SocketError.Success,
                            exception.ToString());
                        return;
                    }

                    throw;
                }
            }

            private void ReceiveCallback(IAsyncResult ar)
            {
                Socket socket = (Socket)ar.AsyncState;
                if (!socket.Connected)
                {
                    return;
                }

                int bytesReceived = 0;
                try
                {
                    bytesReceived = socket.EndReceive(ar);
                }
                catch (Exception exception)
                {
                    Active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ReceiveError,
                            socketException?.SocketErrorCode ?? SocketError.Success,
                            exception.ToString());
                        return;
                    }

                    throw;
                }

                if (bytesReceived <= 0)
                {
                    Close();
                    return;
                }

                MReceiveState.Stream.Position += bytesReceived;
                if (MReceiveState.Stream.Position < MReceiveState.Stream.Length)
                {
                    ReceiveAsync();
                    return;
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

                if (processSuccess)
                {
                    ReceiveAsync();
                }
            }
        }
    }
}