using System.Collections.Generic;

namespace DGame.FixedPoint
{
    /// <summary>
    /// 定点数轴对齐包围盒（AABB），由最小点与最大点定义，用于帧同步下的确定性范围判定与点钳制。
    /// </summary>
    public class FixedPointBounds
    {
        /// <summary>
        /// 根据一组定点坐标点计算其轴对齐包围盒（AABB），并返回对应的包围盒实例。
        /// </summary>
        /// <param name="points">参与计算的定点坐标点列表。</param>
        /// <returns>包含所有点的轴对齐包围盒。</returns>
        public FixedPointBounds GetInstance(List<FixedPointVector3> points)
        {
            FixedPoint64 minX = 0;
            FixedPoint64 maxX = 0;
            FixedPoint64 minY = 0;
            FixedPoint64 maxY = 0;
            FixedPoint64 minZ = 0;
            FixedPoint64 maxZ = 0;

            for (int i = 0; i < points.Count; i++)
            {
                minX = FixedPointMath.Min(minX, points[i].x);
                maxX = FixedPointMath.Max(maxX, points[i].x);
                minY = FixedPointMath.Min(minY, points[i].y);
                maxY = FixedPointMath.Max(maxY, points[i].y);
                minZ = FixedPointMath.Min(minZ, points[i].z);
                maxZ = FixedPointMath.Max(maxZ, points[i].z);
            }

            return new FixedPointBounds(new FixedPointVector3(minX, minY, minZ),
                new FixedPointVector3(maxX, maxY, maxZ));
        }

        /// <summary>
        /// 根据给定的最小点和最大点创建一个包围盒实例。
        /// </summary>
        /// <param name="min">包围盒的最小点。</param>
        /// <param name="max">包围盒的最大点。</param>
        /// <returns>由最小点和最大点构成的包围盒。</returns>
        public FixedPointBounds GetInstance(FixedPointVector3 min, FixedPointVector3 max)
        {
            FixedPointBounds fixedPointBounds = new FixedPointBounds(min, max);
            return fixedPointBounds;
        }

        /// <summary>
        /// 以给定的最小点和最大点构造包围盒。
        /// </summary>
        /// <param name="min">包围盒的最小点。</param>
        /// <param name="max">包围盒的最大点。</param>
        private FixedPointBounds(FixedPointVector3 min, FixedPointVector3 max)
        {
            this.min = min;
            this.max = max;
        }

        /// <summary>
        /// 获取距离指定点最近的、位于包围盒内的点。若点已在包围盒内则原样返回，否则返回钳制到包围盒边界后的点。
        /// </summary>
        /// <param name="point">待求的目标点。</param>
        /// <returns>包围盒内距离目标点最近的点。</returns>
        public FixedPointVector3 GetClosest(FixedPointVector3 point)
        {
            if (Contain(point))
            {
                return point;
            }
            else
            {
                return Clamp(point);
            }
        }

        /// <summary>
        /// 获取包围盒的最小点。
        /// </summary>
        public FixedPointVector3 min { get; private set; }

        /// <summary>
        /// 获取包围盒的最大点。
        /// </summary>
        public FixedPointVector3 max { get; private set; }

        /// <summary>
        /// 判断指定点是否位于包围盒内部（含边界）。
        /// </summary>
        /// <param name="point">待判断的点。</param>
        /// <returns>点在包围盒内（含边界）时返回 true，否则返回 false。</returns>
        public bool Contain(FixedPointVector3 point)
        {
            return point.x >= min.x && point.y >= min.y && point.z >= min.z && point.x <= max.x && point.y <= max.y &&
                   point.z <= max.z;
        }

        /// <summary>
        /// 将指定点的各分量分别钳制到包围盒的 [min, max] 范围内。
        /// </summary>
        /// <param name="point">待钳制的点。</param>
        /// <returns>钳制到包围盒边界后的点。</returns>
        FixedPointVector3 Clamp(FixedPointVector3 point)
        {
            FixedPoint64 x = FixedPointMath.Clamp(point.x, min.x, max.x);
            FixedPoint64 y = FixedPointMath.Clamp(point.y, min.y, max.y);
            FixedPoint64 z = FixedPointMath.Clamp(point.z, min.z, max.z);
            return new FixedPointVector3(x, y, z);
        }
    }
}