using System.IO;

namespace MikuMikuDance.Model
{
    /// <summary>
    /// MMDモデルを表すインターフェイス
    /// </summary>
    /// <remarks>
    /// このクラスを継承し、オリジナルのオブジェクトを作成する場合は引数なしコンストラクタを用意すること
    /// また、Read,Write関数をオーバーライドすること。
    /// </remarks>
    public interface MMDModel
    {
        /// <summary>
        /// MMDモデルバージョン番号
        /// </summary>
        float Version { get; }
        /// <summary>
        /// Read関数
        /// </summary>
        /// <remarks>この関数はModelManagerから呼び出される。呼び出し時にはマジック文字とバージョン番号が読まれた状態で渡される</remarks>
        /// <param name="reader">マジック文字とバージョン番号読み込み済みのBinaryReader</param>
        /// <param name="coordinate">座標系変換指定</param>
        /// <param name="scale">スケール</param>
        void Read(BinaryReader reader, CoordinateType coordinate, float scale);
        /// <summary>
        /// Write関数
        /// </summary>
        /// <remarks>この関数はModelManagerから呼び出される。呼び出し時にはマジック文字とバージョン番号が書かれた状態で渡される</remarks>
        /// <param name="writer">マジック文字とバージョン番号書き込み済みのBinaryWriter</param>
        /// <param name="scale">スケール</param>
        void Write(BinaryWriter writer, float scale);
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
    }
}
