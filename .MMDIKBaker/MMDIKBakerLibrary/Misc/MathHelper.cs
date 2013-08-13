using System;
using System.Collections.Generic;
using System.Text;

namespace MMDIKBakerLibrary.Misc
{
    static class MathHelper
    {
        public static float Pi { get { return (float)Math.PI; } }
        public static float PiOver2 { get { return Pi / 2; } }
        public static decimal Clamp(decimal value, decimal min, decimal max)
        {
            return Math.Max(min, Math.Min(value, max));
        }
        public static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(value, max));
        }


        public static decimal Lerp(decimal value1, decimal value2, decimal amount)
        {
            if (value1 == value2)
            {
                return value1;
            }
            return value1 + (value2 - value1) * Clamp(amount, 0m, 1m);
        }
        public static float Lerp(float value1, float value2, float amount)
        {
            if (value1 == value2)
            {
                return value1;
            }
            return value1 + (value2 - value1) * Clamp(amount, 0, 1);
        }
        public static BezierCurve[] CreateIdentityCurve()
        {
            BezierCurve[] result = new BezierCurve[4];
            for (int i = 0; i < result.Length; i++)
            {
                result[i].v1 = new Vector2 { X = 0.25f, Y = 0.25f };
                result[i].v2 = new Vector2 { X = 0.75f, Y = 0.75f };
            }
            return result;
        }

        public static int GetMaxArgIndex(params decimal[] argument)
        {
            decimal value = decimal.MinValue;
            int index = -1;
            for (int i = 0; i < argument.Length; ++i)
            {
                if (argument[i] > value)
                {
                    index = i;
                    value = argument[i];
                }
            }
            return index;
        }

        public static Vector3 Round(Vector3 vector, int decimals)
        {
            return new Vector3(Math.Round(vector.X, decimals),Math.Round(vector.Y, decimals),Math.Round(vector.Z, decimals));
        }

        public static Quaternion Round(Quaternion rotation, int decimals)
        {
            return new Quaternion(Math.Round(rotation.X, decimals), Math.Round(rotation.Y, decimals), Math.Round(rotation.Z, decimals), Math.Round(rotation.W, decimals));
        }

        internal static Matrix Round(Matrix matrix, int decimals)
        {
            return new Matrix()
            {
                M11 = Math.Round(matrix.M11, decimals),
                M12 = Math.Round(matrix.M12, decimals),
                M13 = Math.Round(matrix.M13, decimals),
                M14 = Math.Round(matrix.M14, decimals),
                M21 = Math.Round(matrix.M21, decimals),
                M22 = Math.Round(matrix.M22, decimals),
                M23 = Math.Round(matrix.M23, decimals),
                M24 = Math.Round(matrix.M24, decimals),
                M31 = Math.Round(matrix.M31, decimals),
                M32 = Math.Round(matrix.M32, decimals),
                M33 = Math.Round(matrix.M33, decimals),
                M34 = Math.Round(matrix.M34, decimals),
                M41 = Math.Round(matrix.M41, decimals),
                M42 = Math.Round(matrix.M42, decimals),
                M43 = Math.Round(matrix.M43, decimals),
                M44 = Math.Round(matrix.M44, decimals)
            };
        }




        public static float ToRadians(float degrees)
        {
            return degrees * 0.01745329f;
        }
        /// <summary>
        /// クォータニオンをYaw(Y回転), Pitch(X回転), Roll(Z回転)に分解する関数
        /// </summary>
        /// <param name="input">分解するクォータニオン</param>
        /// <param name="ZRot">Z軸回転</param>
        /// <param name="XRot">X軸回転(-PI/2～PI/2)</param>
        /// <param name="YRot">Y軸回転</param>
        /// <returns>ジンバルロックが発生した時はfalse。ジンバルロックはX軸回転で発生</returns>
        public static bool FactoringQuaternionZXY(Quaternion input, out float ZRot, out float XRot, out float YRot)
        {
            //クォータニオンの正規化
            Quaternion inputQ = new Quaternion(input.X, input.Y, input.Z, input.W);
            inputQ.Normalize();
            //マトリクスを生成する
            Matrix rot;
            Matrix.CreateFromQuaternion(ref inputQ, out rot);
            //ヨー(X軸周りの回転)を取得
            if ((double)rot.M32 > 1 - 1.0e-4 || (double)rot.M32 < -1 + 1.0e-4)
            {//ジンバルロック判定
                XRot = (rot.M32 < 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2);
                ZRot = 0; YRot = (float)Math.Atan2(-(double)rot.M13, (double) rot.M11);
                return false;
            }
            XRot = -(float)Math.Asin((double)rot.M32);
            //ロールを取得
            ZRot = (float)Math.Asin((double)rot.M12 / Math.Cos(XRot));
            if (float.IsNaN(ZRot))
            {//漏れ対策
                XRot = (rot.M32 < 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2);
                ZRot = 0; YRot = (float)Math.Atan2(-(double)rot.M13, (double)rot.M11);
                return false;
            }
            if (rot.M22 < 0)
                ZRot = MathHelper.Pi - ZRot;
            //ピッチを取得
            YRot = (float)Math.Atan2((double)rot.M31, (double)rot.M33);
            return true;
        }


        /// <summary>
        /// クォータニオンをX,Y,Z回転に分解する関数
        /// </summary>
        /// <param name="input">分解するクォータニオン</param>
        /// <param name="XRot">X軸回転</param>
        /// <param name="YRot">Y軸回転(-PI/2～PI/2)</param>
        /// <param name="ZRot">Z軸回転</param>
        /// <returns></returns>
        public static bool FactoringQuaternionXYZ(Quaternion input, out float XRot, out float YRot, out float ZRot)
        {
            //クォータニオンの正規化
            Quaternion inputQ = new Quaternion(input.X, input.Y, input.Z, input.W);
            inputQ.Normalize();
            //マトリクスを生成する
            Matrix rot;
            Matrix.CreateFromQuaternion(ref inputQ, out rot);
            //Y軸回りの回転を取得
            if ((double)rot.M13 > 1 - 1.0e-4 || (double)rot.M13 < -1 + 1.0e-4)
            {//ジンバルロック判定
                XRot = 0;
                YRot = (rot.M13 < 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2);
                ZRot = -(float)Math.Atan2(-(double)rot.M21, (double)rot.M22);
                return false;
            }
            YRot = -(float)Math.Asin((double)rot.M13);
            //X軸回りの回転を取得
            XRot = (float)Math.Asin((double)rot.M23 / Math.Cos(YRot));
            if (float.IsNaN(XRot))
            {//ジンバルロック判定(漏れ対策)
                XRot = 0;
                YRot = (rot.M13 < 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2);
                ZRot = -(float)Math.Atan2(-(double)rot.M21, (double)rot.M22);
                return false;
            }
            if (rot.M33 < 0)
                XRot = MathHelper.Pi - XRot;
            //Z軸回りの回転を取得
            ZRot = (float)Math.Atan2((double)rot.M12, (double)rot.M11);
            return true;
        }
        /// <summary>
        /// クォータニオンをY,Z,X回転に分解する関数
        /// </summary>
        /// <param name="input">分解するクォータニオン</param>
        /// <param name="YRot">Y軸回転</param>
        /// <param name="ZRot">Z軸回転(-PI/2～PI/2)</param>
        /// <param name="XRot">X軸回転</param>
        /// <returns></returns>
        public static bool FactoringQuaternionYZX(Quaternion input, out float YRot, out float ZRot, out float XRot)
        {
            //クォータニオンの正規化
            Quaternion inputQ = new Quaternion(input.X, input.Y, input.Z, input.W);
            inputQ.Normalize();
            //マトリクスを生成する
            Matrix rot;
            Matrix.CreateFromQuaternion(ref inputQ, out rot);
            //Z軸回りの回転を取得
            if ((double)rot.M21 > 1 - 1.0e-4 || (double)rot.M21 < -1 + 1.0e-4)
            {//ジンバルロック判定
                YRot = 0;
                ZRot = (rot.M21 < 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2);
                XRot = -(float)Math.Atan2(-(double)rot.M32, (double)rot.M33);
                return false;
            }
            ZRot = -(float)Math.Asin((double)rot.M21);
            //Y軸回りの回転を取得
            YRot = (float)Math.Asin((double)rot.M31 / Math.Cos(ZRot));
            if (float.IsNaN(YRot))
            {//ジンバルロック判定(漏れ対策)
                YRot = 0;
                ZRot = (rot.M21 < 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2);
                XRot = -(float)Math.Atan2(-(double)rot.M32, (double)rot.M33);
                return false;
            }
            if (rot.M11 < 0)
                YRot = MathHelper.Pi - YRot;
            //X軸回りの回転を取得
            XRot = (float)Math.Atan2((double)rot.M23, (double)rot.M22);
            return true;
        }
    }
}
