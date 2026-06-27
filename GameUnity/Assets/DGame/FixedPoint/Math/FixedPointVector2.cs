using System;
using System.Runtime.CompilerServices;

#if UNITY_2021_3_OR_NEWER
using UnityEngine;
#endif

namespace DGame.FixedPoint
{
    /// <summary>
    /// 定点数二维向量，元素类型为 <see cref="FixedPoint64"/>，用于帧同步的确定性数学计算。
    /// </summary>
    [Serializable]
    public struct FixedPointVector2 : IEquatable<FixedPointVector2>
    {
        #region 私有字段

        private static FixedPointVector2 zeroVector = new FixedPointVector2(0, 0);
        private static FixedPointVector2 oneVector = new FixedPointVector2(1, 1);

        private static FixedPointVector2 rightVector = new FixedPointVector2(1, 0);
        private static FixedPointVector2 leftVector = new FixedPointVector2(-1, 0);

        private static FixedPointVector2 upVector = new FixedPointVector2(0, 1);
        private static FixedPointVector2 downVector = new FixedPointVector2(0, -1);

        #endregion 私有字段

        #region 公有字段

        /// <summary>
        /// 向量的 X 分量。
        /// </summary>
        public FixedPoint64 x;

        /// <summary>
        /// 向量的 Y 分量。
        /// </summary>
        public FixedPoint64 y;

        #endregion 公有字段

        #region 属性

        /// <summary>
        /// 零向量，分量为 (0, 0)。
        /// </summary>
        public static FixedPointVector2 zero
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return zeroVector; }
        }

        /// <summary>
        /// 单位向量，分量为 (1, 1)。
        /// </summary>
        public static FixedPointVector2 one
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return oneVector; }
        }

        /// <summary>
        /// 右方向向量，分量为 (1, 0)。
        /// </summary>
        public static FixedPointVector2 right
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return rightVector; }
        }

        /// <summary>
        /// 左方向向量，分量为 (-1, 0)。
        /// </summary>
        public static FixedPointVector2 left
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return leftVector; }
        }

        /// <summary>
        /// 上方向向量，分量为 (0, 1)。
        /// </summary>
        public static FixedPointVector2 up
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return upVector; }
        }

        /// <summary>
        /// 下方向向量，分量为 (0, -1)。
        /// </summary>
        public static FixedPointVector2 down
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return downVector; }
        }

        #endregion 属性

        #region 构造函数

        /// <summary>
        /// 标准二维向量构造函数。
        /// </summary>
        /// <param name="x">X 分量。</param>
        /// <param name="y">Y 分量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedPointVector2(FixedPoint64 x, FixedPoint64 y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// 各分量相同的向量构造函数（X 与 Y 都设为同一个值）。
        /// </summary>
        /// <param name="value">同时赋给 X 与 Y 的值。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedPointVector2(FixedPoint64 value)
        {
            x = value;
            y = value;
        }

        /// <summary>
        /// 设置向量的各个分量。
        /// </summary>
        /// <param name="x">X 分量。</param>
        /// <param name="y">Y 分量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(FixedPoint64 x, FixedPoint64 y)
        {
            this.x = x;
            this.y = y;
        }

        #endregion 构造函数

        #region 公有方法

        /// <summary>
        /// 计算向量基于给定法线的反射向量（结果通过 out 参数返回）。
        /// </summary>
        /// <param name="vector">入射向量。</param>
        /// <param name="normal">反射面的法线向量。</param>
        /// <param name="result">反射后的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Reflect(ref FixedPointVector2 vector, ref FixedPointVector2 normal,
            out FixedPointVector2 result)
        {
            FixedPoint64 dot = Dot(vector, normal);
            result.x = vector.x - ((2f * dot) * normal.x);
            result.y = vector.y - ((2f * dot) * normal.y);
        }

        /// <summary>
        /// 计算向量基于给定法线的反射向量。
        /// </summary>
        /// <param name="vector">入射向量。</param>
        /// <param name="normal">反射面的法线向量。</param>
        /// <returns>反射后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 Reflect(FixedPointVector2 vector, FixedPointVector2 normal)
        {
            FixedPointVector2 result;
            Reflect(ref vector, ref normal, out result);
            return result;
        }

        /// <summary>
        /// 将一个向量投影到另一个向量上。
        /// </summary>
        /// <param name="vector">被投影的向量。</param>
        /// <param name="onNormal">投影目标方向向量。</param>
        /// <returns>投影后的向量；若目标方向接近零向量则返回零向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 Project(FixedPointVector2 vector, FixedPointVector2 onNormal)
        {
            var sqrMag = Dot(onNormal, onNormal);

            if (sqrMag < FixedPointMath.Epsilon)
                return zero;
            else
            {
                var dot = Dot(vector, onNormal);
                return new FixedPointVector2(onNormal.x * dot / sqrMag,
                    onNormal.y * dot / sqrMag);
            }
        }

        /// <summary>
        /// 将两个向量逐分量相加。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <returns>两向量之和。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 Add(FixedPointVector2 value1, FixedPointVector2 value2)
        {
            value1.x += value2.x;
            value1.y += value2.y;
            return value1;
        }

        /// <summary>
        /// 将两个向量逐分量相加（结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <param name="result">两向量之和。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(ref FixedPointVector2 value1, ref FixedPointVector2 value2, out FixedPointVector2 result)
        {
            result.x = value1.x + value2.x;
            result.y = value1.y + value2.y;
        }

        /// <summary>
        /// 使用重心坐标在三角形三个顶点之间进行插值。
        /// </summary>
        /// <param name="value1">三角形第一个顶点。</param>
        /// <param name="value2">三角形第二个顶点。</param>
        /// <param name="value3">三角形第三个顶点。</param>
        /// <param name="amount1">对应第二个顶点的重心权重。</param>
        /// <param name="amount2">对应第三个顶点的重心权重。</param>
        /// <returns>插值得到的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 Barycentric(FixedPointVector2 value1, FixedPointVector2 value2,
            FixedPointVector2 value3, FixedPoint64 amount1, FixedPoint64 amount2)
        {
            return new FixedPointVector2(
                FixedPointMath.Barycentric(value1.x, value2.x, value3.x, amount1, amount2),
                FixedPointMath.Barycentric(value1.y, value2.y, value3.y, amount1, amount2));
        }

        /// <summary>
        /// 使用重心坐标在三角形三个顶点之间进行插值（结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value1">三角形第一个顶点。</param>
        /// <param name="value2">三角形第二个顶点。</param>
        /// <param name="value3">三角形第三个顶点。</param>
        /// <param name="amount1">对应第二个顶点的重心权重。</param>
        /// <param name="amount2">对应第三个顶点的重心权重。</param>
        /// <param name="result">插值得到的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Barycentric(ref FixedPointVector2 value1, ref FixedPointVector2 value2,
            ref FixedPointVector2 value3, FixedPoint64 amount1,
            FixedPoint64 amount2, out FixedPointVector2 result)
        {
            result = new FixedPointVector2(
                FixedPointMath.Barycentric(value1.x, value2.x, value3.x, amount1, amount2),
                FixedPointMath.Barycentric(value1.y, value2.y, value3.y, amount1, amount2));
        }

        /// <summary>
        /// 对四个控制点执行 Catmull-Rom 样条插值。
        /// </summary>
        /// <param name="value1">第一个控制点。</param>
        /// <param name="value2">第二个控制点（插值起点）。</param>
        /// <param name="value3">第三个控制点（插值终点）。</param>
        /// <param name="value4">第四个控制点。</param>
        /// <param name="amount">插值权重（通常取值 0 到 1）。</param>
        /// <returns>插值得到的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 CatmullRom(FixedPointVector2 value1, FixedPointVector2 value2,
            FixedPointVector2 value3, FixedPointVector2 value4, FixedPoint64 amount)
        {
            return new FixedPointVector2(
                FixedPointMath.CatmullRom(value1.x, value2.x, value3.x, value4.x, amount),
                FixedPointMath.CatmullRom(value1.y, value2.y, value3.y, value4.y, amount));
        }

        /// <summary>
        /// 对四个控制点执行 Catmull-Rom 样条插值（结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value1">第一个控制点。</param>
        /// <param name="value2">第二个控制点（插值起点）。</param>
        /// <param name="value3">第三个控制点（插值终点）。</param>
        /// <param name="value4">第四个控制点。</param>
        /// <param name="amount">插值权重（通常取值 0 到 1）。</param>
        /// <param name="result">插值得到的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CatmullRom(ref FixedPointVector2 value1, ref FixedPointVector2 value2,
            ref FixedPointVector2 value3, ref FixedPointVector2 value4,
            FixedPoint64 amount, out FixedPointVector2 result)
        {
            result = new FixedPointVector2(
                FixedPointMath.CatmullRom(value1.x, value2.x, value3.x, value4.x, amount),
                FixedPointMath.CatmullRom(value1.y, value2.y, value3.y, value4.y, amount));
        }

        /// <summary>
        /// 将向量的各分量逐分量限制在指定的最小值与最大值范围内。
        /// </summary>
        /// <param name="value1">待限制的向量。</param>
        /// <param name="min">各分量的最小值。</param>
        /// <param name="max">各分量的最大值。</param>
        /// <returns>限制后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 Clamp(FixedPointVector2 value1, FixedPointVector2 min, FixedPointVector2 max)
        {
            return new FixedPointVector2(
                FixedPointMath.Clamp(value1.x, min.x, max.x),
                FixedPointMath.Clamp(value1.y, min.y, max.y));
        }

        /// <summary>
        /// 将向量的各分量逐分量限制在指定的最小值与最大值范围内（结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value1">待限制的向量。</param>
        /// <param name="min">各分量的最小值。</param>
        /// <param name="max">各分量的最大值。</param>
        /// <param name="result">限制后的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp(ref FixedPointVector2 value1, ref FixedPointVector2 min, ref FixedPointVector2 max,
            out FixedPointVector2 result)
        {
            result = new FixedPointVector2(
                FixedPointMath.Clamp(value1.x, min.x, max.x),
                FixedPointMath.Clamp(value1.y, min.y, max.y));
        }

        /// <summary>
        /// 计算两个向量之间的定点精度距离。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <returns>两向量之间的距离。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Distance(FixedPointVector2 value1, FixedPointVector2 value2)
        {
            FixedPoint64 result;
            DistanceSquared(ref value1, ref value2, out result);
            return (FixedPoint64)FixedPoint64.Sqrt(result);
        }

        /// <summary>
        /// 计算两个向量之间的定点精度距离（结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <param name="result">两向量之间的距离。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Distance(ref FixedPointVector2 value1, ref FixedPointVector2 value2, out FixedPoint64 result)
        {
            DistanceSquared(ref value1, ref value2, out result);
            result = (FixedPoint64)FixedPoint64.Sqrt(result);
        }

        /// <summary>
        /// 计算两个向量之间距离的平方（避免开方，性能更高）。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <returns>两向量之间距离的平方。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 DistanceSquared(FixedPointVector2 value1, FixedPointVector2 value2)
        {
            FixedPoint64 result;
            DistanceSquared(ref value1, ref value2, out result);
            return result;
        }

        /// <summary>
        /// 计算两个向量之间距离的平方（结果通过 out 参数返回，避免开方，性能更高）。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <param name="result">两向量之间距离的平方。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DistanceSquared(ref FixedPointVector2 value1, ref FixedPointVector2 value2,
            out FixedPoint64 result)
        {
            result = (value1.x - value2.x) * (value1.x - value2.x) + (value1.y - value2.y) * (value1.y - value2.y);
        }

        /// <summary>
        /// 将第一个向量逐分量除以第二个向量。
        /// </summary>
        /// <param name="value1">被除向量。</param>
        /// <param name="value2">除数向量。</param>
        /// <returns>逐分量相除后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 Divide(FixedPointVector2 value1, FixedPointVector2 value2)
        {
            value1.x /= value2.x;
            value1.y /= value2.y;
            return value1;
        }

        /// <summary>
        /// 将第一个向量逐分量除以第二个向量（结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value1">被除向量。</param>
        /// <param name="value2">除数向量。</param>
        /// <param name="result">逐分量相除后的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Divide(ref FixedPointVector2 value1, ref FixedPointVector2 value2,
            out FixedPointVector2 result)
        {
            result.x = value1.x / value2.x;
            result.y = value1.y / value2.y;
        }

        /// <summary>
        /// 将向量各分量除以一个标量。
        /// </summary>
        /// <param name="value1">被除向量。</param>
        /// <param name="divider">标量除数。</param>
        /// <returns>缩放后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 Divide(FixedPointVector2 value1, FixedPoint64 divider)
        {
            FixedPoint64 factor = 1 / divider;
            value1.x *= factor;
            value1.y *= factor;
            return value1;
        }

        /// <summary>
        /// 将向量各分量除以一个标量（结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value1">被除向量。</param>
        /// <param name="divider">标量除数。</param>
        /// <param name="result">缩放后的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Divide(ref FixedPointVector2 value1, FixedPoint64 divider, out FixedPointVector2 result)
        {
            FixedPoint64 factor = 1 / divider;
            result.x = value1.x * factor;
            result.y = value1.y * factor;
        }

        /// <summary>
        /// 计算两个向量的点乘（数量积）。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <returns>两向量的点乘结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Dot(FixedPointVector2 value1, FixedPointVector2 value2)
        {
            return value1.x * value2.x + value1.y * value2.y;
        }

        /// <summary>
        /// 计算两个向量的点乘（数量积，结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <param name="result">两向量的点乘结果。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Dot(ref FixedPointVector2 value1, ref FixedPointVector2 value2, out FixedPoint64 result)
        {
            result = value1.x * value2.x + value1.y * value2.y;
        }

        /// <summary>
        /// 判断指定对象是否与当前向量相等。
        /// </summary>
        /// <param name="obj">待比较的对象。</param>
        /// <returns>若相等返回 true，否则返回 false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return (obj is FixedPointVector2) ? this == ((FixedPointVector2)obj) : false;
        }

        /// <summary>
        /// 判断指定向量是否与当前向量相等。
        /// </summary>
        /// <param name="other">待比较的向量。</param>
        /// <returns>若相等返回 true，否则返回 false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(FixedPointVector2 other)
        {
            return this == other;
        }

        /// <summary>
        /// 获取当前向量的哈希码。
        /// </summary>
        /// <returns>当前向量的哈希码。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return (int)(x + y);
        }

        /// <summary>
        /// 对两个端点及其切线执行 Hermite 样条插值。
        /// </summary>
        /// <param name="value1">起点位置。</param>
        /// <param name="tangent1">起点处的切线。</param>
        /// <param name="value2">终点位置。</param>
        /// <param name="tangent2">终点处的切线。</param>
        /// <param name="amount">插值权重（通常取值 0 到 1）。</param>
        /// <returns>插值得到的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 Hermite(FixedPointVector2 value1, FixedPointVector2 tangent1,
            FixedPointVector2 value2, FixedPointVector2 tangent2, FixedPoint64 amount)
        {
            FixedPointVector2 result = new FixedPointVector2();
            Hermite(ref value1, ref tangent1, ref value2, ref tangent2, amount, out result);
            return result;
        }

        /// <summary>
        /// 对两个端点及其切线执行 Hermite 样条插值（结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value1">起点位置。</param>
        /// <param name="tangent1">起点处的切线。</param>
        /// <param name="value2">终点位置。</param>
        /// <param name="tangent2">终点处的切线。</param>
        /// <param name="amount">插值权重（通常取值 0 到 1）。</param>
        /// <param name="result">插值得到的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Hermite(ref FixedPointVector2 value1, ref FixedPointVector2 tangent1,
            ref FixedPointVector2 value2, ref FixedPointVector2 tangent2,
            FixedPoint64 amount, out FixedPointVector2 result)
        {
            result.x = FixedPointMath.Hermite(value1.x, tangent1.x, value2.x, tangent2.x, amount);
            result.y = FixedPointMath.Hermite(value1.y, tangent1.y, value2.y, tangent2.y, amount);
        }

        /// <summary>
        /// 获取向量的长度（模）。
        /// </summary>
        public FixedPoint64 magnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                FixedPoint64 result;
                DistanceSquared(ref this, ref zeroVector, out result);
                return FixedPoint64.Sqrt(result);
            }
        }

        /// <summary>
        /// 获取向量长度的平方（避免开方，性能更高）。
        /// </summary>
        public FixedPoint64 sqrMagnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                FixedPoint64 result;
                DistanceSquared(ref this, ref zeroVector, out result);
                return result;
            }
        }

        /// <summary>
        /// 将向量的长度限制在指定的最大长度内，方向保持不变。
        /// </summary>
        /// <param name="vector">待限制的向量。</param>
        /// <param name="maxLength">允许的最大长度。</param>
        /// <returns>限制长度后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 ClampMagnitude(FixedPointVector2 vector, FixedPoint64 maxLength)
        {
            return Normalize(vector) * maxLength;
        }

        /// <summary>
        /// 获取当前向量长度的平方。
        /// </summary>
        /// <returns>当前向量长度的平方。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedPoint64 LengthSquared()
        {
            FixedPoint64 result;
            DistanceSquared(ref this, ref zeroVector, out result);
            return result;
        }

        /// <summary>
        /// 在两个向量之间进行线性插值，插值权重会被限制在 0 到 1 之间。
        /// </summary>
        /// <param name="value1">起点向量。</param>
        /// <param name="value2">终点向量。</param>
        /// <param name="amount">插值权重（会被限制到 0 到 1）。</param>
        /// <returns>插值得到的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 Lerp(FixedPointVector2 value1, FixedPointVector2 value2, FixedPoint64 amount)
        {
            amount = FixedPointMath.Clamp(amount, 0, 1);

            return new FixedPointVector2(
                FixedPointMath.Lerp(value1.x, value2.x, amount),
                FixedPointMath.Lerp(value1.y, value2.y, amount));
        }

        /// <summary>
        /// 在两个向量之间进行线性插值，插值权重不做范围限制（可外推）。
        /// </summary>
        /// <param name="value1">起点向量。</param>
        /// <param name="value2">终点向量。</param>
        /// <param name="amount">插值权重（不做范围限制）。</param>
        /// <returns>插值得到的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 LerpUnclamped(FixedPointVector2 value1, FixedPointVector2 value2,
            FixedPoint64 amount)
        {
            return new FixedPointVector2(
                FixedPointMath.Lerp(value1.x, value2.x, amount),
                FixedPointMath.Lerp(value1.y, value2.y, amount));
        }

        /// <summary>
        /// 在两个向量之间进行线性插值，插值权重不做范围限制（可外推，结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value1">起点向量。</param>
        /// <param name="value2">终点向量。</param>
        /// <param name="amount">插值权重（不做范围限制）。</param>
        /// <param name="result">插值得到的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LerpUnclamped(ref FixedPointVector2 value1, ref FixedPointVector2 value2,
            FixedPoint64 amount, out FixedPointVector2 result)
        {
            result = new FixedPointVector2(
                FixedPointMath.Lerp(value1.x, value2.x, amount),
                FixedPointMath.Lerp(value1.y, value2.y, amount));
        }

        /// <summary>
        /// 逐分量取两个向量的较大值，组成新向量。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <returns>各分量取较大值后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 Max(FixedPointVector2 value1, FixedPointVector2 value2)
        {
            return new FixedPointVector2(
                FixedPointMath.Max(value1.x, value2.x),
                FixedPointMath.Max(value1.y, value2.y));
        }

        /// <summary>
        /// 逐分量取两个向量的较大值，组成新向量（结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <param name="result">各分量取较大值后的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Max(ref FixedPointVector2 value1, ref FixedPointVector2 value2, out FixedPointVector2 result)
        {
            result.x = FixedPointMath.Max(value1.x, value2.x);
            result.y = FixedPointMath.Max(value1.y, value2.y);
        }

        /// <summary>
        /// 逐分量取两个向量的较小值，组成新向量。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <returns>各分量取较小值后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 Min(FixedPointVector2 value1, FixedPointVector2 value2)
        {
            return new FixedPointVector2(
                FixedPointMath.Min(value1.x, value2.x),
                FixedPointMath.Min(value1.y, value2.y));
        }

        /// <summary>
        /// 逐分量取两个向量的较小值，组成新向量（结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <param name="result">各分量取较小值后的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Min(ref FixedPointVector2 value1, ref FixedPointVector2 value2, out FixedPointVector2 result)
        {
            result.x = FixedPointMath.Min(value1.x, value2.x);
            result.y = FixedPointMath.Min(value1.y, value2.y);
        }

        /// <summary>
        /// 将当前向量逐分量乘以另一个向量（缩放当前向量）。
        /// </summary>
        /// <param name="other">用于逐分量缩放的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(FixedPointVector2 other)
        {
            this.x = x * other.x;
            this.y = y * other.y;
        }

        /// <summary>
        /// 将两个向量逐分量相乘，组成新向量。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <returns>逐分量相乘后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 Scale(FixedPointVector2 value1, FixedPointVector2 value2)
        {
            FixedPointVector2 result;
            result.x = value1.x * value2.x;
            result.y = value1.y * value2.y;

            return result;
        }

        /// <summary>
        /// 将两个向量逐分量相乘。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <returns>逐分量相乘后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 Multiply(FixedPointVector2 value1, FixedPointVector2 value2)
        {
            value1.x *= value2.x;
            value1.y *= value2.y;
            return value1;
        }

        /// <summary>
        /// 将向量各分量乘以一个标量。
        /// </summary>
        /// <param name="value1">待缩放的向量。</param>
        /// <param name="scaleFactor">标量缩放因子。</param>
        /// <returns>缩放后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 Multiply(FixedPointVector2 value1, FixedPoint64 scaleFactor)
        {
            value1.x *= scaleFactor;
            value1.y *= scaleFactor;
            return value1;
        }

        /// <summary>
        /// 将向量各分量乘以一个标量（结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value1">待缩放的向量。</param>
        /// <param name="scaleFactor">标量缩放因子。</param>
        /// <param name="result">缩放后的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Multiply(ref FixedPointVector2 value1, FixedPoint64 scaleFactor,
            out FixedPointVector2 result)
        {
            result.x = value1.x * scaleFactor;
            result.y = value1.y * scaleFactor;
        }

        /// <summary>
        /// 将两个向量逐分量相乘（结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <param name="result">逐分量相乘后的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Multiply(ref FixedPointVector2 value1, ref FixedPointVector2 value2,
            out FixedPointVector2 result)
        {
            result.x = value1.x * value2.x;
            result.y = value1.y * value2.y;
        }

        /// <summary>
        /// 对向量取反（反转方向）。
        /// </summary>
        /// <param name="value">待取反的向量。</param>
        /// <returns>取反后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 Negate(FixedPointVector2 value)
        {
            value.x = -value.x;
            value.y = -value.y;
            return value;
        }

        /// <summary>
        /// 对向量取反（反转方向，结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value">待取反的向量。</param>
        /// <param name="result">取反后的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Negate(ref FixedPointVector2 value, out FixedPointVector2 result)
        {
            result.x = -value.x;
            result.y = -value.y;
        }

        /// <summary>
        /// 将当前向量就地归一化（长度变为 1，方向不变）。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            Normalize(ref this, out this);
        }

        /// <summary>
        /// 返回指定向量的归一化结果（长度变为 1，方向不变）。
        /// </summary>
        /// <param name="value">待归一化的向量。</param>
        /// <returns>归一化后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 Normalize(FixedPointVector2 value)
        {
            Normalize(ref value, out value);
            return value;
        }

        /// <summary>
        /// 获取当前向量的归一化版本（长度变为 1，方向不变）。
        /// </summary>
        public FixedPointVector2 normalized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                FixedPointVector2 result;
                FixedPointVector2.Normalize(ref this, out result);

                return result;
            }
        }

        /// <summary>
        /// 将指定向量归一化（长度变为 1，方向不变，结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value">待归一化的向量。</param>
        /// <param name="result">归一化后的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Normalize(ref FixedPointVector2 value, out FixedPointVector2 result)
        {
            FixedPoint64 factor;
            DistanceSquared(ref value, ref zeroVector, out factor);
            factor = 1f / (FixedPoint64)FixedPoint64.Sqrt(factor);
            result.x = value.x * factor;
            result.y = value.y * factor;
        }

        /// <summary>
        /// 在两个向量之间执行平滑插值（首尾速度为零的 S 形过渡）。
        /// </summary>
        /// <param name="value1">起点向量。</param>
        /// <param name="value2">终点向量。</param>
        /// <param name="amount">插值权重。</param>
        /// <returns>平滑插值得到的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 SmoothStep(FixedPointVector2 value1, FixedPointVector2 value2,
            FixedPoint64 amount)
        {
            return new FixedPointVector2(
                FixedPointMath.SmoothStep(value1.x, value2.x, amount),
                FixedPointMath.SmoothStep(value1.y, value2.y, amount));
        }

        /// <summary>
        /// 在两个向量之间执行平滑插值（首尾速度为零的 S 形过渡，结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value1">起点向量。</param>
        /// <param name="value2">终点向量。</param>
        /// <param name="amount">插值权重。</param>
        /// <param name="result">平滑插值得到的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SmoothStep(ref FixedPointVector2 value1, ref FixedPointVector2 value2, FixedPoint64 amount,
            out FixedPointVector2 result)
        {
            result = new FixedPointVector2(
                FixedPointMath.SmoothStep(value1.x, value2.x, amount),
                FixedPointMath.SmoothStep(value1.y, value2.y, amount));
        }

        /// <summary>
        /// 将第一个向量逐分量减去第二个向量。
        /// </summary>
        /// <param name="value1">被减向量。</param>
        /// <param name="value2">减去的向量。</param>
        /// <returns>两向量之差。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 Subtract(FixedPointVector2 value1, FixedPointVector2 value2)
        {
            value1.x -= value2.x;
            value1.y -= value2.y;
            return value1;
        }

        /// <summary>
        /// 将第一个向量逐分量减去第二个向量（结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value1">被减向量。</param>
        /// <param name="value2">减去的向量。</param>
        /// <param name="result">两向量之差。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Subtract(ref FixedPointVector2 value1, ref FixedPointVector2 value2,
            out FixedPointVector2 result)
        {
            result.x = value1.x - value2.x;
            result.y = value1.y - value2.y;
        }

        /// <summary>
        /// 计算两个向量之间的夹角（单位：度）。
        /// </summary>
        /// <param name="a">第一个向量。</param>
        /// <param name="b">第二个向量。</param>
        /// <returns>两向量之间的夹角，单位为度。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Angle(FixedPointVector2 a, FixedPointVector2 b)
        {
            return FixedPoint64.Acos(a.normalized * b.normalized) * FixedPoint64.Rad2Deg;
        }

        /// <summary>
        /// 将当前二维向量转换为三维向量，Z 分量设为 0。
        /// </summary>
        /// <returns>对应的三维向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedPointVector3 ToFpVector3()
        {
            return new FixedPointVector3(this.x, this.y, 0);
        }

        /// <summary>
        /// 返回向量的字符串表示形式，格式为 "(x, y)"。
        /// </summary>
        /// <returns>包含两个分量的字符串。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return $"({x.AsFloat():f1}, {y.AsFloat():f1})";
        }

        #endregion 公有方法

        #region 运算符

        /// <summary>
        /// 一元负号运算符，对向量取反（反转方向）。
        /// </summary>
        /// <param name="value">待取反的向量。</param>
        /// <returns>取反后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 operator -(FixedPointVector2 value)
        {
            value.x = -value.x;
            value.y = -value.y;
            return value;
        }

        /// <summary>
        /// 相等运算符，判断两个向量是否逐分量相等。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <returns>若两向量各分量都相等返回 true，否则返回 false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(FixedPointVector2 value1, FixedPointVector2 value2)
        {
            return value1.x == value2.x && value1.y == value2.y;
        }

        /// <summary>
        /// 不等运算符，判断两个向量是否存在不相等的分量。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <returns>若两向量存在不相等的分量返回 true，否则返回 false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(FixedPointVector2 value1, FixedPointVector2 value2)
        {
            return value1.x != value2.x || value1.y != value2.y;
        }

        /// <summary>
        /// 加法运算符，将两个向量逐分量相加。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <returns>两向量之和。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 operator +(FixedPointVector2 value1, FixedPointVector2 value2)
        {
            value1.x += value2.x;
            value1.y += value2.y;
            return value1;
        }

        /// <summary>
        /// 减法运算符，将第一个向量逐分量减去第二个向量。
        /// </summary>
        /// <param name="value1">被减向量。</param>
        /// <param name="value2">减去的向量。</param>
        /// <returns>两向量之差。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 operator -(FixedPointVector2 value1, FixedPointVector2 value2)
        {
            value1.x -= value2.x;
            value1.y -= value2.y;
            return value1;
        }

        /// <summary>
        /// 乘法运算符（向量与向量），计算两个向量的点乘（数量积）。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <returns>两向量的点乘结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 operator *(FixedPointVector2 value1, FixedPointVector2 value2)
        {
            return FixedPointVector2.Dot(value1, value2);
        }

        /// <summary>
        /// 乘法运算符（向量乘标量），将向量各分量乘以一个标量。
        /// </summary>
        /// <param name="value">待缩放的向量。</param>
        /// <param name="scaleFactor">标量缩放因子。</param>
        /// <returns>缩放后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 operator *(FixedPointVector2 value, FixedPoint64 scaleFactor)
        {
            value.x *= scaleFactor;
            value.y *= scaleFactor;
            return value;
        }

        /// <summary>
        /// 乘法运算符（标量乘向量），将向量各分量乘以一个标量。
        /// </summary>
        /// <param name="scaleFactor">标量缩放因子。</param>
        /// <param name="value">待缩放的向量。</param>
        /// <returns>缩放后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 operator *(FixedPoint64 scaleFactor, FixedPointVector2 value)
        {
            value.x *= scaleFactor;
            value.y *= scaleFactor;
            return value;
        }

        /// <summary>
        /// 除法运算符（向量除向量），将第一个向量逐分量除以第二个向量。
        /// </summary>
        /// <param name="value1">被除向量。</param>
        /// <param name="value2">除数向量。</param>
        /// <returns>逐分量相除后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 operator /(FixedPointVector2 value1, FixedPointVector2 value2)
        {
            value1.x /= value2.x;
            value1.y /= value2.y;
            return value1;
        }

        /// <summary>
        /// 除法运算符（向量除标量），将向量各分量除以一个标量。
        /// </summary>
        /// <param name="value1">被除向量。</param>
        /// <param name="divider">标量除数。</param>
        /// <returns>缩放后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 operator /(FixedPointVector2 value1, FixedPoint64 divider)
        {
            FixedPoint64 factor = 1 / divider;
            value1.x *= factor;
            value1.y *= factor;
            return value1;
        }

        #endregion 运算符

#if UNITY_2021_3_OR_NEWER

        /// <summary>
        /// 由 Unity 的 <see cref="Vector2"/> 构造定点二维向量（仅在 Unity 环境下可用）。
        /// </summary>
        /// <param name="vector2">Unity 二维向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedPointVector2(Vector2 vector2)
        {
            x = vector2.x;
            y = vector2.y;
        }

        /// <summary>
        /// 由 Unity 的 <see cref="Vector2Int"/> 构造定点二维向量（仅在 Unity 环境下可用）。
        /// </summary>
        /// <param name="vector2">Unity 整数二维向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedPointVector2(Vector2Int vector2)
        {
            x = vector2.x;
            y = vector2.y;
        }

        /// <summary>
        /// 将 Unity 的 <see cref="Vector2Int"/> 转为定点向量后与标量相乘（仅在 Unity 环境下可用）。
        /// </summary>
        /// <param name="value1">Unity 整数二维向量。</param>
        /// <param name="scaleFactor">缩放标量。</param>
        /// <returns>相乘得到的定点二维向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector2 Multiply(Vector2Int value1, FixedPoint64 scaleFactor)
        {
            var fixedPoint = new FixedPointVector2(value1);
            Multiply(ref fixedPoint, scaleFactor, out var result);
            return result;
        }

        /// <summary>
        /// 将定点二维向量转换为 Unity 的 <see cref="Vector2"/>（仅在 Unity 环境下可用，转换为浮点，仅用于表现层）。
        /// </summary>
        /// <returns>转换后的 Unity 二维向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 ToVector2()
        {
            return new Vector2(x.AsFloat(), y.AsFloat());
        }

        /// <summary>
        /// 将定点二维向量转换为 Unity 的 <see cref="Vector2Int"/>（仅在 Unity 环境下可用，分量取整，仅用于表现层）。
        /// </summary>
        /// <returns>转换后的 Unity 整数二维向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(x.AsInt(), y.AsInt());
        }

#endif
    }
}
