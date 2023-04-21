using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace System.Net.Sockets.Kcp
{
    /// <summary>
    /// 调整了没存布局，直接拷贝块提升性能。
    /// <para>结构体保存内容只有一个指针，不用担心参数传递过程中的性能</para>
    /// https://github.com/skywind3000/kcp/issues/118#issuecomment-338133930
    /// <para>不要对没有初始化的KcpSegment(内部指针为0，所有属性都将指向位置区域) 进行任何赋值操作，可能导致内存损坏。
    /// 出于性能考虑，没有对此项进行安全检查。</para>
    /// </summary>
    public struct KcpSegment : IKcpSegment
    {
        internal readonly unsafe byte* ptr;
        public unsafe KcpSegment(byte* intPtr, uint appendDateSize)
        {
            this.ptr = intPtr;
            len = appendDateSize;
        }

        /// <summary>
        /// 使用完必须显示释放，否则内存泄漏
        /// </summary>
        /// <param name="appendDateSize"></param>
        /// <returns></returns>
        public static KcpSegment AllocHGlobal(int appendDateSize)
        {
            var total = LocalOffset + HeadOffset + appendDateSize;
            IntPtr intPtr = Marshal.AllocHGlobal(total);
            unsafe
            {
                ///清零    不知道是不是有更快的清0方法？
                Span<byte> span = new Span<byte>(intPtr.ToPointer(), total);
                span.Clear();

                return new KcpSegment((byte*)intPtr.ToPointer(), (uint)appendDateSize);
            }
        }

        /// <summary>
        /// 释放非托管内存
        /// </summary>
        /// <param name="seg"></param>
        public static void FreeHGlobal(KcpSegment seg)
        {
            unsafe
            {
                Marshal.FreeHGlobal((IntPtr)seg.ptr);
            }
        }

        /// 以下为本机使用的参数
        /// <summary>
        /// offset = 0
        /// </summary>
        public uint resendts
        {
            get
            {
                unsafe
                {
                    return *(uint*)(ptr + 0);
                }
            }
            set
            {
                unsafe
                {
                    *(uint*)(ptr + 0) = value;
                }
            }
        }

        /// <summary>
        /// offset = 4
        /// </summary>
        public uint rto
        {
            get
            {
                unsafe
                {
                    return *(uint*)(ptr + 4);
                }
            }
            set
            {
                unsafe
                {
                    *(uint*)(ptr + 4) = value;
                }
            }
        }

        /// <summary>
        /// offset = 8
        /// </summary>
        public uint fastack
        {
            get
            {
                unsafe
                {
                    return *(uint*)(ptr + 8);
                }
            }
            set
            {
                unsafe
                {
                    *(uint*)(ptr + 8) = value;
                }
            }
        }

        /// <summary>
        /// offset = 12
        /// </summary>
        public uint xmit
        {
            get
            {
                unsafe
                {
                    return *(uint*)(ptr + 12);
                }
            }
            set
            {
                unsafe
                {
                    *(uint*)(ptr + 12) = value;
                }
            }
        }

        ///以下为需要网络传输的参数
        public const int LocalOffset = 4 * 4;
        public const int HeadOffset = KcpConst.IKCP_OVERHEAD;

        /// <summary>
        /// offset = <see cref="LocalOffset"/>
        /// </summary>
        /// https://github.com/skywind3000/kcp/issues/134
        public uint conv
        {
            get
            {
                unsafe
                {
                    return *(uint*)(LocalOffset + 0 + ptr);
                }
            }
            set
            {
                unsafe
                {
                    *(uint*)(LocalOffset + 0 + ptr) = value;
                }
            }
        }

        /// <summary>
        /// offset = <see cref="LocalOffset"/> + 4
        /// </summary>
        public byte cmd
        {
            get
            {
                unsafe
                {
                    return *(LocalOffset + 4 + ptr);
                }
            }
            set
            {
                unsafe
                {
                    *(LocalOffset + 4 + ptr) = value;
                }
            }
        }

        /// <summary>
        /// offset = <see cref="LocalOffset"/> + 5
        /// </summary>
        public byte frg
        {
            get
            {
                unsafe
                {
                    return *(LocalOffset + 5 + ptr);
                }
            }
            set
            {
                unsafe
                {
                    *(LocalOffset + 5 + ptr) = value;
                }
            }
        }

        /// <summary>
        /// offset = <see cref="LocalOffset"/> + 6
        /// </summary>
        public ushort wnd
        {
            get
            {
                unsafe
                {
                    return *(ushort*)(LocalOffset + 6 + ptr);
                }
            }
            set
            {
                unsafe
                {
                    *(ushort*)(LocalOffset + 6 + ptr) = value;
                }
            }
        }

        /// <summary>
        /// offset = <see cref="LocalOffset"/> + 8
        /// </summary>
        public uint ts
        {
            get
            {
                unsafe
                {
                    return *(uint*)(LocalOffset + 8 + ptr);
                }
            }
            set
            {
                unsafe
                {
                    *(uint*)(LocalOffset + 8 + ptr) = value;
                }
            }
        }

        /// <summary>
        /// <para> SendNumber? </para>
        /// offset = <see cref="LocalOffset"/> + 12
        /// </summary>
        public uint sn
        {
            get
            {
                unsafe
                {
                    return *(uint*)(LocalOffset + 12 + ptr);
                }
            }
            set
            {
                unsafe
                {
                    *(uint*)(LocalOffset + 12 + ptr) = value;
                }
            }
        }

        /// <summary>
        /// offset = <see cref="LocalOffset"/> + 16
        /// </summary>
        public uint una
        {
            get
            {
                unsafe
                {
                    return *(uint*)(LocalOffset + 16 + ptr);
                }
            }
            set
            {
                unsafe
                {
                    *(uint*)(LocalOffset + 16 + ptr) = value;
                }
            }
        }

        /// <summary>
        /// <para> AppendDateSize </para>
        /// offset = <see cref="LocalOffset"/> + 20
        /// </summary>
        public uint len
        {
            get
            {
                unsafe
                {
                    return *(uint*)(LocalOffset + 20 + ptr);
                }
            }
            private set
            {
                unsafe
                {
                    *(uint*)(LocalOffset + 20 + ptr) = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// https://github.com/skywind3000/kcp/issues/35#issuecomment-263770736
        public Span<byte> data
        {
            get
            {
                unsafe
                {
                    return new Span<byte>(LocalOffset + HeadOffset + ptr, (int)len);
                }
            }
        }



        /// <summary>
        /// 将片段中的要发送的数据拷贝到指定缓冲区
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public int Encode(Span<byte> buffer)
        {
            var datelen = (int)(HeadOffset + len);

            ///备用偏移值 现阶段没有使用
            const int offset = 0;

            if (KcpConst.IsLittleEndian)
            {
                if (BitConverter.IsLittleEndian)
                {
                    ///小端可以一次拷贝
                    unsafe
                    {
                        ///要发送的数据从LocalOffset开始。
                        ///本结构体调整了要发送字段和单机使用字段的位置，让报头数据和数据连续，节约一次拷贝。
                        Span<byte> sendDate = new Span<byte>(ptr + LocalOffset, datelen);
                        sendDate.CopyTo(buffer);
                    }
                }
                else
                {
                    BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(offset), conv);
                    buffer[offset + 4] = cmd;
                    buffer[offset + 5] = frg;
                    BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(offset + 6), wnd);

                    BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(offset + 8), ts);
                    BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(offset + 12), sn);
                    BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(offset + 16), una);
                    BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(offset + 20), len);

                    data.CopyTo(buffer.Slice(HeadOffset));
                }
            }
            else
            {
                if (BitConverter.IsLittleEndian)
                {
                    BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(offset), conv);
                    buffer[offset + 4] = cmd;
                    buffer[offset + 5] = frg;
                    BinaryPrimitives.WriteUInt16BigEndian(buffer.Slice(offset + 6), wnd);

                    BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(offset + 8), ts);
                    BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(offset + 12), sn);
                    BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(offset + 16), una);
                    BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(offset + 20), len);

                    data.CopyTo(buffer.Slice(HeadOffset));
                }
                else
                {
                    ///大端可以一次拷贝
                    unsafe
                    {
                        ///要发送的数据从LocalOffset开始。
                        ///本结构体调整了要发送字段和单机使用字段的位置，让报头数据和数据连续，节约一次拷贝。
                        Span<byte> sendDate = new Span<byte>(ptr + LocalOffset, datelen);
                        sendDate.CopyTo(buffer);
                    }
                }
            }

            return datelen;
        }
    }
}
