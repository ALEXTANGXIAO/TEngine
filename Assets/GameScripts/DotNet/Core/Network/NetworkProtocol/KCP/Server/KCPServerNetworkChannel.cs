using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using TEngine.Core;
// ReSharper disable InconsistentNaming
#pragma warning disable CS8625
#pragma warning disable CS8618

namespace TEngine.Core.Network
{
    public sealed class KCPServerNetworkChannel : ANetworkChannel
    {
        #region 网络主线程

        private int _maxSndWnd;
        private byte[] _rawSendBuffer;
        public readonly uint CreateTime;
        private readonly Socket _socket;
        private Action<uint, uint> _addToUpdate;
        private MemoryStream _receiveMemoryStream;
        public IntPtr KcpIntPtr { get; private set; }

        public override event Action OnDispose;
        public override event Action<APackInfo> OnReceiveMemoryStream;

        public KCPServerNetworkChannel(Scene scene, uint id, long networkId, EndPoint remoteEndPoint, Socket socket, uint createTime) : base(scene, id, networkId)
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return;
            }
#endif
            _socket = socket;
            CreateTime = createTime;
            RemoteEndPoint = remoteEndPoint;
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

            var buff = new byte[5];
            buff.WriteTo(0, (byte) KcpHeader.Disconnect);
            buff.WriteTo(1, Id);
            _socket.SendTo(buff, 5, SocketFlags.None, RemoteEndPoint);

            if (KcpIntPtr != IntPtr.Zero)
            {
                KCPServerNetwork.ConnectionPtrChannel.Remove(KcpIntPtr);
                KCP.KcpRelease(KcpIntPtr);
                KcpIntPtr = IntPtr.Zero;
            }
#if NETDEBUG
            Log.Debug($"KCPServerNetworkChannel ConnectionPtrChannel:{KCPServerNetwork.ConnectionPtrChannel.Count}");
#endif
            _maxSndWnd = 0;
            _addToUpdate = null;
            _receiveMemoryStream?.Dispose();
            ThreadSynchronizationContext.Main.Post(OnDispose);
            base.Dispose();
        }

        public void Connect(IntPtr kcpIntPtr, Action<uint, uint> addToUpdate, int maxSndWnd, NetworkTarget networkTarget, ANetworkMessageScheduler networkMessageScheduler)
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return;
            }
#endif
            KcpIntPtr = kcpIntPtr;
            _maxSndWnd = maxSndWnd;
            _addToUpdate = addToUpdate;
            _rawSendBuffer = new byte[ushort.MaxValue];
            _receiveMemoryStream = MemoryStreamHelper.GetRecyclableMemoryStream();
            PacketParser = APacketParser.CreatePacketParser(networkTarget);
            
            ThreadSynchronizationContext.Main.Post(() =>
            {
                if (IsDisposed)
                {
                    return;
                }

                Session.Create(networkMessageScheduler, this, networkTarget);
            });
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
            if (IsDisposed || KcpIntPtr == IntPtr.Zero)
            {
                return;
            }

            // 检查等待发送的消息，如果超出两倍窗口大小，KCP作者给的建议是要断开连接

            var waitSendSize = KCP.KcpWaitsnd(KcpIntPtr);

            if (waitSendSize > _maxSndWnd)
            {
                Log.Warning($"ERR_KcpWaitSendSizeTooLarge {waitSendSize} > {_maxSndWnd}");
                Dispose();
                return;
            }

            // 发送消息

            KCP.KcpSend(KcpIntPtr, memoryStream.GetBuffer(), (int) memoryStream.Length);
            
            // 因为memoryStream对象池出来的、所以需要手动回收下
            
            memoryStream.Dispose();
            
            _addToUpdate(0, Id);
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
            if (IsDisposed || KcpIntPtr == IntPtr.Zero)
            {
                return;
            }

            for (;;)
            {
                try
                {
                    if (KcpIntPtr == IntPtr.Zero)
                    {
                        return;
                    }
                    
                    // 获得一个完整消息的长度

                    var peekSize = KCP.KcpPeeksize(KcpIntPtr);

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

                    _receiveMemoryStream.SetLength(peekSize);
                    _receiveMemoryStream.Seek(0, SeekOrigin.Begin);
                    var receiveCount = KCP.KcpRecv(KcpIntPtr, _receiveMemoryStream.GetBuffer(), peekSize);

                    // 如果接收的长度跟peekSize不一样，不需要处理，因为消息肯定有问题的(虽然不可能出现)。

                    if (receiveCount != peekSize)
                    {
                        Log.Error($"receiveCount != peekSize receiveCount:{receiveCount} peekSize:{peekSize}");
                        break;
                    }

                    var packInfo = PacketParser.UnPack(_receiveMemoryStream);

                    if (packInfo == null)
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
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Output(IntPtr bytes, int count)
        {
#if TENGINE_DEVELOP
            if (NetworkThread.Instance.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Log.Error("not in NetworkThread!");
                return;
            }
#endif
            if (IsDisposed || KcpIntPtr == IntPtr.Zero)
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
                _rawSendBuffer.WriteTo(1, Id);
                Marshal.Copy(bytes, _rawSendBuffer, 5, count);
                _socket.SendTo(_rawSendBuffer, 0, count + 5, SocketFlags.None, RemoteEndPoint);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        #endregion
    }
}