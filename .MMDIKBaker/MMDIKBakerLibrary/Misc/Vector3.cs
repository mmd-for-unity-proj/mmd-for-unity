using System;
using System.Collections.Generic;
using System.Text;

namespace MMDIKBakerLibrary.Misc
{
    struct Vector3
    {
        public decimal X;
        public decimal Y;
        public decimal Z;
        public bool NaN;
        public static Vector3 Zero { get { return new Vector3(); } }
        public static Vector3 One { get { return new Vector3(1, 1, 1); } }

        public Vector3(decimal x, decimal y, decimal z)
        {
            X = x;
            Y = y;
            Z = z;
            NaN = false;
        }

        public static void Lerp(ref Vector3 value1, ref Vector3 value2, decimal amount, out Vector3 result)
        {
            result.X = value1.X + (value2.X - value1.X) * amount;
            result.Y = value1.Y + (value2.Y - value1.Y) * amount;
            result.Z = value1.Z + (value2.Z - value1.Z) * amount;
            result.NaN = false;
        }

        public decimal Length()
        {
            return (decimal)Math.Sqrt((double)(X * X + Y * Y + Z * Z));
        }

        public static void Transform(ref Vector3 value, ref Quaternion rotation, out Vector3 result)
        {
            decimal xx = rotation.X * rotation.X * 2m;
            decimal xy = rotation.X * rotation.Y * 2m;
            decimal xz = rotation.X * rotation.Z * 2m;
            decimal xw = rotation.W * rotation.X * 2m;
            decimal yy = rotation.Y * rotation.Y * 2m;
            decimal yz = rotation.Y * rotation.Z * 2m;
            decimal yw = rotation.W * rotation.Y * 2m;
            decimal zz = rotation.Z * rotation.Z * 2m;
            decimal zw = rotation.W * rotation.Z * 2m;
            result.X = value.X * (1 - yy - zz) + value.Y * (xy - zw) + value.Z * (xz + yw);
            result.Y = value.X * (xy + zw) + value.Y * (1 - xx - zz) + value.Z * (yz - xw);
            result.Z = value.X * (xz - yw) + value.Y * (yz + xw) + value.Z * (1 - xx - yy);
            result.NaN = false;
        }



        public static Vector3 Normalize(Vector3 value)
        {
	        decimal len2 = value.X * value.X + value.Y * value.Y + value.Z * value.Z;
	        Vector3 result;
            if (len2 == 0)
            {
                result.X = 0;
                result.Y = 0;
                result.Z = 0;
                result.NaN = true;
                return result;
            }
            else
            {
                result.X = value.X / (decimal)Math.Sqrt((double)len2);
                result.Y = value.Y / (decimal)Math.Sqrt((double)len2);
                result.Z = value.Z / (decimal)Math.Sqrt((double)len2);
                result.NaN = false;
                return result;
            }
        }

        public static decimal Dot(Vector3 vector1, Vector3 vector2)
        {
            return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
        }

        public static Vector3 Cross(Vector3 vector1, Vector3 vector2)
        {
	        Vector3 result;
            result.X = vector1.Y * vector2.Z - vector1.Z * vector2.Y;
	        result.Y = vector1.Z * vector2.X - vector1.X * vector2.Z;
	        result.Z = vector1.X * vector2.Y - vector1.Y * vector2.X;
            result.NaN = false;
	        return result;
        }



        public static void Transform(ref Vector3 position, ref Matrix matrix, out Vector3 result)
        {
            result.X = position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41;
            result.Y = position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42;
            result.Z = position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43;
            result.NaN = false;
        }



        public void Normalize()
        {
            decimal len2 = X * X + Y * Y + Z * Z;
            if (len2 == 0)
            {
                NaN = true;
            }
            else
            {
                X /= (decimal)Math.Sqrt((double)len2);
                Y /= (decimal)Math.Sqrt((double)len2);
                Z /= (decimal)Math.Sqrt((double)len2);
            }
        }
    }
}
