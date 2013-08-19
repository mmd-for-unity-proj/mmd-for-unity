using UnityEngine;
using System.Collections;

public class MMDMathf
{
	public static Matrix4x4 CreateRotationXMatrix(float rad)
	{
		Matrix4x4 m = Matrix4x4.identity;
		float cos = Mathf.Cos(rad), sin = Mathf.Sin(rad);
		m.m11 = cos; m.m12 = -sin;
		m.m21 = sin; m.m22 = cos;
		return m;
	}
	
	public static Matrix4x4 CreateRotationYMatrix(float rad)
	{
		Matrix4x4 m = Matrix4x4.identity;
		float cos = Mathf.Cos(rad), sin = Mathf.Sin(rad);
		m.m00 = cos; m.m02 = sin;
		m.m20 = -sin; m.m22 = cos;
		return m;
	}
	
	public static Matrix4x4 CreateRotationZMatrix(float rad)
	{
		Matrix4x4 m = Matrix4x4.identity;
		float cos = Mathf.Cos(rad), sin = Mathf.Sin(rad);
		m.m01 = cos; m.m02 = -sin;
		m.m11 = sin; m.m12 = cos;
		return m;
	}
	
	public static Matrix4x4 CreateRotationMatrixFromRollPitchYaw(float r, float p, float y)
	{
		Matrix4x4 m = Matrix4x4.identity;
		float rc = Mathf.Cos(r), rs = Mathf.Sin(r);	// Z
		float pc = Mathf.Cos(p), ps = Mathf.Sin(p);	// Y
		float yc = Mathf.Cos(y), ys = Mathf.Sin(y);;	// X
		m.m00 = rc*pc; m.m01 = rc*ps*ys-rs*yc; m.m02 = rc*ps*yc+rs*ys;
		m.m10 = rs*pc; m.m11 = rs*ps*ys+rc*yc; m.m12 = rs*ps*yc-rc*ys;
		m.m20 = -ps; m.m21 = pc*ys; m.m22 = pc*yc;
		return m;
	}
	
	public static Vector3 CreatePositionFromMatrix(Matrix4x4 m)
	{
		return new Vector3(m.m30, m.m31, m.m33);
	}
	
	public static Quaternion CreateQuaternionFromRotationMatrix(Matrix4x4 m)
	{
		Quaternion q;
		const float quad = 1.0f / 4.0f;
		q.x = ( m.m00 + m.m11 + m.m22 + 1.0f) * quad;
		q.y = ( m.m00 - m.m11 - m.m22 + 1.0f) * quad;
		q.z = (-m.m00 + m.m11 - m.m22 + 1.0f) * quad;
		q.w = (-m.m00 - m.m11 + m.m22 + 1.0f) * quad;
		if (q.x < 0.0f) q.x = 0.0f;
		if (q.y < 0.0f) q.y = 0.0f;
		if (q.z < 0.0f) q.z = 0.0f;
		if (q.w < 0.0f) q.w = 0.0f;
		q.x = Mathf.Sqrt(q.x);
		q.y = Mathf.Sqrt(q.y);
		q.z = Mathf.Sqrt(q.z);
		q.w = Mathf.Sqrt(q.w);
		if (q.x >= q.y && q.x >= q.z && q.x >= q.w) 
		{
			q.x *= 1.0f;
			q.y *= Sign(m.m22 - m.m13);
			q.z *= Sign(m.m03 - m.m21);
			q.w *= Sign(m.m11 - m.m02);
		}
		else if (q.y >= q.x && q.y >= q.z && q.y >= q.w)
		{
			q.x *= Sign(m.m22 - m.m13);
			q.y *= 1.0f;
			q.z *= Sign(m.m11 + m.m02);
			q.w *= Sign(m.m03 + m.m21);
		}
		else if(q.z >= q.x && q.z >= q.y && q.z >= q.w) {
			q.x *= Sign(m.m03 - m.m21);
			q.y *= Sign(m.m11 + m.m02);
			q.z *= +1.0f;
			q.w *= Sign(m.m22 + m.m13);
		} else if(q.w >= q.x && q.w >= q.y && q.w >= q.z) {
			q.x *= Sign(m.m11 - m.m02);
			q.y *= Sign(m.m21 + m.m03);
			q.z *= Sign(m.m22 + m.m13);
			q.w *= +1.0f;
		}
		float r = 1.0f / Norm(q.x, q.y, q.z, q.w);
		q.x *= r;
		q.y *= r;
		q.z *= r;
		q.w *= r;
		return q;
	}
	
	private static float Sign(float x) 
	{
		return (x >= 0.0f) ? 1.0f : -1.0f;
	}
	
	private static float Norm(float a, float b, float c, float d) 
	{
		return Mathf.Sqrt(a*a+b*b+c*c+d*d);
	}
}
