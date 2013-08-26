using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BoneMorph : MorphBase
{
	public MorphManager.PanelType	panel;
	public int[]					indices;
	public BoneMorphParameter[]		values;
	
	private float					prev_weight_ = 0.0f;
	private BoneMorphParameter[] 	values_cache_ = null;
	
	/// <summary>
	/// モーフ処理
	/// </summary>
	/// <returns>更新したか(true:更新した、false:未更新)</returns>
	/// <param name='composite'>モーフ値</param>
	public bool Compute(BoneMorphParameter[] composite)
	{
		bool result = false;
		//キャッシュ設定
		float weight = base.GetWeight(transform);
		if ((prev_weight_ != weight) || (null == values_cache_)) {
			values_cache_ = values.Select(x=>x * weight).ToArray();
			prev_weight_ = weight;
			result = true;
		}
		
		//反映
		for (int i = 0, i_max = values_cache_.Length; i < i_max; ++i) {
			composite[indices[i]] += values_cache_[i];
		}
		return result;
	}
	
	/// <summary>
	/// ボーンモーフパラメータ
	/// </summary>
	[System.Serializable]
	public class BoneMorphParameter {
		public Vector3		position;	//移動
		public Quaternion	rotation;	//回転
		
		/// <summary>
		/// 零
		/// </summary>
		public static BoneMorphParameter zero {
			get {
				BoneMorphParameter result = new BoneMorphParameter();
				result.position = Vector3.zero;
				result.rotation = Quaternion.identity;
				return result;
			}
		}

		/// <summary>
		/// パラメータの線形補間
		/// </summary>
		/// <returns>補間値</returns>
		/// <param name='lhs'>補間始点</param>
		/// <param name='rhs'>補間終点</param>
		/// <param name='weight'>補間係数(0.0fなら始点、1.0fなら終点)</param>
		public static BoneMorphParameter Lerp (BoneMorphParameter lhs, BoneMorphParameter rhs, float weight) {
			BoneMorphParameter result = new BoneMorphParameter();
			result.position = Vector3.Lerp(lhs.position, rhs.position, weight);
			result.rotation = Quaternion.Slerp(lhs.rotation, rhs.rotation, weight);
			return result;
		}

		/// <summary>
		/// 加算
		/// </summary>
		/// <returns>加算値</returns>
		/// <param name='lhs'>被加数</param>
		/// <param name='rhs'>加数</param>
		public static BoneMorphParameter operator+ (BoneMorphParameter lhs, BoneMorphParameter rhs) {
			BoneMorphParameter result = new BoneMorphParameter();
			result.position = lhs.position + rhs.position;
			result.rotation = lhs.rotation * rhs.rotation; //順回転乗算
			return result;
		}

		/// <summary>
		/// 減算
		/// </summary>
		/// <returns>減算値</returns>
		/// <param name='lhs'>被加数</param>
		/// <param name='rhs'>加数</param>
		public static BoneMorphParameter operator- (BoneMorphParameter lhs, BoneMorphParameter rhs) {
			BoneMorphParameter result = new BoneMorphParameter();
			result.position = lhs.position - rhs.position;
			result.rotation = lhs.rotation * Quaternion.Inverse(rhs.rotation); //逆回転乗算
			return result;
		}

		/// <summary>
		/// スカラー乗算
		/// </summary>
		/// <returns>スカラー乗算値</returns>
		/// <param name='lhs'>被乗数</param>
		/// <param name='rhs'>乗数</param>
		public static BoneMorphParameter operator* (BoneMorphParameter lhs, float rhs) {
			BoneMorphParameter result = new BoneMorphParameter();
			result.position = lhs.position * rhs;
			result.rotation = Quaternion.Slerp(Quaternion.identity, lhs.rotation, rhs); //0.0fが掛けられればQuaternion.identity化する
			return result;
		}
		public static BoneMorphParameter operator* (float lhs, BoneMorphParameter rhs) {
			return rhs * lhs;
		}
	}
}
