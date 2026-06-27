using System;

namespace DGame.FixedPoint
{
    /// <summary>
    /// 基于 Mersenne Twister (MT19937) 算法的定点数随机数生成器。
    /// 该生成器是<b>确定性</b>的：在相同种子下，同一序列的随机调用必定产生完全一致的结果，
    /// 因此适用于帧同步等场景——只要各端使用相同的固定种子并保持相同的调用顺序，随机结果即可复现。
    /// 注意：请勿使用基于时间的无参构造（结果不可复现），帧同步务必显式传入固定种子。
    /// </summary>
    public class FixedPointRandom
    {
        private const int N = 624;
        private const int M = 397;
        private const uint MATRIX_A = 0x9908b0df; // 常量向量 a
        private const uint UPPER_MASK = 0x80000000; // 高位的 w-r 位（最高有效位）
        private const uint LOWER_MASK = 0x7fffffff; // 低位的 r 位（最低有效位）
        private const int MAX_RAND_INT = int.MaxValue;

        private readonly uint[] mag01 = new uint[2] { 0x0, MATRIX_A };
        private readonly uint[] mt = new uint[N];
        private int mti = N + 1; // mti==N+1 表示 mt[N] 尚未初始化

        /// <summary>最大随机整数值（等于 <see cref="int.MaxValue"/>）。</summary>
        public static int MaxRandomInt => int.MaxValue;

        /// <summary>
        /// 使用当前时间作为种子创建新的随机数生成器。
        /// 警告：结果不可复现，禁止用于帧同步。
        /// </summary>
        /// <returns>新建的随机数生成器实例。</returns>
        public static FixedPointRandom New() => new FixedPointRandom();

        /// <summary>
        /// 使用指定种子创建新的随机数生成器，相同种子可复现相同序列（推荐用于帧同步）。
        /// </summary>
        /// <param name="seed">随机种子。</param>
        /// <returns>新建的随机数生成器实例。</returns>
        public static FixedPointRandom New(int seed) => new FixedPointRandom(seed);

        /// <summary>
        /// 使用当前时间初始化随机数生成器。
        /// 警告：结果不可复现，禁止用于帧同步。
        /// </summary>
        public FixedPointRandom()
        {
            init_genrand((uint)DateTime.Now.Millisecond);
        }

        /// <summary>
        /// 使用指定种子初始化随机数生成器，相同种子可复现相同序列（推荐用于帧同步）。
        /// </summary>
        /// <param name="seed">随机种子。</param>
        public FixedPointRandom(int seed)
        {
            init_genrand((uint)seed);
        }

        /// <summary>
        /// 使用种子数组初始化随机数生成器，相同的种子数组可复现相同序列。
        /// </summary>
        /// <param name="init">用于初始化的种子数组。</param>
        public FixedPointRandom(int[] init)
        {
            uint[] array = new uint[init.Length];

            for (int i = 0; i < init.Length; i++)
            {
                array[i] = (uint)init[i];
            }

            init_by_array(array, (uint)array.Length);
        }

        /// <summary>
        /// 生成一个非负随机整数。
        /// </summary>
        /// <returns>范围为 [0, <see cref="int.MaxValue"/>] 的非负整数。</returns>
        public int Next()
        {
            return genrand_int31();
        }

        /// <summary>
        /// 生成指定范围内的随机整数（包含最小值，不包含最大值）。
        /// </summary>
        /// <param name="minValue">范围最小值（包含）。</param>
        /// <param name="maxValue">范围最大值（不包含）。</param>
        /// <returns>位于 [minValue, maxValue) 区间内的随机整数。</returns>
        public int Next(int minValue, int maxValue)
        {
            if (minValue == maxValue)
            {
                return minValue;
            }

            if (minValue > maxValue)
            {
                (maxValue, minValue) = (minValue, maxValue);
            }

            int range = maxValue - minValue;
            return minValue + Next() % range;
        }

        /// <summary>
        /// 生成指定范围内的定点数随机值。
        /// </summary>
        /// <param name="min">范围最小值。</param>
        /// <param name="max">范围最大值。</param>
        /// <returns>位于 [min, max] 区间内的随机定点数。</returns>
        public FixedPoint64 Next(FixedPoint64 min, FixedPoint64 max)
        {
            if (min == max)
            {
                return min;
            }

            if (min > max)
            {
                (max, min) = (min, max);
            }

            return NextFP() * (max - min) + min;
        }

        /// <summary>
        /// 生成 0 到 1 之间的定点数随机值。
        /// </summary>
        /// <returns>位于 [0, 1] 区间内的随机定点数。</returns>
        public FixedPoint64 Next01()
        {
            return Next(FixedPoint64.Zero, FixedPoint64.One);
        }

        /// <summary>
        /// 在指定半径的圆内（实为以原点为中心、边长为 2*radius 的范围）生成随机点。
        /// </summary>
        /// <param name="radius">半径。</param>
        /// <returns>随机生成的二维定点数向量。</returns>
        public FixedPointVector2 RandomInsideCircle(FixedPoint64 radius)
        {
            return new FixedPointVector2
            {
                x = Next(-radius, radius),
                y = Next(-radius, radius)
            };
        }

        /// <summary>
        /// 生成 0 到 1 之间的定点数随机值。
        /// </summary>
        /// <returns>位于 [0, 1] 区间内的随机定点数。</returns>
        public FixedPoint64 NextFP()
        {
            return (FixedPoint64)Next() / (FixedPoint64)MaxRandomInt;
        }

        /// <summary>
        /// 生成 0.0 到 1.0 之间的双精度浮点随机值。
        /// 警告：浮点结果在不同平台上可能存在差异，帧同步逻辑请改用定点数版本。
        /// </summary>
        /// <returns>位于 [0.0, 1.0] 区间内的双精度浮点随机值。</returns>
        public double NextDouble()
        {
            return genrand_real1();
        }

        /// <summary>
        /// 生成随机布尔值。
        /// </summary>
        /// <returns>随机的 true 或 false。</returns>
        public bool NextBool()
        {
            return (Next() & 1) == 0;
        }

        #region Mersenne Twister 核心实现

        private void init_genrand(uint s)
        {
            mt[0] = s & 0xffffffffU;

            for (mti = 1; mti < N; mti++)
            {
                mt[mti] = (uint)(1812433253 * (mt[mti - 1] ^ (mt[mti - 1] >> 30)) + mti);
                mt[mti] &= 0xffffffffU;
            }
        }

        private void init_by_array(uint[] init_key, uint key_length)
        {
            init_genrand(19650218U);
            int i = 1;
            int j = 0;
            uint k = (N > key_length) ? (uint)N : key_length;

            for (; k > 0; k--)
            {
                mt[i] = (uint)((mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1664525))
                               + init_key[j] + j);
                mt[i] &= 0xffffffffU;
                i++;
                j++;

                if (i >= N)
                {
                    mt[0] = mt[N - 1];
                    i = 1;
                }

                if (j >= key_length)
                {
                    j = 0;
                }
            }

            for (k = N - 1; k > 0; k--)
            {
                mt[i] = (uint)((mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1566083941)) - i);
                mt[i] &= 0xffffffffU;
                i++;

                if (i >= N)
                {
                    mt[0] = mt[N - 1];
                    i = 1;
                }
            }

            mt[0] = 0x80000000U;
        }

        private uint genrand_int32()
        {
            uint y;

            if (mti >= N)
            {
                if (mti == N + 1)
                {
                    init_genrand(5489U);
                }

                int kk;

                for (kk = 0; kk < N - M; kk++)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + M] ^ (y >> 1) ^ mag01[y & 0x1U];
                }

                for (; kk < N - 1; kk++)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + M - N] ^ (y >> 1) ^ mag01[y & 0x1U];
                }

                y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
                mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[y & 0x1U];
                mti = 0;
            }

            y = mt[mti++];
            y ^= y >> 11;
            y ^= (y << 7) & 0x9d2c5680U;
            y ^= (y << 15) & 0xefc60000U;
            y ^= y >> 18;
            return y;
        }

        private int genrand_int31()
        {
            return (int)(genrand_int32() >> 1);
        }

        private double genrand_real1()
        {
            return genrand_int32() * (1.0 / 4294967296.0);
        }

        private double genrand_real2()
        {
            return genrand_int32() * (1.0 / 4294967296.0);
        }

        private double genrand_real3()
        {
            return (((double)genrand_int32()) + 0.5) * (1.0 / 4294967296.0);
        }

        private double genrand_res53()
        {
            uint a = genrand_int32() >> 5;
            uint b = genrand_int32() >> 6;
            return ((double)a * 67108864.0 + (double)b) * (1.0 / 9007199254740992.0);
        }

        #endregion
    }
}
