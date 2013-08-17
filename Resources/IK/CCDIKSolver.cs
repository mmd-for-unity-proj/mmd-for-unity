using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CCDIKSolver : MonoBehaviour
{
	/* MEMO
	 * --------------------
	 * IKボーンにこのスクリプトを適用して、Solveを呼び出す。
	 * target = ターゲット（IKボーンを目指すボーン）
	 * chains = IK影響下ボーン 
	 * --------------------
	 */

	// Target
	//   IKボーンを目指すボーン
	public Transform target;

	// Loop count
	//   ushortだとInspectorに出ない
	public int iterations;

	// rad limit
	//   "単位角"
	public float controll_weight;

	// IK影響下ボーン
	public Transform[] chains;

	// レイの描画（デバッグ用）
	public bool drawRay = false;

	// 計算
	public void Solve()
	{
        // 有効化されてなかった場合は実行しない
        if (!this.enabled) return;

		// ループ回数で計算していく
		for (int tries = 0, _mt = iterations; tries < _mt; tries++)
		{
			// IK影響下ボーンごとに
			for (int _i = 0, _m = chains.Length; _i < _m; _i++)
			{
				var bone = chains[_i];
				var bonePos = bone.position;

				// エフェクタ設定
				var effectorPos = target.position;
				var effectorDirection = (effectorPos - bonePos);

				// ターゲット設定
				var targetDirection = (transform.position - bonePos);

				// 線を引いといてみる
				if (drawRay)
				{
					Debug.DrawRay(bonePos, effectorDirection, Color.green);
					Debug.DrawRay(bonePos, targetDirection, Color.red);
				}

				// 内積
				effectorDirection = effectorDirection.normalized;
				targetDirection = targetDirection.normalized;
				float rotateDot = Vector3.Dot(effectorDirection, targetDirection);

				// 角度算出。
				//   controll_weightによる一回の計算での制限
				float rotateAngle = Mathf.Acos(rotateDot);
				if (float.IsNaN(rotateAngle)) continue;
				var limit = 4 * controll_weight * (_i + 1);
				if (rotateAngle > limit)
					rotateAngle = limit;
				if (rotateAngle < -limit)
					rotateAngle = -limit;
				rotateAngle *= Mathf.Rad2Deg;

				// 外積で回転軸算出
				//   lockDirectionを見て、回転制限 => 一旦なし
				var rotateAxis = Vector3.Cross(effectorDirection, targetDirection).normalized;

				// 大丈夫そうなら
				if (float.IsNaN(rotateAxis.x) || float.IsNaN(rotateAxis.y) || float.IsNaN(rotateAxis.z))
					continue;

				// 回す
				var rotate = Quaternion.AngleAxis(rotateAngle, rotateAxis);
				bone.rotation = rotate * bone.rotation;

				// 角度制限
				limitter(bone);
			}
		}
	}

	// 角度制限
	void limitter(Transform bone)
	{
		// 足首のZ回転
		if (bone.name.Contains("足首"))
		{
			var vv = bone.localEulerAngles;
			vv.z = 0;
			bone.localRotation = Quaternion.Euler(vv);
			return;
		}

		// 本来なら設定値に基づいてやるけど、とりあえず膝限定
		if (!bone.name.Contains("ひざ"))
			return;

		// オイラー角を取得
		var v = bone.localEulerAngles;

		// y,z回転を無効化
		if (adjust_rot(v.y) == adjust_rot(v.z))
		{
			v.y = adjust_rot(v.y);
			v.z = adjust_rot(v.z);
		}

		// 逆に曲がらないように、制限してあげる
		if (v.x < 90 && v.x > 2 && ((v.y == 0 && v.z == 0) || (v.y == 180 && v.z == 180)))
			v.x = 360 - v.x * 0.99f;

		bone.localRotation = Quaternion.Euler(v);
	}

	// 0か180か近い方に固定
	int adjust_rot(float n)
	{
		if (Mathf.Abs(n) > Mathf.Abs(180 - n) && Mathf.Abs(360 - n) > Mathf.Abs(180 - n))
			return 180;
		else
			return 0;
	}

	// MMDEngine.cs の方でIKボーン全体も保持してるので、
	// そっちからSolve()を呼び出している。
	//--------------------
	//void LateUpdate()
	//{
	//    Solve();
	//}
}
