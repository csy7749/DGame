using System;
using System.Runtime.CompilerServices;

#if UNITY_2021_3_OR_NEWER
using UnityEngine;
#endif

namespace DGame.FixedPoint
{
    /// <summary>
    /// 定点数三维向量，元素类型为 <see cref="FixedPoint64"/>，用于帧同步的确定性数学计算。
    /// </summary>
    [Serializable]
    public struct FixedPointVector3
    {

        private static FixedPoint64 ZeroEpsilonSq = FixedPointMath.Epsilon;
        internal static FixedPointVector3 InternalZero;
        internal static FixedPointVector3 Arbitrary;

        /// <summary>向量的 X 分量。</summary>
        public FixedPoint64 x;

        /// <summary>向量的 Y 分量。</summary>
        public FixedPoint64 y;

        /// <summary>向量的 Z 分量。</summary>
        public FixedPoint64 z;

        #region 静态只读变量

        /// <summary>
        /// 分量为 (0, 0, 0) 的向量。
        /// </summary>
        public static readonly FixedPointVector3 zero;

        /// <summary>
        /// 分量为 (-1, 0, 0) 的向量。
        /// </summary>
        public static readonly FixedPointVector3 left;

        /// <summary>
        /// 分量为 (1, 0, 0) 的向量。
        /// </summary>
        public static readonly FixedPointVector3 right;

        /// <summary>
        /// 分量为 (0, 1, 0) 的向量。
        /// </summary>
        public static readonly FixedPointVector3 up;

        /// <summary>
        /// 分量为 (0, -1, 0) 的向量。
        /// </summary>
        public static readonly FixedPointVector3 down;

        /// <summary>
        /// 分量为 (0, 0, -1) 的向量。
        /// </summary>
        public static readonly FixedPointVector3 back;

        /// <summary>
        /// 分量为 (0, 0, 1) 的向量。
        /// </summary>
        public static readonly FixedPointVector3 forward;

        /// <summary>
        /// 分量为 (1, 1, 1) 的向量。
        /// </summary>
        public static readonly FixedPointVector3 one;

        /// <summary>
        /// 各分量均为 <see cref="FixedPoint64.MinValue"/> 的向量。
        /// </summary>
        public static readonly FixedPointVector3 MinValue;

        /// <summary>
        /// 各分量均为 <see cref="FixedPoint64.MaxValue"/> 的向量。
        /// </summary>
        public static readonly FixedPointVector3 MaxValue;

        #endregion

        #region 私有静态构造函数

        static FixedPointVector3()
        {
            one = new FixedPointVector3(1, 1, 1);
            zero = new FixedPointVector3(0, 0, 0);
            left = new FixedPointVector3(-1, 0, 0);
            right = new FixedPointVector3(1, 0, 0);
            up = new FixedPointVector3(0, 1, 0);
            down = new FixedPointVector3(0, -1, 0);
            back = new FixedPointVector3(0, 0, -1);
            forward = new FixedPointVector3(0, 0, 1);
            MinValue = new FixedPointVector3(FixedPoint64.MinValue);
            MaxValue = new FixedPointVector3(FixedPoint64.MaxValue);
            Arbitrary = new FixedPointVector3(1, 1, 1);
            InternalZero = zero;
        }

        #endregion

        /// <summary>
        /// 返回各分量取绝对值后的向量。
        /// </summary>
        /// <param name="other">源向量。</param>
        /// <returns>各分量取绝对值后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector3 Abs(FixedPointVector3 other)
        {
            return new FixedPointVector3(FixedPoint64.Abs(other.x), FixedPoint64.Abs(other.y),
                FixedPoint64.Abs(other.z));
        }

        /// <summary>
        /// 将一个向量投影到另一个向量上。
        /// </summary>
        /// <param name="vector">被投影的向量。</param>
        /// <param name="onNormal">投影目标方向向量。</param>
        /// <returns>投影后的向量；若目标方向接近零向量则返回零向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector3 Project(FixedPointVector3 vector, FixedPointVector3 onNormal)
        {
            var sqrMag = Dot(onNormal, onNormal);

            if (sqrMag < FixedPointMath.Epsilon)
                return zero;
            else
            {
                var dot = Dot(vector, onNormal) / sqrMag;
                return new FixedPointVector3(onNormal.x * dot,
                    onNormal.y * dot,
                    onNormal.z * dot);
            }
        }

        /// <summary>
        /// 获取向量长度的平方（避免开方，性能更高）。
        /// </summary>
        /// <returns>向量长度的平方。</returns>
        public FixedPoint64 sqrMagnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (((this.x * this.x) + (this.y * this.y)) + (this.z * this.z));
        }

        /// <summary>
        /// 获取仅由 X、Z 分量构成的水平向量长度的平方（忽略 Y 分量）。
        /// </summary>
        public FixedPoint64 sqrMagnitudeXZ
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ((this.x * this.x) + (this.z * this.z));
        }

        /// <summary>
        /// 获取向量的长度（模）。
        /// </summary>
        /// <returns>向量的长度。</returns>
        public FixedPoint64 magnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var num = ((this.x * this.x) + (this.y * this.y)) + (this.z * this.z);
                return FixedPoint64.Sqrt(num);
            }
        }

        /// <summary>
        /// 将向量的长度限制在指定的最大长度内，方向保持不变。
        /// </summary>
        /// <param name="vector">待限制的向量。</param>
        /// <param name="maxLength">允许的最大长度。</param>
        /// <returns>限制长度后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector3 ClampMagnitude(FixedPointVector3 vector, FixedPoint64 maxLength)
        {
            return Normalize(vector) * maxLength;
        }

        /// <summary>
        /// 获取当前向量的归一化版本（长度变为 1，方向不变）。
        /// </summary>
        /// <returns>归一化后的向量。</returns>
        public FixedPointVector3 normalized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var result = new FixedPointVector3(this.x, this.y, this.z);
                result.Normalize();
                return result;
            }
        }

        /// <summary>
        /// 使用整数分量构造一个新的三维向量。
        /// </summary>
        /// <param name="x">向量的 X 分量。</param>
        /// <param name="y">向量的 Y 分量。</param>
        /// <param name="z">向量的 Z 分量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedPointVector3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// 使用定点数分量构造一个新的三维向量。
        /// </summary>
        /// <param name="x">向量的 X 分量。</param>
        /// <param name="y">向量的 Y 分量。</param>
        /// <param name="z">向量的 Z 分量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedPointVector3(FixedPoint64 x, FixedPoint64 y, FixedPoint64 z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// 将当前向量逐分量乘以给定向量的对应分量（缩放当前向量）。
        /// </summary>
        /// <param name="other">用于逐分量缩放的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(FixedPointVector3 other)
        {
            this.x = x * other.x;
            this.y = y * other.y;
            this.z = z * other.z;
        }

        /// <summary>
        /// 设置向量的各个分量为指定值。
        /// </summary>
        /// <param name="x">向量的 X 分量。</param>
        /// <param name="y">向量的 Y 分量。</param>
        /// <param name="z">向量的 Z 分量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(FixedPoint64 x, FixedPoint64 y, FixedPoint64 z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// 构造一个所有分量都相同的三维向量。
        /// </summary>
        /// <param name="xyz">同时赋给 X、Y、Z 的值。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedPointVector3(FixedPoint64 xyz)
        {
            this.x = xyz;
            this.y = xyz;
            this.z = xyz;
        }

        /// <summary>
        /// 在两个向量之间进行线性插值。
        /// </summary>
        /// <param name="from">起点向量。</param>
        /// <param name="to">终点向量。</param>
        /// <param name="percent">插值权重。</param>
        /// <returns>插值得到的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector3 Lerp(FixedPointVector3 from, FixedPointVector3 to, FixedPoint64 percent)
        {
            return from + (to - from) * percent;
        }

        #region 返回向量的字符串表示形式

        /// <summary>
        /// 返回向量的字符串表示形式，格式为 "(x, y, z)"。
        /// </summary>
        /// <returns>包含三个分量的字符串。</returns>
        public override string ToString()
        {
            return $"({x.AsFloat():f1}, {y.AsFloat():f1}, {z.AsFloat():f1})";
        }

        #endregion

        #region 判断对象是否相等

        /// <summary>
        /// 判断指定对象是否与当前向量相等。
        /// </summary>
        /// <param name="obj">待比较的对象。</param>
        /// <returns>若相等返回 true，否则返回 false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (!(obj is FixedPointVector3)) return false;
            var other = (FixedPointVector3)obj;

            return (((x == other.x) && (y == other.y)) && (z == other.z));
        }

        #endregion

        /// <summary>
        /// 将两个向量逐分量相乘，组成新向量。
        /// </summary>
        /// <param name="vecA">第一个向量。</param>
        /// <param name="vecB">第二个向量。</param>
        /// <returns>逐分量相乘后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector3 Scale(FixedPointVector3 vecA, FixedPointVector3 vecB)
        {
            FixedPointVector3 result;
            result.x = vecA.x * vecB.x;
            result.y = vecA.y * vecB.y;
            result.z = vecA.z * vecB.z;

            return result;
        }

        #region 相等运算符

        /// <summary>
        /// 相等运算符，判断两个向量是否逐分量相等。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <returns>若两向量各分量都相等返回 true，否则返回 false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(FixedPointVector3 value1, FixedPointVector3 value2)
        {
            return (((value1.x == value2.x) && (value1.y == value2.y)) && (value1.z == value2.z));
        }

        #endregion

        #region 不等运算符

        /// <summary>
        /// 不等运算符，判断两个向量是否存在不相等的分量。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <returns>若两向量完全相等返回 false，否则返回 true。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(FixedPointVector3 value1, FixedPointVector3 value2)
        {
            if ((value1.x == value2.x) && (value1.y == value2.y))
            {
                return (value1.z != value2.z);
            }

            return true;
        }

        #endregion

        #region 逐分量取较小值

        /// <summary>
        /// 逐分量取两个向量的较小值，组成新向量。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <returns>各分量取较小值后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector3 Min(FixedPointVector3 value1, FixedPointVector3 value2)
        {
            FixedPointVector3.Min(ref value1, ref value2, out var result);
            return result;
        }

        /// <summary>
        /// 逐分量取两个向量的较小值，组成新向量（结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <param name="result">各分量取较小值后的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Min(ref FixedPointVector3 value1, ref FixedPointVector3 value2, out FixedPointVector3 result)
        {
            result.x = (value1.x < value2.x) ? value1.x : value2.x;
            result.y = (value1.y < value2.y) ? value1.y : value2.y;
            result.z = (value1.z < value2.z) ? value1.z : value2.z;
        }

        #endregion

        #region 逐分量取较大值

        /// <summary>
        /// 逐分量取两个向量的较大值，组成新向量。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <returns>各分量取较大值后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector3 Max(FixedPointVector3 value1, FixedPointVector3 value2)
        {
            FixedPointVector3.Max(ref value1, ref value2, out var result);
            return result;
        }

        /// <summary>
        /// 计算两个向量之间的距离。
        /// </summary>
        /// <param name="v1">第一个向量。</param>
        /// <param name="v2">第二个向量。</param>
        /// <returns>两向量之间的距离。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Distance(FixedPointVector3 v1, FixedPointVector3 v2)
        {
            return FixedPoint64.Sqrt((v1.x - v2.x) * (v1.x - v2.x) + (v1.y - v2.y) * (v1.y - v2.y) +
                                     (v1.z - v2.z) * (v1.z - v2.z));
        }

        /// <summary>
        /// 计算两个向量在 XZ 水平面上的距离（忽略 Y 分量）。
        /// </summary>
        /// <param name="v1">第一个向量。</param>
        /// <param name="v2">第二个向量。</param>
        /// <returns>两向量在 XZ 平面上的距离。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 DistanceXZ(FixedPointVector3 v1, FixedPointVector3 v2)
        {
            return FixedPoint64.Sqrt((v1.x - v2.x) * (v1.x - v2.x) + (v1.z - v2.z) * (v1.z - v2.z));
        }

        /// <summary>
        /// 逐分量取两个向量的较大值，组成新向量（结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <param name="result">各分量取较大值后的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Max(ref FixedPointVector3 value1, ref FixedPointVector3 value2, out FixedPointVector3 result)
        {
            result.x = (value1.x > value2.x) ? value1.x : value2.x;
            result.y = (value1.y > value2.y) ? value1.y : value2.y;
            result.z = (value1.z > value2.z) ? value1.z : value2.z;
        }

        #endregion

        #region 将向量置零

        /// <summary>
        /// 将向量的所有分量置零。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MakeZero()
        {
            x = FixedPoint64.Zero;
            y = FixedPoint64.Zero;
            z = FixedPoint64.Zero;
        }

        #endregion

        #region 判断是否为零向量

        /// <summary>
        /// 判断向量是否为零向量（长度为零）。
        /// </summary>
        /// <returns>若为零向量返回 true，否则返回 false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsZero()
        {
            return (this.sqrMagnitude == FixedPoint64.Zero);
        }

        /// <summary>
        /// 判断向量是否接近零向量（长度接近零）。
        /// </summary>
        /// <returns>若接近零向量返回 true，否则返回 false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNearlyZero()
        {
            return (this.sqrMagnitude < ZeroEpsilonSq);
        }

        #endregion

        #region 使用矩阵对向量进行变换

        /// <summary>
        /// 使用给定矩阵对向量进行变换。
        /// </summary>
        /// <param name="position">待变换的向量。</param>
        /// <param name="matrix">变换矩阵。</param>
        /// <returns>变换后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector3 Transform(FixedPointVector3 position, FixedPointMatrix matrix)
        {
            FixedPointVector3.Transform(ref position, ref matrix, out var result);
            return result;
        }

        /// <summary>
        /// 使用给定矩阵对向量进行变换（结果通过 out 参数返回）。
        /// </summary>
        /// <param name="position">待变换的向量。</param>
        /// <param name="matrix">变换矩阵。</param>
        /// <param name="result">变换后的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Transform(ref FixedPointVector3 position, ref FixedPointMatrix matrix,
            out FixedPointVector3 result)
        {
            var num0 = ((position.x * matrix.M11) + (position.y * matrix.M21)) + (position.z * matrix.M31);
            var num1 = ((position.x * matrix.M12) + (position.y * matrix.M22)) + (position.z * matrix.M32);
            var num2 = ((position.x * matrix.M13) + (position.y * matrix.M23)) + (position.z * matrix.M33);

            result.x = num0;
            result.y = num1;
            result.z = num2;
        }

        /// <summary>
        /// 使用给定矩阵的转置对向量进行变换（结果通过 out 参数返回）。
        /// </summary>
        /// <param name="position">待变换的向量。</param>
        /// <param name="matrix">变换矩阵。</param>
        /// <param name="result">变换后的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TransposedTransform(ref FixedPointVector3 position, ref FixedPointMatrix matrix,
            out FixedPointVector3 result)
        {
            var num0 = ((position.x * matrix.M11) + (position.y * matrix.M12)) + (position.z * matrix.M13);
            var num1 = ((position.x * matrix.M21) + (position.y * matrix.M22)) + (position.z * matrix.M23);
            var num2 = ((position.x * matrix.M31) + (position.y * matrix.M32)) + (position.z * matrix.M33);

            result.x = num0;
            result.y = num1;
            result.z = num2;
        }

        #endregion

        #region 计算两个向量的点乘

        /// <summary>
        /// 计算两个向量的点乘（数量积）。
        /// </summary>
        /// <param name="vector1">第一个向量。</param>
        /// <param name="vector2">第二个向量。</param>
        /// <returns>两向量的点乘结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Dot(FixedPointVector3 vector1, FixedPointVector3 vector2)
        {
            return FixedPointVector3.Dot(ref vector1, ref vector2);
        }

        /// <summary>
        /// 计算两个向量的点乘（数量积）。
        /// </summary>
        /// <param name="vector1">第一个向量。</param>
        /// <param name="vector2">第二个向量。</param>
        /// <returns>两向量的点乘结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Dot(ref FixedPointVector3 vector1, ref FixedPointVector3 vector2)
        {
            return ((vector1.x * vector2.x) + (vector1.y * vector2.y)) + (vector1.z * vector2.z);
        }

        #endregion

        #region 将两个向量相加

        /// <summary>
        /// 将两个向量相加。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <returns>两向量之和。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector3 Add(FixedPointVector3 value1, FixedPointVector3 value2)
        {
            FixedPointVector3.Add(ref value1, ref value2, out var result);
            return result;
        }

        /// <summary>
        /// 将两个向量相加（结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <param name="result">两向量之和。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(ref FixedPointVector3 value1, ref FixedPointVector3 value2, out FixedPointVector3 result)
        {
            FixedPoint64 num0 = value1.x + value2.x;
            FixedPoint64 num1 = value1.y + value2.y;
            FixedPoint64 num2 = value1.z + value2.z;

            result.x = num0;
            result.y = num1;
            result.z = num2;
        }

        #endregion

        /// <summary>
        /// 将向量各分量除以一个标量。
        /// </summary>
        /// <param name="value1">被除向量。</param>
        /// <param name="scaleFactor">标量除数。</param>
        /// <returns>缩放后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector3 Divide(FixedPointVector3 value1, FixedPoint64 scaleFactor)
        {
            FixedPointVector3.Divide(ref value1, scaleFactor, out var result);
            return result;
        }

        /// <summary>
        /// 将向量各分量除以一个标量（结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value1">被除向量。</param>
        /// <param name="scaleFactor">标量除数。</param>
        /// <param name="result">缩放后的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Divide(ref FixedPointVector3 value1, FixedPoint64 scaleFactor, out FixedPointVector3 result)
        {
            result.x = value1.x / scaleFactor;
            result.y = value1.y / scaleFactor;
            result.z = value1.z / scaleFactor;
        }

        #region 将第一个向量减去第二个向量

        /// <summary>
        /// 将第一个向量减去第二个向量。
        /// </summary>
        /// <param name="value1">被减向量。</param>
        /// <param name="value2">减去的向量。</param>
        /// <returns>两向量之差。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector3 Subtract(FixedPointVector3 value1, FixedPointVector3 value2)
        {
            FixedPointVector3.Subtract(ref value1, ref value2, out var result);
            return result;
        }

        /// <summary>
        /// 将第一个向量减去第二个向量（结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value1">被减向量。</param>
        /// <param name="value2">减去的向量。</param>
        /// <param name="result">两向量之差。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Subtract(ref FixedPointVector3 value1, ref FixedPointVector3 value2,
            out FixedPointVector3 result)
        {
            FixedPoint64 num0 = value1.x - value2.x;
            FixedPoint64 num1 = value1.y - value2.y;
            FixedPoint64 num2 = value1.z - value2.z;

            result.x = num0;
            result.y = num1;
            result.z = num2;
        }

        #endregion

        #region 计算两个向量的叉乘

        /// <summary>
        /// 计算两个向量的叉乘（向量积）。
        /// </summary>
        /// <param name="vector1">第一个向量。</param>
        /// <param name="vector2">第二个向量。</param>
        /// <returns>两向量的叉乘结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector3 Cross(FixedPointVector3 vector1, FixedPointVector3 vector2)
        {
            FixedPointVector3.Cross(ref vector1, ref vector2, out var result);
            return result;
        }

        /// <summary>
        /// 计算两个向量的叉乘（向量积，结果通过 out 参数返回）。
        /// </summary>
        /// <param name="vector1">第一个向量。</param>
        /// <param name="vector2">第二个向量。</param>
        /// <param name="result">两向量的叉乘结果。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Cross(ref FixedPointVector3 vector1, ref FixedPointVector3 vector2,
            out FixedPointVector3 result)
        {
            FixedPoint64 num3 = (vector1.y * vector2.z) - (vector1.z * vector2.y);
            FixedPoint64 num2 = (vector1.z * vector2.x) - (vector1.x * vector2.z);
            FixedPoint64 num = (vector1.x * vector2.y) - (vector1.y * vector2.x);
            result.x = num3;
            result.y = num2;
            result.z = num;
        }

        #endregion

        #region 获取哈希码

        /// <summary>
        /// 获取当前向量的哈希码。
        /// </summary>
        /// <returns>当前向量的哈希码。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
        }

        #endregion

        #region 对向量取反

        /// <summary>
        /// 就地反转当前向量的方向（各分量取反）。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Negate()
        {
            this.x = -this.x;
            this.y = -this.y;
            this.z = -this.z;
        }

        /// <summary>
        /// 反转指定向量的方向（各分量取反）。
        /// </summary>
        /// <param name="value">待反转的向量。</param>
        /// <returns>取反后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector3 Negate(FixedPointVector3 value)
        {
            FixedPointVector3.Negate(ref value, out var result);
            return result;
        }

        /// <summary>
        /// 反转指定向量的方向（各分量取反，结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value">待反转的向量。</param>
        /// <param name="result">取反后的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Negate(ref FixedPointVector3 value, out FixedPointVector3 result)
        {
            var num0 = -value.x;
            var num1 = -value.y;
            var num2 = -value.z;

            result.x = num0;
            result.y = num1;
            result.z = num2;
        }

        #endregion

        #region 将向量归一化

        /// <summary>
        /// 返回指定向量的归一化结果（长度变为 1，方向不变）。
        /// </summary>
        /// <param name="value">待归一化的向量。</param>
        /// <returns>归一化后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector3 Normalize(FixedPointVector3 value)
        {
            FixedPointVector3.Normalize(ref value, out var result);
            return result;
        }

        /// <summary>
        /// 将当前向量就地归一化（长度变为 1，方向不变）。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            var num2 = ((this.x * this.x) + (this.y * this.y)) + (this.z * this.z);
            var num = FixedPoint64.One / FixedPoint64.Sqrt(num2);
            this.x *= num;
            this.y *= num;
            this.z *= num;
        }

        /// <summary>
        /// 将指定向量归一化（长度变为 1，方向不变，结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value">待归一化的向量。</param>
        /// <param name="result">归一化后的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Normalize(ref FixedPointVector3 value, out FixedPointVector3 result)
        {
            var num2 = ((value.x * value.x) + (value.y * value.y)) + (value.z * value.z);
            var num = FixedPoint64.One / FixedPoint64.Sqrt(num2);
            result.x = value.x * num;
            result.y = value.y * num;
            result.z = value.z * num;
        }

        #endregion

        #region 交换两个向量

        /// <summary>
        /// 交换两个向量的各个分量。
        /// </summary>
        /// <param name="vector1">参与交换的第一个向量。</param>
        /// <param name="vector2">参与交换的第二个向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap(ref FixedPointVector3 vector1, ref FixedPointVector3 vector2)
        {
            var temp = vector1.x;
            vector1.x = vector2.x;
            vector2.x = temp;

            temp = vector1.y;
            vector1.y = vector2.y;
            vector2.y = temp;

            temp = vector1.z;
            vector1.z = vector2.z;
            vector2.z = temp;
        }

        #endregion

        #region 向量与标量相乘

        /// <summary>
        /// 将向量各分量乘以一个标量。
        /// </summary>
        /// <param name="value1">待缩放的向量。</param>
        /// <param name="scaleFactor">标量缩放因子。</param>
        /// <returns>缩放后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector3 Multiply(FixedPointVector3 value1, FixedPoint64 scaleFactor)
        {
            Multiply(ref value1, scaleFactor, out var result);
            return result;
        }

        /// <summary>
        /// 将向量各分量乘以一个标量（结果通过 out 参数返回）。
        /// </summary>
        /// <param name="value1">待缩放的向量。</param>
        /// <param name="scaleFactor">标量缩放因子。</param>
        /// <param name="result">缩放后的向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Multiply(ref FixedPointVector3 value1, FixedPoint64 scaleFactor,
            out FixedPointVector3 result)
        {
            result.x = value1.x * scaleFactor;
            result.y = value1.y * scaleFactor;
            result.z = value1.z * scaleFactor;
        }

        #endregion

        #region 取模运算符（叉乘）

        /// <summary>
        /// 取模运算符（%），计算两个向量的叉乘（向量积）。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <returns>两向量的叉乘结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector3 operator %(FixedPointVector3 value1, FixedPointVector3 value2)
        {
            FixedPointVector3.Cross(ref value1, ref value2, out var result);
            return result;
        }

        #endregion

        #region 乘法运算符（向量与向量的点乘）

        /// <summary>
        /// 乘法运算符（向量与向量），计算两个向量的点乘（数量积）。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <returns>两向量的点乘结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 operator *(FixedPointVector3 value1, FixedPointVector3 value2)
        {
            return FixedPointVector3.Dot(ref value1, ref value2);
        }

        #endregion

        #region 乘法运算符（向量乘标量）

        /// <summary>
        /// 乘法运算符（向量乘标量），将向量各分量乘以一个标量。
        /// </summary>
        /// <param name="value1">待缩放的向量。</param>
        /// <param name="value2">标量缩放因子。</param>
        /// <returns>缩放后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector3 operator *(FixedPointVector3 value1, FixedPoint64 value2)
        {
            FixedPointVector3.Multiply(ref value1, value2, out var result);
            return result;
        }

        #endregion

        #region 乘法运算符（标量乘向量）

        /// <summary>
        /// 乘法运算符（标量乘向量），将向量各分量乘以一个标量。
        /// </summary>
        /// <param name="value1">标量缩放因子。</param>
        /// <param name="value2">待缩放的向量。</param>
        /// <returns>缩放后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector3 operator *(FixedPoint64 value1, FixedPointVector3 value2)
        {
            FixedPointVector3.Multiply(ref value2, value1, out var result);
            return result;
        }

        #endregion

        #region 减法运算符

        /// <summary>
        /// 减法运算符，将第一个向量减去第二个向量。
        /// </summary>
        /// <param name="value1">被减向量。</param>
        /// <param name="value2">减去的向量。</param>
        /// <returns>两向量之差。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector3 operator -(FixedPointVector3 value1, FixedPointVector3 value2)
        {
            FixedPointVector3.Subtract(ref value1, ref value2, out var result);
            return result;
        }

        /// <summary>
        /// 一元负号运算符，对向量取反（反转方向）。
        /// </summary>
        /// <param name="value">待取反的向量。</param>
        /// <returns>取反后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector3 operator -(FixedPointVector3 value)
        {
            return new FixedPointVector3(-value.x, -value.y, -value.z);
        }

        #endregion

        #region 加法运算符

        /// <summary>
        /// 加法运算符，将两个向量相加。
        /// </summary>
        /// <param name="value1">第一个向量。</param>
        /// <param name="value2">第二个向量。</param>
        /// <returns>两向量之和。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector3 operator +(FixedPointVector3 value1, FixedPointVector3 value2)
        {
            FixedPointVector3.Add(ref value1, ref value2, out var result);
            return result;
        }

        #endregion

        /// <summary>
        /// 除法运算符（向量除标量），将向量各分量除以一个标量。
        /// </summary>
        /// <param name="value1">被除向量。</param>
        /// <param name="value2">标量除数。</param>
        /// <returns>缩放后的向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector3 operator /(FixedPointVector3 value1, FixedPoint64 value2)
        {
            Divide(ref value1, value2, out var result);
            return result;
        }

        /// <summary>
        /// 计算两个向量之间的夹角（单位：度）。
        /// </summary>
        /// <param name="a">第一个向量。</param>
        /// <param name="b">第二个向量。</param>
        /// <returns>两向量之间的夹角，单位为度。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Angle(FixedPointVector3 a, FixedPointVector3 b)
        {
            return FixedPoint64.Acos(a.normalized * b.normalized) * FixedPoint64.Rad2Deg;
        }

        /// <summary>
        /// 将当前三维向量转换为二维向量，仅保留 X、Y 分量。
        /// </summary>
        /// <returns>对应的二维向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedPointVector2 ToFpVector2()
        {
            return new FixedPointVector2(x, y);
        }

#if UNITY_2021_3_OR_NEWER

        /// <summary>
        /// 由 Unity 的 <see cref="Vector3"/> 构造定点三维向量（仅在 Unity 环境下可用）。
        /// </summary>
        /// <param name="vector3">Unity 三维向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedPointVector3(Vector3 vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }

        /// <summary>
        /// 由 Unity 的 <see cref="Vector3Int"/> 构造定点三维向量（仅在 Unity 环境下可用）。
        /// </summary>
        /// <param name="vector3">Unity 整数三维向量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedPointVector3(Vector3Int vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }

        /// <summary>
        /// 将 Unity 的 <see cref="Vector3Int"/> 转为定点向量后与标量相乘（仅在 Unity 环境下可用）。
        /// </summary>
        /// <param name="value1">Unity 整数三维向量。</param>
        /// <param name="scaleFactor">缩放标量。</param>
        /// <returns>相乘得到的定点三维向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointVector3 Multiply(Vector3Int value1, FixedPoint64 scaleFactor)
        {
            var fixedPoint = new FixedPointVector3(value1);
            Multiply(ref fixedPoint, scaleFactor, out var result);
            return result;
        }

        /// <summary>
        /// 将定点三维向量转换为 Unity 的 <see cref="Vector3"/>（仅在 Unity 环境下可用，转换为浮点，仅用于表现层）。
        /// </summary>
        /// <returns>转换后的 Unity 三维向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 ToVector3()
        {
            return new Vector3(x.AsFloat(), y.AsFloat(), z.AsFloat());
        }

        /// <summary>
        /// 将定点三维向量转换为 Unity 的 <see cref="Vector3Int"/>（仅在 Unity 环境下可用，分量取整，仅用于表现层）。
        /// </summary>
        /// <returns>转换后的 Unity 整数三维向量。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3Int ToVector3Int()
        {
            return new Vector3Int(x.AsInt(), y.AsInt(), z.AsInt());
        }

#endif
    }
}
