namespace TEngine
{
    public static partial class Utility
    {
        /// <summary>
        /// 位运算相关的实用函数。
        /// </summary>
        public static class Bit
        {
            public static bool HasBit(long val, int idx)
            {
                return (val & 1L << idx) != 0L;
            }

            public static void SetBit(ref long val, int idx, bool isSet)
            {
                if (isSet)
                {
                    val |= (1L << idx);
                }
                else
                {
                    val &= ~(1L << idx);
                }
            }

            public static bool HasBit(int val, int idx)
            {
                return (val & 1 << idx) != 0;
            }


            public static void SetBit(ref int val, int idx, bool isSet)
            {
                if (isSet)
                {
                    val |= (1 << idx);
                }
                else
                {
                    val &= ~(1 << idx);
                }
            }

            public static bool HasBit(byte val, byte idx)
            {
                return (val & 1 << idx) != 0;
            }

            public static void SetBit(ref byte val, byte idx)
            {
                val |= (byte)(1 << idx);
            }

            public static byte ToByte(byte idx)
            {
                return (byte)(1 << idx);
            }
        }
    }
}