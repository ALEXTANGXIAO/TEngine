using System.Net;
using System.Net.Sockets;


namespace TEngine.Net
{
    class NetUtil
    {
        public static bool IsHaveIpV6Address(IPAddress[] IPs, ref IPAddress[] outIPs)
        {
            int v6Count = 0;
            for (int i = 0; i < IPs.Length; i++)
            {
                if (AddressFamily.InterNetworkV6.Equals(IPs[i].AddressFamily))
                {
                    v6Count++;
                }
            }
            if (v6Count > 0)
            {
                outIPs = new IPAddress[v6Count];
                int resIndex = 0;
                for (int i = 0; i < IPs.Length; i++)
                {
                    if (AddressFamily.InterNetworkV6.Equals(IPs[i].AddressFamily))
                    {
                        outIPs[resIndex++] = IPs[i];
                    }
                }

                return true;
            }

            return false;
        }

        public static IPEndPoint GetEndPoint(string server, int port)
        {
            IPAddress[] IPs = Dns.GetHostAddresses(server);
            IPAddress[] finalIPS = IPs;
            if (Socket.OSSupportsIPv6 && NetUtil.IsHaveIpV6Address(IPs, ref finalIPS))
            {
                TLogger.LogError("socket use addr ipV6: {0}, IP count:{1} AddressFamily[{2}]", server, finalIPS.Length, finalIPS[0].AddressFamily);
            }

            if (finalIPS.Length > 0)
            {
                return new IPEndPoint(finalIPS[0], port);
            }

            return null;
        }

    }
}
