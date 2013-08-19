using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GroupMorph : MorphBase
{
	public MorphManager.PanelType	panel;
	public int[]					indices;
	public float[]					values;
	
	private float			prev_weight_ = 0.0f;
	private float[] 		values_cache_ = null;
	
	/// <summary>
	/// モーフ処理
	/// </summary>
	public void Compute(float[] composite)
	{
		//キャッシュ設定
		float weight = base.GetWeight(transform);
		if ((prev_weight_ != weight) || (null == values_cache_)) {
			values_cache_ = values.Select(x=>x * weight).ToArray();
			prev_weight_ = weight;
		}
		
		//反映
		for (int i = 0, i_max = values_cache_.Length; i < i_max; ++i) {
			composite[indices[i]] += values_cache_[i];
		}
	}
}
