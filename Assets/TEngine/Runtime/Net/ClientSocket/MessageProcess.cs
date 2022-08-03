using System;
using System.Linq;
using Google.Protobuf;
using TEngineProto;

namespace TEngine.Net
{
    /// <summary>
    /// 消息处理管线
    /// </summary>
    public class MessageProcess
    {
        private const int BufferHead = 4;

        private static byte[] buffer = new byte[2048];

        private int m_startIndex;

        public byte[] Buffer => buffer;

        public int StartIndex => m_startIndex;

        public int RemSize => buffer.Length - m_startIndex;

        public void ReadBuffer(int buffLength, Action<MainPack> handleResponse = null)
        {
            m_startIndex += buffLength;

            if (m_startIndex <= BufferHead)
            {
                return;
            }

            int bodyCount = buffLength - BufferHead;

            while (true)
            {
                if (m_startIndex >= (bodyCount + BufferHead))
                {
                    MainPack mainPack = (MainPack)MainPack.Descriptor.Parser.ParseFrom(buffer, BufferHead, bodyCount);

                    handleResponse?.Invoke(mainPack);

                    Array.Copy(buffer, buffLength, buffer, 0, m_startIndex - buffLength);

                    m_startIndex -= buffLength;
                }
                else
                {
                    break;
                }
            }
        }

        public static byte[] PackData(MainPack pack)
        {
            byte[] data = pack.ToByteArray();
            byte[] head = BitConverter.GetBytes(data.Length);
            return head.Concat(data).ToArray();
        }

        public static byte[] PackDataUdp(MainPack pack)
        {
            return pack.ToByteArray();
        }
    }
}