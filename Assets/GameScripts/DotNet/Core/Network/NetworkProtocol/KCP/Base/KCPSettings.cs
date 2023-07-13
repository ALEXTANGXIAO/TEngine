using System;

namespace TEngine.Core.Network
{
    public class KCPSettings
    {
        public int Mtu { get; private set; }
        public int SendWindowSize { get; private set; }
        public int ReceiveWindowSize { get; private set; }
        public int MaxSendWindowSize { get; private set; }

        public static KCPSettings Create(NetworkTarget networkTarget)
        {
            var settings = new KCPSettings();
            
            switch (networkTarget)
            {
                case NetworkTarget.Outer:
                {
                    // 外网设置470的原因:
                    // 1、mtu设置过大有可能路由器过滤掉
                    // 2、降低 mtu 到 470，同样数据虽然会发更多的包，但是小包在路由层优先级更高
                    settings.Mtu = 470;
                    settings.SendWindowSize = 256;
                    settings.ReceiveWindowSize = 256;
                    settings.MaxSendWindowSize = 256 * 2;
                    break;
                }
                case NetworkTarget.Inner:
                {
                    // 内网设置1400的原因
                    // 1、一般都是同一台服务器来运行多个进程来处理
                    // 2、内网每个进程跟其他进程只有一个通道进行发送、所以发送的数量会比较大
                    // 3、如果不把窗口设置大点、会出现消息滞后。
                    // 4、因为内网发送的可不只是外网转发数据、还有可能是其他进程的通讯
                    settings.Mtu = 1400;
                    settings.SendWindowSize = 1024;
                    settings.ReceiveWindowSize = 1024;
                    settings.MaxSendWindowSize = 1024 * 2;
                    break;
                }
                default:
                {
                    throw new NotSupportedException($"KCPServerNetwork NotSupported NetworkType:{networkTarget}");
                }
            }

            return settings;
        }
    }
}