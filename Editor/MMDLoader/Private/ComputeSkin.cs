using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
//using Mono.Simd;

namespace MMD
{
	namespace Skin
	{
		/// <summary>
		/// スキンの計算を行う
		/// </summary>
		public class ComputeMorph
		{
			/// <summary>
			/// モーフベクトルをweight値から計算する
			/// </summary>
			/// <param name="morphVector">モーフベクトル</param>
			/// <param name="weight">ウェイト</param>
			/// <returns>表情の移動ベクトル</returns>
			public static void Compute(ref Vector3[] resultVector, Vector3[] morphVector, float weight)
			{
				// モーフベクトルを伸び縮みさせたものを結果として返す
				for (int i = 0; i < morphVector.Length; i++)
					resultVector[i] = morphVector[i] * weight;
			}
		}
	}
}