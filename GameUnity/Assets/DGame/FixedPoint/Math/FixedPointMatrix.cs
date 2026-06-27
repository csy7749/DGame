namespace DGame.FixedPoint
{

    /// <summary>
    /// 定点 3x3 矩阵，常用于确定性的旋转与变换计算。
    /// </summary>
    public struct FixedPointMatrix
    {
        /// <summary>
        /// 第 1 行第 1 列元素。
        /// </summary>
        public FixedPoint64 M11; // 第 1 行向量

        /// <summary>
        /// 第 1 行第 2 列元素。
        /// </summary>
        public FixedPoint64 M12;

        /// <summary>
        /// 第 1 行第 3 列元素。
        /// </summary>
        public FixedPoint64 M13;

        /// <summary>
        /// 第 2 行第 1 列元素。
        /// </summary>
        public FixedPoint64 M21; // 第 2 行向量

        /// <summary>
        /// 第 2 行第 2 列元素。
        /// </summary>
        public FixedPoint64 M22;

        /// <summary>
        /// 第 2 行第 3 列元素。
        /// </summary>
        public FixedPoint64 M23;

        /// <summary>
        /// 第 3 行第 1 列元素。
        /// </summary>
        public FixedPoint64 M31; // 第 3 行向量

        /// <summary>
        /// 第 3 行第 2 列元素。
        /// </summary>
        public FixedPoint64 M32;

        /// <summary>
        /// 第 3 行第 3 列元素。
        /// </summary>
        public FixedPoint64 M33;

        internal static FixedPointMatrix InternalIdentity;

        /// <summary>
        /// 单位矩阵。
        /// </summary>
        public static readonly FixedPointMatrix Identity;

        /// <summary>
        /// 零矩阵（所有元素均为 0）。
        /// </summary>
        public static readonly FixedPointMatrix Zero;

        static FixedPointMatrix()
        {
            Zero = new FixedPointMatrix();

            Identity = new FixedPointMatrix();
            Identity.M11 = FixedPoint64.One;
            Identity.M22 = FixedPoint64.One;
            Identity.M33 = FixedPoint64.One;

            InternalIdentity = Identity;
        }

        /// <summary>
        /// 获取该矩阵对应的欧拉角（单位：度）。
        /// </summary>
        public FixedPointVector3 eulerAngles
        {
            get
            {
                FixedPointVector3 result = new FixedPointVector3();

                result.x = FixedPointMath.Atan2(M32, M33) * FixedPoint64.Rad2Deg;
                result.y = FixedPointMath.Atan2(-M31, FixedPointMath.Sqrt(M32 * M32 + M33 * M33)) *
                           FixedPoint64.Rad2Deg;
                result.z = FixedPointMath.Atan2(M21, M11) * FixedPoint64.Rad2Deg;

                return result * -1;
            }
        }

        /// <summary>
        /// 获取该矩阵对应的旋转四元数。
        /// </summary>
        public FixedPointQuaternion quaternion
        {
            get { return FixedPointQuaternion.CreateFromMatrix(this); }
        }

        /// <summary>
        /// 根据偏航、俯仰、翻滚角创建旋转矩阵。
        /// </summary>
        /// <param name="yaw">绕 Y 轴的偏航角（单位：弧度）。</param>
        /// <param name="pitch">绕 X 轴的俯仰角（单位：弧度）。</param>
        /// <param name="roll">绕 Z 轴的翻滚角（单位：弧度）。</param>
        /// <returns>对应的旋转矩阵。</returns>
        public static FixedPointMatrix CreateFromYawPitchRoll(FixedPoint64 yaw, FixedPoint64 pitch, FixedPoint64 roll)
        {
            FixedPointMatrix matrix;
            FixedPointQuaternion quaternion;
            FixedPointQuaternion.CreateFromYawPitchRoll(yaw, pitch, roll, out quaternion);
            CreateFromQuaternion(ref quaternion, out matrix);
            return matrix;
        }

        /// <summary>
        /// 创建绕 X 轴旋转指定弧度的旋转矩阵。
        /// </summary>
        /// <param name="radians">旋转角度（单位：弧度）。</param>
        /// <returns>绕 X 轴旋转的矩阵。</returns>
        public static FixedPointMatrix CreateRotationX(FixedPoint64 radians)
        {
            FixedPointMatrix matrix;
            FixedPoint64 num2 = FixedPoint64.Cos(radians);
            FixedPoint64 num = FixedPoint64.Sin(radians);
            matrix.M11 = FixedPoint64.One;
            matrix.M12 = FixedPoint64.Zero;
            matrix.M13 = FixedPoint64.Zero;
            matrix.M21 = FixedPoint64.Zero;
            matrix.M22 = num2;
            matrix.M23 = num;
            matrix.M31 = FixedPoint64.Zero;
            matrix.M32 = -num;
            matrix.M33 = num2;
            return matrix;
        }

        /// <summary>
        /// 创建绕 X 轴旋转指定弧度的旋转矩阵。
        /// </summary>
        /// <param name="radians">旋转角度（单位：弧度）。</param>
        /// <param name="result">输出的绕 X 轴旋转矩阵。</param>
        public static void CreateRotationX(FixedPoint64 radians, out FixedPointMatrix result)
        {
            FixedPoint64 num2 = FixedPoint64.Cos(radians);
            FixedPoint64 num = FixedPoint64.Sin(radians);
            result.M11 = FixedPoint64.One;
            result.M12 = FixedPoint64.Zero;
            result.M13 = FixedPoint64.Zero;
            result.M21 = FixedPoint64.Zero;
            result.M22 = num2;
            result.M23 = num;
            result.M31 = FixedPoint64.Zero;
            result.M32 = -num;
            result.M33 = num2;
        }

        /// <summary>
        /// 创建绕 Y 轴旋转指定弧度的旋转矩阵。
        /// </summary>
        /// <param name="radians">旋转角度（单位：弧度）。</param>
        /// <returns>绕 Y 轴旋转的矩阵。</returns>
        public static FixedPointMatrix CreateRotationY(FixedPoint64 radians)
        {
            FixedPointMatrix matrix;
            FixedPoint64 num2 = FixedPoint64.Cos(radians);
            FixedPoint64 num = FixedPoint64.Sin(radians);
            matrix.M11 = num2;
            matrix.M12 = FixedPoint64.Zero;
            matrix.M13 = -num;
            matrix.M21 = FixedPoint64.Zero;
            matrix.M22 = FixedPoint64.One;
            matrix.M23 = FixedPoint64.Zero;
            matrix.M31 = num;
            matrix.M32 = FixedPoint64.Zero;
            matrix.M33 = num2;
            return matrix;
        }

        /// <summary>
        /// 创建绕 Y 轴旋转指定弧度的旋转矩阵。
        /// </summary>
        /// <param name="radians">旋转角度（单位：弧度）。</param>
        /// <param name="result">输出的绕 Y 轴旋转矩阵。</param>
        public static void CreateRotationY(FixedPoint64 radians, out FixedPointMatrix result)
        {
            FixedPoint64 num2 = FixedPoint64.Cos(radians);
            FixedPoint64 num = FixedPoint64.Sin(radians);
            result.M11 = num2;
            result.M12 = FixedPoint64.Zero;
            result.M13 = -num;
            result.M21 = FixedPoint64.Zero;
            result.M22 = FixedPoint64.One;
            result.M23 = FixedPoint64.Zero;
            result.M31 = num;
            result.M32 = FixedPoint64.Zero;
            result.M33 = num2;
        }

        /// <summary>
        /// 创建绕 Z 轴旋转指定弧度的旋转矩阵。
        /// </summary>
        /// <param name="radians">旋转角度（单位：弧度）。</param>
        /// <returns>绕 Z 轴旋转的矩阵。</returns>
        public static FixedPointMatrix CreateRotationZ(FixedPoint64 radians)
        {
            FixedPointMatrix matrix;
            FixedPoint64 num2 = FixedPoint64.Cos(radians);
            FixedPoint64 num = FixedPoint64.Sin(radians);
            matrix.M11 = num2;
            matrix.M12 = num;
            matrix.M13 = FixedPoint64.Zero;
            matrix.M21 = -num;
            matrix.M22 = num2;
            matrix.M23 = FixedPoint64.Zero;
            matrix.M31 = FixedPoint64.Zero;
            matrix.M32 = FixedPoint64.Zero;
            matrix.M33 = FixedPoint64.One;
            return matrix;
        }


        /// <summary>
        /// 创建绕 Z 轴旋转指定弧度的旋转矩阵。
        /// </summary>
        /// <param name="radians">旋转角度（单位：弧度）。</param>
        /// <param name="result">输出的绕 Z 轴旋转矩阵。</param>
        public static void CreateRotationZ(FixedPoint64 radians, out FixedPointMatrix result)
        {
            FixedPoint64 num2 = FixedPoint64.Cos(radians);
            FixedPoint64 num = FixedPoint64.Sin(radians);
            result.M11 = num2;
            result.M12 = num;
            result.M13 = FixedPoint64.Zero;
            result.M21 = -num;
            result.M22 = num2;
            result.M23 = FixedPoint64.Zero;
            result.M31 = FixedPoint64.Zero;
            result.M32 = FixedPoint64.Zero;
            result.M33 = FixedPoint64.One;
        }

        /// <summary>
        /// 使用 9 个元素初始化定点矩阵。
        /// </summary>
        /// <param name="m11">第 1 行第 1 列元素。</param>
        /// <param name="m12">第 1 行第 2 列元素。</param>
        /// <param name="m13">第 1 行第 3 列元素。</param>
        /// <param name="m21">第 2 行第 1 列元素。</param>
        /// <param name="m22">第 2 行第 2 列元素。</param>
        /// <param name="m23">第 2 行第 3 列元素。</param>
        /// <param name="m31">第 3 行第 1 列元素。</param>
        /// <param name="m32">第 3 行第 2 列元素。</param>
        /// <param name="m33">第 3 行第 3 列元素。</param>

        #region 使用 9 个元素的构造函数

        public FixedPointMatrix(FixedPoint64 m11, FixedPoint64 m12, FixedPoint64 m13, FixedPoint64 m21,
            FixedPoint64 m22, FixedPoint64 m23, FixedPoint64 m31, FixedPoint64 m32, FixedPoint64 m33)
        {
            this.M11 = m11;
            this.M12 = m12;
            this.M13 = m13;
            this.M21 = m21;
            this.M22 = m22;
            this.M23 = m23;
            this.M31 = m31;
            this.M32 = m32;
            this.M33 = m33;
        }

        #endregion

        #region 矩阵相乘

        /// <summary>
        /// 矩阵相乘。注意：矩阵乘法不满足交换律。
        /// </summary>
        /// <param name="matrix1">第一个矩阵。</param>
        /// <param name="matrix2">第二个矩阵。</param>
        /// <returns>两个矩阵的乘积。</returns>
        public static FixedPointMatrix Multiply(FixedPointMatrix matrix1, FixedPointMatrix matrix2)
        {
            FixedPointMatrix result;
            FixedPointMatrix.Multiply(ref matrix1, ref matrix2, out result);
            return result;
        }

        /// <summary>
        /// 矩阵相乘。注意：矩阵乘法不满足交换律。
        /// </summary>
        /// <param name="matrix1">第一个矩阵。</param>
        /// <param name="matrix2">第二个矩阵。</param>
        /// <param name="result">输出的两个矩阵之积。</param>
        public static void Multiply(ref FixedPointMatrix matrix1, ref FixedPointMatrix matrix2,
            out FixedPointMatrix result)
        {
            FixedPoint64 num0 = ((matrix1.M11 * matrix2.M11) + (matrix1.M12 * matrix2.M21)) +
                                (matrix1.M13 * matrix2.M31);
            FixedPoint64 num1 = ((matrix1.M11 * matrix2.M12) + (matrix1.M12 * matrix2.M22)) +
                                (matrix1.M13 * matrix2.M32);
            FixedPoint64 num2 = ((matrix1.M11 * matrix2.M13) + (matrix1.M12 * matrix2.M23)) +
                                (matrix1.M13 * matrix2.M33);
            FixedPoint64 num3 = ((matrix1.M21 * matrix2.M11) + (matrix1.M22 * matrix2.M21)) +
                                (matrix1.M23 * matrix2.M31);
            FixedPoint64 num4 = ((matrix1.M21 * matrix2.M12) + (matrix1.M22 * matrix2.M22)) +
                                (matrix1.M23 * matrix2.M32);
            FixedPoint64 num5 = ((matrix1.M21 * matrix2.M13) + (matrix1.M22 * matrix2.M23)) +
                                (matrix1.M23 * matrix2.M33);
            FixedPoint64 num6 = ((matrix1.M31 * matrix2.M11) + (matrix1.M32 * matrix2.M21)) +
                                (matrix1.M33 * matrix2.M31);
            FixedPoint64 num7 = ((matrix1.M31 * matrix2.M12) + (matrix1.M32 * matrix2.M22)) +
                                (matrix1.M33 * matrix2.M32);
            FixedPoint64 num8 = ((matrix1.M31 * matrix2.M13) + (matrix1.M32 * matrix2.M23)) +
                                (matrix1.M33 * matrix2.M33);

            result.M11 = num0;
            result.M12 = num1;
            result.M13 = num2;
            result.M21 = num3;
            result.M22 = num4;
            result.M23 = num5;
            result.M31 = num6;
            result.M32 = num7;
            result.M33 = num8;
        }

        #endregion

        #region 矩阵相加

        /// <summary>
        /// 矩阵相加。
        /// </summary>
        /// <param name="matrix1">第一个矩阵。</param>
        /// <param name="matrix2">第二个矩阵。</param>
        /// <returns>两个矩阵的和。</returns>
        public static FixedPointMatrix Add(FixedPointMatrix matrix1, FixedPointMatrix matrix2)
        {
            FixedPointMatrix result;
            FixedPointMatrix.Add(ref matrix1, ref matrix2, out result);
            return result;
        }

        /// <summary>
        /// 矩阵相加。
        /// </summary>
        /// <param name="matrix1">第一个矩阵。</param>
        /// <param name="matrix2">第二个矩阵。</param>
        /// <param name="result">输出的两个矩阵之和。</param>
        public static void Add(ref FixedPointMatrix matrix1, ref FixedPointMatrix matrix2, out FixedPointMatrix result)
        {
            result.M11 = matrix1.M11 + matrix2.M11;
            result.M12 = matrix1.M12 + matrix2.M12;
            result.M13 = matrix1.M13 + matrix2.M13;
            result.M21 = matrix1.M21 + matrix2.M21;
            result.M22 = matrix1.M22 + matrix2.M22;
            result.M23 = matrix1.M23 + matrix2.M23;
            result.M31 = matrix1.M31 + matrix2.M31;
            result.M32 = matrix1.M32 + matrix2.M32;
            result.M33 = matrix1.M33 + matrix2.M33;
        }

        #endregion

        #region 矩阵求逆

        /// <summary>
        /// 计算指定矩阵的逆矩阵。
        /// </summary>
        /// <param name="matrix">要求逆的矩阵。</param>
        /// <returns>逆矩阵。</returns>
        public static FixedPointMatrix Inverse(FixedPointMatrix matrix)
        {
            FixedPointMatrix result;
            FixedPointMatrix.Inverse(ref matrix, out result);
            return result;
        }

        /// <summary>
        /// 计算矩阵的行列式。
        /// </summary>
        /// <returns>矩阵的行列式值。</returns>
        public FixedPoint64 Determinant()
        {
            return M11 * M22 * M33 + M12 * M23 * M31 + M13 * M21 * M32 -
                   M31 * M22 * M13 - M32 * M23 * M11 - M33 * M21 * M12;
        }

        /// <summary>
        /// 通过伴随矩阵除以行列式的方式计算矩阵的逆。
        /// </summary>
        /// <param name="matrix">要求逆的矩阵。</param>
        /// <param name="result">输出的逆矩阵。</param>
        public static void Invert(ref FixedPointMatrix matrix, out FixedPointMatrix result)
        {
            FixedPoint64 determinantInverse = 1 / matrix.Determinant();
            FixedPoint64 m11 = (matrix.M22 * matrix.M33 - matrix.M23 * matrix.M32) * determinantInverse;
            FixedPoint64 m12 = (matrix.M13 * matrix.M32 - matrix.M33 * matrix.M12) * determinantInverse;
            FixedPoint64 m13 = (matrix.M12 * matrix.M23 - matrix.M22 * matrix.M13) * determinantInverse;

            FixedPoint64 m21 = (matrix.M23 * matrix.M31 - matrix.M21 * matrix.M33) * determinantInverse;
            FixedPoint64 m22 = (matrix.M11 * matrix.M33 - matrix.M13 * matrix.M31) * determinantInverse;
            FixedPoint64 m23 = (matrix.M13 * matrix.M21 - matrix.M11 * matrix.M23) * determinantInverse;

            FixedPoint64 m31 = (matrix.M21 * matrix.M32 - matrix.M22 * matrix.M31) * determinantInverse;
            FixedPoint64 m32 = (matrix.M12 * matrix.M31 - matrix.M11 * matrix.M32) * determinantInverse;
            FixedPoint64 m33 = (matrix.M11 * matrix.M22 - matrix.M12 * matrix.M21) * determinantInverse;

            result.M11 = m11;
            result.M12 = m12;
            result.M13 = m13;

            result.M21 = m21;
            result.M22 = m22;
            result.M23 = m23;

            result.M31 = m31;
            result.M32 = m32;
            result.M33 = m33;
        }

        /// <summary>
        /// 计算指定矩阵的逆矩阵。
        /// </summary>
        /// <param name="matrix">要求逆的矩阵。</param>
        /// <param name="result">输出的逆矩阵。</param>
        public static void Inverse(ref FixedPointMatrix matrix, out FixedPointMatrix result)
        {
            FixedPoint64 det = 1024 * matrix.M11 * matrix.M22 * matrix.M33 -
                               1024 * matrix.M11 * matrix.M23 * matrix.M32 -
                               1024 * matrix.M12 * matrix.M21 * matrix.M33 +
                               1024 * matrix.M12 * matrix.M23 * matrix.M31 +
                               1024 * matrix.M13 * matrix.M21 * matrix.M32 -
                               1024 * matrix.M13 * matrix.M22 * matrix.M31;

            FixedPoint64 num11 = 1024 * matrix.M22 * matrix.M33 - 1024 * matrix.M23 * matrix.M32;
            FixedPoint64 num12 = 1024 * matrix.M13 * matrix.M32 - 1024 * matrix.M12 * matrix.M33;
            FixedPoint64 num13 = 1024 * matrix.M12 * matrix.M23 - 1024 * matrix.M22 * matrix.M13;

            FixedPoint64 num21 = 1024 * matrix.M23 * matrix.M31 - 1024 * matrix.M33 * matrix.M21;
            FixedPoint64 num22 = 1024 * matrix.M11 * matrix.M33 - 1024 * matrix.M31 * matrix.M13;
            FixedPoint64 num23 = 1024 * matrix.M13 * matrix.M21 - 1024 * matrix.M23 * matrix.M11;

            FixedPoint64 num31 = 1024 * matrix.M21 * matrix.M32 - 1024 * matrix.M31 * matrix.M22;
            FixedPoint64 num32 = 1024 * matrix.M12 * matrix.M31 - 1024 * matrix.M32 * matrix.M11;
            FixedPoint64 num33 = 1024 * matrix.M11 * matrix.M22 - 1024 * matrix.M21 * matrix.M12;

            if (det == 0)
            {
                result.M11 = FixedPoint64.PositiveInfinity;
                result.M12 = FixedPoint64.PositiveInfinity;
                result.M13 = FixedPoint64.PositiveInfinity;
                result.M21 = FixedPoint64.PositiveInfinity;
                result.M22 = FixedPoint64.PositiveInfinity;
                result.M23 = FixedPoint64.PositiveInfinity;
                result.M31 = FixedPoint64.PositiveInfinity;
                result.M32 = FixedPoint64.PositiveInfinity;
                result.M33 = FixedPoint64.PositiveInfinity;
            }
            else
            {
                result.M11 = num11 / det;
                result.M12 = num12 / det;
                result.M13 = num13 / det;
                result.M21 = num21 / det;
                result.M22 = num22 / det;
                result.M23 = num23 / det;
                result.M31 = num31 / det;
                result.M32 = num32 / det;
                result.M33 = num33 / det;
            }

        }

        #endregion

        #region 矩阵乘以缩放因子

        /// <summary>
        /// 将矩阵乘以一个缩放因子。
        /// </summary>
        /// <param name="matrix1">要缩放的矩阵。</param>
        /// <param name="scaleFactor">缩放因子。</param>
        /// <returns>缩放后的矩阵。</returns>
        public static FixedPointMatrix Multiply(FixedPointMatrix matrix1, FixedPoint64 scaleFactor)
        {
            FixedPointMatrix result;
            FixedPointMatrix.Multiply(ref matrix1, scaleFactor, out result);
            return result;
        }

        /// <summary>
        /// 将矩阵乘以一个缩放因子。
        /// </summary>
        /// <param name="matrix1">要缩放的矩阵。</param>
        /// <param name="scaleFactor">缩放因子。</param>
        /// <param name="result">输出的缩放后矩阵。</param>
        public static void Multiply(ref FixedPointMatrix matrix1, FixedPoint64 scaleFactor, out FixedPointMatrix result)
        {
            FixedPoint64 num = scaleFactor;
            result.M11 = matrix1.M11 * num;
            result.M12 = matrix1.M12 * num;
            result.M13 = matrix1.M13 * num;
            result.M21 = matrix1.M21 * num;
            result.M22 = matrix1.M22 * num;
            result.M23 = matrix1.M23 * num;
            result.M31 = matrix1.M31 * num;
            result.M32 = matrix1.M32 * num;
            result.M33 = matrix1.M33 * num;
        }

        #endregion

        #region 从四元数创建矩阵

        /// <summary>
        /// 根据观察位置与目标点创建朝向矩阵（上方向默认为世界 Y 轴）。
        /// </summary>
        /// <param name="position">观察者位置。</param>
        /// <param name="target">注视的目标点。</param>
        /// <returns>对应的朝向矩阵。</returns>
        public static FixedPointMatrix CreateFromLookAt(FixedPointVector3 position, FixedPointVector3 target)
        {
            FixedPointMatrix result;
            LookAt(target - position, FixedPointVector3.up, out result);
            return result;
        }

        /// <summary>
        /// 根据前方向与上方向创建朝向矩阵。
        /// </summary>
        /// <param name="forward">前方向。</param>
        /// <param name="upwards">参考的上方向。</param>
        /// <returns>对应的朝向矩阵。</returns>
        public static FixedPointMatrix LookAt(FixedPointVector3 forward, FixedPointVector3 upwards)
        {
            FixedPointMatrix result;
            LookAt(forward, upwards, out result);

            return result;
        }

        /// <summary>
        /// 根据前方向与上方向创建朝向矩阵。
        /// </summary>
        /// <param name="forward">前方向。</param>
        /// <param name="upwards">参考的上方向。</param>
        /// <param name="result">输出的朝向矩阵。</param>
        public static void LookAt(FixedPointVector3 forward, FixedPointVector3 upwards, out FixedPointMatrix result)
        {
            FixedPointVector3 zaxis = forward;
            zaxis.Normalize();
            FixedPointVector3 xaxis = FixedPointVector3.Cross(upwards, zaxis);
            xaxis.Normalize();
            FixedPointVector3 yaxis = FixedPointVector3.Cross(zaxis, xaxis);

            result.M11 = xaxis.x;
            result.M21 = yaxis.x;
            result.M31 = zaxis.x;
            result.M12 = xaxis.y;
            result.M22 = yaxis.y;
            result.M32 = zaxis.y;
            result.M13 = xaxis.z;
            result.M23 = yaxis.z;
            result.M33 = zaxis.z;
        }

        /// <summary>
        /// 根据四元数创建表示旋转朝向的矩阵。
        /// </summary>
        /// <param name="quaternion">用于创建矩阵的四元数。</param>
        /// <returns>表示该朝向的矩阵。</returns>
        public static FixedPointMatrix CreateFromQuaternion(FixedPointQuaternion quaternion)
        {
            FixedPointMatrix result;
            FixedPointMatrix.CreateFromQuaternion(ref quaternion, out result);
            return result;
        }

        /// <summary>
        /// 根据四元数创建表示旋转朝向的矩阵。
        /// </summary>
        /// <param name="quaternion">用于创建矩阵的四元数。</param>
        /// <param name="result">输出的表示该朝向的矩阵。</param>
        public static void CreateFromQuaternion(ref FixedPointQuaternion quaternion, out FixedPointMatrix result)
        {
            FixedPoint64 num9 = quaternion.x * quaternion.x;
            FixedPoint64 num8 = quaternion.y * quaternion.y;
            FixedPoint64 num7 = quaternion.z * quaternion.z;
            FixedPoint64 num6 = quaternion.x * quaternion.y;
            FixedPoint64 num5 = quaternion.z * quaternion.w;
            FixedPoint64 num4 = quaternion.z * quaternion.x;
            FixedPoint64 num3 = quaternion.y * quaternion.w;
            FixedPoint64 num2 = quaternion.y * quaternion.z;
            FixedPoint64 num = quaternion.x * quaternion.w;
            result.M11 = FixedPoint64.One - (2 * (num8 + num7));
            result.M12 = 2 * (num6 + num5);
            result.M13 = 2 * (num4 - num3);
            result.M21 = 2 * (num6 - num5);
            result.M22 = FixedPoint64.One - (2 * (num7 + num9));
            result.M23 = 2 * (num2 + num);
            result.M31 = 2 * (num4 + num3);
            result.M32 = 2 * (num2 - num);
            result.M33 = FixedPoint64.One - (2 * (num8 + num9));
        }

        #endregion

        #region 矩阵转置

        /// <summary>
        /// 创建矩阵的转置矩阵。
        /// </summary>
        /// <param name="matrix">要转置的矩阵。</param>
        /// <returns>转置后的矩阵。</returns>
        public static FixedPointMatrix Transpose(FixedPointMatrix matrix)
        {
            FixedPointMatrix result;
            FixedPointMatrix.Transpose(ref matrix, out result);
            return result;
        }

        /// <summary>
        /// 创建矩阵的转置矩阵。
        /// </summary>
        /// <param name="matrix">要转置的矩阵。</param>
        /// <param name="result">输出的转置矩阵。</param>
        public static void Transpose(ref FixedPointMatrix matrix, out FixedPointMatrix result)
        {
            result.M11 = matrix.M11;
            result.M12 = matrix.M21;
            result.M13 = matrix.M31;
            result.M21 = matrix.M12;
            result.M22 = matrix.M22;
            result.M23 = matrix.M32;
            result.M31 = matrix.M13;
            result.M32 = matrix.M23;
            result.M33 = matrix.M33;
        }

        #endregion

        #region 乘法运算符

        /// <summary>
        /// 矩阵乘法运算符。
        /// </summary>
        /// <param name="value1">第一个矩阵。</param>
        /// <param name="value2">第二个矩阵。</param>
        /// <returns>两个矩阵的乘积。</returns>
        public static FixedPointMatrix operator *(FixedPointMatrix value1, FixedPointMatrix value2)
        {
            FixedPointMatrix result;
            FixedPointMatrix.Multiply(ref value1, ref value2, out result);
            return result;
        }

        #endregion

        /// <summary>
        /// 计算矩阵的迹（主对角线元素之和）。
        /// </summary>
        /// <returns>矩阵的迹。</returns>
        public FixedPoint64 Trace()
        {
            return this.M11 + this.M22 + this.M33;
        }

        #region 加法运算符

        /// <summary>
        /// 矩阵加法运算符。
        /// </summary>
        /// <param name="value1">第一个矩阵。</param>
        /// <param name="value2">第二个矩阵。</param>
        /// <returns>两个矩阵的和。</returns>
        public static FixedPointMatrix operator +(FixedPointMatrix value1, FixedPointMatrix value2)
        {
            FixedPointMatrix result;
            FixedPointMatrix.Add(ref value1, ref value2, out result);
            return result;
        }

        #endregion

        #region 减法运算符

        /// <summary>
        /// 矩阵减法运算符。
        /// </summary>
        /// <param name="value1">第一个矩阵。</param>
        /// <param name="value2">第二个矩阵。</param>
        /// <returns>两个矩阵的差。</returns>
        public static FixedPointMatrix operator -(FixedPointMatrix value1, FixedPointMatrix value2)
        {
            FixedPointMatrix result;
            FixedPointMatrix.Multiply(ref value2, -FixedPoint64.One, out value2);
            FixedPointMatrix.Add(ref value1, ref value2, out result);
            return result;
        }

        #endregion

        /// <summary>
        /// 判断两个矩阵的所有元素是否相等。
        /// </summary>
        /// <param name="value1">第一个矩阵。</param>
        /// <param name="value2">第二个矩阵。</param>
        /// <returns>所有元素相等返回 true，否则返回 false。</returns>
        public static bool operator ==(FixedPointMatrix value1, FixedPointMatrix value2)
        {
            return value1.M11 == value2.M11 &&
                   value1.M12 == value2.M12 &&
                   value1.M13 == value2.M13 &&
                   value1.M21 == value2.M21 &&
                   value1.M22 == value2.M22 &&
                   value1.M23 == value2.M23 &&
                   value1.M31 == value2.M31 &&
                   value1.M32 == value2.M32 &&
                   value1.M33 == value2.M33;
        }

        /// <summary>
        /// 判断两个矩阵是否存在不相等的元素。
        /// </summary>
        /// <param name="value1">第一个矩阵。</param>
        /// <param name="value2">第二个矩阵。</param>
        /// <returns>存在不相等元素返回 true，否则返回 false。</returns>
        public static bool operator !=(FixedPointMatrix value1, FixedPointMatrix value2)
        {
            return value1.M11 != value2.M11 ||
                   value1.M12 != value2.M12 ||
                   value1.M13 != value2.M13 ||
                   value1.M21 != value2.M21 ||
                   value1.M22 != value2.M22 ||
                   value1.M23 != value2.M23 ||
                   value1.M31 != value2.M31 ||
                   value1.M32 != value2.M32 ||
                   value1.M33 != value2.M33;
        }

        /// <summary>
        /// 判断当前矩阵是否与指定对象相等。
        /// </summary>
        /// <param name="obj">要比较的对象。</param>
        /// <returns>对象为矩阵且所有元素相等时返回 true，否则返回 false。</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is FixedPointMatrix)) return false;
            FixedPointMatrix other = (FixedPointMatrix)obj;

            return this.M11 == other.M11 &&
                   this.M12 == other.M12 &&
                   this.M13 == other.M13 &&
                   this.M21 == other.M21 &&
                   this.M22 == other.M22 &&
                   this.M23 == other.M23 &&
                   this.M31 == other.M31 &&
                   this.M32 == other.M32 &&
                   this.M33 == other.M33;
        }

        /// <summary>
        /// 返回该矩阵的哈希码。
        /// </summary>
        /// <returns>由各元素哈希码异或得到的哈希码。</returns>
        public override int GetHashCode()
        {
            return M11.GetHashCode() ^
                   M12.GetHashCode() ^
                   M13.GetHashCode() ^
                   M21.GetHashCode() ^
                   M22.GetHashCode() ^
                   M23.GetHashCode() ^
                   M31.GetHashCode() ^
                   M32.GetHashCode() ^
                   M33.GetHashCode();
        }

        #region 从轴角创建旋转矩阵

        /// <summary>
        /// 创建一个绕指定轴旋转指定角度的旋转矩阵。
        /// </summary>
        /// <param name="axis">旋转轴。</param>
        /// <param name="angle">旋转角度（单位：弧度）。</param>
        /// <param name="result">输出的旋转矩阵。</param>
        public static void CreateFromAxisAngle(ref FixedPointVector3 axis, FixedPoint64 angle,
            out FixedPointMatrix result)
        {
            FixedPoint64 x = axis.x;
            FixedPoint64 y = axis.y;
            FixedPoint64 z = axis.z;
            FixedPoint64 num2 = FixedPoint64.Sin(angle);
            FixedPoint64 num = FixedPoint64.Cos(angle);
            FixedPoint64 num11 = x * x;
            FixedPoint64 num10 = y * y;
            FixedPoint64 num9 = z * z;
            FixedPoint64 num8 = x * y;
            FixedPoint64 num7 = x * z;
            FixedPoint64 num6 = y * z;
            result.M11 = num11 + (num * (FixedPoint64.One - num11));
            result.M12 = (num8 - (num * num8)) + (num2 * z);
            result.M13 = (num7 - (num * num7)) - (num2 * y);
            result.M21 = (num8 - (num * num8)) - (num2 * z);
            result.M22 = num10 + (num * (FixedPoint64.One - num10));
            result.M23 = (num6 - (num * num6)) + (num2 * x);
            result.M31 = (num7 - (num * num7)) + (num2 * y);
            result.M32 = (num6 - (num * num6)) - (num2 * x);
            result.M33 = num9 + (num * (FixedPoint64.One - num9));
        }

        /// <summary>
        /// 创建一个绕指定轴旋转指定角度的旋转矩阵。
        /// </summary>
        /// <param name="angle">旋转角度（单位：弧度）。</param>
        /// <param name="axis">旋转轴。</param>
        /// <returns>对应的旋转矩阵。</returns>
        public static FixedPointMatrix AngleAxis(FixedPoint64 angle, FixedPointVector3 axis)
        {
            FixedPointMatrix result;
            CreateFromAxisAngle(ref axis, angle, out result);
            return result;
        }

        #endregion

        /// <summary>
        /// 返回该矩阵各元素原始值的字符串表示。
        /// </summary>
        /// <returns>以竖线分隔的各元素原始值字符串。</returns>
        public override string ToString()
        {
            return
                $"{M11.RawValue}|{M12.RawValue}|{M13.RawValue}|{M21.RawValue}|{M22.RawValue}|{M23.RawValue}|{M31.RawValue}|{M32.RawValue}|{M33.RawValue}";
        }
    }
}