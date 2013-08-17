using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BoneController : MonoBehaviour
{
	public BoneController additive_parent;
	public float additive_rate;
	public CCDIKSolver ik_solver;
	public BoneController[] ik_solver_targets;

	public bool add_local;
	public bool add_move;
	public bool add_rotate;
	public Vector3 delta_move = Vector3.zero;
	public Quaternion delta_rotate = Quaternion.identity;
	private Vector3 prev_position;
	private Quaternion prev_rotation;

	/// <summary>
	/// 初回更新前処理
	/// </summary>
	void Start()
	{
		if (null != ik_solver) {
			ik_solver = transform.GetComponent<CCDIKSolver>();
			if (0 == ik_solver_targets.Length) {
				ik_solver_targets = Enumerable.Repeat(ik_solver.target, 1)
												.Concat(ik_solver.chains)
												.Select(x=>x.GetComponent<BoneController>())
												.ToArray();
			}
		}
		UpdatePrevTransform();
	}

	/// <summary>
	/// ボーン変形
	/// </summary>
	public void Process()
	{
		if (null != additive_parent) {
			if (add_move) {
				transform.localPosition += additive_parent.delta_move * additive_rate;
			}
			if (add_rotate) {
				Quaternion delta_rotate_rate;
				if (0.0f < additive_rate) {
					delta_rotate_rate = Quaternion.Slerp(Quaternion.identity, additive_parent.delta_rotate, additive_rate);
				} else {
					Quaternion additive_parent_delta_rotate_reverse = Quaternion.Inverse(additive_parent.delta_rotate);
					delta_rotate_rate = Quaternion.Slerp(Quaternion.identity, additive_parent_delta_rotate_reverse, -additive_rate);
				}
				transform.localRotation *= delta_rotate_rate;
			}
		}

		// IK計算
		if (null != ik_solver) {
			ik_solver.Solve();
			//IK制御下の差分座標更新
			foreach (var ik_solver_target in ik_solver_targets) {
				ik_solver_target.UpdateDeltaTransform();
			}
		}
		
		UpdateDeltaTransform();
	}
	
	/// <summary>
	/// 差分量更新
	/// </summary>
	public void UpdateDeltaTransform() {
		if (add_local) {
			//ローカル付与(親も含めた変形量算出)
			delta_move = transform.position - prev_position;
			delta_rotate = Quaternion.Inverse(prev_rotation) * transform.rotation;
		} else {
			//通常付与(このボーン単体での変形量算出)
			delta_move = transform.localPosition - prev_position;
			delta_rotate = Quaternion.Inverse(prev_rotation) * transform.localRotation;
		}
	}
	
	/// <summary>
	/// 差分基点座標更新
	/// </summary>
	public void UpdatePrevTransform() {
		if (add_local) {
			//ローカル付与(親も含めた変形量算出)
			prev_position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
			prev_rotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
		} else {
			//通常付与(このボーン単体での変形量算出)
			prev_position = transform.localPosition;
			prev_rotation = transform.localRotation;
		}
	}
}
