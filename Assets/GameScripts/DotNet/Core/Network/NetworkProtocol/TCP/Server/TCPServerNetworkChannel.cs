using System;
using System.IO;
using System.Net.Sockets;
using TEngine.DataStructure;
// ReSharper disable InconsistentNaming
#pragma warning disable CS8622
#pragma warning disable CS8601
#pragma warning disable CS8618

namespace TEngine.Core.Network
{
    public sealed class TCPServerNetworkChannel : ANetworkChannel
    {
        #region 网络主线程

        private bool _isSending;
        private readonly Socket _socket;
        private readonly CircularBuffer _sendBuffer = new CircularBuffer();
        private readonly CircularBuffer _receiveBuffer = new CircularBuffer();
        private readonly SocketAsyncEventArgs _outArgs = new SocketAsyncEventArgs();
        private readonly SocketAsyncEventArgs _innArgs = new SocketAsyncEventArgs();

        public override event Action OnDispose;
        public override event Action<APackInfo> OnReceiveMemoryStream;

        public TCPServerNetworkChannel(uint id, Socket socket, ANetwork network) : base(network.Scene, id, network.Id)
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return;
            }
#endif
            _socket = socket;
            _socket.NoDelay = true;
            RemoteEndPoint = _socket.RemoteEndPoint;

            _innArgs.Completed += OnComplete;
            _outArgs.Completed += OnComplete;

            PacketParser = APacketParser.CreatePacketParser(network.NetworkTarget);
        }

        public override void Dispose()
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

            if (_socket.Connected)
            {
                _socket.Disconnect(true);
                _socket.Close();
            }
            
            _outArgs.Dispose();
            _innArgs.Dispose();
            _sendBuffer.Dispose();
            _receiveBuffer.Dispose();
            ThreadSynchronizationContext.Main.Post(OnDispose);
            base.Dispose();
        }
        
        public void Send(MemoryStream memoryStream)
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
                    if (IsDisposed)
                    {
                        return;
                    }
                    
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

        public void Receive()
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

                    if (!PacketParser.UnPack(_receiveBuffer, out var packInfo))
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
                    Log.Debug($"RemoteAddress:{RemoteEndPoint} \n{e}");
                    Dispose();
                }
                catch (Exception e)
                {
                    Log.Error($"RemoteAddress:{RemoteEndPoint} \n{e}");
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
                default:
                {
                    throw new Exception($"Socket Error: {asyncEventArgs.LastOperation}");
                }
            }
        }

        #endregion
    }
}