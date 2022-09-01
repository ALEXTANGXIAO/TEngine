using System;
using System.IO;
using ProtoBuf;
using ProtoBuf.Meta;
using TEngineProto;

namespace TEngine.Runtime
{
    public class NetworkChannelHelper : INetworkChannelHelper
    {
        private readonly MemoryStream m_CachedStream = new MemoryStream(1024 * 8);
        private INetworkChannel m_NetworkChannel = null;

        /// <summary>
        /// 获取消息包头长度。
        /// </summary>
        public int PacketHeaderLength => sizeof(int);

        /// <summary>
        /// 初始化网络频道辅助器。
        /// </summary>
        /// <param name="networkChannel">网络频道。</param>
        public void Initialize(INetworkChannel networkChannel)
        {
            m_NetworkChannel = networkChannel;

            GameEventMgr.Instance.AddEventListener<INetworkChannel,object>(NetWorkEventId.NetworkConnectedEvent,OnNetworkConnected);
            GameEventMgr.Instance.AddEventListener<INetworkChannel>(NetWorkEventId.NetworkClosedEvent,OnNetworkClosed);
            GameEventMgr.Instance.AddEventListener<INetworkChannel,int>(NetWorkEventId.NetworkMissHeartBeatEvent,OnNetworkMissHeartBeat);
            GameEventMgr.Instance.AddEventListener<INetworkChannel,NetworkErrorCode,string>(NetWorkEventId.NetworkErrorEvent,OnNetworkError);
            GameEventMgr.Instance.AddEventListener<INetworkChannel,object>(NetWorkEventId.NetworkCustomErrorEvent,OnNetworkCustomError);
            
            m_NetworkChannel.RegisterHandler((int)ActionCode.HeartBeat,HandleHeartBeat);
        }

        private void HandleHeartBeat(MainPack mainPack)
        {
            
        }

        /// <summary>
        /// 准备进行连接。
        /// </summary>
        public void PrepareForConnecting()
        {
            m_NetworkChannel.Socket.ReceiveBufferSize = 1024 * 64;
            m_NetworkChannel.Socket.SendBufferSize = 1024 * 64;
        }

        /// <summary>
        /// 发送心跳消息包。
        /// </summary>
        /// <returns>是否发送心跳消息包成功。</returns>
        public bool SendHeartBeat()
        {
            m_NetworkChannel.Send(CSHeartBeatHandler.AllocHeartBeatPack());
            return true;
        }

        /// <summary>
        /// 序列化消息包。
        /// </summary>
        /// <typeparam name="T">消息包类型。</typeparam>
        /// <param name="packet">要序列化的消息包。</param>
        /// <param name="destination">要序列化的目标流。</param>
        /// <returns>是否序列化成功。</returns>
        public bool Serialize(MainPack packet, Stream destination)
        {
            m_CachedStream.SetLength(m_CachedStream.Capacity); // 此行防止 Array.Copy 的数据无法写入
            m_CachedStream.Position = 0L;
            Serializer.SerializeWithLengthPrefix(m_CachedStream, packet, PrefixStyle.Fixed32);
            MemoryPool.Release((IMemory)packet);
            m_CachedStream.WriteTo(destination);
            return true;
        }

        /// <summary>
        /// 反序列化消息包。
        /// </summary>
        /// <param name="packetHeader">消息包头。</param>
        /// <param name="source">要反序列化的来源流。</param>
        /// <param name="customErrorData">用户自定义错误数据。</param>
        /// <returns>反序列化后的消息包。</returns>
        public MainPack DeserializePacket(Stream source, out object customErrorData)
        {
            // 注意：此函数并不在主线程调用！
            customErrorData = null;

            MainPack packet = null;
            Type packetType = typeof(MainPack);
            if (packetType != null)
            {
                packet = (MainPack)RuntimeTypeModel.Default.DeserializeWithLengthPrefix(source, MemoryPool.Acquire(packetType), packetType, PrefixStyle.Fixed32, 0);
            }
            else
            {
                Log.Warning("Can not deserialize packet for packet id '{0}'.", packet.actioncode.ToString());
            }
            return packet;
        }

        /// <summary>
        /// 关闭并清理网络频道辅助器。
        /// </summary>
        public void Shutdown()
        {
            GameEventMgr.Instance.RemoveEventListener<INetworkChannel,object>(NetWorkEventId.NetworkConnectedEvent,OnNetworkConnected);
            GameEventMgr.Instance.RemoveEventListener<INetworkChannel>(NetWorkEventId.NetworkClosedEvent,OnNetworkClosed);
            GameEventMgr.Instance.RemoveEventListener<INetworkChannel,int>(NetWorkEventId.NetworkMissHeartBeatEvent,OnNetworkMissHeartBeat);
            GameEventMgr.Instance.RemoveEventListener<INetworkChannel,NetworkErrorCode,string>(NetWorkEventId.NetworkErrorEvent,OnNetworkError);
            GameEventMgr.Instance.RemoveEventListener<INetworkChannel,object>(NetWorkEventId.NetworkCustomErrorEvent,OnNetworkCustomError);
            m_NetworkChannel = null;
        }

        #region Handle
        private void OnNetworkConnected(INetworkChannel channel,object userData)
        {
            if (channel != m_NetworkChannel)
            {
                return;
            }

            if (channel.Socket == null)
            {
                return;
            }
            Log.Info("Network channel '{0}' connected, local address '{1}', remote address '{2}'.", channel.Name, channel.Socket.LocalEndPoint.ToString(), channel.Socket.RemoteEndPoint.ToString());
        }

        private void OnNetworkClosed(INetworkChannel channel)
        {
            if (channel != m_NetworkChannel)
            {
                return;
            }
            Log.Warning("Network channel '{0}' closed.", channel.Name);
        }

        private void OnNetworkMissHeartBeat(INetworkChannel channel,int missCount)
        {
            if (channel != m_NetworkChannel)
            {
                return;
            }

            Log.Warning("Network channel '{0}' miss heart beat '{1}' times.", channel.Name, missCount.ToString());

            if (missCount < 2)
            {
                return;
            }
            channel.Close();
        }

        private void OnNetworkError(INetworkChannel channel,NetworkErrorCode errorCode,string errorMessage)
        {
            if (channel != m_NetworkChannel)
            {
                return;
            }

            Log.Fatal("Network channel '{0}' error, error code is '{1}', error message is '{2}'.", channel.Name, errorCode.ToString(), errorMessage);

            channel.Close();
        }

        private void OnNetworkCustomError(INetworkChannel channel,object customErrorData)
        {
            if (channel != m_NetworkChannel)
            {
                return;
            }
            
            Log.Fatal("Network channel '{0}' error, error code is '{1}', error message is '{2}'.", channel.Name, customErrorData.ToString());
        }
        #endregion
    }
}