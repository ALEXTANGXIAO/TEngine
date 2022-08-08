using System;
using System.Net;
using System.Net.Sockets;
using TEngineProto;


namespace TEngine.Net
{
    public class TcpConnection
    {
        private Socket socket;
        private string m_Host;
        private int m_Port;
        private MessageProcess message;
        private GameClient gameClient;

        public TcpConnection(GameClient gameClient)
        {
            message = new MessageProcess();
            this.gameClient = gameClient;
        }

        public bool Connect(string host, int port ,bool async = false)
        {
            if (socket == null)
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            else
            {
                if (socket.Connected)
                {
                    socket.Close();
                }
            }

            TLogger.LogInfo("start connect server[{0}:{1}]...", host, port);

            gameClient.Status = GameClientStatus.StatusInit;
            try
            {
                if (async)
                {
                    IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(host), port);
                    SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                    args.RemoteEndPoint = ipPoint;

                    args.Completed += (obj, socketError) =>
                    {
                        if (socketError.SocketError == SocketError.Success)
                        {
                            TLogger.LogInfoSuccessd("connect server[{0}:{1}] success!!!", host, port);
                            SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();
                            receiveArgs.SetBuffer(message.Buffer, 0, message.Buffer.Length);
                            receiveArgs.Completed += ReceiveCallBackAsync;
                            this.socket.ReceiveAsync(receiveArgs);
                            gameClient.Status = GameClientStatus.StatusConnect;
                        }
                        else
                        {
                            TLogger.LogError("connect server failed" + socketError.SocketError);
                        }
                    };
                    socket.ConnectAsync(args);
                }
                else
                {
                    socket.Connect(host, port);
                    StartReceive();
                    gameClient.Status = GameClientStatus.StatusConnect;
                }
            }
            catch (Exception e)
            {
                TLogger.LogError(e.Message);
                TLogger.LogError("socket connect {0}:{1} failed", host, port);
                return false;
            }

            //TLogger.LogInfoSuccessd("connect server[{0}:{1}] success!!!", host, port);
            m_Host = host;
            m_Port = port;
            return true;
        }

        void ReceiveCallBackAsync(object obj, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                message.ReadBuffer(args.BytesTransferred, gameClient.HandleResponse);

                args.SetBuffer(message.StartIndex, message.RemSize);
                if (this.socket != null && this.socket.Connected)
                {
                    socket.ReceiveAsync(args);
                }
                else
                {
                    Close();
                }
            }
            else
            {
                TLogger.LogError("socket receive error" + args.SocketError);
                Close();
            }
        }

        void StartReceive()
        {
            socket.BeginReceive(message.Buffer, message.StartIndex, message.RemSize, SocketFlags.None, ReceiveCallback, null);
        }

        void ReceiveCallback(IAsyncResult asyncResult)
        {
            try
            {
                if (socket == null || socket.Connected == false)
                {
                    return;
                }

                int buffLength = socket.EndReceive(asyncResult);

                if (buffLength == 0)
                {
                    Close();

                    return;
                }

                message.ReadBuffer(buffLength, gameClient.HandleResponse);

                StartReceive();
            }
            catch (Exception e)
            {
                TLogger.LogError("TcpConnection DisConnected:   " + e);
                Close();
            }
        }

        public bool SendCsMsg(MainPack mainPack)
        {
            if (socket == null || socket.Connected == false)
            {
                return false;
            }

            try
            {
                socket.Send(MessageProcess.PackData(mainPack));
                return true;
            }
            catch (Exception e)
            {
                TLogger.LogError("TcpConnection SendCsMsg:   " + e);
                return false;
            }
        }

        public void Close()
        {
            if (socket != null && socket.Connected)
            {
                socket.Close();
            }
            gameClient.Status = GameClientStatus.StatusInit;
        }
    }
}
