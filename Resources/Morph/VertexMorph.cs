using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class VertexMorph : MonoBehaviour
{
	public MorphManager.PanelType	panel;
	public int[]					indices;
	public Vector3[]				values;
	
	private float			prev_weight_ = 0.0f;
	private Vector3[] 		values_cache_ = null;
	
	/// <summary>
	/// モーフ処理
	/// </summary>
	public void Compute(Vector3[] composite)
	{
		//キャッシュ設定
		float weight = transform.localPosition.z;
		if ((prev_weight_ != weight) || (null == values_cache_)) {
			values_cache_ = values.Select(x=>x*weight).ToArray();
			prev_weight_ = weight;
		}
		
		//反映
		for (int i = 0, i_max = values_cache_.Length; i < i_max; ++i) {
			composite[indices[i]] += values_cache_[i];
		}
	}
}
