using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KcpProject
{
    class ByteBuffer : ICloneable
    {
        //字节缓存区
        private byte[] buf;
        //读取索引
        private int readIndex = 0;
        //写入索引
        private int writeIndex = 0;
        //读取索引标记
        private int markReadIndex = 0;
        //写入索引标记
        private int markWirteIndex = 0;
        //缓存区字节数组的长度
        private int capacity;

        //对象池
        private static List<ByteBuffer> pool = new List<ByteBuffer>();
        private static int poolMaxCount = 200;

        //此对象是否池化
        private bool isPool = false;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="capacity">初始容量</param>
        private ByteBuffer(int capacity)
        {
            this.buf = new byte[capacity];
            this.capacity = capacity;
            this.readIndex = 0;
            this.writeIndex = 0;
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="bytes">初始字节数组</param>
        private ByteBuffer(byte[] bytes)
        {
            this.buf = new byte[bytes.Length];
            Array.Copy(bytes, 0, buf, 0, buf.Length);
            this.capacity = buf.Length;
            this.readIndex = 0;
            this.writeIndex = bytes.Length + 1;
        }

        /// <summary>
        /// 构建一个capacity长度的字节缓存区ByteBuffer对象
        /// </summary>
        /// <param name="capacity">初始容量</param>
        /// <param name="fromPool">
        /// true表示获取一个池化的ByteBuffer对象，池化的对象必须在调用Dispose后才会推入池中，此方法为线程安全的。
        /// 当为true时，从池中获取的对象的实际capacity值。
        /// </param>
        /// <returns>ByteBuffer对象</returns>
        public static ByteBuffer Allocate(int capacity, bool fromPool = false)
        {
            if (!fromPool)
            {
                return new ByteBuffer(capacity);
            }
            lock (pool)
            {
                ByteBuffer bbuf;
                if (pool.Count == 0)
                {
                    bbuf = new ByteBuffer(capacity)
                    {
                        isPool = true
                    };
                    return bbuf;
                }
                int lastIndex = pool.Count - 1;
                bbuf = pool[lastIndex];
                pool.RemoveAt(lastIndex);
                if (!bbuf.isPool)
                {
                    bbuf.isPool = true;
                }
                return bbuf;
            }
        }

        /// <summary>
        /// 构建一个以bytes为字节缓存区的ByteBuffer对象，一般不推荐使用
        /// </summary>
        /// <param name="bytes">初始字节数组</param>
        /// <param name="fromPool">
        /// true表示获取一个池化的ByteBuffer对象，池化的对象必须在调用Dispose后才会推入池中，此方法为线程安全的。
        /// </param>
        /// <returns>ByteBuffer对象</returns>
        public static ByteBuffer Allocate(byte[] bytes, bool fromPool = false)
        {
            if (!fromPool)
            {
                return new ByteBuffer(bytes);
            }
            lock (pool)
            {
                ByteBuffer bbuf;
                if (pool.Count == 0)
                {
                    bbuf = new ByteBuffer(bytes)
                    {
                        isPool = true
                    };
                    return bbuf;
                }
                int lastIndex = pool.Count - 1;
                bbuf = pool[lastIndex];
                bbuf.WriteBytes(bytes);
                pool.RemoveAt(lastIndex);
                if (!bbuf.isPool)
                {
                    bbuf.isPool = true;
                }
                return bbuf;
            }
        }

        /// <summary>
        /// 根据value，确定大于此length的最近的2次方数，如length=7，则返回值为8；length=12，则返回16
        /// </summary>
        /// <param name="value">参考容量</param>
        /// <returns>比参考容量大的最接近的2次方数</returns>
        private int FixLength(int value)
        {
            if (value == 0)
            {
                return 1;
            }
            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            return value + 1;
            //int n = 2;
            //int b = 2;
            //while (b < length)
            //{
            //    b = 2 << n;
            //    n++;
            //}
            //return b;
        }

        /// <summary>
        /// 翻转字节数组，如果本地字节序列为高字节序列，则进行翻转以转换为低字节序列
        /// </summary>
        /// <param name="bytes">待转为高字节序的字节数组</param>
        /// <returns>低字节序列的字节数组</returns>
        private byte[] Flip(byte[] bytes)
        {
            //if (BitConverter.IsLittleEndian)
            //{
            //    Array.Reverse(bytes);
            //}
            return bytes;
        }

        /// <summary>
        /// 确定内部字节缓存数组的大小
        /// </summary>
        /// <param name="currLen">当前容量</param>
        /// <param name="futureLen">将来的容量</param>
        /// <returns>当前缓冲区的最大容量</returns>
        private int FixSizeAndReset(int currLen, int futureLen)
        {
            if (futureLen > currLen)
            {
                //以原大小的2次方数的两倍确定内部字节缓存区大小
                int size = FixLength(currLen) * 2;
                if (futureLen > size)
                {
                    //以将来的大小的2次方的两倍确定内部字节缓存区大小
                    size = FixLength(futureLen) * 2;
                }
                byte[] newbuf = new byte[size];
                Array.Copy(buf, 0, newbuf, 0, currLen);
                buf = newbuf;
                capacity = size;
            }
            return futureLen;
        }

        /// <summary>
        /// 确保有这么多字节可以用来写入
        /// </summary>
        /// <param name="minBytes"></param>
        public void EnsureWritableBytes(int minBytes)
        {
            // 如果没有足够的空间进行写入了
            if (WritableBytes < minBytes)
            {

                // 优先整理空间
                if (ReaderIndex >= minBytes)
                {
                    // 整理出来可用空间
                    TrimReadedBytes();
                }
                else
                {
                    // 空间不足时，重新分配内存
                    FixSizeAndReset(buf.Length, buf.Length + minBytes);
                }
            }
        }

        public void TrimReadedBytes()
        {
            Buffer.BlockCopy(buf, readIndex, buf, 0, writeIndex - readIndex);
            writeIndex -= readIndex;
            readIndex = 0;
        }

        /// <summary>
        /// 将bytes字节数组从startIndex开始的length字节写入到此缓存区
        /// </summary>
        /// <param name="bytes">待写入的字节数据</param>
        /// <param name="startIndex">写入的开始位置</param>
        /// <param name="length">写入的长度</param>
        public void WriteBytes(byte[] bytes, int startIndex, int length)
        {
            if (length <= 0 || startIndex < 0) return;

            int total = length + writeIndex;
            int len = buf.Length;
            FixSizeAndReset(len, total);
            Array.Copy(bytes, startIndex, buf, writeIndex, length);
            writeIndex = total;
        }

        /// <summary>
        /// 将字节数组中从0到length的元素写入缓存区
        /// </summary>
        /// <param name="bytes">待写入的字节数据</param>
        /// <param name="length">写入的长度</param>
        public void WriteBytes(byte[] bytes, int length)
        {
            WriteBytes(bytes, 0, length);
        }

        /// <summary>
        /// 将字节数组全部写入缓存区
        /// </summary>
        /// <param name="bytes">待写入的字节数据</param>
        public void WriteBytes(byte[] bytes)
        {
            WriteBytes(bytes, bytes.Length);
        }

        /// <summary>
        /// 将一个ByteBuffer的有效字节区写入此缓存区中
        /// </summary>
        /// <param name="buffer">待写入的字节缓存区</param>
        public void Write(ByteBuffer buffer)
        {
            if (buffer == null) return;
            if (buffer.ReadableBytes <= 0) return;
            WriteBytes(buffer.ToArray());
        }

        /// <summary>
        /// 写入一个int16数据
        /// </summary>
        /// <param name="value">short数据</param>
        public void WriteShort(short value)
        {
            WriteBytes(Flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个uint16数据
        /// </summary>
        /// <param name="value">ushort数据</param>
        public void WriteUshort(ushort value)
        {
            WriteBytes(Flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个int32数据
        /// </summary>
        /// <param name="value">int数据</param>
        public void WriteInt(int value)
        {
            //byte[] array = new byte[4];
            //for (int i = 3; i >= 0; i--)
            //{
            //    array[i] = (byte)(value & 0xff);
            //    value = value >> 8;
            //}
            //Array.Reverse(array);
            //Write(array);
            WriteBytes(Flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个uint32数据
        /// </summary>
        /// <param name="value">uint数据</param>
        public void WriteUint(uint value)
        {
            WriteBytes(Flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个int64数据
        /// </summary>
        /// <param name="value">long数据</param>
        public void WriteLong(long value)
        {
            WriteBytes(Flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个uint64数据
        /// </summary>
        /// <param name="value">ulong数据</param>
        public void WriteUlong(ulong value)
        {
            WriteBytes(Flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个float数据
        /// </summary>
        /// <param name="value">float数据</param>
        public void WriteFloat(float value)
        {
            WriteBytes(Flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个byte数据
        /// </summary>
        /// <param name="value">byte数据</param>
        public void WriteByte(byte value)
        {
            int afterLen = writeIndex + 1;
            int len = buf.Length;
            FixSizeAndReset(len, afterLen);
            buf[writeIndex] = value;
            writeIndex = afterLen;
        }

        /// <summary>
        /// 写入一个byte数据
        /// </summary>
        /// <param name="value">byte数据</param>
        public void WriteByte(int value)
        {
            byte b = (byte)value;
            WriteByte(b);
        }

        /// <summary>
        /// 写入一个double类型数据
        /// </summary>
        /// <param name="value">double数据</param>
        public void WriteDouble(double value)
        {
            WriteBytes(Flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个字符
        /// </summary>
        /// <param name="value"></param>
        public void WriteChar(char value)
        {
            WriteBytes(Flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个布尔型数据
        /// </summary>
        /// <param name="value"></param>
        public void WriteBoolean(bool value)
        {
            WriteBytes(Flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 读取一个字节
        /// </summary>
        /// <returns>字节数据</returns>
        public byte ReadByte()
        {
            byte b = buf[readIndex];
            readIndex++;
            return b;
        }

        /// <summary>
        /// 获取从index索引处开始len长度的字节
        /// </summary>
        /// <param name="index"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        private byte[] Get(int index, int len)
        {
            byte[] bytes = new byte[len];
            Array.Copy(buf, index, bytes, 0, len);
            return Flip(bytes);
        }

        /// <summary>
        /// 从读取索引位置开始读取len长度的字节数组
        /// </summary>
        /// <param name="len">待读取的字节长度</param>
        /// <returns>字节数组</returns>
        private byte[] Read(int len)
        {
            byte[] bytes = Get(readIndex, len);
            readIndex += len;
            return bytes;
        }

        /// <summary>
        /// 读取一个uint16数据
        /// </summary>
        /// <returns>ushort数据</returns>
        public ushort ReadUshort()
        {
            return BitConverter.ToUInt16(Read(2), 0);
        }

        /// <summary>
        /// 读取一个int16数据
        /// </summary>
        /// <returns>short数据</returns>
        public short ReadShort()
        {
            return BitConverter.ToInt16(Read(2), 0);
        }

        /// <summary>
        /// 读取一个uint32数据
        /// </summary>
        /// <returns>uint数据</returns>
        public uint ReadUint()
        {
            return BitConverter.ToUInt32(Read(4), 0);
        }

        /// <summary>
        /// 读取一个int32数据
        /// </summary>
        /// <returns>int数据</returns>
        public int ReadInt()
        {
            return BitConverter.ToInt32(Read(4), 0);
        }

        /// <summary>
        /// 读取一个uint64数据
        /// </summary>
        /// <returns>ulong数据</returns>
        public ulong ReadUlong()
        {
            return BitConverter.ToUInt64(Read(8), 0);
        }

        /// <summary>
        /// 读取一个long数据
        /// </summary>
        /// <returns>long数据</returns>
        public long ReadLong()
        {
            return BitConverter.ToInt64(Read(8), 0);
        }

        /// <summary>
        /// 读取一个float数据
        /// </summary>
        /// <returns>float数据</returns>
        public float ReadFloat()
        {
            return BitConverter.ToSingle(Read(4), 0);
        }

        /// <summary>
        /// 读取一个double数据
        /// </summary>
        /// <returns>double数据</returns>
        public double ReadDouble()
        {
            return BitConverter.ToDouble(Read(8), 0);
        }

        /// <summary>
        /// 读取一个字符
        /// </summary>
        /// <returns></returns>
        public char ReadChar()
        {
            return BitConverter.ToChar(Read(2), 0);
        }

        /// <summary>
        /// 读取布尔型数据
        /// </summary>
        /// <returns></returns>
        public bool ReadBoolean()
        {
            return BitConverter.ToBoolean(Read(1), 0);
        }

        /// <summary>
        /// 从读取索引位置开始读取len长度的字节到disbytes目标字节数组中
        /// </summary>
        /// <param name="disbytes">读取的字节将存入此字节数组</param>
        /// <param name="disstart">目标字节数组的写入索引</param>
        /// <param name="len">读取的长度</param>
        public void ReadBytes(byte[] disbytes, int disstart, int len)
        {
            int size = disstart + len;
            for (int i = disstart; i < size; i++)
            {
                disbytes[i] = this.ReadByte();
            }
        }

        public byte[] ReadBytes(int len)
        {
            return ReadBytes(readIndex, len);
        }

        public byte[] ReadBytes(int index, int len)
        {
            if (ReadableBytes < len)
                throw new Exception("no more readable bytes");

            var buffer = new byte[len];
            Array.Copy(buf, index, buffer, 0, len);
            readIndex += len;
            return buffer;
        }

        /// <summary>
        /// 获取一个字节
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte GetByte(int index)
        {
            return buf[index];
        }

        /// <summary>
        /// 获取一个字节
        /// </summary>
        /// <returns></returns>
        public byte GetByte()
        {
            return GetByte(readIndex);
        }

        /// <summary>
        /// 获取一个双精度浮点数据，不改变数据内容
        /// </summary>
        /// <param name="index">字节索引</param>
        /// <returns></returns>
        public double GetDouble(int index)
        {
            return BitConverter.ToDouble(Get(index, 8), 0);
        }

        /// <summary>
        /// 获取一个双精度浮点数据，不改变数据内容
        /// </summary>
        /// <returns></returns>
        public double GetDouble()
        {
            return GetDouble(readIndex);
        }

        /// <summary>
        /// 获取一个浮点数据，不改变数据内容
        /// </summary>
        /// <param name="index">字节索引</param>
        /// <returns></returns>
        public float GetFloat(int index)
        {
            return BitConverter.ToSingle(Get(index, 4), 0);
        }

        /// <summary>
        /// 获取一个浮点数据，不改变数据内容
        /// </summary>
        /// <returns></returns>
        public float GetFloat()
        {
            return GetFloat(readIndex);
        }

        /// <summary>
        /// 获取一个长整形数据，不改变数据内容
        /// </summary>
        /// <param name="index">字节索引</param>
        /// <returns></returns>
        public long GetLong(int index)
        {
            return BitConverter.ToInt64(Get(index, 8), 0);
        }

        /// <summary>
        /// 获取一个长整形数据，不改变数据内容
        /// </summary>
        /// <returns></returns>
        public long GetLong()
        {
            return GetLong(readIndex);
        }

        /// <summary>
        /// 获取一个长整形数据，不改变数据内容
        /// </summary>
        /// <param name="index">字节索引</param>
        /// <returns></returns>
        public ulong GetUlong(int index)
        {
            return BitConverter.ToUInt64(Get(index, 8), 0);
        }

        /// <summary>
        /// 获取一个长整形数据，不改变数据内容
        /// </summary>
        /// <returns></returns>
        public ulong GetUlong()
        {
            return GetUlong(readIndex);
        }

        /// <summary>
        /// 获取一个整形数据，不改变数据内容
        /// </summary>
        /// <param name="index">字节索引</param>
        /// <returns></returns>
        public int GetInt(int index)
        {
            return BitConverter.ToInt32(Get(index, 4), 0);
        }

        /// <summary>
        /// 获取一个整形数据，不改变数据内容
        /// </summary>
        /// <returns></returns>
        public int GetInt()
        {
            return GetInt(readIndex);
        }

        /// <summary>
        /// 获取一个整形数据，不改变数据内容
        /// </summary>
        /// <param name="index">字节索引</param>
        /// <returns></returns>
        public uint GetUint(int index)
        {
            return BitConverter.ToUInt32(Get(index, 4), 0);
        }

        /// <summary>
        /// 获取一个整形数据，不改变数据内容
        /// </summary>
        /// <returns></returns>
        public uint GetUint()
        {
            return GetUint(readIndex);
        }

        /// <summary>
        /// 获取一个短整形数据，不改变数据内容
        /// </summary>
        /// <param name="index">字节索引</param>
        /// <returns></returns>
        public int GetShort(int index)
        {
            return BitConverter.ToInt16(Get(index, 2), 0);
        }

        /// <summary>
        /// 获取一个短整形数据，不改变数据内容
        /// </summary>
        /// <returns></returns>
        public int GetShort()
        {
            return GetShort(readIndex);
        }

        /// <summary>
        /// 获取一个短整形数据，不改变数据内容
        /// </summary>
        /// <param name="index">字节索引</param>
        /// <returns></returns>
        public int GetUshort(int index)
        {
            return BitConverter.ToUInt16(Get(index, 2), 0);
        }

        /// <summary>
        /// 获取一个短整形数据，不改变数据内容
        /// </summary>
        /// <returns></returns>
        public int GetUshort()
        {
            return GetUshort(readIndex);
        }

        /// <summary>
        /// 获取一个char数据，不改变数据内容
        /// </summary>
        /// <param name="index">字节索引</param>
        /// <returns></returns>
        public char GetChar(int index)
        {
            return BitConverter.ToChar(Get(index, 2), 0);
        }

        /// <summary>
        /// 获取一个char数据，不改变数据内容
        /// </summary>
        /// <returns></returns>
        public char GetChar()
        {
            return GetChar(readIndex);
        }

        /// <summary>
        /// 获取一个布尔数据，不改变数据内容
        /// </summary>
        /// <param name="index">字节索引</param>
        /// <returns></returns>
        public bool GetBoolean(int index)
        {
            return BitConverter.ToBoolean(Get(index, 1), 0);
        }

        /// <summary>
        /// 获取一个布尔数据，不改变数据内容
        /// </summary>
        /// <returns></returns>
        public bool GetBoolean()
        {
            return GetBoolean(readIndex);
        }

        /// <summary>
        /// 清除已读字节并重建缓存区
        /// </summary>
        public void DiscardReadBytes()
        {
            if (readIndex <= 0) return;
            int len = buf.Length - readIndex;
            byte[] newbuf = new byte[len];
            Array.Copy(buf, readIndex, newbuf, 0, len);
            buf = newbuf;
            writeIndex -= readIndex;
            markReadIndex -= readIndex;
            if (markReadIndex < 0)
            {
                //markReadIndex = readIndex;
                markReadIndex = 0;
            }
            markWirteIndex -= readIndex;
            if (markWirteIndex < 0 || markWirteIndex < readIndex || markWirteIndex < markReadIndex)
            {
                markWirteIndex = writeIndex;
            }
            readIndex = 0;
        }

        /// <summary>
        /// 设置/获取读指针位置
        /// </summary>
        public int ReaderIndex
        {
            get
            {
                return readIndex;
            }
            set
            {
                if (value < 0) return;
                readIndex = value;
            }
        }

        /// <summary>
        /// 设置/获取写指针位置
        /// </summary>
        public int WriterIndex
        {
            get
            {
                return writeIndex;
            }
            set
            {
                if (value < 0) return;
                writeIndex = value;
            }
        }

        /// <summary>
        /// 标记读取的索引位置
        /// </summary>
        public void MarkReaderIndex()
        {
            markReadIndex = readIndex;
        }

        /// <summary>
        /// 标记写入的索引位置
        /// </summary>
        public void MarkWriterIndex()
        {
            markWirteIndex = writeIndex;
        }

        /// <summary>
        /// 将读取的索引位置重置为标记的读取索引位置
        /// </summary>
        public void ResetReaderIndex()
        {
            readIndex = markReadIndex;
        }

        /// <summary>
        /// 将写入的索引位置重置为标记的写入索引位置
        /// </summary>
        public void ResetWriterIndex()
        {
            writeIndex = markWirteIndex;
        }

        /// <summary>
        /// 可读的有效字节数
        /// </summary>
        /// <returns>可读的字节数</returns>
        public int ReadableBytes
        {
            get
            {
                return writeIndex - readIndex;
            }
        }

        /// <summary>
        /// 可写的剩余空间数
        /// </summary>
        /// <returns>可写的字节数</returns>
        public int WritableBytes
        {
            get
            {
                return capacity - writeIndex;
            }
        }

        /// <summary>
        /// 获取缓存区容量大小
        /// </summary>
        /// <returns>缓存区容量</returns>
        public int Capacity
        {
            get
            {
                return this.capacity;
            }
        }

        public byte[] RawBuffer
        {
            get
            {
                return buf;
            }
        }

        /// <summary>
        /// 获取可读的字节数组
        /// </summary>
        /// <returns>字节数据</returns>
        public byte[] ToArray()
        {
            byte[] bytes = new byte[writeIndex - readIndex];
            Array.Copy(buf, readIndex, bytes, 0, bytes.Length);
            return bytes;
        }

        /// <summary>
        /// 简单的数据类型
        /// </summary>
        public enum DataType
        {
            //byte类型
            BYTE = 1,
            //short类型
            SHORT = 2,
            //int类型
            INT = 3,
            //long类型
            LONG = 4
        }

        /// <summary>
        /// 写入一个数据
        /// </summary>
        /// <param name="value">待写入的数据</param>
        /// <param name="type">待写入的数据类型</param>
        private void WriteValue(int value, DataType type)
        {
            switch (type)
            {
                case DataType.BYTE:
                    this.WriteByte(value);
                    break;
                case DataType.SHORT:
                    this.WriteShort((short)value);
                    break;
                case DataType.LONG:
                    this.WriteLong((long)value);
                    break;
                default:
                    this.WriteInt(value);
                    break;
            }
        }

        /// <summary>
        /// 读取一个值，值类型根据type决定，int或short或byte
        /// </summary>
        /// <param name="type">值类型</param>
        /// <returns>int数据</returns>
        private int ReadValue(DataType type)
        {
            switch (type)
            {
                case DataType.BYTE:
                    return (int)ReadByte();
                case DataType.SHORT:
                    return (int)ReadShort();
                case DataType.INT:
                    return (int)ReadInt();
                case DataType.LONG:
                    return (int)ReadLong();
                default:
                    return -1;
            }
        }

        /// <summary>
        /// 写入可变长的UTF-8字符串
        /// <para>以长度类型（byte:1, short:2, int:3) + 长度（根据长度类型写入到字节缓冲区） + 字节数组表示一个字符串</para>
        /// </summary>
        /// <param name="content"></param>
        //public void WriteUTF8VarString(string content)
        //{
        //    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);
        //    ValueType lenType;
        //    if (bytes.Length <= byte.MaxValue)
        //    {
        //        lenType = ValueType.BYTE;
        //    }
        //    else if (bytes.Length <= short.MaxValue)
        //    {
        //        lenType = ValueType.SHORT;
        //    }
        //    else
        //    {
        //        lenType = ValueType.INT;
        //    }
        //    WriteByte((int)lenType);
        //    if (lenType == ValueType.BYTE)
        //    {
        //        WriteByte(bytes.Length);
        //    }
        //    else if (lenType == ValueType.SHORT)
        //    {
        //        WriteShort((short)bytes.Length);
        //    }
        //    else
        //    {
        //        WriteInt(bytes.Length);
        //    }
        //    WriteBytes(bytes);
        //}

        /// <summary>
        /// 读取可变长的UTF-8字符串
        /// <para>以长度类型（byte:1, short:2, int:3) + 长度（根据长度类型从字节缓冲区读取） + 字节数组表示一个字符串</para>
        /// </summary>
        /// <returns></returns>
        //public string ReadUTF8VarString()
        //{
        //    int lenTypeValue = ReadByte();
        //    int len = 0;
        //    if (lenTypeValue == (int)ValueType.BYTE)
        //    {
        //        len = ReadByte();
        //    }
        //    else if (lenTypeValue == (int)ValueType.SHORT)
        //    {
        //        len = ReadShort();
        //    }
        //    else if (lenTypeValue == (int)ValueType.INT)
        //    {
        //        len = ReadInt();
        //    }
        //    if (len > 0)
        //    {
        //        byte[] bytes = new byte[len];
        //        ReadBytes(bytes, 0, len);
        //        return System.Text.Encoding.UTF8.GetString(bytes);
        //    }
        //    return "";
        //}

        /// <summary>
        /// 写入一个UTF-8字符串，UTF-8字符串无高低字节序问题
        /// <para>写入缓冲区的结构为字符串字节长度（类型由lenType指定） + 字符串字节数组</para>
        /// </summary>
        /// <param name="content">待写入的字符串</param>
        /// <param name="lenType">写入的字符串长度类型</param>
        public void WriteUTF8String(string content, DataType lenType)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);
            int max;
            if (lenType == DataType.BYTE)
            {
                WriteByte(bytes.Length);
                max = byte.MaxValue;
            }
            else if (lenType == DataType.SHORT)
            {
                WriteShort((short)bytes.Length);
                max = short.MaxValue;
            }
            else
            {
                WriteInt(bytes.Length);
                max = int.MaxValue;
            }
            if (bytes.Length > max)
            {
                WriteBytes(bytes, 0, max);
            }
            else
            {
                WriteBytes(bytes, 0, bytes.Length);
            }
        }

        /// <summary>
        /// 写入以short表示的字符串字节长度和字符串字节数据
        /// </summary>
        /// <param name="content"></param>
        public void WriteUTF(string content)
        {
            this.WriteUTF8String(content, DataType.SHORT);
        }

        /// <summary>
        /// 读取一个UTF-8字符串，UTF-8字符串无高低字节序问题
        /// </summary>
        /// <param name="len">需读取的字符串长度</param>
        /// <returns>字符串</returns>
        public string ReadUTF8String(int len)
        {
            byte[] bytes = new byte[len];
            this.ReadBytes(bytes, 0, len);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// 读取一个UTF-8字符串，UTF-8字符串无高低字节序问题
        /// </summary>
        /// <param name="lenType">字符串长度类型</param>
        /// <returns>字符串</returns>
        public string ReadUTF8String(DataType lenType)
        {
            int len = ReadValue(lenType);
            return ReadUTF8String(len);
        }

        /// <summary>
        /// 读取short类型的字符串字节长度，然后根据此长度读取对应数量的字节数据后转为字符串
        /// </summary>
        /// <returns>UTF-8字符串</returns>
        public string ReadUTF()
        {
            return this.ReadUTF8String(DataType.SHORT);
        }

        /// <summary>
        /// 复制一个对象，具有与原对象相同的数据，不改变原对象的数据，不包括已读数据
        /// </summary>
        /// <returns></returns>
        public ByteBuffer Copy()
        {
            if (buf == null)
            {
                return new ByteBuffer(16);
            }
            if (readIndex < writeIndex)
            {
                byte[] newbytes = new byte[writeIndex - readIndex];
                Array.Copy(buf, readIndex, newbytes, 0, newbytes.Length);
                ByteBuffer buffer = new ByteBuffer(newbytes.Length);
                buffer.WriteBytes(newbytes);
                buffer.isPool = this.isPool;
                return buffer;
            }
            return new ByteBuffer(16);
        }

        /// <summary>
        /// 深度复制，具有与原对象相同的数据，不改变原对象的数据，包括已读数据
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            if (buf == null)
            {
                return new ByteBuffer(16);
            }
            ByteBuffer newBuf = new ByteBuffer(buf)
            {
                capacity = this.capacity,
                readIndex = this.readIndex,
                writeIndex = this.writeIndex,
                markReadIndex = this.markReadIndex,
                markWirteIndex = this.markWirteIndex,
                isPool = this.isPool
            };
            return newBuf;
        }

        /// <summary>
        /// 遍历所有的字节数据
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(Action<byte> action)
        {
            for (int i = 0; i < this.ReadableBytes; i++)
            {
                action.Invoke(this.buf[i]);
            }
        }

        /// <summary>
        /// 清空此对象，但保留字节缓存数组（空数组）
        /// </summary>
        public void Clear()
        {
            readIndex = 0;
            writeIndex = 0;
            markReadIndex = 0;
            markWirteIndex = 0;
            capacity = buf.Length;
        }

        /// <summary>
        /// 释放对象，清除字节缓存数组，如果此对象为可池化，那么调用此方法将会把此对象推入到池中等待下次调用
        /// </summary>
        public void Dispose()
        {
            if (isPool) {
                lock (pool) {
                    if (pool.Count < poolMaxCount) {
                        this.Clear();
                        pool.Add(this);
                        return;
                    }
                }
            }

            readIndex = 0;
            writeIndex = 0;
            markReadIndex = 0;
            markWirteIndex = 0;
            capacity = 0;
            buf = null;
        }
    }
}