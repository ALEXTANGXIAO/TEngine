using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TEngine.DataStructure;

namespace TEngine.Core
{
    public static class RandomHelper
    {
        private static readonly Random Random = new Random();

        private static readonly byte[] Byte8 = new byte[8];
        private static readonly byte[] Byte2 = new byte[2];

        public static ulong RandUInt64()
        {
            Random.NextBytes(Byte8);
            return BitConverter.ToUInt64(Byte8, 0);
        }

        public static long RandInt64()
        {
            Random.NextBytes(Byte8);
            return BitConverter.ToInt64(Byte8, 0);
        }

        public static uint RandUInt32()
        {
            return (uint) Random.Next();
        }

        public static ushort RandUInt16()
        {
            Random.NextBytes(Byte2);
            return BitConverter.ToUInt16(Byte2, 0);
        }

        /// <summary>
        /// 获取lower与Upper之间的随机数,包含下限，不包含上限
        /// </summary>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        public static int RandomNumber(int lower, int upper)
        {
            return Random.Next(lower, upper);
        }

        public static bool RandomBool()
        {
            return Random.Next(2) == 0;
        }

        public static T RandomArray<T>(this T[] array)
        {
            return array[RandomNumber(0, array.Count() - 1)];
        }

        public static T RandomArray<T>(this List<T> array)
        {
            return array[RandomNumber(0, array.Count() - 1)];
        }

        /// <summary>
        /// 打乱数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr">要打乱的数组</param>
        public static void BreakRank<T>(List<T> arr)
        {
            if (arr == null || arr.Count < 2)
            {
                return;
            }

            for (var i = 0; i < arr.Count / 2; i++)
            {
                var index = Random.Next(0, arr.Count);
                (arr[index], arr[arr.Count - index - 1]) = (arr[arr.Count - index - 1], arr[index]);
            }
        }

        public static float RandFloat01()
        {
            var value = Random.NextDouble();
            return (float) value;
        }

        private static int Rand(int n)
        {
            var rd = new Random();
            // 注意，返回值是左闭右开，所以maxValue要加1
            return rd.Next(1, n + 1);
        }

        /// <summary>
        /// 通过权重随机
        /// </summary>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static int RandomByWeight(int[] weights)
        {
            var sum = weights.Sum();
            var numberRand = Rand(sum);
            var sumTemp = 0;
            for (var i = 0; i < weights.Length; i++)
            {
                sumTemp += weights[i];
                if (numberRand <= sumTemp)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 固定概率的随机、就是某数值上限里比出多少次
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int RandomByFixedProbability(int[] args)
        {
            var argCount = args.Length;
            var sum = args.Sum();
            var random = Random.NextDouble() * sum;
            while (sum > random)
            {
                sum -= args[argCount - 1];
                argCount--;
            }

            return argCount;
        }

        /// <summary>
        /// 返回随机数。
        /// </summary>
        /// <param name="containNegative">是否包含负数。</param>
        /// <returns>返回一个随机的单精度浮点数。</returns>
        public static float NextFloat(bool containNegative = false)
        {
            float f;
            var buffer = new byte[4];
            if (containNegative)
            {
                do
                {
                    Random.NextBytes(buffer);
                    f = BitConverter.ToSingle(buffer, 0);
                } while ((f >= float.MinValue && f < float.MaxValue) == false);

                return f;
            }

            do
            {
                Random.NextBytes(buffer);
                f = BitConverter.ToSingle(buffer, 0);
            } while ((f >= 0 && f < float.MaxValue) == false);

            return f;
        }

        /// <summary>
        /// 返回一个小于所指定最大值的非负随机数。
        /// </summary>
        /// <param name="maxValue">要生成的随机数的上限（随机数不能取该上限值）。 maxValue 必须大于或等于零。</param>
        /// <returns>大于等于零且小于 maxValue 的单精度浮点数，即：返回值的范围通常包括零但不包括 maxValue。 不过，如果 maxValue 等于零，则返回 maxValue。</returns>
        public static float NextFloat(float maxValue)
        {
            if (maxValue.Equals(0))
            {
                return maxValue;
            }

            if (maxValue < 0)
            {
                throw new ArgumentOutOfRangeException("“maxValue”必须大于 0。", "maxValue");
            }

            float f;
            var buffer = new byte[4];

            do
            {
                Random.NextBytes(buffer);
                f = BitConverter.ToSingle(buffer, 0);
            } while ((f >= 0 && f < maxValue) == false);

            return f;
        }

        /// <summary>
        /// 返回一个指定范围内的随机数。
        /// </summary>
        /// <param name="minValue">返回的随机数的下界（随机数可取该下界值）。</param>
        /// <param name="maxValue">返回的随机数的上界（随机数不能取该上界值）。 maxValue 必须大于或等于 minValue。</param>
        /// <returns>一个大于等于 minValue 且小于 maxValue 的单精度浮点数，即：返回的值范围包括 minValue 但不包括 maxValue。 如果 minValue 等于 maxValue，则返回 minValue。</returns>
        public static float NextFloat(float minValue, float maxValue)
        {
            if (minValue.Equals(maxValue))
            {
                return minValue;
            }

            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("“minValue”不能大于 maxValue。", "minValue");
            }

            float f;
            var buffer = new byte[4];

            do
            {
                Random.NextBytes(buffer);
                f = BitConverter.ToSingle(buffer, 0);
            } while ((f >= minValue && f < maxValue) == false);

            return f;
        }

        /// <summary>
        /// 在4个Vector2点范围内随机出一个位置
        /// </summary>
        /// <param name="minX">X轴最小值</param>
        /// <param name="maxX">X轴最大值</param>
        /// <param name="minY">Y轴最小值</param>
        /// <param name="maxY">Y轴最大值</param>
        /// <returns></returns>
        public static Vector2 NextVector2(float minX, float maxX, float minY, float maxY)
        {
            return new Vector2(NextFloat(minX, maxX), NextFloat(minY, maxY));
        }

        public static string RandomNumberCode(int len = 6)
        {
            int num = 0;
            for (int i = 0; i < len; i++)
            {
                int number = RandomNumber(0, 10);
                num = num * 10 + number;
            }

            return num.ToString();
        }
    }
}