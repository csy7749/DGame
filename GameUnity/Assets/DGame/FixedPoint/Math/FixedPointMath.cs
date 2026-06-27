using System.Runtime.CompilerServices;

namespace DGame.FixedPoint
{

    /// <summary>
    /// 定点数常用数学运算工具类（开方、三角函数、插值、取整等）。
    /// </summary>
    public sealed class FixedPointMath
    {

        /// <summary>
        /// 圆周率 PI 常量。
        /// </summary>
        public static FixedPoint64 Pi = FixedPoint64.Pi;

        /// <summary>
        /// 圆周率的一半（PI / 2）常量。
        /// </summary>
        public static FixedPoint64 PiOver2 = FixedPoint64.PiOver2;

        /// <summary>
        /// 一个极小值，常用于判断数值结果是否近似为零。
        /// </summary>
        public static FixedPoint64 Epsilon = FixedPoint64.Epsilon;

        /// <summary>
        /// 角度转弧度的换算常量。
        /// </summary>
        public static FixedPoint64 Deg2Rad = FixedPoint64.Deg2Rad;

        /// <summary>
        /// 弧度转角度的换算常量。
        /// </summary>
        public static FixedPoint64 Rad2Deg = FixedPoint64.Rad2Deg;

        #region public static FP Sqrt(FP number)

        /// <summary>
        /// 计算平方根。
        /// </summary>
        /// <param name="number">要计算平方根的数值。</param>
        /// <returns>返回该数值的平方根。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Sqrt(FixedPoint64 number)
        {
            return FixedPoint64.Sqrt(number);
        }

        #endregion

        #region public static FP Max(FP val1, FP val2)

        /// <summary>
        /// 取两个值中的较大值。
        /// </summary>
        /// <param name="val1">第一个值。</param>
        /// <param name="val2">第二个值。</param>
        /// <returns>返回较大的值。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Max(FixedPoint64 val1, FixedPoint64 val2)
        {
            return (val1 > val2) ? val1 : val2;
        }

        #endregion

        #region public static FP Min(FP val1, FP val2)

        /// <summary>
        /// 取两个值中的较小值。
        /// </summary>
        /// <param name="val1">第一个值。</param>
        /// <param name="val2">第二个值。</param>
        /// <returns>返回较小的值。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Min(FixedPoint64 val1, FixedPoint64 val2)
        {
            return (val1 < val2) ? val1 : val2;
        }

        #endregion

        #region public static FP Max(FP val1, FP val2,FP val3)

        /// <summary>
        /// 取三个值中的最大值。
        /// </summary>
        /// <param name="val1">第一个值。</param>
        /// <param name="val2">第二个值。</param>
        /// <param name="val3">第三个值。</param>
        /// <returns>返回最大的值。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Max(FixedPoint64 val1, FixedPoint64 val2, FixedPoint64 val3)
        {
            FixedPoint64 max12 = (val1 > val2) ? val1 : val2;
            return (max12 > val3) ? max12 : val3;
        }

        #endregion

        #region public static FP Clamp(FP value, FP min, FP max)

        /// <summary>
        /// 将值限制在 [min, max] 区间内。
        /// </summary>
        /// <param name="value">要限制的值。</param>
        /// <param name="min">区间最小值。</param>
        /// <param name="max">区间最大值。</param>
        /// <returns>限制后的值。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Clamp(FixedPoint64 value, FixedPoint64 min, FixedPoint64 max)
        {
            value = (value > max) ? max : value;
            value = (value < min) ? min : value;
            return value;
        }

        #endregion

        #region public static void Absolute(ref JMatrix matrix,out JMatrix result)

        /// <summary>
        /// 将矩阵每个元素的符号取为正（取各元素的绝对值）。
        /// </summary>
        /// <param name="matrix">输入矩阵。</param>
        /// <param name="result">输出的绝对值矩阵。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Absolute(ref FixedPointMatrix matrix, out FixedPointMatrix result)
        {
            result.M11 = FixedPoint64.Abs(matrix.M11);
            result.M12 = FixedPoint64.Abs(matrix.M12);
            result.M13 = FixedPoint64.Abs(matrix.M13);
            result.M21 = FixedPoint64.Abs(matrix.M21);
            result.M22 = FixedPoint64.Abs(matrix.M22);
            result.M23 = FixedPoint64.Abs(matrix.M23);
            result.M31 = FixedPoint64.Abs(matrix.M31);
            result.M32 = FixedPoint64.Abs(matrix.M32);
            result.M33 = FixedPoint64.Abs(matrix.M33);
        }

        #endregion

        /// <summary>
        /// 返回指定值的正弦值。
        /// </summary>
        /// <param name="value">输入的弧度值。</param>
        /// <returns>正弦值。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Sin(FixedPoint64 value)
        {
            return FixedPoint64.Sin(value);
        }

        /// <summary>
        /// 返回指定值的余弦值。
        /// </summary>
        /// <param name="value">输入的弧度值。</param>
        /// <returns>余弦值。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Cos(FixedPoint64 value)
        {
            return FixedPoint64.Cos(value);
        }

        /// <summary>
        /// 返回指定值的正切值。
        /// </summary>
        /// <param name="value">输入的弧度值。</param>
        /// <returns>正切值。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Tan(FixedPoint64 value)
        {
            return FixedPoint64.Tan(value);
        }

        /// <summary>
        /// 返回指定值的反正弦值。
        /// </summary>
        /// <param name="value">输入值，取值范围 [-1, 1]。</param>
        /// <returns>反正弦值（弧度）。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Asin(FixedPoint64 value)
        {
            return FixedPoint64.Asin(value);
        }

        /// <summary>
        /// 返回指定值的反余弦值。
        /// </summary>
        /// <param name="value">输入值，取值范围 [-1, 1]。</param>
        /// <returns>反余弦值（弧度）。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Acos(FixedPoint64 value)
        {
            return FixedPoint64.Acos(value);
        }

        /// <summary>
        /// 返回指定值的反正切值。
        /// </summary>
        /// <param name="value">输入值。</param>
        /// <returns>反正切值（弧度）。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Atan(FixedPoint64 value)
        {
            return FixedPoint64.Atan(value);
        }

        /// <summary>
        /// 根据坐标 (x, y) 返回反正切值，可区分象限。
        /// </summary>
        /// <param name="y">点的 y 坐标。</param>
        /// <param name="x">点的 x 坐标。</param>
        /// <returns>该点相对于原点的方位角（弧度）。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Atan2(FixedPoint64 y, FixedPoint64 x)
        {
            return FixedPoint64.Atan2(y, x);
        }

        /// <summary>
        /// 返回小于或等于指定数值的最大整数（向下取整）。
        /// </summary>
        /// <param name="value">输入值。</param>
        /// <returns>向下取整后的值。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Floor(FixedPoint64 value)
        {
            return FixedPoint64.Floor(value);
        }

        /// <summary>
        /// 返回大于或等于指定数值的最小整数（向上取整）。
        /// </summary>
        /// <param name="value">输入值。</param>
        /// <returns>向上取整后的值。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Ceiling(FixedPoint64 value)
        {
            return value;
        }

        /// <summary>
        /// 将数值四舍五入到最接近的整数。
        /// 若数值恰好处于两个整数中间，则返回偶数（银行家舍入）。
        /// </summary>
        /// <param name="value">输入值。</param>
        /// <returns>取整后的值。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Round(FixedPoint64 value)
        {
            return FixedPoint64.Round(value);
        }

        /// <summary>
        /// 返回表示定点数符号的整数：正数返回 1，零返回 0，负数返回 -1。
        /// </summary>
        /// <param name="value">输入值。</param>
        /// <returns>符号值（1、0 或 -1）。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sign(FixedPoint64 value)
        {
            return FixedPoint64.Sign(value);
        }

        /// <summary>
        /// 返回定点数的绝对值。
        /// 注意：Abs(FixedPoint64.MinValue) == FixedPoint64.MaxValue。
        /// </summary>
        /// <param name="value">输入值。</param>
        /// <returns>绝对值。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Abs(FixedPoint64 value)
        {
            return FixedPoint64.Abs(value);
        }

        /// <summary>
        /// 计算三角形的重心坐标插值（用于在三个顶点值之间按权重插值）。
        /// </summary>
        /// <param name="value1">第一个顶点的值。</param>
        /// <param name="value2">第二个顶点的值。</param>
        /// <param name="value3">第三个顶点的值。</param>
        /// <param name="amount1">第二个顶点的权重系数。</param>
        /// <param name="amount2">第三个顶点的权重系数。</param>
        /// <returns>重心坐标插值结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Barycentric(FixedPoint64 value1, FixedPoint64 value2, FixedPoint64 value3,
            FixedPoint64 amount1, FixedPoint64 amount2)
        {
            return value1 + (value2 - value1) * amount1 + (value3 - value1) * amount2;
        }

        /// <summary>
        /// 使用 Catmull-Rom 样条对四个控制值进行插值。
        /// </summary>
        /// <param name="value1">第一个控制值。</param>
        /// <param name="value2">第二个控制值（插值起点）。</param>
        /// <param name="value3">第三个控制值（插值终点）。</param>
        /// <param name="value4">第四个控制值。</param>
        /// <param name="amount">插值权重，取值范围 [0, 1]。</param>
        /// <returns>样条插值结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 CatmullRom(FixedPoint64 value1, FixedPoint64 value2, FixedPoint64 value3,
            FixedPoint64 value4, FixedPoint64 amount)
        {
            // 采用公式来源：http://www.mvps.org/directx/articles/catmull/
            // 内部使用定点数（FP）运算以避免精度损失
            FixedPoint64 amountSquared = amount * amount;
            FixedPoint64 amountCubed = amountSquared * amount;
            return (FixedPoint64)(0.5 * (2.0 * value2 +
                                         (value3 - value1) * amount +
                                         (2.0 * value1 - 5.0 * value2 + 4.0 * value3 - value4) * amountSquared +
                                         (3.0 * value2 - value1 - 3.0 * value3 + value4) * amountCubed));
        }

        /// <summary>
        /// 计算两个值之间的距离（差值的绝对值）。
        /// </summary>
        /// <param name="value1">第一个值。</param>
        /// <param name="value2">第二个值。</param>
        /// <returns>两值之间的距离。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Distance(FixedPoint64 value1, FixedPoint64 value2)
        {
            return FixedPoint64.Abs(value1 - value2);
        }

        /// <summary>
        /// 使用 Hermite 样条进行插值（依据起止值及其切线）。
        /// </summary>
        /// <param name="value1">起点值。</param>
        /// <param name="tangent1">起点处的切线。</param>
        /// <param name="value2">终点值。</param>
        /// <param name="tangent2">终点处的切线。</param>
        /// <param name="amount">插值权重，取值范围 [0, 1]。</param>
        /// <returns>Hermite 插值结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Hermite(FixedPoint64 value1, FixedPoint64 tangent1, FixedPoint64 value2,
            FixedPoint64 tangent2, FixedPoint64 amount)
        {
            // 全部转换为定点数（FP）运算以避免精度损失
            // 否则当参数 amount 取较大数值时，结果会变成 NaN 而非 Infinity
            FixedPoint64 v1 = value1, v2 = value2, t1 = tangent1, t2 = tangent2, s = amount, result;
            FixedPoint64 sCubed = s * s * s;
            FixedPoint64 sSquared = s * s;

            if (amount == 0f)
                result = value1;
            else if (amount == 1f)
                result = value2;
            else
                result = (2 * v1 - 2 * v2 + t2 + t1) * sCubed +
                         (3 * v2 - 3 * v1 - 2 * t1 - t2) * sSquared +
                         t1 * s +
                         v1;
            return (FixedPoint64)result;
        }

        /// <summary>
        /// 在两个值之间进行线性插值。
        /// </summary>
        /// <param name="value1">起始值。</param>
        /// <param name="value2">结束值。</param>
        /// <param name="amount">插值权重，取值范围 [0, 1]。</param>
        /// <returns>线性插值结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Lerp(FixedPoint64 value1, FixedPoint64 value2, FixedPoint64 amount)
        {
            return value1 + (value2 - value1) * amount;
        }

        /// <summary>
        /// 线性插值的逆运算：计算 amount 在 [value1, value2] 区间内所处的归一化比例。
        /// </summary>
        /// <param name="value1">区间起始值。</param>
        /// <param name="value2">区间结束值。</param>
        /// <param name="amount">区间内的某个值。</param>
        /// <returns>amount 对应的插值比例。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 InverseLerp(FixedPoint64 value1, FixedPoint64 value2, FixedPoint64 amount)
        {
            return (amount - value1) / (value2 - value1);
        }

        /// <summary>
        /// 在两个值之间进行平滑（缓入缓出）插值。
        /// 期望 amount 处于 0 与 1 之间；amount 小于 0 时返回 value1，大于 1 时返回 value2。
        /// </summary>
        /// <param name="value1">起始值。</param>
        /// <param name="value2">结束值。</param>
        /// <param name="amount">插值权重，会被限制到 [0, 1]。</param>
        /// <returns>平滑插值结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 SmoothStep(FixedPoint64 value1, FixedPoint64 value2, FixedPoint64 amount)
        {
            // 期望 amount 处于 0 与 1 之间
            // 若 amount 小于 0，返回 value1
            // 若 amount 大于 1，返回 value2
            FixedPoint64 result = Clamp(amount, 0f, 1f);
            result = Hermite(value1, 0f, value2, 0f, result);
            return result;
        }
    }
}
