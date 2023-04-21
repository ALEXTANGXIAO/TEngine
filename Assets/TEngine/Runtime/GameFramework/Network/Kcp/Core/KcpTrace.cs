using System;
using System.Collections.Generic;
using System.Text;

namespace System.Net.Sockets.Kcp
{
    public partial class KcpCore<Segment>
    {
        public KcpLogMask LogMask { get; set; } = KcpLogMask.IKCP_LOG_PARSE_DATA | KcpLogMask.IKCP_LOG_NEED_SEND | KcpLogMask.IKCP_LOG_DEAD_LINK;

        public virtual bool CanLog(KcpLogMask mask)
        {
            if ((mask & LogMask) == 0)
            {
                return false;
            }

#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
            if (TraceListener != null)
            {
                return true;
            }
#endif
            return false;
        }

#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
        public System.Diagnostics.TraceListener TraceListener { get; set; }
#endif

        public virtual void LogFail(string message)
        {
#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
            TraceListener?.Fail(message);
#endif
        }

        public virtual void LogWriteLine(string message, string category)
        {
#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
            TraceListener?.WriteLine(message, category);
#endif
        }

        [Obsolete("一定要先判断CanLog 内部判断是否存在TraceListener,避免在没有TraceListener时生成字符串", true)]
        public virtual void LogWriteLine(string message, KcpLogMask mask)
        {
#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
            if (CanLog(mask))
            {
                LogWriteLine(message, mask.ToString());
            }
#endif
        }
    }

    [Flags]
    public enum KcpLogMask
    {
        IKCP_LOG_OUTPUT = 1 << 0,
        IKCP_LOG_INPUT = 1 << 1,
        IKCP_LOG_SEND = 1 << 2,
        IKCP_LOG_RECV = 1 << 3,
        IKCP_LOG_IN_DATA = 1 << 4,
        IKCP_LOG_IN_ACK = 1 << 5,
        IKCP_LOG_IN_PROBE = 1 << 6,
        IKCP_LOG_IN_WINS = 1 << 7,
        IKCP_LOG_OUT_DATA = 1 << 8,
        IKCP_LOG_OUT_ACK = 1 << 9,
        IKCP_LOG_OUT_PROBE = 1 << 10,
        IKCP_LOG_OUT_WINS = 1 << 11,

        IKCP_LOG_PARSE_DATA = 1 << 12,
        IKCP_LOG_NEED_SEND = 1 << 13,
        IKCP_LOG_DEAD_LINK = 1 << 14,
    }
}


