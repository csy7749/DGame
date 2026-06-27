namespace DGame.FixedPoint
{
    /// <summary>
    /// 以整数存储的四元数，用于序列化或网络传输等需要紧凑、确定性表示的场景。
    /// 使用前可通过 <see cref="ToFixedPointQuaternion"/> 转换为定点数四元数参与运算。
    /// </summary>
    [System.Serializable]
    public struct QuaternionInt
    {
        /// <summary>四元数的 x 分量。</summary>
        public int x;
        /// <summary>四元数的 y 分量。</summary>
        public int y;
        /// <summary>四元数的 z 分量。</summary>
        public int z;
        /// <summary>四元数的 w 分量（标量部分）。</summary>
        public int w;

        /// <summary>
        /// 使用指定的整数分量构造四元数。
        /// </summary>
        /// <param name="x">x 分量。</param>
        /// <param name="y">y 分量。</param>
        /// <param name="z">z 分量。</param>
        /// <param name="w">w 分量（标量部分）。</param>
        public QuaternionInt(int x, int y, int z, int w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        /// <summary>
        /// 将当前整数四元数转换为定点数四元数 <see cref="FixedPointQuaternion"/>。
        /// </summary>
        /// <returns>对应的定点数四元数。</returns>
        public FixedPointQuaternion ToFixedPointQuaternion()
        {
            return new FixedPointQuaternion(x, y, z, w);
        }
    }
}