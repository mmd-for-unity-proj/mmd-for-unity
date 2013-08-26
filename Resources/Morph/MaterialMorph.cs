using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MaterialMorph : MorphBase
{
	public enum OperationType {
		Mul,
		Add,
	}

	public MorphManager.PanelType		panel;
	public int[]						indices;
	public MaterialMorphParameter[]		values;
	public OperationType[]				operation;
	
	private float						prev_weight_ = 0.0f;
	private MaterialMorphParameter[] 	values_cache_ = null;
	
	/// <summary>
	/// モーフ処理
	/// </summary>
	/// <returns>更新したか(true:更新した、false:未更新)</returns>
	/// <param name='composite_mul'>乗算モーフ値</param>
	/// <param name='composite_add'>加算モーフ値</param>
	/// <exception cref='System.IndexOutOfRangeException'>乗算でも加算でも無い演算パラメータが設定されている</exception>
	public bool Compute(MaterialMorphParameter[] composite_mul, MaterialMorphParameter[] composite_add)
	{
		bool result = false;
		//キャッシュ設定
		float weight = base.GetWeight(transform);
		if ((prev_weight_ != weight) || (null == values_cache_)) {
			values_cache_ = new MaterialMorphParameter[values.Length];
			for (int i = 0, i_max = values_cache_.Length; i < i_max; ++i) {
				switch (operation[i]) {
				case OperationType.Mul: //乗算
					{
						values_cache_[i] = MaterialMorphParameter.Lerp(MaterialMorphParameter.one, values[i], weight);
					}
					break;
				case OperationType.Add: //加算
					{
						values_cache_[i] = values[i] * weight;
					}
					break;
				default:
					throw new System.IndexOutOfRangeException();
				}
			}
			prev_weight_ = weight;
			result = true;
		}
		
		//反映
		for (int i = 0, i_max = values_cache_.Length; i < i_max; ++i) {
			switch (operation[i]) {
			case OperationType.Mul: //乗算
				{
					composite_mul[indices[i]] *= values_cache_[i];
				}
				break;
			case OperationType.Add: //加算
				{
					composite_add[indices[i]] += values_cache_[i];
				}
				break;
			default:
				throw new System.IndexOutOfRangeException();
			}
		}
		return result;
	}
	
	/// <summary>
	/// 材質モーフパラメータ
	/// </summary>
	[System.Serializable]
	public class MaterialMorphParameter {
		public Color color; 		//拡散色(非透過度をαチャンネルに格納)
		public Color specular;		//反射色(反射強度をαチャンネルに格納)
		public Color ambient;		//環境色
		public Color outline_color;	//エッジ色
		public float outline_width;	//エッジ幅
		public Color texture_color;	//テクスチャ色
		public Color sphere_color;	//スフィア色
		public Color toon_color;	//トゥーン色
		
		/// <summary>
		/// 零
		/// </summary>
		public static MaterialMorphParameter zero {
			get {
				MaterialMorphParameter result = new MaterialMorphParameter();
				result.texture_color = result.sphere_color = result.toon_color = result.outline_color = result.ambient = result.specular = result.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
				result.outline_width = 0.0f;
				return result;
			}
		}

		/// <summary>
		/// 壱
		/// </summary>
		public static MaterialMorphParameter one {
			get {
				MaterialMorphParameter result = new MaterialMorphParameter();
				result.texture_color = result.sphere_color = result.toon_color = result.outline_color = result.ambient = result.specular = result.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
				result.outline_width = 1.0f;
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
		public static MaterialMorphParameter Lerp (MaterialMorphParameter lhs, MaterialMorphParameter rhs, float weight) {
			MaterialMorphParameter result = new MaterialMorphParameter();
			float lhs_weight = 1.0f - weight;
			float rhs_weight = weight;
			result.color = lhs.color * lhs_weight + rhs.color * rhs_weight;
			result.specular = lhs.specular * lhs_weight + rhs.specular * rhs_weight;
			result.ambient = lhs.ambient * lhs_weight + rhs.ambient * rhs_weight;
			result.outline_color = lhs.outline_color * lhs_weight + rhs.outline_color * rhs_weight;
			result.outline_width = lhs.outline_width * lhs_weight + rhs.outline_width * rhs_weight;
			result.texture_color = lhs.texture_color * lhs_weight + rhs.texture_color * rhs_weight;
			result.sphere_color = lhs.sphere_color * lhs_weight + rhs.sphere_color * rhs_weight;
			result.toon_color = lhs.toon_color * lhs_weight + rhs.toon_color * rhs_weight;
			return result;
		}

		/// <summary>
		/// 加算
		/// </summary>
		/// <returns>加算値</returns>
		/// <param name='lhs'>被加数</param>
		/// <param name='rhs'>加数</param>
		public static MaterialMorphParameter operator+ (MaterialMorphParameter lhs, MaterialMorphParameter rhs) {
			MaterialMorphParameter result = new MaterialMorphParameter();
			result.color = lhs.color + rhs.color;
			result.specular = lhs.specular + rhs.specular;
			result.ambient = lhs.ambient + rhs.ambient;
			result.outline_color = lhs.outline_color + rhs.outline_color;
			result.outline_width = lhs.outline_width + rhs.outline_width;
			result.texture_color = lhs.texture_color + rhs.texture_color;
			result.sphere_color = lhs.sphere_color + rhs.sphere_color;
			result.toon_color = lhs.toon_color + rhs.toon_color;
			return result;
		}

		/// <summary>
		/// スカラー加算
		/// </summary>
		/// <returns>スカラー加算値</returns>
		/// <param name='lhs'>被加数</param>
		/// <param name='rhs'>加数</param>
		public static MaterialMorphParameter operator+ (MaterialMorphParameter lhs, float rhs) {
			MaterialMorphParameter result = new MaterialMorphParameter();
			Color rhs_color = new Color(rhs, rhs, rhs, rhs);
			result.color = lhs.color + rhs_color;
			result.specular = lhs.specular + rhs_color;
			result.ambient = lhs.ambient + rhs_color;
			result.outline_color = lhs.outline_color + rhs_color;
			result.outline_width = lhs.outline_width + rhs;
			result.texture_color = lhs.texture_color + rhs_color;
			result.sphere_color = lhs.sphere_color + rhs_color;
			result.toon_color = lhs.toon_color + rhs_color;
			return result;
		}
		public static MaterialMorphParameter operator+ (float lhs, MaterialMorphParameter rhs) {
			return rhs + lhs;
		}

		/// <summary>
		/// 減算
		/// </summary>
		/// <returns>減算値</returns>
		/// <param name='lhs'>被加数</param>
		/// <param name='rhs'>加数</param>
		public static MaterialMorphParameter operator- (MaterialMorphParameter lhs, MaterialMorphParameter rhs) {
			MaterialMorphParameter result = new MaterialMorphParameter();
			result.color = lhs.color - rhs.color;
			result.specular = lhs.specular - rhs.specular;
			result.ambient = lhs.ambient - rhs.ambient;
			result.outline_color = lhs.outline_color - rhs.outline_color;
			result.outline_width = lhs.outline_width - rhs.outline_width;
			result.texture_color = lhs.texture_color - rhs.texture_color;
			result.sphere_color = lhs.sphere_color - rhs.sphere_color;
			result.toon_color = lhs.toon_color - rhs.toon_color;
			return result;
		}

		/// <summary>
		/// スカラー減算
		/// </summary>
		/// <returns>スカラー減算値</returns>
		/// <param name='lhs'>被加数</param>
		/// <param name='rhs'>加数</param>
		public static MaterialMorphParameter operator- (MaterialMorphParameter lhs, float rhs) {
			MaterialMorphParameter result = new MaterialMorphParameter();
			Color rhs_color = new Color(rhs, rhs, rhs, rhs);
			result.color = lhs.color - rhs_color;
			result.specular = lhs.specular - rhs_color;
			result.ambient = lhs.ambient - rhs_color;
			result.outline_color = lhs.outline_color - rhs_color;
			result.outline_width = lhs.outline_width - rhs;
			result.texture_color = lhs.texture_color - rhs_color;
			result.sphere_color = lhs.sphere_color - rhs_color;
			result.toon_color = lhs.toon_color - rhs_color;
			return result;
		}
		public static MaterialMorphParameter operator- (float lhs, MaterialMorphParameter rhs) {
			MaterialMorphParameter result = new MaterialMorphParameter();
			Color lhs_color = new Color(lhs, lhs, lhs, lhs);
			result.color = lhs_color - rhs.color;
			result.specular = lhs_color - rhs.specular;
			result.ambient = lhs_color - rhs.ambient;
			result.outline_color = lhs_color - rhs.outline_color;
			result.outline_width = lhs - rhs.outline_width;
			result.texture_color = lhs_color - rhs.texture_color;
			result.sphere_color = lhs_color - rhs.sphere_color;
			result.toon_color = lhs_color - rhs.toon_color;
			return result;
		}

		/// <summary>
		/// 乗算
		/// </summary>
		/// <returns>乗算値</returns>
		/// <param name='lhs'>被乗数</param>
		/// <param name='rhs'>乗数</param>
		public static MaterialMorphParameter operator* (MaterialMorphParameter lhs, MaterialMorphParameter rhs) {
			MaterialMorphParameter result = new MaterialMorphParameter();
			result.color = lhs.color * rhs.color;
			result.specular = lhs.specular * rhs.specular;
			result.ambient = lhs.ambient * rhs.ambient;
			result.outline_color = lhs.outline_color * rhs.outline_color;
			result.outline_width = lhs.outline_width * rhs.outline_width;
			result.texture_color = lhs.texture_color * rhs.texture_color;
			result.sphere_color = lhs.sphere_color * rhs.sphere_color;
			result.toon_color = lhs.toon_color * rhs.toon_color;
			return result;
		}

		/// <summary>
		/// スカラー乗算
		/// </summary>
		/// <returns>スカラー乗算値</returns>
		/// <param name='lhs'>被乗数</param>
		/// <param name='rhs'>乗数</param>
		public static MaterialMorphParameter operator* (MaterialMorphParameter lhs, float rhs) {
			MaterialMorphParameter result = new MaterialMorphParameter();
			result.color = lhs.color * rhs;
			result.specular = lhs.specular * rhs;
			result.ambient = lhs.ambient * rhs;
			result.outline_color = lhs.outline_color * rhs;
			result.outline_width = lhs.outline_width * rhs;
			result.texture_color = lhs.texture_color * rhs;
			result.sphere_color = lhs.sphere_color * rhs;
			result.toon_color = lhs.toon_color * rhs;
			return result;
		}
		public static MaterialMorphParameter operator* (float lhs, MaterialMorphParameter rhs) {
			return rhs * lhs;
		}
	}
}
