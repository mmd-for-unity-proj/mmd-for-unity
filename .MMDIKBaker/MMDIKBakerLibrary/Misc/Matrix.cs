using System;
using System.Collections.Generic;
using System.Text;

namespace MMDIKBakerLibrary.Misc
{
    struct Matrix
    {
        public decimal M11;
        public decimal M12;
        public decimal M13;
        public decimal M14;
        public decimal M21;
        public decimal M22;
        public decimal M23;
        public decimal M24;
        public decimal M31;
        public decimal M32;
        public decimal M33;
        public decimal M34;
        public decimal M41;
        public decimal M42;
        public decimal M43;
        public decimal M44;
        
        
        public static Matrix Identity
        {
            get
            {
                return new Matrix { M11 = 1, M12 = 0, M13 = 0, M14 = 0, M21 = 0, M22 = 1, M23 = 0, M24 = 0, M31 = 0, M32 = 0, M33 = 1, M34 = 0, M41 = 0, M42 = 0, M43 = 0, M44 = 1 };
            }
        }
        public Vector3 Translation
        {
	        get
	        {
		        Vector3 result;
                result.X = this.M41;
		        result.Y = this.M42;
		        result.Z = this.M43;
                result.NaN = false;
		        return result;
	        }
	
        }


        public static void CreateTranslation(decimal x, decimal y, decimal z, out Matrix result)
        {
            result.M11 = 1;
            result.M12 = 0;
            result.M13 = 0;
            result.M14 = 0;
            result.M21 = 0;
            result.M22 = 1;
            result.M23 = 0;
            result.M24 = 0;
            result.M31 = 0;
            result.M32 = 0;
            result.M33 = 1;
            result.M34 = 0;
            result.M41 = x;
            result.M42 = y;
            result.M43 = z;
            result.M44 = 1;
        }

        public static void Invert(ref Matrix matrix, out Matrix result)
        {
            decimal temp1 = matrix.M33 * matrix.M44 - matrix.M34 * matrix.M43;
            decimal temp2 = matrix.M32 * matrix.M44 - matrix.M34 * matrix.M42;
            decimal temp3 = matrix.M32 * matrix.M43 - matrix.M33 * matrix.M42;
            decimal temp4 = matrix.M31 * matrix.M44 - matrix.M34 * matrix.M41;
            decimal temp5 = matrix.M31 * matrix.M43 - matrix.M33 * matrix.M41;
            decimal temp6 = matrix.M31 * matrix.M42 - matrix.M32 * matrix.M41;
            decimal temp7 = matrix.M22 * temp1 - matrix.M23 * temp2 + matrix.M24 * temp3;
            decimal temp8 = -(matrix.M21 * temp1 - matrix.M23 * temp4 + matrix.M24 * temp5);
            decimal temp9 = matrix.M21 * temp2 - matrix.M22 * temp4 + matrix.M24 * temp6;
            decimal temp10 = -(matrix.M21 * temp3 - matrix.M22 * temp5 + matrix.M23 * temp6);
            decimal temp11 = matrix.M23 * matrix.M44 - matrix.M24 * matrix.M43;
            decimal temp12 = matrix.M22 * matrix.M44 - matrix.M24 * matrix.M42;
            decimal temp13 = matrix.M22 * matrix.M43 - matrix.M23 * matrix.M42;
            decimal temp14 = matrix.M21 * matrix.M44 - matrix.M24 * matrix.M41;
            decimal temp15 = matrix.M21 * matrix.M43 - matrix.M23 * matrix.M41;
            decimal temp16 = matrix.M21 * matrix.M42 - matrix.M22 * matrix.M41;
            decimal temp17 = matrix.M23 * matrix.M34 - matrix.M24 * matrix.M33;
            decimal temp18 = matrix.M22 * matrix.M34 - matrix.M24 * matrix.M32;
            decimal temp19 = matrix.M22 * matrix.M33 - matrix.M23 * matrix.M32;
            decimal temp20 = matrix.M21 * matrix.M34 - matrix.M24 * matrix.M31;
            decimal temp21 = matrix.M21 * matrix.M33 - matrix.M23 * matrix.M31;
            decimal temp22 = matrix.M21 * matrix.M32 - matrix.M22 * matrix.M31;
            decimal det = 1m / (matrix.M11 * temp7 + matrix.M12 * temp8 + matrix.M13 * temp9 + matrix.M14 * temp10);
            result.M11 = temp7 * det;
            result.M21 = temp8 * det;
            result.M31 = temp9 * det;
            result.M41 = temp10 * det;
            result.M12 = -(matrix.M12 * temp1 - matrix.M13 * temp2 + matrix.M14 * temp3) * det;
            result.M22 = (matrix.M11 * temp1 - matrix.M13 * temp4 + matrix.M14 * temp5) * det;
            result.M32 = -(matrix.M11 * temp2 - matrix.M12 * temp4 + matrix.M14 * temp6) * det;
            result.M42 = (matrix.M11 * temp3 - matrix.M12 * temp5 + matrix.M13 * temp6) * det;
            result.M13 = (matrix.M12 * temp11 - matrix.M13 * temp12 + matrix.M14 * temp13) * det;
            result.M23 = -(matrix.M11 * temp11 - matrix.M13 * temp14 + matrix.M14 * temp15) * det;
            result.M33 = (matrix.M11 * temp12 - matrix.M12 * temp14 + matrix.M14 * temp16) * det;
            result.M43 = -(matrix.M11 * temp13 - matrix.M12 * temp15 + matrix.M13 * temp16) * det;
            result.M14 = -(matrix.M12 * temp17 - matrix.M13 * temp18 + matrix.M14 * temp19) * det;
            result.M24 = (matrix.M11 * temp17 - matrix.M13 * temp20 + matrix.M14 * temp21) * det;
            result.M34 = -(matrix.M11 * temp18 - matrix.M12 * temp20 + matrix.M14 * temp22) * det;
            result.M44 = (matrix.M11 * temp19 - matrix.M12 * temp21 + matrix.M13 * temp22) * det;
        }

        public static void Multiply(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
        {
            result.M11 = matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21 + matrix1.M13 * matrix2.M31 + matrix1.M14 * matrix2.M41;
            result.M12 = matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22 + matrix1.M13 * matrix2.M32 + matrix1.M14 * matrix2.M42;
            result.M13 = matrix1.M11 * matrix2.M13 + matrix1.M12 * matrix2.M23 + matrix1.M13 * matrix2.M33 + matrix1.M14 * matrix2.M43;
            result.M14 =  matrix1.M11 * matrix2.M14 + matrix1.M12 * matrix2.M24 + matrix1.M13 * matrix2.M34 + matrix1.M14 * matrix2.M44;
            result.M21 = matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21 + matrix1.M23 * matrix2.M31 + matrix1.M24 * matrix2.M41;
            result.M22 = matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22 + matrix1.M23 * matrix2.M32 + matrix1.M24 * matrix2.M42;
            result.M23 = matrix1.M21 * matrix2.M13 + matrix1.M22 * matrix2.M23 + matrix1.M23 * matrix2.M33 + matrix1.M24 * matrix2.M43;
            result.M24 = matrix1.M21 * matrix2.M14 + matrix1.M22 * matrix2.M24 + matrix1.M23 * matrix2.M34 + matrix1.M24 * matrix2.M44;
            result.M31 = matrix1.M31 * matrix2.M11 + matrix1.M32 * matrix2.M21 + matrix1.M33 * matrix2.M31 + matrix1.M34 * matrix2.M41;
            result.M32 = matrix1.M31 * matrix2.M12 + matrix1.M32 * matrix2.M22 + matrix1.M33 * matrix2.M32 + matrix1.M34 * matrix2.M42;
            result.M33 = matrix1.M31 * matrix2.M13 + matrix1.M32 * matrix2.M23 + matrix1.M33 * matrix2.M33 + matrix1.M34 * matrix2.M43;
            result.M34 = matrix1.M31 * matrix2.M14 + matrix1.M32 * matrix2.M24 + matrix1.M33 * matrix2.M34 + matrix1.M34 * matrix2.M44;
            result.M41 = matrix1.M41 * matrix2.M11 + matrix1.M42 * matrix2.M21 + matrix1.M43 * matrix2.M31 + matrix1.M44 * matrix2.M41;
            result.M42 = matrix1.M41 * matrix2.M12 + matrix1.M42 * matrix2.M22 + matrix1.M43 * matrix2.M32 + matrix1.M44 * matrix2.M42;
            result.M43 = matrix1.M41 * matrix2.M13 + matrix1.M42 * matrix2.M23 + matrix1.M43 * matrix2.M33 + matrix1.M44 * matrix2.M43;
            result.M44 =  matrix1.M41 * matrix2.M14 + matrix1.M42 * matrix2.M24 + matrix1.M43 * matrix2.M34 + matrix1.M44 * matrix2.M44;
        }
        public static Matrix operator *(Matrix matrix1, Matrix matrix2)
        {
            Matrix result;
            Matrix.Multiply(ref matrix1, ref matrix2, out result);
            return result;
        }
        public static void CreateScale(ref Vector3 scale, out Matrix result)
        {
            result.M11 = scale.X;
            result.M12 = 0;
            result.M13 = 0;
            result.M14 = 0;
            result.M21 = 0;
            result.M22 = scale.Y;
            result.M23 = 0;
            result.M24 = 0;
            result.M31 = 0;
            result.M32 = 0;
            result.M33 = scale.Z;
            result.M34 = 0;
            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;
        }
        public static void CreateFromQuaternion(Quaternion quaternion, out Matrix result)
        {
            quaternion.Normalize();
            decimal xx = quaternion.X * quaternion.X;
            decimal yy = quaternion.Y * quaternion.Y;
            decimal zz = quaternion.Z * quaternion.Z;
            decimal xy = quaternion.X * quaternion.Y;
            decimal yz = quaternion.Y * quaternion.Z;
            decimal zx = quaternion.Z * quaternion.X;
            decimal xw = quaternion.X * quaternion.W;
            decimal yw = quaternion.Y * quaternion.W;
            decimal zw = quaternion.Z * quaternion.W;
            result.M11 = 1 - 2 * (yy + zz);
            result.M12 = 2 * (xy + zw);
            result.M13 = 2 * (zx - yw);
            result.M14 = 0;
            result.M21 = 2 * (xy - zw);
            result.M22 = 1 - 2 * (zz + xx);
            result.M23 = 2 * (yz + xw);
            result.M24 = 0;
            result.M31 = 2 * (zx + yw);
            result.M32 = 2 * (yz - xw);
            result.M33 = 1 - 2 * (yy + xx);
            result.M34 = 0;
            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;
        }
        public static void CreateTranslation(ref Vector3 position, out Matrix result)
        {
            result.M11 = 1;
            result.M12 = 0;
            result.M13 = 0;
            result.M14 = 0;
            result.M21 = 0;
            result.M22 = 1;
            result.M23 = 0;
            result.M24 = 0;
            result.M31 = 0;
            result.M32 = 0;
            result.M33 = 1;
            result.M34 = 0;
            result.M41 = position.X;
            result.M42 = position.Y;
            result.M43 = position.Z;
            result.M44 = 1;
        }

        public void Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation)
        {
            //移動行列の切り出し
            translation = new Vector3(M41, M42, M43);
            //スケールの切り出しと回転行列の作成
            scale = new Vector3();
            Matrix rotMatrix = new Matrix();
            Vector3 temp;
            temp = new Vector3(M11, M12, M13);
            scale.X = temp.Length();
            if (scale.X > 0)
            {
                rotMatrix.M11 = M11 / scale.X;
                rotMatrix.M12 = M12 / scale.X;
                rotMatrix.M13 = M13 / scale.X;
            }
            temp = new Vector3(M21, M22, M23);
            scale.Y = temp.Length();
            if (scale.X > 0)
            {
                rotMatrix.M21 = M21 / scale.Y;
                rotMatrix.M22 = M22 / scale.Y;
                rotMatrix.M23 = M23 / scale.Y;
            }
            temp = new Vector3(M31, M32, M33);
            scale.Z = temp.Length();
            if (scale.X > 0)
            {
                rotMatrix.M31 = M31 / scale.Z;
                rotMatrix.M32 = M32 / scale.Z;
                rotMatrix.M33 = M33 / scale.Z;
            }
            if (scale.Length() == 0)
            {
                throw new ArgumentException("scale成分が不明");
            }
            //回転行列をクォータニオンに変換する
            rotation = new Quaternion();
            decimal w = (decimal)Math.Sqrt((double)Math.Max(rotMatrix.M11 + rotMatrix.M22 + rotMatrix.M33 + 1, 0)) / 2;
            decimal tempX = (decimal)Math.Sqrt((double)Math.Max(rotMatrix.M11 - rotMatrix.M22 - rotMatrix.M33 + 1, 0)) / 2;
            decimal tempY = (decimal)Math.Sqrt((double)Math.Max(-rotMatrix.M11 + rotMatrix.M22 - rotMatrix.M33 + 1, 0)) / 2;
            decimal tempZ = (decimal)Math.Sqrt((double)Math.Max(-rotMatrix.M11 - rotMatrix.M22 + rotMatrix.M33 + 1, 0)) / 2;
            int MaxIndex = MathHelper.GetMaxArgIndex(tempX, tempY, tempZ, w);
            switch (MaxIndex)
            {
                case 0://x
                    rotation.X = tempX;
                    rotation.Y = (rotMatrix.M12 + rotMatrix.M21) / (4 * Math.Abs(tempX));
                    rotation.Z = (rotMatrix.M31 + rotMatrix.M13) / (4 * Math.Abs(tempX));
                    rotation.W = (rotMatrix.M23 - rotMatrix.M32) / (4 * Math.Abs(tempX));
                    break;
                case 1:
                    rotation.X = (rotMatrix.M12 + rotMatrix.M21) / (4 * Math.Abs(tempY));
                    rotation.Y = tempY;
                    rotation.Z = (rotMatrix.M23 + rotMatrix.M32) / (4 * Math.Abs(tempY));
                    rotation.W = (rotMatrix.M31 - rotMatrix.M13) / (4 * Math.Abs(tempY));
                    break;
                case 2:
                    rotation.X = (rotMatrix.M31 + rotMatrix.M13) / (4 * Math.Abs(tempZ));
                    rotation.Y = (rotMatrix.M23 + rotMatrix.M32) / (4 * Math.Abs(tempZ));
                    rotation.Z = tempZ;
                    rotation.W = (rotMatrix.M12 - rotMatrix.M21) / (4 * Math.Abs(tempZ));
                    break;
                default://w
                    rotation.X = (rotMatrix.M23 - rotMatrix.M32) / (4 * Math.Abs(w));
                    rotation.Y = (rotMatrix.M31 - rotMatrix.M13) / (4 * Math.Abs(w));
                    rotation.Z = (rotMatrix.M12 - rotMatrix.M21) / (4 * Math.Abs(w));
                    rotation.W = w;
                    break;
            }
        }
        public static void Compose(Vector3 scale,Quaternion rotation, Vector3 translation, out Matrix result)
        {
            Matrix scaleMat, rotateMat, translationMat;
            Matrix.CreateScale(ref scale, out scaleMat);
            Matrix.CreateFromQuaternion(rotation, out rotateMat);
            Matrix.CreateTranslation(ref translation, out translationMat);
            Matrix temp;
            Multiply(ref scaleMat, ref rotateMat, out temp);
            Multiply(ref temp, ref translationMat, out result);
        }


        internal static void GetTranslation(ref Matrix matrix, out Vector3 trans)
        {
            trans = matrix.Translation;
        }

        public static Matrix CreateRotationX(float radians)
        {
            Matrix result;
            decimal single1 = (decimal)Math.Cos((double)radians);
            decimal single2 = (decimal)Math.Sin((double)radians);
            result.M11 = 1;
            result.M12 = 0;
            result.M13 = 0;
            result.M14 = 0;
            result.M21 = 0;
            result.M22 = single1;
            result.M23 = single2;
            result.M24 = 0;
            result.M31 = 0;
            result.M32 = -single2;
            result.M33 = single1;
            result.M34 = 0;
            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;
            return result;
        }
        public static Matrix CreateRotationY(float radians)
        {
            Matrix result;
            decimal single1 = (decimal)Math.Cos((double)radians);
            decimal single2 = (decimal)Math.Sin((double)radians);
            result.M11 = single1;
            result.M12 = 0;
            result.M13 = -single2;
            result.M14 = 0;
            result.M21 = 0;
            result.M22 = 1;
            result.M23 = 0;
            result.M24 = 0;
            result.M31 = single2;
            result.M32 = 0;
            result.M33 = single1;
            result.M34 = 0;
            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;
            return result;
        }
        public static Matrix CreateRotationZ(float radians)
        {
            Matrix result;
            decimal single1 = (decimal)Math.Cos((double)radians);
            decimal single2 = (decimal)Math.Sin((double)radians);
            result.M11 = single1;
            result.M12 = single2;
            result.M13 = 0;
            result.M14 = 0;
            result.M21 = -single2;
            result.M22 = single1;
            result.M23 = 0;
            result.M24 = 0;
            result.M31 = 0;
            result.M32 = 0;
            result.M33 = 1;
            result.M34 = 0;
            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;
            return result;
        }
        public static void CreateFromQuaternion(ref Quaternion quaternion, out Matrix result)
        {
            decimal xx = quaternion.X * quaternion.X;
            decimal yy = quaternion.Y * quaternion.Y;
            decimal zz = quaternion.Z * quaternion.Z;
            decimal xy = quaternion.X * quaternion.Y;
            decimal zw = quaternion.Z * quaternion.W;
            decimal zx = quaternion.Z * quaternion.X;
            decimal yw = quaternion.Y * quaternion.W;
            decimal yz = quaternion.Y * quaternion.Z;
            decimal xw = quaternion.X * quaternion.W;
            result.M11 = 1 - 2 * (yy + zz);
            result.M12 = 2 * (xy + zw);
            result.M13 = 2 * (zx - yw);
            result.M14 = 0;
            result.M21 = 2 * (xy - zw);
            result.M22 = 1 - 2 * (zz + xx);
            result.M23 = 2 * (yz + xw);
            result.M24 = 0;
            result.M31 = 2 * (zx + yw);
            result.M32 = 2 * (yz - xw);
            result.M33 = 1 - 2 * (yy + xx);
            result.M34 = 0;
            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;
        }


    }
}
