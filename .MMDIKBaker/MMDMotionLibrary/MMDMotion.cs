using System.IO;

namespace MikuMikuDance.Motion
{
    /// <summary>
    /// MMDモーションを表すインターフェイス
    /// </summary>
    public interface MMDMotion
    {
        /// <summary>
        /// MMDモーションバージョン番号
        /// </summary>
        int Version { get; }
        /// <summary>
        /// Read関数
        /// </summary>
        /// <remarks>この関数はModelManagerから呼び出される。呼び出し時にはマジック文字とバージョン番号が読まれた状態で渡される</remarks>
        /// <param name="reader">マジック文字とバージョン番号読み込み済みのBinaryReader</param>
        /// <param name="coordinate">変換先座標系</param>
        /// <param name="scale">スケーリング値</param>
        void Read(BinaryReader reader, CoordinateType coordinate, float scale);
        /// <summary>
        /// 保持しているデータの座標系
        /// </summary>
        CoordinateType Coordinate { get; }
#if false
        /// <summary>
        /// スケーリング
        /// </summary>
        /// <param name="ScaleFactor">拡大倍率</param>
        void Scale(float ScaleFactor);
#endif
        /// <summary>
        /// モーションの書き出し
        /// </summary>
        /// <param name="writer">書き出し</param>
        /// <param name="scale">スケーリング値</param>
        void Write(BinaryWriter writer, float scale);
    }
}
