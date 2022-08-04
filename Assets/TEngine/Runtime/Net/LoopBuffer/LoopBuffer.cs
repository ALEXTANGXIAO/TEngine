using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TEngine.Net
{
    public class LoopBuffer
    {
        private int _size;
        private int _head;
        private int _tail;
        private byte[] _data;

        public bool IsEmpty => (_data == null) || (_size == 0);

        public int Size => _size;

        public int Capacity => _data != null ? _data.Length : 0;

        public int Remain => Capacity - _size;

        public LoopBuffer() : this(0)
        {

        }

        public LoopBuffer(int capacity)
        {
            if (capacity < 0)
            {
                throw new System.ArgumentException("The buffer capacity must be greater than or equal to zero.");
            }

            _size = 0;
            _head = 0;
            _tail = 0;
            _data = new byte[capacity];
        }

        public void Reserve(long capacity)
        {
            if (capacity < 0)
            {
                throw new System.ArgumentException("Invalid reserve capacity!", nameof(capacity));
            }
            if (capacity > Capacity)
            {
                byte[] data = new byte[System.Math.Max(capacity, 2 * Capacity)];
                if (_size > 0)
                {
                    if (_head < _tail)
                    {
                        System.Buffer.BlockCopy(_data, _head, data, 0, _size);
                    }
                    else
                    {
                        System.Buffer.BlockCopy(_data, _head, data, 0, Capacity - _head);
                        System.Buffer.BlockCopy(_data, 0, data, Capacity - _head, _tail);
                    }

                }

                _head = 0;
                _tail = _size;
                _data = data;
            }
        }

        public void Clear()
        {
            _size = 0;
            _head = 0;
            _tail = 0;
        }

        public int Put(byte[] src)
        {
            return Put(src, 0, src.Length);
        }

        public int Put(byte[] src, int offset, int count)
        {
            if (count > Remain)
            {
                Reserve(_size + count);
            }

            if (count > 0)
            {
                lock (this)
                {
                    if (_head < _tail)
                    {
                        int right = Capacity - _tail;

                        if (right >= count)
                        {
                            System.Buffer.BlockCopy(src, offset, _data, _tail, count);
                        }
                        else
                        {
                            System.Buffer.BlockCopy(src, offset, _data, _tail, right);
                            System.Buffer.BlockCopy(src, offset + right, _data, 0, count - right);
                        }
                    }
                    else
                    {
                        System.Buffer.BlockCopy(src, offset, _data, _tail, count);
                    }

                    _tail = (_tail + count) % Capacity;
                    _size += count;
                }
            }

            return count;
        }

        public void Skip(int count)
        {
            _head += count;
            if (_head >= Capacity)
            {
                _head -= Capacity;
            }
        }

        public byte[] Get(int count)
        {
            var dst = new byte[count];
            Get(dst);
            return dst;
        }

        public int Get(byte[] dst)
        {
            return Get(dst, 0, dst.Length);
        }

        public int Get(Buffer buffer, int count)
        {
            int rc = Get(buffer.Data, buffer.Offset, count);
            buffer.AddSize(rc);

            return rc;
        }

        public int Get(byte[] dst, int offset, int count)
        {
            count = System.Math.Min(count, _size);

            if (count > 0)
            {
                lock (this)
                {
                    if (_head < _tail)
                    {
                        System.Buffer.BlockCopy(_data, _head, dst, offset, count);
                    }
                    else
                    {
                        int right = Capacity - _head;

                        if (right >= count)
                        {
                            System.Buffer.BlockCopy(_data, _head, dst, offset, count);
                        }
                        else
                        {
                            System.Buffer.BlockCopy(_data, _head, dst, offset, right);
                            System.Buffer.BlockCopy(_data, 0, dst, offset + right, count - right);
                        }
                    }

                    _head = (_head + count) % Capacity;
                    _size -= count;

                    if (_size == 0)
                    {
                        _head = 0;
                        _tail = 0;
                    }
                }
            }

            return count;
        }

        public bool Peek(byte[] dst, int offset, int count)
        {
            lock (this)
            {
                if (offset + count < _size)
                {
                    if (_head < _tail)
                    {
                        System.Buffer.BlockCopy(_data, _head, dst, offset, count);
                    }
                    else
                    {
                        int right = Capacity - _head;

                        if (right >= count)
                        {
                            System.Buffer.BlockCopy(_data, _head, dst, offset, count);
                        }
                        else
                        {
                            System.Buffer.BlockCopy(_data, _head, dst, offset, right);
                            System.Buffer.BlockCopy(_data, 0, dst, offset + right, count - right);
                        }
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
