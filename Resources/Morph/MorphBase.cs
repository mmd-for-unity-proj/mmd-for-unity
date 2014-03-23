using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MorphBase : MonoBehaviour
{
    public int[] indices;

	public float group_weight = 0.0f;
    public MorphManager.PanelType panel;
	
	/// <summary>
	/// ウェイト計算
	/// </summary>
	public float GetWeight(Transform transform)
	{
		return transform.localPosition.z + group_weight;
	}
}
