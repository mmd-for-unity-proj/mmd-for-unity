using System;
using System.Collections.Generic;
using System.Text;

namespace MMDIKBakerLibrary.Misc
{
    struct Quaternion
    {
        public decimal X;
        public decimal Y;
        public decimal Z;
        public decimal W;

        public static Quaternion Identity { get { return new Quaternion(0, 0, 0, 1); } }

        public Quaternion(decimal x, decimal y, decimal z, decimal w)
        {
            X = x; Y = y; Z = z; W = w;
        }
        public static void Slerp(ref Quaternion quaternion1, ref Quaternion quaternion2, decimal amount, out Quaternion result)
        {
            decimal rate2;
            decimal rate1;
            decimal dot = quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W;
            bool flag = false;
            if (dot < 0)
            {
                flag = true;
                dot = -dot;
            }
            if (dot > 0.999999m)
            {
                rate1 = 1 - amount;
                rate2 = (flag ? -amount : amount);
            }
            else
            {
                decimal ph = (decimal)Math.Acos((double)dot);
                rate1 = (decimal)Math.Sin((double)((1 - amount) * ph))  * (decimal)(1.0f/(float)Math.Sin((double)ph));
                rate2 = (decimal)Math.Sin((double)(amount * ph)) * (decimal)(1.0f/(float)Math.Sin((double)ph));
                if (flag)
                {
                    rate2 = -rate2;
                }
            }
            result.X = rate1 * quaternion1.X + rate2 * quaternion2.X;
            result.Y = rate1 * quaternion1.Y + rate2 * quaternion2.Y;
            result.Z = rate1 * quaternion1.Z + rate2 * quaternion2.Z;
            result.W = rate1 * quaternion1.W + rate2 * quaternion2.W;
        }
        public static Quaternion CreateFromAxisAngle(Vector3 axis, decimal angle)
        {
	        decimal temp = (decimal)Math.Sin((double)angle * 0.5);
	        Quaternion result;
            result.X = axis.X * temp;
	        result.Y = axis.Y * temp;
	        result.Z = axis.Z * temp;
	        result.W = (decimal)Math.Cos((double)angle * 0.5);
	        return result;
        }



        public void Normalize()
        {
            decimal temp = 1 / (decimal)Math.Sqrt((double)(this.X * this.X + this.Y * this.Y + this.Z * this.Z + this.W * this.W));
            this.X = this.X * temp;
            this.Y = this.Y * temp;
            this.Z = this.Z * temp;
            this.W = this.W * temp;
        }

        public static void Multiply(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
        {
            result.X = quaternion1.X * quaternion2.W + quaternion2.X * quaternion1.W + quaternion1.Y * quaternion2.Z - quaternion1.Z * quaternion2.Y;
            result.Y = quaternion1.Y * quaternion2.W + quaternion2.Y * quaternion1.W + quaternion1.Z * quaternion2.X - quaternion1.X * quaternion2.Z;
            result.Z = quaternion1.Z * quaternion2.W + quaternion2.Z * quaternion1.W + quaternion1.X * quaternion2.Y - quaternion1.Y * quaternion2.X;
            result.W = quaternion1.W * quaternion2.W - (quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z);
        }
        public static Quaternion CreateFromRotationMatrix(Matrix matrix)
        {
            decimal diag = matrix.M11 + matrix.M22 + matrix.M33;
            Quaternion result = new Quaternion();
            if (diag > 0)
            {
                decimal temp1 = (decimal)Math.Sqrt((double)diag + 1);
                result.W = temp1 * 0.5m;
                temp1 = 0.5m / temp1;
                result.X = (matrix.M23 - matrix.M32) * temp1;
                result.Y = (matrix.M31 - matrix.M13) * temp1;
                result.Z = (matrix.M12 - matrix.M21) * temp1;
            }
            else
            {
                if (matrix.M11 >= matrix.M22 && matrix.M11 >= matrix.M33)
                {
                    decimal temp2 = (decimal)Math.Sqrt((double)(1m + matrix.M11 - matrix.M22 - matrix.M33));
                    decimal temp3 = 0.5m / temp2;
                    result.X = 0.5m * temp2;
                    result.Y = (matrix.M12 + matrix.M21) * temp3;
                    result.Z = (matrix.M13 + matrix.M31) * temp3;
                    result.W = (matrix.M23 - matrix.M32) * temp3;
                }
            }
            return result;
        }
        public static Quaternion CreateFromYawPitchRoll(float yaw, float pitch, float roll)
        {
            decimal sin_roll = (decimal)Math.Sin((double)roll * 0.5);
            decimal cos_roll = (decimal)Math.Cos((double)roll * 0.5);
            decimal sin_pitch = (decimal)Math.Sin((double)pitch * 0.5);
            decimal cos_pitch = (decimal)Math.Cos((double)pitch * 0.5);
            decimal sin_yaw = (decimal)Math.Sin((double)yaw * 0.5);
            decimal cos_yaw = (decimal)Math.Cos((double)yaw * 0.5);
            Quaternion result;
            result.X = cos_yaw * sin_pitch * cos_roll + sin_yaw * cos_pitch * sin_roll;
	        result.Y = sin_yaw * cos_pitch * cos_roll - cos_yaw * sin_pitch * sin_roll;
	        result.Z = cos_yaw * cos_pitch * sin_roll - sin_yaw * sin_pitch * cos_roll;
	        result.W = cos_yaw * cos_pitch * cos_roll + sin_yaw * sin_pitch * sin_roll;
	        return result;
        }
    }
}
