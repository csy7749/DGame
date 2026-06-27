using System;
using System.Runtime.CompilerServices;

#if UNITY_2021_3_OR_NEWER
using UnityEngine;
#endif

namespace DGame.FixedPoint
{

    /// <summary>
    /// 表示旋转朝向的定点四元数。
    /// </summary>
    [Serializable]
    public struct FixedPointQuaternion
    {

        /// <summary>四元数的 X 分量。</summary>
        public FixedPoint64 x;

        /// <summary>四元数的 Y 分量。</summary>
        public FixedPoint64 y;

        /// <summary>四元数的 Z 分量。</summary>
        public FixedPoint64 z;

        /// <summary>四元数的 W 分量。</summary>
        public FixedPoint64 w;

        /// <summary>单位四元数（0, 0, 0, 1），表示无旋转。</summary>
        public static readonly FixedPointQuaternion identity;

        static FixedPointQuaternion()
        {
            identity = new FixedPointQuaternion(0, 0, 0, 1);
        }

        /// <summary>
        /// 使用指定的分量初始化定点四元数。
        /// </summary>
        /// <param name="x">四元数的 X 分量。</param>
        /// <param name="y">四元数的 Y 分量。</param>
        /// <param name="z">四元数的 Z 分量。</param>
        /// <param name="w">四元数的 W 分量。</param>
        public FixedPointQuaternion(FixedPoint64 x, FixedPoint64 y, FixedPoint64 z, FixedPoint64 w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        /// <summary>
        /// 由整型四元数 <see cref="QuaternionInt"/> 构造定点四元数。
        /// </summary>
        /// <param name="quaternionInt">用于初始化各分量的整型四元数。</param>
        public FixedPointQuaternion(QuaternionInt quaternionInt)
        {
            x = quaternionInt.x;
            y = quaternionInt.y;
            z = quaternionInt.z;
            w = quaternionInt.w;
        }

        /// <summary>
        /// 设置四元数的各个分量。
        /// </summary>
        /// <param name="new_x">新的 X 分量。</param>
        /// <param name="new_y">新的 Y 分量。</param>
        /// <param name="new_z">新的 Z 分量。</param>
        /// <param name="new_w">新的 W 分量。</param>
        public void Set(FixedPoint64 new_x, FixedPoint64 new_y, FixedPoint64 new_z, FixedPoint64 new_w)
        {
            x = new_x;
            y = new_y;
            z = new_z;
            w = new_w;
        }

        /// <summary>
        /// 将本四元数设置为从一个方向旋转到另一个方向的旋转。
        /// </summary>
        /// <param name="fromDirection">起始方向。</param>
        /// <param name="toDirection">目标方向。</param>
        public void SetFromToRotation(FixedPointVector3 fromDirection, FixedPointVector3 toDirection)
        {
            FixedPointQuaternion targetRotation = FixedPointQuaternion.FromToRotation(fromDirection, toDirection);
            this.Set(targetRotation.x, targetRotation.y, targetRotation.z, targetRotation.w);
        }

        /// <summary>
        /// 获取该四元数对应的欧拉角（单位：度）。
        /// </summary>
        public FixedPointVector3 eulerAngles
        {
            get
            {
                var result = new FixedPointVector3();

                var y_sqr = y * y;
                var t0 = -2.0f * (y_sqr + z * z) + 1.0f;
                var t1 = +2.0f * (x * y - w * z);
                var t2 = -2.0f * (x * z + w * y);
                var t3 = +2.0f * (y * z - w * x);
                var t4 = -2.0f * (x * x + y_sqr) + 1.0f;

                t2 = t2 > 1.0f ? 1.0f : t2;
                t2 = t2 < -1.0f ? -1.0f : t2;

                result.x = FixedPoint64.Atan2(t3, t4) * FixedPoint64.Rad2Deg;
                result.y = FixedPoint64.Asin(t2) * FixedPoint64.Rad2Deg;
                result.z = FixedPoint64.Atan2(t1, t0) * FixedPoint64.Rad2Deg;

                return result * -1;
            }
        }

        /// <summary>
        /// 计算两个四元数之间的夹角（单位：度）。
        /// </summary>
        /// <param name="a">第一个四元数。</param>
        /// <param name="b">第二个四元数。</param>
        /// <returns>两个旋转之间的夹角。</returns>
        public static FixedPoint64 Angle(FixedPointQuaternion a, FixedPointQuaternion b)
        {
            FixedPointQuaternion aInv = FixedPointQuaternion.Inverse(a);
            FixedPointQuaternion f = b * aInv;

            FixedPoint64 angle = FixedPoint64.Acos(f.w) * 2 * FixedPoint64.Rad2Deg;

            if (angle > 180)
            {
                angle = 360 - angle;
            }

            return angle;
        }
        
        #region 四元数相加

        /// <summary>
        /// 将两个四元数相加。
        /// </summary>
        /// <param name="quaternion1">第一个四元数。</param>
        /// <param name="quaternion2">第二个四元数。</param>
        /// <returns>两个四元数的和。</returns>
        public static FixedPointQuaternion Add(FixedPointQuaternion quaternion1, FixedPointQuaternion quaternion2)
        {
            Add(ref quaternion1, ref quaternion2, out var result);
            return result;
        }

        /// <summary>
        /// 创建一个朝向指定前方向的旋转（上方向默认为世界 Y 轴）。
        /// </summary>
        /// <param name="forward">期望朝向的前方向。</param>
        /// <returns>对应的旋转四元数。</returns>
        public static FixedPointQuaternion LookRotation(FixedPointVector3 forward)
        {
            return CreateFromMatrix(FixedPointMatrix.LookAt(forward, FixedPointVector3.up));
        }

        /// <summary>
        /// 创建一个朝向指定前方向、并以指定上方向为参考的旋转。
        /// </summary>
        /// <param name="forward">期望朝向的前方向。</param>
        /// <param name="upwards">参考的上方向。</param>
        /// <returns>对应的旋转四元数。</returns>
        public static FixedPointQuaternion LookRotation(FixedPointVector3 forward, FixedPointVector3 upwards)
        {
            return CreateFromMatrix(FixedPointMatrix.LookAt(forward, upwards));
        }

        /// <summary>
        /// 在两个四元数之间进行球面线性插值（Slerp）。
        /// </summary>
        /// <param name="from">起始旋转。</param>
        /// <param name="to">目标旋转。</param>
        /// <param name="t">插值系数，会被限制到 [0, 1]。</param>
        /// <returns>插值得到的旋转四元数。</returns>
        public static FixedPointQuaternion Slerp(FixedPointQuaternion from, FixedPointQuaternion to, FixedPoint64 t)
        {
            t = FixedPointMath.Clamp(t, 0, 1);

            FixedPoint64 dot = Dot(from, to);

            if (dot < 0.0f)
            {
                to = Multiply(to, -1);
                dot = -dot;
            }

            FixedPoint64 halfTheta = FixedPoint64.Acos(dot);

            return Multiply(
                Multiply(from, FixedPoint64.Sin((1 - t) * halfTheta)) + Multiply(to, FixedPoint64.Sin(t * halfTheta)),
                1 / FixedPoint64.Sin(halfTheta));
        }

        /// <summary>
        /// 以限定的最大角度，从一个旋转朝目标旋转方向插值。
        /// </summary>
        /// <param name="from">起始旋转。</param>
        /// <param name="to">目标旋转。</param>
        /// <param name="maxDegreesDelta">本次允许旋转的最大角度（单位：度）。</param>
        /// <returns>朝目标旋转后得到的旋转四元数。</returns>
        public static FixedPointQuaternion RotateTowards(FixedPointQuaternion from, FixedPointQuaternion to,
            FixedPoint64 maxDegreesDelta)
        {
            FixedPoint64 dot = Dot(from, to);

            if (dot < 0.0f)
            {
                to = Multiply(to, -1);
                dot = -dot;
            }

            FixedPoint64 halfTheta = FixedPoint64.Acos(dot);
            FixedPoint64 theta = halfTheta * 2;

            maxDegreesDelta *= FixedPoint64.Deg2Rad;

            if (maxDegreesDelta >= theta)
            {
                return to;
            }

            maxDegreesDelta /= theta;

            return Multiply(
                Multiply(from, FixedPoint64.Sin((1 - maxDegreesDelta) * halfTheta)) +
                Multiply(to, FixedPoint64.Sin(maxDegreesDelta * halfTheta)), 1 / FixedPoint64.Sin(halfTheta));
        }

        /// <summary>
        /// 根据绕 X、Y、Z 轴的欧拉角（单位：度）创建旋转四元数。
        /// </summary>
        /// <param name="x">绕 X 轴的旋转角（俯仰），单位：度。</param>
        /// <param name="y">绕 Y 轴的旋转角（偏航），单位：度。</param>
        /// <param name="z">绕 Z 轴的旋转角（翻滚），单位：度。</param>
        /// <returns>对应的旋转四元数。</returns>
        public static FixedPointQuaternion Euler(FixedPoint64 x, FixedPoint64 y, FixedPoint64 z)
        {
            x *= FixedPoint64.Deg2Rad;
            y *= FixedPoint64.Deg2Rad;
            z *= FixedPoint64.Deg2Rad;

            CreateFromYawPitchRoll(y, x, z, out var rotation);

            return rotation;
        }

        /// <summary>
        /// 根据欧拉角向量（单位：度）创建旋转四元数。
        /// </summary>
        /// <param name="eulerAngles">欧拉角向量，分量分别对应绕 X、Y、Z 轴的旋转角。</param>
        /// <returns>对应的旋转四元数。</returns>
        public static FixedPointQuaternion Euler(FixedPointVector3 eulerAngles)
        {
            return Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);
        }

        /// <summary>
        /// 创建一个绕指定轴旋转指定角度的四元数。
        /// </summary>
        /// <param name="angle">旋转角度（单位：度）。</param>
        /// <param name="axis">旋转轴。</param>
        /// <returns>对应的旋转四元数。</returns>
        public static FixedPointQuaternion AngleAxis(FixedPoint64 angle, FixedPointVector3 axis)
        {
            axis = axis * FixedPoint64.Deg2Rad;
            axis.Normalize();

            FixedPoint64 halfAngle = angle * FixedPoint64.Deg2Rad * FixedPoint64.Half;

            FixedPointQuaternion rotation;
            FixedPoint64 sin = FixedPoint64.Sin(halfAngle);

            rotation.x = axis.x * sin;
            rotation.y = axis.y * sin;
            rotation.z = axis.z * sin;
            rotation.w = FixedPoint64.Cos(halfAngle);

            return rotation;
        }

        /// <summary>
        /// 根据偏航（Yaw）、俯仰（Pitch）、翻滚（Roll）角（单位：弧度）创建旋转四元数。
        /// </summary>
        /// <param name="yaw">绕 Y 轴的偏航角（单位：弧度）。</param>
        /// <param name="pitch">绕 X 轴的俯仰角（单位：弧度）。</param>
        /// <param name="roll">绕 Z 轴的翻滚角（单位：弧度）。</param>
        /// <param name="result">输出的旋转四元数。</param>
        public static void CreateFromYawPitchRoll(FixedPoint64 yaw, FixedPoint64 pitch, FixedPoint64 roll,
            out FixedPointQuaternion result)
        {
            FixedPoint64 num9 = roll * FixedPoint64.Half;
            FixedPoint64 num6 = FixedPoint64.Sin(num9);
            FixedPoint64 num5 = FixedPoint64.Cos(num9);
            FixedPoint64 num8 = pitch * FixedPoint64.Half;
            FixedPoint64 num4 = FixedPoint64.Sin(num8);
            FixedPoint64 num3 = FixedPoint64.Cos(num8);
            FixedPoint64 num7 = yaw * FixedPoint64.Half;
            FixedPoint64 num2 = FixedPoint64.Sin(num7);
            FixedPoint64 num = FixedPoint64.Cos(num7);
            result.x = ((num * num4) * num5) + ((num2 * num3) * num6);
            result.y = ((num2 * num3) * num5) - ((num * num4) * num6);
            result.z = ((num * num3) * num6) - ((num2 * num4) * num5);
            result.w = ((num * num3) * num5) + ((num2 * num4) * num6);
        }

        /// <summary>
        /// 将两个四元数相加。
        /// </summary>
        /// <param name="quaternion1">第一个四元数。</param>
        /// <param name="quaternion2">第二个四元数。</param>
        /// <param name="result">输出的两个四元数之和。</param>
        public static void Add(ref FixedPointQuaternion quaternion1, ref FixedPointQuaternion quaternion2,
            out FixedPointQuaternion result)
        {
            result.x = quaternion1.x + quaternion2.x;
            result.y = quaternion1.y + quaternion2.y;
            result.z = quaternion1.z + quaternion2.z;
            result.w = quaternion1.w + quaternion2.w;
        }

        #endregion

        /// <summary>
        /// 计算四元数的共轭（取 X、Y、Z 分量的相反数，W 不变）。
        /// </summary>
        /// <param name="value">输入的四元数。</param>
        /// <returns>共轭四元数。</returns>
        public static FixedPointQuaternion Conjugate(FixedPointQuaternion value)
        {
            FixedPointQuaternion quaternion;
            quaternion.x = -value.x;
            quaternion.y = -value.y;
            quaternion.z = -value.z;
            quaternion.w = value.w;
            return quaternion;
        }

        /// <summary>
        /// 计算两个四元数的点积。
        /// </summary>
        /// <param name="a">第一个四元数。</param>
        /// <param name="b">第二个四元数。</param>
        /// <returns>两个四元数的点积。</returns>
        public static FixedPoint64 Dot(FixedPointQuaternion a, FixedPointQuaternion b)
        {
            return a.w * b.w + a.x * b.x + a.y * b.y + a.z * b.z;
        }

        /// <summary>
        /// 计算四元数的逆（用于反向旋转）。
        /// </summary>
        /// <param name="rotation">输入的旋转四元数。</param>
        /// <returns>逆四元数。</returns>
        public static FixedPointQuaternion Inverse(FixedPointQuaternion rotation)
        {
            FixedPoint64 invNorm = FixedPoint64.One / ((rotation.x * rotation.x) + (rotation.y * rotation.y) +
                                                       (rotation.z * rotation.z) + (rotation.w * rotation.w));
            return FixedPointQuaternion.Multiply(FixedPointQuaternion.Conjugate(rotation), invNorm);
        }

        /// <summary>
        /// 创建一个把指定起始向量旋转到目标向量的四元数。
        /// </summary>
        /// <param name="fromVector">起始向量。</param>
        /// <param name="toVector">目标向量。</param>
        /// <returns>对应的旋转四元数。</returns>
        public static FixedPointQuaternion FromToRotation(FixedPointVector3 fromVector, FixedPointVector3 toVector)
        {
            FixedPointVector3 w = FixedPointVector3.Cross(fromVector, toVector);
            FixedPointQuaternion q =
                new FixedPointQuaternion(w.x, w.y, w.z, FixedPointVector3.Dot(fromVector, toVector));
            q.w += FixedPoint64.Sqrt(fromVector.sqrMagnitude * toVector.sqrMagnitude);
            q.Normalize();

            return q;
        }

        /// <summary>
        /// 在两个四元数之间进行线性插值（插值系数会被限制到 [0, 1]）。
        /// </summary>
        /// <param name="a">起始旋转。</param>
        /// <param name="b">目标旋转。</param>
        /// <param name="t">插值系数，会被限制到 [0, 1]。</param>
        /// <returns>插值得到并归一化后的旋转四元数。</returns>
        public static FixedPointQuaternion Lerp(FixedPointQuaternion a, FixedPointQuaternion b, FixedPoint64 t)
        {
            t = FixedPointMath.Clamp(t, FixedPoint64.Zero, FixedPoint64.One);

            return LerpUnclamped(a, b, t);
        }

        /// <summary>
        /// 在两个四元数之间进行线性插值（不对插值系数做范围限制）。
        /// </summary>
        /// <param name="a">起始旋转。</param>
        /// <param name="b">目标旋转。</param>
        /// <param name="t">插值系数。</param>
        /// <returns>插值得到并归一化后的旋转四元数。</returns>
        public static FixedPointQuaternion LerpUnclamped(FixedPointQuaternion a, FixedPointQuaternion b, FixedPoint64 t)
        {
            FixedPointQuaternion result =
                FixedPointQuaternion.Multiply(a, (1 - t)) + FixedPointQuaternion.Multiply(b, t);
            result.Normalize();

            return result;
        }
        
        #region 四元数相减
        
        /// <summary>
        /// 将两个四元数相减。
        /// </summary>
        /// <param name="quaternion1">第一个四元数。</param>
        /// <param name="quaternion2">第二个四元数。</param>
        /// <returns>两个四元数的差。</returns>
        public static FixedPointQuaternion Subtract(FixedPointQuaternion quaternion1, FixedPointQuaternion quaternion2)
        {
            Subtract(ref quaternion1, ref quaternion2, out var result);
            return result;
        }

        /// <summary>
        /// 将两个四元数相减。
        /// </summary>
        /// <param name="quaternion1">第一个四元数。</param>
        /// <param name="quaternion2">第二个四元数。</param>
        /// <param name="result">输出的两个四元数之差。</param>
        public static void Subtract(ref FixedPointQuaternion quaternion1, ref FixedPointQuaternion quaternion2,
            out FixedPointQuaternion result)
        {
            result.x = quaternion1.x - quaternion2.x;
            result.y = quaternion1.y - quaternion2.y;
            result.z = quaternion1.z - quaternion2.z;
            result.w = quaternion1.w - quaternion2.w;
        }

        #endregion
        
        #region 四元数相乘

        /// <summary>
        /// 将两个四元数相乘（即组合两个旋转）。
        /// </summary>
        /// <param name="quaternion1">第一个四元数。</param>
        /// <param name="quaternion2">第二个四元数。</param>
        /// <returns>两个四元数的乘积。</returns>
        public static FixedPointQuaternion Multiply(FixedPointQuaternion quaternion1, FixedPointQuaternion quaternion2)
        {
            Multiply(ref quaternion1, ref quaternion2, out var result);
            return result;
        }

        /// <summary>
        /// 将两个四元数相乘（即组合两个旋转）。
        /// </summary>
        /// <param name="quaternion1">第一个四元数。</param>
        /// <param name="quaternion2">第二个四元数。</param>
        /// <param name="result">输出的两个四元数之积。</param>
        public static void Multiply(ref FixedPointQuaternion quaternion1, ref FixedPointQuaternion quaternion2,
            out FixedPointQuaternion result)
        {
            FixedPoint64 x = quaternion1.x;
            FixedPoint64 y = quaternion1.y;
            FixedPoint64 z = quaternion1.z;
            FixedPoint64 w = quaternion1.w;
            FixedPoint64 num4 = quaternion2.x;
            FixedPoint64 num3 = quaternion2.y;
            FixedPoint64 num2 = quaternion2.z;
            FixedPoint64 num = quaternion2.w;
            FixedPoint64 num12 = (y * num2) - (z * num3);
            FixedPoint64 num11 = (z * num4) - (x * num2);
            FixedPoint64 num10 = (x * num3) - (y * num4);
            FixedPoint64 num9 = ((x * num4) + (y * num3)) + (z * num2);
            result.x = ((x * num) + (num4 * w)) + num12;
            result.y = ((y * num) + (num3 * w)) + num11;
            result.z = ((z * num) + (num2 * w)) + num10;
            result.w = (w * num) - num9;
        }

        #endregion
        
        #region 四元数缩放

        /// <summary>
        /// 对四元数进行缩放。
        /// </summary>
        /// <param name="quaternion1">要缩放的四元数。</param>
        /// <param name="scaleFactor">缩放因子。</param>
        /// <returns>缩放后的四元数。</returns>
        public static FixedPointQuaternion Multiply(FixedPointQuaternion quaternion1, FixedPoint64 scaleFactor)
        {
            Multiply(ref quaternion1, scaleFactor, out var result);
            return result;
        }

        /// <summary>
        /// 对四元数进行缩放。
        /// </summary>
        /// <param name="quaternion1">要缩放的四元数。</param>
        /// <param name="scaleFactor">缩放因子。</param>
        /// <param name="result">输出的缩放后四元数。</param>
        public static void Multiply(ref FixedPointQuaternion quaternion1, FixedPoint64 scaleFactor,
            out FixedPointQuaternion result)
        {
            result.x = quaternion1.x * scaleFactor;
            result.y = quaternion1.y * scaleFactor;
            result.z = quaternion1.z * scaleFactor;
            result.w = quaternion1.w * scaleFactor;
        }

        #endregion
        
        #region 四元数归一化

        /// <summary>
        /// 将四元数归一化，使其长度为 1。
        /// </summary>
        public void Normalize()
        {
            FixedPoint64 num2 = (((this.x * this.x) + (this.y * this.y)) + (this.z * this.z)) + (this.w * this.w);
            FixedPoint64 num = 1 / (FixedPoint64.Sqrt(num2));
            this.x *= num;
            this.y *= num;
            this.z *= num;
            this.w *= num;
        }

        #endregion
        
        #region 从矩阵创建四元数

        /// <summary>
        /// 从旋转矩阵创建四元数。
        /// </summary>
        /// <param name="matrix">表示旋转朝向的矩阵。</param>
        /// <returns>表示该朝向的四元数。</returns>
        public static FixedPointQuaternion CreateFromMatrix(FixedPointMatrix matrix)
        {
            CreateFromMatrix(ref matrix, out var result);
            return result;
        }

        /// <summary>
        /// 从旋转矩阵创建四元数。
        /// </summary>
        /// <param name="matrix">表示旋转朝向的矩阵。</param>
        /// <param name="result">输出的表示该朝向的四元数。</param>
        public static void CreateFromMatrix(ref FixedPointMatrix matrix, out FixedPointQuaternion result)
        {
            FixedPoint64 num8 = (matrix.M11 + matrix.M22) + matrix.M33;

            if (num8 > FixedPoint64.Zero)
            {
                FixedPoint64 num = FixedPoint64.Sqrt((num8 + FixedPoint64.One));
                result.w = num * FixedPoint64.Half;
                num = FixedPoint64.Half / num;
                result.x = (matrix.M23 - matrix.M32) * num;
                result.y = (matrix.M31 - matrix.M13) * num;
                result.z = (matrix.M12 - matrix.M21) * num;
            }
            else if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
            {
                FixedPoint64 num7 = FixedPoint64.Sqrt((((FixedPoint64.One + matrix.M11) - matrix.M22) - matrix.M33));
                FixedPoint64 num4 = FixedPoint64.Half / num7;
                result.x = FixedPoint64.Half * num7;
                result.y = (matrix.M12 + matrix.M21) * num4;
                result.z = (matrix.M13 + matrix.M31) * num4;
                result.w = (matrix.M23 - matrix.M32) * num4;
            }
            else if (matrix.M22 > matrix.M33)
            {
                FixedPoint64 num6 = FixedPoint64.Sqrt((((FixedPoint64.One + matrix.M22) - matrix.M11) - matrix.M33));
                FixedPoint64 num3 = FixedPoint64.Half / num6;
                result.x = (matrix.M21 + matrix.M12) * num3;
                result.y = FixedPoint64.Half * num6;
                result.z = (matrix.M32 + matrix.M23) * num3;
                result.w = (matrix.M31 - matrix.M13) * num3;
            }
            else
            {
                FixedPoint64 num5 = FixedPoint64.Sqrt((((FixedPoint64.One + matrix.M33) - matrix.M11) - matrix.M22));
                FixedPoint64 num2 = FixedPoint64.Half / num5;
                result.x = (matrix.M31 + matrix.M13) * num2;
                result.y = (matrix.M32 + matrix.M23) * num2;
                result.z = FixedPoint64.Half * num5;
                result.w = (matrix.M12 - matrix.M21) * num2;
            }
        }

        #endregion
        
        #region 乘法运算符

        /// <summary>
        /// 四元数乘法运算符（组合两个旋转）。
        /// </summary>
        /// <param name="value1">第一个四元数。</param>
        /// <param name="value2">第二个四元数。</param>
        /// <returns>两个四元数的乘积。</returns>
        public static FixedPointQuaternion operator *(FixedPointQuaternion value1, FixedPointQuaternion value2)
        {
            Multiply(ref value1, ref value2, out var result);
            return result;
        }

        #endregion

        #region 加法运算符

        /// <summary>
        /// 四元数加法运算符。
        /// </summary>
        /// <param name="value1">第一个四元数。</param>
        /// <param name="value2">第二个四元数。</param>
        /// <returns>两个四元数的和。</returns>
        public static FixedPointQuaternion operator +(FixedPointQuaternion value1, FixedPointQuaternion value2)
        {
            Add(ref value1, ref value2, out var result);
            return result;
        }

        #endregion

        #region 减法运算符

        /// <summary>
        /// 四元数减法运算符。
        /// </summary>
        /// <param name="value1">第一个四元数。</param>
        /// <param name="value2">第二个四元数。</param>
        /// <returns>两个四元数的差。</returns>
        public static FixedPointQuaternion operator -(FixedPointQuaternion value1, FixedPointQuaternion value2)
        {
            Subtract(ref value1, ref value2, out var result);
            return result;
        }

        #endregion

        /// <summary>
        /// 用四元数旋转一个三维向量。
        /// </summary>
        /// <param name="quat">旋转四元数。</param>
        /// <param name="vec">要旋转的向量。</param>
        /// <returns>旋转后的向量。</returns>
        public static FixedPointVector3 operator *(FixedPointQuaternion quat, FixedPointVector3 vec)
        {
            var num = quat.x * 2f;
            var num2 = quat.y * 2f;
            var num3 = quat.z * 2f;
            var num4 = quat.x * num;
            var num5 = quat.y * num2;
            var num6 = quat.z * num3;
            var num7 = quat.x * num2;
            var num8 = quat.x * num3;
            var num9 = quat.y * num3;
            var num10 = quat.w * num;
            var num11 = quat.w * num2;
            var num12 = quat.w * num3;

            FixedPointVector3 result;
            result.x = (1f - (num5 + num6)) * vec.x + (num7 - num12) * vec.y + (num8 + num11) * vec.z;
            result.y = (num7 + num12) * vec.x + (1f - (num4 + num6)) * vec.y + (num9 - num10) * vec.z;
            result.z = (num8 - num11) * vec.x + (num9 + num10) * vec.y + (1f - (num4 + num5)) * vec.z;

            return result;
        }


        /// <summary>
        /// 将四元数的各分量同除以一个标量。
        /// </summary>
        /// <param name="value1">被除的四元数。</param>
        /// <param name="value2">除数标量。</param>
        /// <returns>各分量相除后的四元数。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPointQuaternion operator /(FixedPointQuaternion value1, FixedPoint64 value2)
        {
            return new FixedPointQuaternion(value1.x / value2, value1.y / value2, value1.z / value2, value1.w / value2);
        }

        /// <summary>
        /// 返回四元数各分量的字符串表示。
        /// </summary>
        /// <returns>格式化后的字符串。</returns>
        public override string ToString()
        {
            return $"({x.AsFloat():f1}, {y.AsFloat():f1}, {z.AsFloat():f1}, {w.AsFloat():f1})";
        }

#if UNITY_2021_3_OR_NEWER

        /// <summary>
        /// 由 Unity 的 <see cref="Quaternion"/> 构造定点四元数（仅在 Unity 环境下可用）。
        /// </summary>
        /// <param name="quaternion">Unity 四元数。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedPointQuaternion(Quaternion quaternion)
        {
            x = quaternion.x;
            y = quaternion.y;
            z = quaternion.z;
            w = quaternion.w;
        }

        /// <summary>
        /// 将定点四元数转换为 Unity 的 <see cref="Quaternion"/>（仅在 Unity 环境下可用，转换为浮点，仅用于表现层）。
        /// </summary>
        /// <returns>转换后的 Unity 四元数。</returns>
        public Quaternion ToQuaternion()
            => new Quaternion(x.AsFloat(), y.AsFloat(), z.AsFloat(), w.AsFloat());

#endif

    }
}