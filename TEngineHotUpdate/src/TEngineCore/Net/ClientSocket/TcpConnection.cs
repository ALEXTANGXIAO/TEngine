using System;
using System.Collections.Generic;
using System.Net.Sockets;
using TEngineProto;


namespace TEngineCore.Net
{
    public class TcpConnection
    {
        private Socket socket;
        private string m_Host;
        private int m_Port;
        private Message message;
        private GameClient gameClient;

        public TcpConnection(GameClient gameClient)
        {
            message = new Message();
            this.gameClient = gameClient;
        }

        public bool Connect(string host, int port)
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
                socket.Connect(host, port);
                StartReceive();
                gameClient.Status = GameClientStatus.StatusConnect;
            }
            catch (Exception e)
            {
                TLogger.LogError(e.Message);
                TLogger.LogError("socket connect {0}:{1} failed", host, port);
                return false;
            }

            TLogger.LogInfoSuccessd("connect server[{0}:{1}] success!!!", host, port);
            m_Host = host;
            m_Port = port;
            return true;
        }

        void StartReceive()
        {
            socket.BeginReceive(message.Buffer, message.StartIndex, message.Remsize, SocketFlags.None, ReceiveCallback, null);
        }

        void ReceiveCallback(IAsyncResult asyncResult)
        {
            try
            {
                if (socket == null || socket.Connected == false)
                {
                    return;
                }

                int Length = socket.EndReceive(asyncResult);

                if (Length == 0)
                {
                    Close();

                    return;
                }

                message.ReadBuffer(Length, gameClient.HandleResponse);

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
                socket.Send(Message.PackData(mainPack));
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
