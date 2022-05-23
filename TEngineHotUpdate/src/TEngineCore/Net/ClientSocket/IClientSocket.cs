using System;
using System.Net;
using System.Net.Sockets;

namespace TEngineCore.Net
{
    public enum ClientSocketEventType
    {
        EventConnected,
        EventConnectFail,
        EventDisconnected,
    }

    public interface IClientSocket
    {
        /// <summary>
        /// 是否连接了
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 是否是流协议
        /// </summary>
        bool IsStream { get; }

        /// <summary>
        /// 心跳间隔
        /// </summary>
        int HeartBeatInterval { get; }

        /// <summary>
        /// 本地绑定地址
        /// </summary>
        EndPoint LocalAddr { get; }

        SocketError LastSockError { get; }

        string LastErrDesc { get; }

        /// <summary>
        /// 注册系统事件
        /// </summary>
        /// <param name="handler"></param>
        void RegEventHandle(Action<ClientSocketEventType> handler);

        /// <summary>
        /// 连接请求
        /// </summary>
        /// <param name="server"></param>
        /// <param name="port"></param>
        /// <param name="iTimeout"></param>
        /// <param name="retryNum"></param>
        /// <returns></returns>
        bool Connect(string server, int port, int iTimeout, int retryNum);

        /// <summary>
        /// 关闭连接
        /// </summary>
        void Close();

        /// <summary>
        /// 关闭连接
        /// </summary>
        void Shutdown();

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <returns></returns>
        bool Send(byte[] data, int offset, int len);

        /// <summary>
        /// 发送快捷数据,不用保证丢包
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        bool SendUdpTypeData(byte[] data, int offset, int len);

        /// <summary>
        /// 是否支持udp的包
        /// </summary>
        /// <returns></returns>
        bool IsSupportUdpType();

        /// <summary>
        /// 收包处理
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="iOffset"></param>
        /// <param name="maxSize"></param>
        /// <returns></returns>
        int Recv(byte[] buf, int iOffset, int maxSize);

        /// <summary>
        /// 循环调用
        /// </summary>
        void Update();

        /// <summary>
        /// 最后一帧，保证肯定要包发出去，减少延迟
        /// </summary>
        void LateUpdate();

        /// <summary>
        /// 获取写队列的个数
        /// </summary>
        /// <returns></returns>
        int GetSendQueueCount();

        /// <summary>
        /// 像底层注册错误打印，当缓冲区满之类的，调用上层的统计来打印
        /// </summary>
        /// <param name="debugCmd"></param>
        void RegDebugCmdHandle(Action debugCmd);
    }
}
