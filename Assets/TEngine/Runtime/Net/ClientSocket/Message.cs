
using System;
using System.Linq;
using Google.Protobuf;
using TEngineProto;

namespace TEngine.Net
{
    public class Message
    {
        private const int BufferHead = 4;

        private static byte[] buffer = new byte[1024];

        private int startindex;

        public byte[] Buffer
        {
            get { return buffer; }
        }

        public int StartIndex
        {
            get { return startindex; }
        }

        public int Remsize
        {
            get { return buffer.Length - startindex; }
        }

        public void ReadBuffer(byte[] bufBytes, Action<MainPack> handleResponse = null)
        {
            var length = bufBytes.Length;

            for (int i = 0; i < length; i++)
            {
                Buffer[i] = bufBytes[i];
            }

            startindex += length;

            if (startindex <= BufferHead)
            {
                return;
            }

            int count = length - BufferHead;

            while (true)
            {
                if (startindex >= (count + BufferHead))
                {
                    MainPack pack = (MainPack)MainPack.Descriptor.Parser.ParseFrom(buffer, BufferHead, count);

                    if (handleResponse != null)
                    {
                        handleResponse(pack);
                    }

                    Array.Copy(buffer, length, buffer, 0, startindex - length);

                    startindex -= length;
                }
                else
                {
                    break;
                }
            }
        }

        public void ReadBuffer(int length, Action<MainPack> handleResponse = null)
        {
            startindex += length;

            if (startindex <= BufferHead)
            {
                return;
            }

            int count = length - BufferHead;

            while (true)
            {
                if (startindex >= (count + BufferHead))
                {
                    MainPack pack = (MainPack)MainPack.Descriptor.Parser.ParseFrom(buffer, BufferHead, count);

                    if (handleResponse != null)
                    {
                        handleResponse(pack);
                    }

                    Array.Copy(buffer, length, buffer, 0, startindex - length);

                    startindex -= length;
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