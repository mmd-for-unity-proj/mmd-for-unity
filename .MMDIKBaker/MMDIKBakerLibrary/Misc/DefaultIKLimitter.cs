using System;
using System.Collections.Generic;
using System.Text;

namespace MMDIKBakerLibrary.Misc
{
    /// <summary>
    /// 回転制限クラス
    /// </summary>
    public class RotationLimit
    {
        /// <summary>
        /// 最大回転
        /// </summary>
        /// <remarks>X回転、Y回転、Z回転制限</remarks>
        public float[] MaxRot { get; protected set; }
        /// <summary>
        /// 最小回転
        /// </summary>
        /// <remarks>X回転、Y回転、Z回転制限</remarks>
        public float[] MinRot { get; protected set; }
        /// <summary>
        /// 角度の反射調整機能使用フラグ
        /// </summary>
        /// <remarks>IKのCCDソルブは足のIKが＜が＞になる感じの解を出してくるので、反射してやると上手くいくっぽい</remarks>
        public bool[] Mirror { get; private set; }
        /// <summary>
        /// 角度の反射調整の反発係数
        /// </summary>
        public float[] Restitution { get; private set; }
        /// <summary>
        /// 角速度の"粘性"係数。IKのソルブの過程で解が"飛ぶ"のを防ぐために設定
        /// </summary>
        public float[] Stickness { get; private set; }


        /// <summary>
        /// 既定のコンストラクタ
        /// </summary>
        public RotationLimit()
        {
            MaxRot = new float[3];
            MinRot = new float[3];
            Mirror = new bool[3];
            Restitution = new float[3];
            for (int i = 0; i < 3; i++)
            {
                MaxRot[i] = MathHelper.Pi;
                MinRot[i] = -MathHelper.Pi;
                Mirror[i] = false;
                Restitution[i] = 0.5f;
            }


        }


        /// <summary>
        /// 指定した角度をアジャストする
        /// </summary>
        /// <param name="value">回転角</param>
        /// <param name="index">回転軸</param>
        /// <returns>アジャスト済み角度</returns>
        public float Adjust(float value, int index)
        {
            if (MinRot[index] > MaxRot[index])
            {//角度が逆なら入れ替えておく
                float temp = MinRot[index];
                MinRot[index] = MaxRot[index];
                MaxRot[index] = temp;
            }
            if (MaxRot[index] < value)
            {
                if (Mirror[index])
                    return MaxRot[index] * (1 + Restitution[index]) - value * Restitution[index];
                else
                    return MaxRot[index];
            }
            else if (MinRot[index] > value)
            {
                if (Mirror[index])
                    return MinRot[index] * (1 + Restitution[index]) - value * Restitution[index];
                else
                    return MinRot[index];
            }
            else
                return value;

        }
    }
    /// <summary>
    /// IK計算時の回転軸制限クラス
    /// </summary>
    class RotationAxisLimit
    {
        /// <summary>
        /// 回転制限を行う軸(X,Y,Z)
        /// </summary>
        public bool[] Limits { get; private set; }
        /// <summary>
        /// 既定のコンストラクタ
        /// </summary>
        public RotationAxisLimit()
        {
            Limits = new bool[3] { false, false, false };
        }
        /// <summary>
        /// 回転軸制限の適用
        /// </summary>
        /// <param name="rotationAxis"></param>
        public void Adjust(ref Vector3 rotationAxis)
        {
            if (Limits[0])
            {
                rotationAxis.X = 0;
            }
            if (Limits[1])
            {
                rotationAxis.Y = 0;
            }
            if (Limits[2])
            {
                rotationAxis.Z = 0;
            }
        }
    }
    /// <summary>
    /// 標準IKLimitter
    /// </summary>
    class DefaltIKLimitter : IIKLimitter
    {
        /// <summary>
        /// 総合稼働軸制限一覧
        /// </summary>
        /// <remarks>ボーン名マッチング用の正規表現オブジェクトと許可回転軸(親ボーン基準)</remarks>
        Dictionary<string, RotationLimit> TotalRotationLimits { get; set; }

        /// <summary>
        /// IK補正回転軸制限一覧
        /// </summary>
        /// <remarks>IK計算をする際の回転軸の制限</remarks>
        Dictionary<string, RotationAxisLimit> RotationAxisLimits { get; set; }

        /// <summary>
        /// 既定のコンストラクタ
        /// </summary>
        public DefaltIKLimitter()
        {
            //総合稼働制限
            TotalRotationLimits = new Dictionary<string, RotationLimit>();
            RotationLimit limit;
            limit = new RotationLimit();
            limit.MaxRot[0] = MathHelper.Pi;
            limit.MinRot[0] = MathHelper.ToRadians(3f);//3度ぐらい制限を設けてやると上手くいく。
            limit.MinRot[1] = 0;
            limit.MaxRot[1] = 0;
            limit.MinRot[2] = 0;
            limit.MaxRot[2] = 0;
            limit.Mirror[0] = true;
            limit.Restitution[0] = 0.99f;
            TotalRotationLimits.Add("左ひざ", limit);
            limit = new RotationLimit();
            limit.MaxRot[0] = MathHelper.Pi;
            limit.MinRot[0] = MathHelper.ToRadians(3f);//3度ぐらい制限を設けてやると上手くいく。
            limit.MinRot[1] = 0;
            limit.MaxRot[1] = 0;
            limit.MinRot[2] = 0;
            limit.MaxRot[2] = 0;
            limit.Mirror[0] = true;
            limit.Restitution[0] = 0.99f;
            TotalRotationLimits.Add("右ひざ", limit);

            RotationAxisLimits = new Dictionary<string, RotationAxisLimit>();
            RotationAxisLimit axisLimit;
            axisLimit = new RotationAxisLimit();
            axisLimit.Limits[0] = false;
            axisLimit.Limits[1] = true;
            axisLimit.Limits[2] = false;
            RotationAxisLimits.Add("左足", axisLimit);
            axisLimit = new RotationAxisLimit();
            axisLimit.Limits[0] = false;
            axisLimit.Limits[1] = true;
            axisLimit.Limits[2] = false;
            RotationAxisLimits.Add("右足", axisLimit);

            //IKのソルブ及びそれの調整計算に関するメモ
            //上記数値調整計算及び各種数値設定はMMDの元コード推定(リバースエンジニアリング、逆コンパイラとかはしてないからRエンジニアって言うのか分からないけど)する過程で落ち着いている今のところの解です。
            //ほんとの解法は樋口さんが知ってるんだろうけどｗ
            //解法は今のところIK-CCD法がMMDにとって最適だと考えてます。
            //理由として
            //・ひざのボーンにIKソルブ時の角度制限が入っているっぽいので、ソルブにボーンの角度を扱う必要があること
            //・高速解法が必要であること(MMDが非常に軽いことと、イテレーションの存在とその回数を考えると、軽いアルゴリズムを使ってないとつじつまが合わない)
            //が上げられます
            //そこで、CCD,Particleかの二つで、角度を使い易かったCCDを選びました。
            //ひざの角度調整はCCDのクセを抑える理由もあって工夫してあります。
            //CCDのクセとして、正しい解が＜だとしたら、＞という解を出してくることが多いという問題があります。(＞＜は足ですｗ)
            //そのために"反発係数"なる謎なパラメータを付けてますｗ
            //また、解がほとんどまっすぐな解を出す際に、|な感じの解で固定されてしまう問題があるため、3度ぐらい下限を入れています(どうも、MMDの方も入れてるっぽいけど、よく分からない……)
            //これは現在の推定結果です。もっと再現性が高い解があれば、改造して、ぜひ教えてください

            //2012/01/19追記分
            //IK計算の本家MMDとの微妙なズレを発見。
            //場所は付属モーション(true my heart)の腰振りのあと＼(^o^)／な動作に移行する腰振りのところ
            //足の動きが微妙に本家とズレていることを発見
            //原因を調査したところ、右足、左足のY軸回転の動きがIKの計算により補正されてしまうのが原因と判明
            //そのため、新たにaxisLimit機能を追加し、右足、左足がIK計算でY軸回転を補正されないようにした。
            //これにより他のモーションに影響が出ないかは現在調査中
        }



        #region IIKLimitter メンバー
        /// <summary>
        /// 制限の適用
        /// </summary>
        /// <param name="bone">対象となるボーン</param>
        public void Adjust(Model.MMDBone bone)
        {
            if (!TotalRotationLimits.ContainsKey(bone.Name))
                return;
            float YRot, XRot, ZRot;
            int FactoringType = 0;
            //if (MMDMath.FactoringQuaternionZXY(rot, out ZRot, out XRot, out YRot))
            //まずはXYZで分解
            if (!MathHelper.FactoringQuaternionXYZ(bone.LocalTransform.Rotation, out XRot, out YRot, out ZRot))
            {//ジンバルロック対策
                //YZXで分解
                if (!MathHelper.FactoringQuaternionYZX(bone.LocalTransform.Rotation, out YRot, out ZRot, out XRot))
                {
                    //ZXYで分解
                    MathHelper.FactoringQuaternionZXY(bone.LocalTransform.Rotation, out ZRot, out XRot, out YRot);
                    FactoringType = 2;
                }
                else
                    FactoringType = 1;
            }
            else
                FactoringType = 0;

            RotationLimit lim = TotalRotationLimits[bone.Name];
            XRot = lim.Adjust(XRot, 0);
            YRot = lim.Adjust(YRot, 1);
            ZRot = lim.Adjust(ZRot, 2);
            if (FactoringType == 0)
                bone.LocalTransform.Rotation = Quaternion.CreateFromRotationMatrix(
                        Matrix.CreateRotationX(XRot) *
                        Matrix.CreateRotationY(YRot) *
                        Matrix.CreateRotationZ(ZRot));
            else if (FactoringType == 1)
                bone.LocalTransform.Rotation = Quaternion.CreateFromRotationMatrix(
                        Matrix.CreateRotationY(YRot) *
                        Matrix.CreateRotationZ(ZRot) *
                        Matrix.CreateRotationX(XRot));
            else
                bone.LocalTransform.Rotation = Quaternion.CreateFromYawPitchRoll(YRot, XRot, ZRot);

        }
        /// <summary>
        /// 回転軸制限の適用
        /// </summary>
        /// <param name="boneName">ボーン名</param>
        /// <param name="rotationAxis">回転軸</param>
        public void Adjust(string boneName, ref Vector3 rotationAxis)
        {
            RotationAxisLimit limit;
            if (RotationAxisLimits.TryGetValue(boneName, out limit))
            {
                limit.Adjust(ref rotationAxis);
            }
        }
        #endregion
    }
}
