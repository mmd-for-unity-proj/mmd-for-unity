using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MMDSkinsScript : MonoBehaviour
{
	// 表情の種類
	public enum SkinType
	{
		Base,
		EyeBrow,
		Eye,
		Lip,
		Other,
	}

	// 全ての頂点データからターゲットとなる頂点インデックス
	public int[] targetIndices;

	// モーフ先へのベクトル
	public Vector3[] morphTarget;

	// 表情の種類
	public SkinType skinType;

	// 前フレームのウェイト値
	float prev_weight = 0;
	
	// ウェイト付きモーフ結果
	public Vector3[] current_morph=null;

	// Use this for initialization
	void Start () 
	{
		
	}

	// モーフの計算
	public bool Compute(Vector3[] composite)
	{
		bool computed_morph = false;	// 計算したかどうか

		float weight = transform.localPosition.z;
		
		if(current_morph==null || targetIndices.Length!=current_morph.Length)
			current_morph=new Vector3[targetIndices.Length];

		if (weight != prev_weight)
		{
			computed_morph = true;
			for (int i = 0; i < targetIndices.Length; i++)
				current_morph[i]=morphTarget[i] * weight;
		}
		for (int i = 0; i < targetIndices.Length; i++)
		{
			if(targetIndices[i]<composite.Length)
				composite[targetIndices[i]] += current_morph[i];
		}

		prev_weight = weight;
		return computed_morph;
	}

}
