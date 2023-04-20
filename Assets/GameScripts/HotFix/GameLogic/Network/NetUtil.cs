using System.Net;
using System.Net.Sockets;

namespace TEngine
{
    public class NetUtil
    {
        public static bool IsHaveIpV6Address(IPAddress[] ipAddresses, ref IPAddress[] outIPs)
        {
            int v6Count = 0;
            for (int i = 0; i < ipAddresses.Length; i++)
            {
                if (AddressFamily.InterNetworkV6.Equals(ipAddresses[i].AddressFamily))
                {
                    v6Count++;
                }
            }

            if (v6Count > 0)
            {
                outIPs = new IPAddress[v6Count];
                int resIndex = 0;
                for (int i = 0; i < ipAddresses.Length; i++)
                {
                    if (AddressFamily.InterNetworkV6.Equals(ipAddresses[i].AddressFamily))
                    {
                        outIPs[resIndex++] = ipAddresses[i];
                    }
                }

                return true;
            }

            return false;
        }

        public static IPEndPoint GetEndPoint(string server, int port)
        {
            IPAddress[] ps = Dns.GetHostAddresses(server);
            IPAddress[] finalIps = ps;
            if (Socket.OSSupportsIPv6 && NetUtil.IsHaveIpV6Address(ps, ref finalIps))
            {
                Log.Error("socket use addr ipV6: {0}, IP count:{1} AddressFamily[{2}]", server, finalIps.Length, finalIps[0].AddressFamily);
            }

            if (finalIps.Length > 0)
            {
                return new IPEndPoint(finalIps[0], port);
            }

            return null;
        }
    }
}