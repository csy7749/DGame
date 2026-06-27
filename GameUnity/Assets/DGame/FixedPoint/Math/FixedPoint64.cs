using System;
using System.IO;

#if UNITY_2021_3_OR_NEWER
using UnityEngine;
#endif

namespace DGame.FixedPoint
{
    [Serializable]
    public partial struct FixedPoint64 : IEquatable<FixedPoint64>, IComparable<FixedPoint64>
    {
        /// <summary>
        /// 定点数的底层原始整数值（Q31.32 格式，高 32 位为整数部分，低 32 位为小数部分）。
        /// </summary>
#if UNITY_2021_3_OR_NEWER
        [SerializeField]
#endif
        private long rawValue;

        /// <summary>
        /// 定点数底层原始值的最大值（等于 <see cref="long.MaxValue"/>）。
        /// </summary>
        public const long MAX_VALUE = long.MaxValue;

        /// <summary>
        /// 定点数底层原始值的最小值（等于 <see cref="long.MinValue"/>）。
        /// </summary>
        public const long MIN_VALUE = long.MinValue;

        /// <summary>
        /// 定点数底层存储的总位数（64 位）。
        /// </summary>
        public const int NUM_BITS = 64;

        /// <summary>
        /// 小数部分所占的位数（32 位），即 Q31.32 中的小数位数。
        /// </summary>
        public const int FRACTIONAL_PLACES = 32;

        /// <summary>
        /// 数值 1 对应的原始定点数表示（1 左移 32 位）。
        /// </summary>
        public const long ONE = 1L << FRACTIONAL_PLACES;

        /// <summary>
        /// 数值 10 对应的原始定点数表示。
        /// </summary>
        public const long TEN = 10L << FRACTIONAL_PLACES;

        /// <summary>
        /// 数值 0.5 对应的原始定点数表示。
        /// </summary>
        public const long HALF = 1L << (FRACTIONAL_PLACES - 1);

        /// <summary>
        /// 2π 对应的原始定点数表示。
        /// </summary>
        public const long PI_TIMES_2 = 0x6487ED511;

        /// <summary>
        /// π 对应的原始定点数表示。
        /// </summary>
        public const long PI = 0x3243F6A88;

        /// <summary>
        /// π/2 对应的原始定点数表示。
        /// </summary>
        public const long PI_OVER_2 = 0x1921FB544;

        /// <summary>
        /// 三角函数查找表（LUT）的条目数量，由 π/2 右移 15 位推导得出。
        /// </summary>
        public const int LUT_SIZE = (int)(PI_OVER_2 >> 15);

        // 该类型的精度为 2^-32，即 2,3283064365386962890625E-10
        /// <summary>
        /// 该定点数类型的精度，即最小可表示步长 2^-32（约 2.3283064365386962890625E-10）。
        /// </summary>
        public static readonly decimal
            Precision = (decimal)(new FixedPoint64(1L)); //0.00000000023283064365386962890625m;

        /// <summary>
        /// 可表示的最大定点数值（接近原始最大值，留出余量以避免溢出语义冲突）。
        /// </summary>
        public static readonly FixedPoint64 MaxValue = new FixedPoint64(MAX_VALUE - 1);

        /// <summary>
        /// 可表示的最小定点数值（接近原始最小值，留出余量以避免与 NaN 等特殊值冲突）。
        /// </summary>
        public static readonly FixedPoint64 MinValue = new FixedPoint64(MIN_VALUE + 2);

        /// <summary>
        /// 定点数 1。
        /// </summary>
        public static readonly FixedPoint64 One = new FixedPoint64(ONE);

        /// <summary>
        /// 定点数 10。
        /// </summary>
        public static readonly FixedPoint64 Ten = new FixedPoint64(TEN);

        /// <summary>
        /// 定点数 0.5。
        /// </summary>
        public static readonly FixedPoint64 Half = new FixedPoint64(HALF);

        /// <summary>
        /// 定点数 0。
        /// </summary>
        public static readonly FixedPoint64 Zero = new FixedPoint64();

        /// <summary>
        /// 正无穷大的定点数表示（以最大原始值表示）。
        /// </summary>
        public static readonly FixedPoint64 PositiveInfinity = new FixedPoint64(MAX_VALUE);

        /// <summary>
        /// 负无穷大的定点数表示。
        /// </summary>
        public static readonly FixedPoint64 NegativeInfinity = new FixedPoint64(MIN_VALUE + 1);

        /// <summary>
        /// 非数字（NaN）的定点数表示（以最小原始值表示）。
        /// </summary>
        public static readonly FixedPoint64 NaN = new FixedPoint64(MIN_VALUE);

        /// <summary>
        /// 定点数 0.1（10 的负 1 次方）。
        /// </summary>
        public static readonly FixedPoint64 EN1 = FixedPoint64.One / 10;

        /// <summary>
        /// 定点数 0.01（10 的负 2 次方）。
        /// </summary>
        public static readonly FixedPoint64 EN2 = FixedPoint64.One / 100;

        /// <summary>
        /// 定点数 0.001（10 的负 3 次方）。
        /// </summary>
        public static readonly FixedPoint64 EN3 = FixedPoint64.One / 1000;

        /// <summary>
        /// 定点数 0.0001（10 的负 4 次方）。
        /// </summary>
        public static readonly FixedPoint64 EN4 = FixedPoint64.One / 10000;

        /// <summary>
        /// 定点数 0.00001（10 的负 5 次方）。
        /// </summary>
        public static readonly FixedPoint64 EN5 = FixedPoint64.One / 100000;

        /// <summary>
        /// 定点数 0.000001（10 的负 6 次方）。
        /// </summary>
        public static readonly FixedPoint64 EN6 = FixedPoint64.One / 1000000;

        /// <summary>
        /// 定点数 0.0000001（10 的负 7 次方）。
        /// </summary>
        public static readonly FixedPoint64 EN7 = FixedPoint64.One / 10000000;

        /// <summary>
        /// 定点数 0.00000001（10 的负 8 次方）。
        /// </summary>
        public static readonly FixedPoint64 EN8 = FixedPoint64.One / 100000000;

        /// <summary>
        /// 用于近似相等比较的极小误差容差（取值为 <see cref="EN3"/>，即 0.001）。
        /// </summary>
        public static readonly FixedPoint64 Epsilon = FixedPoint64.EN3;

        /// <summary>
        /// 圆周率 π 的定点数值。
        /// </summary>
        public static readonly FixedPoint64 Pi = new FixedPoint64(PI);

        /// <summary>
        /// π/2 的定点数值。
        /// </summary>
        public static readonly FixedPoint64 PiOver2 = new FixedPoint64(PI_OVER_2);

        /// <summary>
        /// 2π 的定点数值。
        /// </summary>
        public static readonly FixedPoint64 PiTimes2 = new FixedPoint64(PI_TIMES_2);

        /// <summary>
        /// π 的倒数（1/π）的定点数值。
        /// </summary>
        public static readonly FixedPoint64 PiInv = (FixedPoint64)0.3183098861837906715377675267M;

        /// <summary>
        /// π/2 的倒数（2/π）的定点数值。
        /// </summary>
        public static readonly FixedPoint64 PiOver2Inv = (FixedPoint64)0.6366197723675813430755350535M;

        /// <summary>
        /// 角度转弧度的换算系数（π/180）。
        /// </summary>
        public static readonly FixedPoint64 Deg2Rad = Pi / new FixedPoint64(180);

        /// <summary>
        /// 弧度转角度的换算系数（180/π）。
        /// </summary>
        public static readonly FixedPoint64 Rad2Deg = new FixedPoint64(180) / Pi;

        /// <summary>
        /// 三角函数查找表的索引间隔系数，用于将角度映射到 LUT 索引。
        /// </summary>
        public static readonly FixedPoint64 LutInterval = (FixedPoint64)(LUT_SIZE - 1) / PiOver2;

        /// <summary>
        /// 返回指定定点数的符号。
        /// </summary>
        /// <param name="value">要判断符号的定点数。</param>
        /// <returns>负数返回 -1，正数返回 1，零返回 0。</returns>
        public static int Sign(FixedPoint64 value)
        {
            return
                value.rawValue < 0 ? -1 :
                value.rawValue > 0 ? 1 :
                0;
        }

        /// <summary>
        /// 返回指定定点数的绝对值。对最小值进行了饱和处理（返回 <see cref="MaxValue"/>）以避免溢出。
        /// </summary>
        /// <param name="value">输入定点数。</param>
        /// <returns>输入值的绝对值。</returns>
        public static FixedPoint64 Abs(FixedPoint64 value)
        {
            if (value.rawValue == MIN_VALUE)
            {
                return MaxValue;
            }

            // 无分支实现，参见 http://www.strchr.com/optimized_abs_function
            var mask = value.rawValue >> 63;
            return new FixedPoint64((value.rawValue + mask) ^ mask);
        }

        /// <summary>
        /// 返回定点数的绝对值（无分支快速实现）。
        /// 注意：FastAbs(FixedPoint64.MinValue) 的结果未定义。
        /// </summary>
        /// <param name="value">输入定点数。</param>
        /// <returns>输入值的绝对值。</returns>
        public static FixedPoint64 FastAbs(FixedPoint64 value)
        {
            // 无分支实现，参见 http://www.strchr.com/optimized_abs_function
            var mask = value.rawValue >> 63;
            return new FixedPoint64((value.rawValue + mask) ^ mask);
        }


        /// <summary>
        /// 返回小于或等于指定定点数的最大整数（向下取整）。
        /// </summary>
        /// <param name="value">输入定点数。</param>
        /// <returns>向下取整后的定点数。</returns>
        public static FixedPoint64 Floor(FixedPoint64 value)
        {
            // 直接将小数部分清零即可
            return new FixedPoint64((long)((ulong)value.rawValue & 0xFFFFFFFF00000000));
        }

        /// <summary>
        /// 返回大于或等于指定定点数的最小整数（向上取整）。
        /// </summary>
        /// <param name="value">输入定点数。</param>
        /// <returns>向上取整后的定点数。</returns>
        public static FixedPoint64 Ceiling(FixedPoint64 value)
        {
            var hasFractionalPart = (value.rawValue & 0x00000000FFFFFFFF) != 0;
            return hasFractionalPart ? Floor(value) + One : value;
        }

        /// <summary>
        /// 将定点数四舍五入到最接近的整数。
        /// 当数值恰好处于两个整数中间时，采用银行家舍入法（向最接近的偶数取整），与 System.Math.Round() 一致。
        /// </summary>
        /// <param name="value">输入定点数。</param>
        /// <returns>四舍五入后的定点数。</returns>
        public static FixedPoint64 Round(FixedPoint64 value)
        {
            var fractionalPart = value.rawValue & 0x00000000FFFFFFFF;
            var integralPart = Floor(value);

            if (fractionalPart < 0x80000000)
            {
                return integralPart;
            }

            if (fractionalPart > 0x80000000)
            {
                return integralPart + One;
            }

            // 当数值恰好处于两个值的中间时，向最接近的偶数取整
            // 这是 System.Math.Round() 所采用的方法。
            return (integralPart.rawValue & ONE) == 0
                ? integralPart
                : integralPart + One;
        }

        /// <summary>
        /// 定点数加法运算符。直接对底层原始值求和（不做溢出检查）。
        /// </summary>
        /// <param name="x">被加数。</param>
        /// <param name="y">加数。</param>
        /// <returns>两数之和。</returns>
        public static FixedPoint64 operator +(FixedPoint64 x, FixedPoint64 y)
        {
            return new FixedPoint64(x.rawValue + y.rawValue);
        }

        /// <summary>
        /// 带溢出检查的加法：发生溢出时根据操作数符号饱和到 <see cref="MaxValue"/> 或 <see cref="MinValue"/>。
        /// </summary>
        /// <param name="x">被加数。</param>
        /// <param name="y">加数。</param>
        /// <returns>饱和处理后的两数之和。</returns>
        public static FixedPoint64 OverflowAdd(FixedPoint64 x, FixedPoint64 y)
        {
            var xl = x.rawValue;
            var yl = y.rawValue;
            var sum = xl + yl;

            // 当两个操作数符号相同，而和与 x 的符号不同时（即发生溢出）
            if (((~(xl ^ yl) & (xl ^ sum)) & MIN_VALUE) != 0)
            {
                sum = xl > 0 ? MAX_VALUE : MIN_VALUE;
            }

            return new FixedPoint64(sum);
        }

        /// <summary>
        /// 不做溢出检查的快速加法，性能优先。
        /// </summary>
        /// <param name="x">被加数。</param>
        /// <param name="y">加数。</param>
        /// <returns>两数之和。</returns>
        public static FixedPoint64 FastAdd(FixedPoint64 x, FixedPoint64 y)
        {
            return new FixedPoint64(x.rawValue + y.rawValue);
        }

        /// <summary>
        /// 定点数减法运算符。直接对底层原始值求差（不做溢出检查）。
        /// </summary>
        /// <param name="x">被减数。</param>
        /// <param name="y">减数。</param>
        /// <returns>两数之差。</returns>
        public static FixedPoint64 operator -(FixedPoint64 x, FixedPoint64 y)
        {
            return new FixedPoint64(x.rawValue - y.rawValue);
        }

        /// <summary>
        /// 带溢出检查的减法：发生溢出时根据操作数符号饱和到 <see cref="MaxValue"/> 或 <see cref="MinValue"/>。
        /// </summary>
        /// <param name="x">被减数。</param>
        /// <param name="y">减数。</param>
        /// <returns>饱和处理后的两数之差。</returns>
        public static FixedPoint64 OverflowSub(FixedPoint64 x, FixedPoint64 y)
        {
            var xl = x.rawValue;
            var yl = y.rawValue;
            var diff = xl - yl;

            // 当两个操作数符号不同，而差与 x 的符号不同时（即发生溢出）
            if ((((xl ^ yl) & (xl ^ diff)) & MIN_VALUE) != 0)
            {
                diff = xl < 0 ? MIN_VALUE : MAX_VALUE;
            }

            return new FixedPoint64(diff);
        }

        /// <summary>
        /// 不做溢出检查的快速减法，性能优先。
        /// </summary>
        /// <param name="x">被减数。</param>
        /// <param name="y">减数。</param>
        /// <returns>两数之差。</returns>
        public static FixedPoint64 FastSub(FixedPoint64 x, FixedPoint64 y)
        {
            return new FixedPoint64(x.rawValue - y.rawValue);
        }

        /// <summary>
        /// 定点数乘法运算符。按 Q31.32 格式将高低 32 位拆分后相乘并累加，直接返回结果（不做溢出检查）。
        /// </summary>
        /// <param name="x">被乘数。</param>
        /// <param name="y">乘数。</param>
        /// <returns>两数之积。</returns>
        public static FixedPoint64 operator *(FixedPoint64 x, FixedPoint64 y)
        {
            var xl = x.rawValue;
            var yl = y.rawValue;

            var xlo = (ulong)(xl & 0x00000000FFFFFFFF);
            var xhi = xl >> FRACTIONAL_PLACES;
            var ylo = (ulong)(yl & 0x00000000FFFFFFFF);
            var yhi = yl >> FRACTIONAL_PLACES;

            var lolo = xlo * ylo;
            var lohi = (long)xlo * yhi;
            var hilo = xhi * (long)ylo;
            var hihi = xhi * yhi;

            var loResult = lolo >> FRACTIONAL_PLACES;
            var midResult1 = lohi;
            var midResult2 = hilo;
            var hiResult = hihi << FRACTIONAL_PLACES;

            var sum = (long)loResult + midResult1 + midResult2 + hiResult;
            FixedPoint64 result; // = default(FP);
            result.rawValue = sum;
            return result;
        }

        static long AddOverflowHelper(long x, long y, ref bool overflow)
        {
            var sum = x + y;
            // 当 sign(x) ^ sign(y) != sign(sum) 时，x + y 发生溢出
            overflow |= ((x ^ y ^ sum) & MIN_VALUE) != 0;
            return sum;
        }

        /// <summary>
        /// 带溢出检查的乘法：在 Q31.32 定点乘法过程中检测进位与符号异常，发生溢出时饱和到
        /// <see cref="MaxValue"/> 或 <see cref="MinValue"/>。
        /// </summary>
        /// <param name="x">被乘数。</param>
        /// <param name="y">乘数。</param>
        /// <returns>饱和处理后的两数之积。</returns>
        public static FixedPoint64 OverflowMul(FixedPoint64 x, FixedPoint64 y)
        {
            var xl = x.rawValue;
            var yl = y.rawValue;

            var xlo = (ulong)(xl & 0x00000000FFFFFFFF);
            var xhi = xl >> FRACTIONAL_PLACES;
            var ylo = (ulong)(yl & 0x00000000FFFFFFFF);
            var yhi = yl >> FRACTIONAL_PLACES;

            var lolo = xlo * ylo;
            var lohi = (long)xlo * yhi;
            var hilo = xhi * (long)ylo;
            var hihi = xhi * yhi;

            var loResult = lolo >> FRACTIONAL_PLACES;
            var midResult1 = lohi;
            var midResult2 = hilo;
            var hiResult = hihi << FRACTIONAL_PLACES;

            bool overflow = false;
            var sum = AddOverflowHelper((long)loResult, midResult1, ref overflow);
            sum = AddOverflowHelper(sum, midResult2, ref overflow);
            sum = AddOverflowHelper(sum, hiResult, ref overflow);

            bool opSignsEqual = ((xl ^ yl) & MIN_VALUE) == 0;

            // 若两个操作数符号相同，而结果符号为负，
            // 则说明乘法发生了正向溢出；
            // 反之同理
            if (opSignsEqual)
            {
                if (sum < 0 || (overflow && xl > 0))
                {
                    return MaxValue;
                }
            }
            else
            {
                if (sum > 0)
                {
                    return MinValue;
                }
            }

            // 若 hihi 的高 32 位（在结果中未被使用）既不全为 0 也不全为 1，
            // 则说明结果发生了溢出。
            var topCarry = hihi >> FRACTIONAL_PLACES;

            if (topCarry != 0 && topCarry != -1 /*&& xl != -17 && yl != -17*/)
            {
                return opSignsEqual ? MaxValue : MinValue;
            }

            // 若两数符号不同，且两个操作数的绝对值都大于 1，
            // 而结果大于那个负操作数，则发生了负向溢出。
            if (!opSignsEqual)
            {
                long posOp, negOp;

                if (xl > yl)
                {
                    posOp = xl;
                    negOp = yl;
                }
                else
                {
                    posOp = yl;
                    negOp = xl;
                }

                if (sum > negOp && negOp < -ONE && posOp > ONE)
                {
                    return MinValue;
                }
            }

            return new FixedPoint64(sum);
        }

        /// <summary>
        /// 不做溢出检查的快速乘法，性能优先，适用于已确保不会溢出的场景。
        /// </summary>
        /// <param name="x">被乘数。</param>
        /// <param name="y">乘数。</param>
        /// <returns>两数之积。</returns>
        public static FixedPoint64 FastMul(FixedPoint64 x, FixedPoint64 y)
        {
            var xl = x.rawValue;
            var yl = y.rawValue;

            var xlo = (ulong)(xl & 0x00000000FFFFFFFF);
            var xhi = xl >> FRACTIONAL_PLACES;
            var ylo = (ulong)(yl & 0x00000000FFFFFFFF);
            var yhi = yl >> FRACTIONAL_PLACES;

            var lolo = xlo * ylo;
            var lohi = (long)xlo * yhi;
            var hilo = xhi * (long)ylo;
            var hihi = xhi * yhi;

            var loResult = lolo >> FRACTIONAL_PLACES;
            var midResult1 = lohi;
            var midResult2 = hilo;
            var hiResult = hihi << FRACTIONAL_PLACES;

            var sum = (long)loResult + midResult1 + midResult2 + hiResult;
            FixedPoint64 result; // = default(FP);
            result.rawValue = sum;
            return result;
            //return new FP(sum);
        }

        //[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        /// 计算无符号 64 位整数中前导零的个数（最高有效位之前连续的 0 的数量）。
        /// </summary>
        /// <param name="x">待计算的无符号 64 位整数。</param>
        /// <returns>前导零的位数。</returns>
        public static int CountLeadingZeroes(ulong x)
        {
            int result = 0;

            while ((x & 0xF000000000000000) == 0)
            {
                result += 4;
                x <<= 4;
            }

            while ((x & 0x8000000000000000) == 0)
            {
                result += 1;
                x <<= 1;
            }

            return result;
        }

        /// <summary>
        /// 定点数除法运算符。采用逐位长除法实现，被零除时返回 <see cref="MAX_VALUE"/>，溢出时饱和到极值。
        /// </summary>
        /// <param name="x">被除数。</param>
        /// <param name="y">除数。</param>
        /// <returns>两数之商。</returns>
        public static FixedPoint64 operator /(FixedPoint64 x, FixedPoint64 y)
        {
            var xl = x.rawValue;
            var yl = y.rawValue;

            if (yl == 0)
            {
                return MAX_VALUE;
                //throw new DivideByZeroException();
            }

            var remainder = (ulong)(xl >= 0 ? xl : -xl);
            var divider = (ulong)(yl >= 0 ? yl : -yl);
            var quotient = 0UL;
            var bitPos = NUM_BITS / 2 + 1;


            // 若除数能被 2^n 整除，则利用这一点进行优化。
            while ((divider & 0xF) == 0 && bitPos >= 4)
            {
                divider >>= 4;
                bitPos -= 4;
            }

            while (remainder != 0 && bitPos >= 0)
            {
                int shift = CountLeadingZeroes(remainder);

                if (shift > bitPos)
                {
                    shift = bitPos;
                }

                remainder <<= shift;
                bitPos -= shift;

                var div = remainder / divider;
                remainder = remainder % divider;
                quotient += div << bitPos;

                // 检测溢出
                if ((div & ~(0xFFFFFFFFFFFFFFFF >> bitPos)) != 0)
                {
                    return ((xl ^ yl) & MIN_VALUE) == 0 ? MaxValue : MinValue;
                }

                remainder <<= 1;
                --bitPos;
            }

            // 舍入
            ++quotient;
            var result = (long)(quotient >> 1);

            if (((xl ^ yl) & MIN_VALUE) != 0)
            {
                result = -result;
            }

            return new FixedPoint64(result);
        }

        /// <summary>
        /// 定点数取模运算符。对 x % y 求余，并对 MinValue % -1 这一特殊情形返回 0 以避免溢出。
        /// </summary>
        /// <param name="x">被除数。</param>
        /// <param name="y">除数。</param>
        /// <returns>取模结果。</returns>
        public static FixedPoint64 operator %(FixedPoint64 x, FixedPoint64 y)
        {
            return new FixedPoint64(
                x.rawValue == MIN_VALUE & y.rawValue == -1
                    ? 0
                    : x.rawValue % y.rawValue);
        }

        /// <summary>
        /// 尽可能快地执行取模运算；当 x == MinValue 且 y == -1 时会抛出异常。
        /// 若需更可靠（但更慢）的取模，请使用取模运算符（%）。
        /// </summary>
        /// <param name="x">被除数。</param>
        /// <param name="y">除数。</param>
        /// <returns>取模结果。</returns>
        public static FixedPoint64 FastMod(FixedPoint64 x, FixedPoint64 y)
        {
            return new FixedPoint64(x.rawValue % y.rawValue);
        }

        /// <summary>
        /// 定点数取负运算符。对 <see cref="MinValue"/> 做饱和处理，返回 <see cref="MaxValue"/>。
        /// </summary>
        /// <param name="x">输入定点数。</param>
        /// <returns>取负后的定点数。</returns>
        public static FixedPoint64 operator -(FixedPoint64 x)
        {
            return x.rawValue == MIN_VALUE ? MaxValue : new FixedPoint64(-x.rawValue);
        }

        /// <summary>
        /// 判断两个定点数是否相等。
        /// </summary>
        /// <param name="x">左操作数。</param>
        /// <param name="y">右操作数。</param>
        /// <returns>相等返回 true，否则返回 false。</returns>
        public static bool operator ==(FixedPoint64 x, FixedPoint64 y)
        {
            return x.rawValue == y.rawValue;
        }

        /// <summary>
        /// 判断两个定点数是否不相等。
        /// </summary>
        /// <param name="x">左操作数。</param>
        /// <param name="y">右操作数。</param>
        /// <returns>不相等返回 true，否则返回 false。</returns>
        public static bool operator !=(FixedPoint64 x, FixedPoint64 y)
        {
            return x.rawValue != y.rawValue;
        }

        /// <summary>
        /// 判断 x 是否大于 y。
        /// </summary>
        /// <param name="x">左操作数。</param>
        /// <param name="y">右操作数。</param>
        /// <returns>x 大于 y 返回 true，否则返回 false。</returns>
        public static bool operator >(FixedPoint64 x, FixedPoint64 y)
        {
            return x.rawValue > y.rawValue;
        }

        /// <summary>
        /// 判断 x 是否小于 y。
        /// </summary>
        /// <param name="x">左操作数。</param>
        /// <param name="y">右操作数。</param>
        /// <returns>x 小于 y 返回 true，否则返回 false。</returns>
        public static bool operator <(FixedPoint64 x, FixedPoint64 y)
        {
            return x.rawValue < y.rawValue;
        }

        /// <summary>
        /// 判断 x 是否大于或等于 y。
        /// </summary>
        /// <param name="x">左操作数。</param>
        /// <param name="y">右操作数。</param>
        /// <returns>x 大于或等于 y 返回 true，否则返回 false。</returns>
        public static bool operator >=(FixedPoint64 x, FixedPoint64 y)
        {
            return x.rawValue >= y.rawValue;
        }

        /// <summary>
        /// 判断 x 是否小于或等于 y。
        /// </summary>
        /// <param name="x">左操作数。</param>
        /// <param name="y">右操作数。</param>
        /// <returns>x 小于或等于 y 返回 true，否则返回 false。</returns>
        public static bool operator <=(FixedPoint64 x, FixedPoint64 y)
        {
            return x.rawValue <= y.rawValue;
        }


        /// <summary>
        /// 返回指定定点数的平方根。
        /// </summary>
        /// <param name="x">输入定点数（必须为非负数）。</param>
        /// <returns>输入值的平方根。</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 当参数为负数时抛出。
        /// </exception>
        public static FixedPoint64 Sqrt(FixedPoint64 x)
        {
            var xl = x.rawValue;

            if (xl < 0)
            {
                // 我们无法像 Single 和 Double 那样表示无穷大，而且当 x < 0 时
                // Sqrt 在数学上是未定义的。因此这里直接抛出异常。
                throw new ArgumentOutOfRangeException("Negative value passed to Sqrt", "x");
            }

            var num = (ulong)xl;
            var result = 0UL;

            // 次高位
            var bit = 1UL << (NUM_BITS - 2);

            while (bit > num)
            {
                bit >>= 2;
            }

            // 主体部分执行两次，以避免在计算中使用 128 位数值。
            for (var i = 0; i < 2; ++i)
            {
                // 首先求出答案的高 48 位。
                while (bit != 0)
                {
                    if (num >= result + bit)
                    {
                        num -= result + bit;
                        result = (result >> 1) + bit;
                    }
                    else
                    {
                        result = result >> 1;
                    }

                    bit >>= 2;
                }

                if (i == 0)
                {
                    // 然后再处理一次，求出最低的 16 位。
                    if (num > (1UL << (NUM_BITS / 2)) - 1)
                    {
                        // 余数 'num' 过大，无法左移 32 位，
                        // 因此我们必须手动给 result 加 1，
                        // 并相应地调整 'num'。
                        // num = a - (result + 0.5)^2
                        //       = num + result^2 - (result + 0.5)^2
                        //       = num - result - 0.5
                        num -= result;
                        num = (num << (NUM_BITS / 2)) - 0x80000000UL;
                        result = (result << (NUM_BITS / 2)) + 0x80000000UL;
                    }
                    else
                    {
                        num <<= (NUM_BITS / 2);
                        result <<= (NUM_BITS / 2);
                    }

                    bit = 1UL << (NUM_BITS / 2 - 2);
                }
            }

            // 最后，如果下一位本应为 1，则将结果向上舍入。
            if (num > result)
            {
                ++result;
            }

            return new FixedPoint64((long)result);
        }

        /// <summary>
        /// 返回 x 的正弦值（x 为弧度）。
        /// 对较小的 x 约有 9 位小数精度；随着 x 增大精度可能下降。
        /// 通过查找表配合线性插值实现确定性计算。
        /// </summary>
        /// <param name="x">输入角度（弧度）。</param>
        /// <returns>x 的正弦值。</returns>
        public static FixedPoint64 Sin(FixedPoint64 x)
        {
            bool flipHorizontal, flipVertical;
            var clampedL = ClampSinValue(x.rawValue, out flipHorizontal, out flipVertical);
            var clamped = new FixedPoint64(clampedL);

            // 在查找表中找到最接近的两个值并执行线性插值
            // 这正是该函数在 x86 上性能低下的原因——不过在 x64 上表现良好
            var rawIndex = FastMul(clamped, LutInterval);
            var roundedIndex = Round(rawIndex);
            var indexError = 0; //FastSub(rawIndex, roundedIndex);

            var nearestValue =
                new FixedPoint64(SinLut[flipHorizontal ? SinLut.Length - 1 - (int)roundedIndex : (int)roundedIndex]);
            var secondNearestValue =
                new FixedPoint64(SinLut[
                    flipHorizontal
                        ? SinLut.Length - 1 - (int)roundedIndex - Sign(indexError)
                        : (int)roundedIndex + Sign(indexError)]);

            var delta = FastMul(indexError, FastAbs(FastSub(nearestValue, secondNearestValue))).rawValue;
            var interpolatedValue = nearestValue.rawValue + (flipHorizontal ? -delta : delta);
            var finalValue = flipVertical ? -interpolatedValue : interpolatedValue;
            FixedPoint64 a2 = new FixedPoint64(finalValue);
            return a2;
        }

        /// <summary>
        /// 返回 x 正弦值的快速粗略近似（x 为弧度）。
        /// 比 <see cref="Sin"/> 更快，但精度仅约 4-5 位小数（针对足够小的 x）。
        /// </summary>
        /// <param name="x">输入角度（弧度）。</param>
        /// <returns>x 的近似正弦值。</returns>
        public static FixedPoint64 FastSin(FixedPoint64 x)
        {
            bool flipHorizontal, flipVertical;
            var clampedL = ClampSinValue(x.rawValue, out flipHorizontal, out flipVertical);

            // 这里利用 SinLut 表的条目数恰好等于 (PI_OVER_2 >> 15) 这一事实，
            // 直接用角度索引到该表中
            var rawIndex = (uint)(clampedL >> 15);

            if (rawIndex >= LUT_SIZE)
            {
                rawIndex = LUT_SIZE - 1;
            }

            var nearestValue = SinLut[flipHorizontal ? SinLut.Length - 1 - (int)rawIndex : (int)rawIndex];
            return new FixedPoint64(flipVertical ? -nearestValue : nearestValue);
        }



        //[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        /// 将正弦角度的原始值规约到 [0, π/2) 区间，并输出水平/垂直镜像标志，供查找表取值时还原完整周期。
        /// </summary>
        /// <param name="angle">原始角度值（定点数底层表示）。</param>
        /// <param name="flipHorizontal">输出参数：是否需要水平镜像（角度落在 [π/2, π) 区间）。</param>
        /// <param name="flipVertical">输出参数：是否需要垂直镜像（角度落在 [π, 2π) 区间）。</param>
        /// <returns>规约到 [0, π/2) 后的角度原始值。</returns>
        public static long ClampSinValue(long angle, out bool flipHorizontal, out bool flipVertical)
        {
            // 使用取模将值规约到 0 - 2*PI；这非常慢，但据我所知没有更好的办法
            var clamped2Pi = angle % PI_TIMES_2;

            if (angle < 0)
            {
                clamped2Pi += PI_TIMES_2;
            }

            // 查找表只包含 0 - PiOver2 的值；其余所有值都必须通过
            // 垂直或水平镜像得到
            flipVertical = clamped2Pi >= PI;
            // 从 (angle % 2PI) 求出 (angle % PI)——比再做一次取模快得多
            var clampedPi = clamped2Pi;

            while (clampedPi >= PI)
            {
                clampedPi -= PI;
            }

            flipHorizontal = clampedPi >= PI_OVER_2;
            // 从 (angle % PI) 求出 (angle % PI_OVER_2)——比再做一次取模快得多
            var clampedPiOver2 = clampedPi;

            if (clampedPiOver2 >= PI_OVER_2)
            {
                clampedPiOver2 -= PI_OVER_2;
            }

            return clampedPiOver2;
        }

        /// <summary>
        /// 返回 x 的余弦值（x 为弧度）。实现细节参见 <see cref="Sin"/>。
        /// </summary>
        /// <param name="x">输入角度（弧度）。</param>
        /// <returns>x 的余弦值。</returns>
        public static FixedPoint64 Cos(FixedPoint64 x)
        {
            var xl = x.rawValue;
            var rawAngle = xl + (xl > 0 ? -PI - PI_OVER_2 : PI_OVER_2);
            FixedPoint64 a2 = Sin(new FixedPoint64(rawAngle));
            return a2;
        }

        /// <summary>
        /// 返回 x 余弦值的快速粗略近似（x 为弧度）。实现细节参见 <see cref="FastSin"/>。
        /// </summary>
        /// <param name="x">输入角度（弧度）。</param>
        /// <returns>x 的近似余弦值。</returns>
        public static FixedPoint64 FastCos(FixedPoint64 x)
        {
            var xl = x.rawValue;
            var rawAngle = xl + (xl > 0 ? -PI - PI_OVER_2 : PI_OVER_2);
            return FastSin(new FixedPoint64(rawAngle));
        }

        /// <summary>
        /// 返回 x 的正切值（x 为弧度）。
        /// </summary>
        /// <param name="x">输入角度（弧度）。</param>
        /// <returns>x 的正切值。</returns>
        /// <remarks>
        /// 此函数未经充分测试，可能存在较大误差。
        /// </remarks>
        public static FixedPoint64 Tan(FixedPoint64 x)
        {
            var clampedPi = x.rawValue % PI;
            var flip = false;

            if (clampedPi < 0)
            {
                clampedPi = -clampedPi;
                flip = true;
            }

            if (clampedPi > PI_OVER_2)
            {
                flip = !flip;
                clampedPi = PI_OVER_2 - (clampedPi - PI_OVER_2);
            }

            var clamped = new FixedPoint64(clampedPi);

            // 在查找表中找到最接近的两个值并执行线性插值
            var rawIndex = FastMul(clamped, LutInterval);
            var roundedIndex = Round(rawIndex);
            var indexError = FastSub(rawIndex, roundedIndex);

            var nearestValue = new FixedPoint64(TanLut[(int)roundedIndex]);
            var secondNearestValue = new FixedPoint64(TanLut[(int)roundedIndex + Sign(indexError)]);

            var delta = FastMul(indexError, FastAbs(FastSub(nearestValue, secondNearestValue))).rawValue;
            var interpolatedValue = nearestValue.rawValue + delta;
            var finalValue = flip ? -interpolatedValue : interpolatedValue;
            FixedPoint64 a2 = new FixedPoint64(finalValue);
            return a2;
        }

        /// <summary>
        /// 返回 y 的反正切值，即 atan(y)（等价于 Atan2(y, 1)）。
        /// </summary>
        /// <param name="y">输入值。</param>
        /// <returns>反正切值（弧度）。</returns>
        public static FixedPoint64 Atan(FixedPoint64 y)
        {
            return Atan2(y, 1);
        }

        /// <summary>
        /// 返回点 (x, y) 相对于原点的方位角 atan2(y, x)，结果范围为 (-π, π]。
        /// </summary>
        /// <param name="y">点的 y 坐标。</param>
        /// <param name="x">点的 x 坐标。</param>
        /// <returns>方位角（弧度）。</returns>
        public static FixedPoint64 Atan2(FixedPoint64 y, FixedPoint64 x)
        {
            var yl = y.rawValue;
            var xl = x.rawValue;

            if (xl == 0)
            {
                if (yl > 0)
                {
                    return PiOver2;
                }

                if (yl == 0)
                {
                    return Zero;
                }

                return -PiOver2;
            }

            FixedPoint64 atan;
            var z = y / x;

            FixedPoint64 sm = FixedPoint64.EN2 * 28;

            // 处理溢出
            if (One + sm * z * z == MaxValue)
            {
                return y < Zero ? -PiOver2 : PiOver2;
            }

            if (Abs(z) < One)
            {
                atan = z / (One + sm * z * z);

                if (xl < 0)
                {
                    if (yl < 0)
                    {
                        return atan - Pi;
                    }

                    return atan + Pi;
                }
            }
            else
            {
                atan = PiOver2 - z / (z * z + sm);

                if (yl < 0)
                {
                    return atan - Pi;
                }
            }

            return atan;
        }

        /// <summary>
        /// 返回 value 的反正弦值 asin(value)，结果范围为 [-π/2, π/2]。
        /// </summary>
        /// <param name="value">输入值（应在 [-1, 1] 范围内）。</param>
        /// <returns>反正弦值（弧度）。</returns>
        public static FixedPoint64 Asin(FixedPoint64 value)
        {
            return FastSub(PiOver2, Acos(value));
        }

        /// <summary>
        /// 返回 value 的反余弦值 acos(value)，结果范围为 [0, π]。
        /// 通过查找表配合线性插值实现确定性计算。
        /// </summary>
        /// <param name="value">输入值（应在 [-1, 1] 范围内）。</param>
        /// <returns>反余弦值（弧度）。</returns>
        public static FixedPoint64 Acos(FixedPoint64 value)
        {
            if (value == 0)
            {
                return FixedPoint64.PiOver2;
            }

            bool flip = false;

            if (value < 0)
            {
                value = -value;
                flip = true;
            }

            // 在查找表中找到最接近的两个值并执行线性插值
            var rawIndex = FastMul(value, LUT_SIZE);
            var roundedIndex = Round(rawIndex);

            if (roundedIndex >= LUT_SIZE)
            {
                roundedIndex = LUT_SIZE - 1;
            }

            var indexError = FastSub(rawIndex, roundedIndex);
            var nearestValue = new FixedPoint64(AcosLut[(int)roundedIndex]);

            var nextIndex = (int)roundedIndex + Sign(indexError);

            if (nextIndex >= LUT_SIZE)
            {
                nextIndex = LUT_SIZE - 1;
            }

            var secondNearestValue = new FixedPoint64(AcosLut[nextIndex]);

            var delta = FastMul(indexError, FastAbs(FastSub(nearestValue, secondNearestValue))).rawValue;
            FixedPoint64 interpolatedValue = new FixedPoint64(nearestValue.rawValue + delta);
            FixedPoint64 finalValue = flip ? (FixedPoint64.Pi - interpolatedValue) : interpolatedValue;

            return finalValue;
        }

        /// <summary>
        /// 将 64 位整数隐式转换为定点数。
        /// </summary>
        /// <param name="value">要转换的整数值。</param>
        /// <returns>对应的定点数。</returns>
        public static implicit operator FixedPoint64(long value)
        {
            return new FixedPoint64(value * ONE);
        }

        /// <summary>
        /// 将定点数显式转换为 64 位整数（向零截断小数部分）。
        /// </summary>
        /// <param name="value">要转换的定点数。</param>
        /// <returns>转换后的整数值。</returns>
        public static explicit operator long(FixedPoint64 value)
        {
            return value.rawValue >> FRACTIONAL_PLACES;
        }

        /// <summary>
        /// 将单精度浮点数隐式转换为定点数。
        /// </summary>
        /// <param name="value">要转换的浮点值。</param>
        /// <returns>对应的定点数。</returns>
        public static implicit operator FixedPoint64(float value)
        {
            return new FixedPoint64((long)(value * ONE));
        }

        /// <summary>
        /// 将定点数显式转换为单精度浮点数（可能损失精度）。
        /// </summary>
        /// <param name="value">要转换的定点数。</param>
        /// <returns>转换后的浮点值。</returns>
        public static explicit operator float(FixedPoint64 value)
        {
            return (float)value.rawValue / ONE;
        }

        /// <summary>
        /// 将双精度浮点数隐式转换为定点数。
        /// </summary>
        /// <param name="value">要转换的双精度浮点值。</param>
        /// <returns>对应的定点数。</returns>
        public static implicit operator FixedPoint64(double value)
        {
            return new FixedPoint64((long)(value * ONE));
        }

        /// <summary>
        /// 将定点数显式转换为双精度浮点数（可能损失精度）。
        /// </summary>
        /// <param name="value">要转换的定点数。</param>
        /// <returns>转换后的双精度浮点值。</returns>
        public static explicit operator double(FixedPoint64 value)
        {
            return (double)value.rawValue / ONE;
        }

        /// <summary>
        /// 将 decimal 数值显式转换为定点数。
        /// </summary>
        /// <param name="value">要转换的 decimal 值。</param>
        /// <returns>对应的定点数。</returns>
        public static explicit operator FixedPoint64(decimal value)
        {
            return new FixedPoint64((long)(value * ONE));
        }

        /// <summary>
        /// 将 32 位整数隐式转换为定点数。
        /// </summary>
        /// <param name="value">要转换的整数值。</param>
        /// <returns>对应的定点数。</returns>
        public static implicit operator FixedPoint64(int value)
        {
            return new FixedPoint64(value * ONE);
        }

        /// <summary>
        /// 将定点数显式转换为 decimal 数值。
        /// </summary>
        /// <param name="value">要转换的定点数。</param>
        /// <returns>转换后的 decimal 值。</returns>
        public static explicit operator decimal(FixedPoint64 value)
        {
            return (decimal)value.rawValue / ONE;
        }

        /// <summary>
        /// 将当前定点数转换为单精度浮点数。
        /// </summary>
        /// <returns>对应的单精度浮点值。</returns>
        public readonly float AsFloat()
        {
            return (float)this;
        }

        /// <summary>
        /// 将当前定点数转换为 32 位整数（向零截断小数部分）。
        /// </summary>
        /// <returns>对应的整数值。</returns>
        public int AsInt()
        {
            return (int)this;
        }

        /// <summary>
        /// 将当前定点数转换为 64 位整数（向零截断小数部分）。
        /// </summary>
        /// <returns>对应的 64 位整数值。</returns>
        public long AsLong()
        {
            return (long)this;
        }

        /// <summary>
        /// 将当前定点数转换为双精度浮点数。
        /// </summary>
        /// <returns>对应的双精度浮点值。</returns>
        public double AsDouble()
        {
            return (double)this;
        }

        /// <summary>
        /// 将当前定点数转换为 decimal 数值。
        /// </summary>
        /// <returns>对应的 decimal 值。</returns>
        public decimal AsDecimal()
        {
            return (decimal)this;
        }

        /// <summary>
        /// 将指定定点数转换为单精度浮点数。
        /// </summary>
        /// <param name="value">要转换的定点数。</param>
        /// <returns>对应的单精度浮点值。</returns>
        public static float ToFloat(FixedPoint64 value)
        {
            return (float)value;
        }

        /// <summary>
        /// 将指定定点数转换为 32 位整数（向零截断小数部分）。
        /// </summary>
        /// <param name="value">要转换的定点数。</param>
        /// <returns>对应的整数值。</returns>
        public static int ToInt(FixedPoint64 value)
        {
            return (int)value;
        }

        /// <summary>
        /// 从单精度浮点数创建定点数。
        /// </summary>
        /// <param name="value">输入的浮点值。</param>
        /// <returns>对应的定点数。</returns>
        public static FixedPoint64 FromFloat(float value)
        {
            return (FixedPoint64)value;
        }

        /// <summary>
        /// 判断指定定点数是否为正无穷或负无穷。
        /// </summary>
        /// <param name="value">要判断的定点数。</param>
        /// <returns>是无穷大返回 true，否则返回 false。</returns>
        public static bool IsInfinity(FixedPoint64 value)
        {
            return value == NegativeInfinity || value == PositiveInfinity;
        }

        /// <summary>
        /// 判断指定定点数是否为非数字（NaN）。
        /// </summary>
        /// <param name="value">要判断的定点数。</param>
        /// <returns>是 NaN 返回 true，否则返回 false。</returns>
        public static bool IsNaN(FixedPoint64 value)
        {
            return value == NaN;
        }

        /// <summary>
        /// 判断当前定点数是否与指定对象相等。
        /// </summary>
        /// <param name="obj">要比较的对象。</param>
        /// <returns>对象为 FixedPoint64 且底层原始值相等时返回 true，否则返回 false。</returns>
        public override bool Equals(object obj)
        {
            return obj is FixedPoint64 && ((FixedPoint64)obj).rawValue == rawValue;
        }

        /// <summary>
        /// 返回当前定点数的哈希码。
        /// </summary>
        /// <returns>基于底层原始值计算的哈希码。</returns>
        public override int GetHashCode()
        {
            return rawValue.GetHashCode();
        }

        /// <summary>
        /// 判断当前定点数是否与另一个定点数相等。
        /// </summary>
        /// <param name="other">要比较的定点数。</param>
        /// <returns>相等返回 true，否则返回 false。</returns>
        public bool Equals(FixedPoint64 other)
        {
            return rawValue == other.rawValue;
        }

        /// <summary>
        /// 将当前定点数与另一个定点数进行大小比较。
        /// </summary>
        /// <param name="other">要比较的定点数。</param>
        /// <returns>小于返回负数，等于返回 0，大于返回正数。</returns>
        public int CompareTo(FixedPoint64 other)
        {
            return rawValue.CompareTo(other.rawValue);
        }

        /// <summary>
        /// 返回当前定点数的字符串表示（以单精度浮点数格式输出）。
        /// </summary>
        /// <returns>定点数的字符串表示。</returns>
        public override string ToString()
        {
            return ((float)this).ToString();
        }

        /// <summary>
        /// 从底层原始整数值直接构造定点数。
        /// </summary>
        /// <param name="rawValue">Q31.32 格式的底层原始整数值。</param>
        /// <returns>对应的定点数。</returns>
        public static FixedPoint64 FromRaw(long rawValue)
        {
            return new FixedPoint64(rawValue);
        }

        internal static void GenerateAcosLut()
        {
            using (var writer = new StreamWriter("Fix64AcosLut.cs"))
            {
                writer.Write(
                    @"namespace TrueSync {
    partial struct FP {
        public static readonly long[] AcosLut = new[] {");
                int lineCounter = 0;

                for (int i = 0; i < LUT_SIZE; ++i)
                {
                    var angle = i / ((float)(LUT_SIZE - 1));

                    if (lineCounter++ % 8 == 0)
                    {
                        writer.WriteLine();
                        writer.Write("            ");
                    }

                    var acos = FixedPointMath.Acos(angle);
                    var rawValue = ((FixedPoint64)acos).rawValue;
                    writer.Write($"0x{rawValue:X}L, ");
                }

                writer.Write(
                    @"
        };
    }
}");
            }
        }

        internal static void GenerateSinLut()
        {
            using (var writer = new StreamWriter("Fix64SinLut.cs"))
            {
                writer.Write(
                    @"namespace FixMath.NET {
    partial struct Fix64 {
        public static readonly long[] SinLut = new[] {");
                int lineCounter = 0;

                for (int i = 0; i < LUT_SIZE; ++i)
                {
                    var angle = i * FixedPoint64.PI * 0.5 / (LUT_SIZE - 1);

                    if (lineCounter++ % 8 == 0)
                    {
                        writer.WriteLine();
                        writer.Write("            ");
                    }

                    var sin = FixedPointMath.Sin(angle);
                    var rawValue = ((FixedPoint64)sin).rawValue;
                    writer.Write($"0x{rawValue:X}L, ");
                }

                writer.Write(
                    @"
        };
    }
}");
            }
        }

        internal static void GenerateTanLut()
        {
            using (var writer = new StreamWriter("Fix64TanLut.cs"))
            {
                writer.Write(
                    @"namespace FixMath.NET {
    partial struct Fix64 {
        public static readonly long[] TanLut = new[] {");
                int lineCounter = 0;

                for (int i = 0; i < LUT_SIZE; ++i)
                {
                    var angle = i * FixedPoint64.PI * 0.5 / (LUT_SIZE - 1);

                    if (lineCounter++ % 8 == 0)
                    {
                        writer.WriteLine();
                        writer.Write("            ");
                    }

                    var tan = FixedPointMath.Tan(angle);

                    if (tan > (double)MaxValue || tan < 0.0)
                    {
                        tan = (double)MaxValue;
                    }

                    var rawValue = (((decimal)tan > (decimal)MaxValue || tan < 0.0) ? MaxValue : (FixedPoint64)tan)
                        .rawValue;
                    writer.Write($"0x{rawValue:X}L, ");
                }

                writer.Write(
                    @"
        };
    }
}");
            }
        }

        /// <summary>
        /// 底层整数表示（Q31.32 格式的原始值）。
        /// </summary>
        public long RawValue => rawValue;

        /// <summary>
        /// 从原始值构造的构造函数；仅供内部使用。
        /// </summary>
        /// <param name="rawValue">Q31.32 格式的底层原始整数值。</param>
        FixedPoint64(long rawValue)
        {
            this.rawValue = rawValue;
        }

        /// <summary>
        /// 从 32 位整数构造定点数。
        /// </summary>
        /// <param name="value">输入的整数值。</param>
        public FixedPoint64(int value)
        {
            rawValue = value * ONE;
        }
    }
}