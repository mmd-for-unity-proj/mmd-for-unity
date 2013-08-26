using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class UvMorph : MorphBase
{
	public MorphManager.PanelType	panel;
	public int[]					indices;
	public Vector2[]				values;
	
	private float			prev_weight_ = 0.0f;
	private Vector2[] 		values_cache_ = null;
	
	/// <summary>
	/// モーフ処理
	/// </summary>
	/// <returns>更新したか(true:更新した、false:未更新)</returns>
	/// <param name='composite'>モーフ値</param>
	public bool Compute(Vector2[] composite)
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
}
