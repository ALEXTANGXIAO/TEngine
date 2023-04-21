using System;
using System.Net;
using System.Net.Sockets;

namespace TEngine
{
    /// <summary>
    /// 网络频道接口。
    /// </summary>
    public interface INetworkChannel
    {
        /// <summary>
        /// 获取网络频道名称。
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// 获取网络频道所使用的 Socket。
        /// </summary>
        Socket Socket
        {
            get;
        }

        /// <summary>
        /// 获取是否已连接。
        /// </summary>
        bool Connected
        {
            get;
        }

        /// <summary>
        /// 获取网络服务类型。
        /// </summary>
        ServiceType ServiceType
        {
            get;
        }

        /// <summary>
        /// 获取网络地址类型。
        /// </summary>
        AddressFamily AddressFamily
        {
            get;
        }

        /// <summary>
        /// 获取要发送的消息包数量。
        /// </summary>
        int SendPacketCount
        {
            get;
        }

        /// <summary>
        /// 获取累计发送的消息包数量。
        /// </summary>
        int SentPacketCount
        {
            get;
        }

        /// <summary>
        /// 获取已接收未处理的消息包数量。
        /// </summary>
        int ReceivePacketCount
        {
            get;
        }

        /// <summary>
        /// 获取累计已接收的消息包数量。
        /// </summary>
        int ReceivedPacketCount
        {
            get;
        }

        /// <summary>
        /// 获取或设置当收到消息包时是否重置心跳流逝时间。
        /// </summary>
        bool ResetHeartBeatElapseSecondsWhenReceivePacket
        {
            get;
            set;
        }

        /// <summary>
        /// 获取丢失心跳的次数。
        /// </summary>
        int MissHeartBeatCount
        {
            get;
        }

        /// <summary>
        /// 获取或设置心跳间隔时长，以秒为单位。
        /// </summary>
        float HeartBeatInterval
        {
            get;
            set;
        }

        /// <summary>
        /// 获取心跳等待时长，以秒为单位。
        /// </summary>
        float HeartBeatElapseSeconds
        {
            get;
        }

        /// <summary>
        /// 注册网络消息包处理函数。
        /// </summary>
        /// <param name="msgId">网络消息包id。</param>
        /// <param name="msgDelegate">要注册的网络消息包处理函数。</param>
        void RegisterMsgHandler(int msgId, CsMsgDelegate msgDelegate);
        
        /// <summary>
        /// 移除网络消息包处理函数。
        /// </summary>
        /// <param name="msgId">网络消息包id。</param>
        /// <param name="msgDelegate">要注册的网络消息包处理函数。</param>
        void RemoveMsgHandler(int msgId, CsMsgDelegate msgDelegate);

        /// <summary>
        /// 连接到远程主机。
        /// </summary>
        /// <param name="ipAddress">远程主机的 IP 地址。</param>
        /// <param name="port">远程主机的端口号。</param>
        void Connect(IPAddress ipAddress, int port);

        /// <summary>
        /// 连接到远程主机。
        /// </summary>
        /// <param name="ipAddress">远程主机的 IP 地址。</param>
        /// <param name="port">远程主机的端口号。</param>
        /// <param name="userData">用户自定义数据。</param>
        void Connect(IPAddress ipAddress, int port, object userData);

        /// <summary>
        /// 关闭网络频道。
        /// </summary>
        void Close();

        /// <summary>
        /// 向远程主机发送消息包。
        /// </summary>
        /// <typeparam name="T">消息包类型。</typeparam>
        /// <param name="packet">要发送的消息包。</param>
        /// <returns>消息包是否发送成功。</returns>
        bool Send<T>(T packet) where T : Packet;
        
        /// <summary>
        /// 向远程主机发送消息包并注册消息回调。
        /// </summary>
        /// <typeparam name="T">消息包类型。</typeparam>
        /// <param name="pack">要发送的消息包。</param>
        /// <param name="resHandler">要注册的回调。</param>
        /// <param name="needShowWaitUI">是否需要等待UI。</param>
        /// <returns>消息包是否发送成功。</returns>
        bool Send<T>(T pack, CsMsgDelegate resHandler, bool needShowWaitUI = false) where T : Packet;
    }
}
