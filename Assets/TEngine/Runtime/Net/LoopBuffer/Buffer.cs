namespace TEngine.Net
{
    public class Buffer
    {
        private int _size;
        private int _offset;
        private byte[] _data;

        public bool IsEmpty => _size == 0;

        public byte[] Data => _data;

        public int Capacity => _data.Length;

        public int Size => _size;

        public int Offset => _offset;

        public byte this[int index] => _data[index];

        public Buffer() : this(0)
        {

        }

        public Buffer(long capacity)
        {
            _size = 0;
            _offset = 0;
            _data = new byte[capacity];
        }

        public void Reserve(int capacity)
        {
            if (capacity < 0)
            {
                throw new System.ArgumentException("Invalid reserve capacity!", nameof(capacity));
            }

            if (capacity > Capacity)
            {
                byte[] data = new byte[System.Math.Max(capacity, 2 * Capacity)];
                System.Buffer.BlockCopy(_data, 0, data, 0, _size);
                _data = data;
            }
        }

        public void Clear()
        {
            _size = 0;
            _offset = 0;
        }

        public void Rewind() { _offset = 0; }

        public void Shift(int offset) { _offset += offset; }

        public void Unshift(int offset) { _offset -= offset; }

        public void AddSize(int size)
        {
            _size += size;
            _data[_size] = 0;
        }

        public int Append(byte[] src)
        {
            Reserve(_size + src.Length);
            System.Buffer.BlockCopy(src, 0, _data, _size, src.Length);
            _size += src.Length;
            return src.Length;
        }

        public int Append(byte src)
        {
            Reserve(_size + 1);
            _data[_size++] = src;

            return 1;
        }

        public int Append(byte[] src, int offset, int count)
        {
            Reserve(_size + count);
            System.Buffer.BlockCopy(src, offset, _data, _size, count);
            _size += count;
            return count;
        }

        public int Append(string text)
        {
            Reserve(_size + System.Text.Encoding.UTF8.GetMaxByteCount(text.Length));
            int result = System.Text.Encoding.UTF8.GetBytes(text, 0, text.Length, _data, (int)_size);
            _size += result;
            return result;
        }

        public string ExtractString(long offset, long size)
        {
            if ((offset + size) > Size)
                throw new System.ArgumentException("Invalid offset & size!", nameof(offset));

            return System.Text.Encoding.UTF8.GetString(_data, (int)offset, (int)size);
        }

        public override string ToString()
        {
            return ExtractString(0, _size);
        }
    }
}
