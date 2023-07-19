using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace TEngine.Core
{
    public static class NetworkHelper
    {
        public static string[] GetAddressIPs()
        {
            var list = new List<string>();
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
                {
                    continue;
                }
    
                foreach (var add in networkInterface.GetIPProperties().UnicastAddresses)
                {
                    list.Add(add.Address.ToString());
                }
            }
    
            return list.ToArray();
        }
        
        public static IPEndPoint ToIPEndPoint(string host, int port)
        {
            return new IPEndPoint(IPAddress.Parse(host), port);
        }
        
        public static IPEndPoint ToIPEndPoint(string address)
        {
            var index = address.LastIndexOf(':');
            var host = address.Substring(0, index);
            var p = address.Substring(index + 1);
            var port = int.Parse(p);
            return ToIPEndPoint(host, port);
        }
        
        public static string IPEndPointToStr(this IPEndPoint self)
        {
            return $"{self.Address}:{self.Port}";
        }
    
        public static void SetSioUdpConnReset(Socket socket)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }
    
            /*
            目前这个问题只有Windows下才会出现。
            服务器端在发送数据时捕获到了一个异常，
            这个异常导致原因应该是远程客户端的UDP监听已停止导致数据发送出错。
            按理说UDP是无连接的，报这个异常是不合理的
            这个异常让整UDP的服务监听也停止了。
            这样就因为一个客户端的数据发送无法到达而导致了服务挂了，所有客户端都无法与服务器通信了
            想详细了解看下https://blog.csdn.net/sunzhen6251/article/details/124168805*/
            const uint IOC_IN = 0x80000000;
            const uint IOC_VENDOR = 0x18000000;
            const int SIO_UDP_CONNRESET = unchecked((int) (IOC_IN | IOC_VENDOR | 12));
    
            socket.IOControl(SIO_UDP_CONNRESET, new[] {Convert.ToByte(false)}, null);
        }
    }
}