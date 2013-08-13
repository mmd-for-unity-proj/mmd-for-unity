
namespace MikuMikuDance.Motion
{
    /// <summary>
    /// 座標系を表す列挙体
    /// </summary>
    public enum CoordinateType
    {
        /// <summary>
        /// 左手座標系
        /// </summary>
        /// <remarks>MMDの標準座標系</remarks>
        LeftHandedCoordinate = 1,
        /// <summary>
        /// 右手座標系
        /// </summary>
        /// <remarks>XNAの標準座標系</remarks>
        RightHandedCoordinate = -1,
    }
}
