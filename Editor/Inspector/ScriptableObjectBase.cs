using UnityEngine;
using System.Collections;

public class ScriptableObjectBase
#if !(UNITY_3_5 || UNITY_3_4 || UNITY_3_3)
	: ScriptableObject
#endif
{
	public string assetPath;
}
