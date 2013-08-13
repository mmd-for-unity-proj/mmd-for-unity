using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace xfile {
	
	public class Converter {
		static public string[] InvalidCheck(StreamReader s, int checkNum, char split) {
			string[] str = s.ReadLine().Split(split);
			if (str.Length < checkNum) {
				str = s.ReadLine().Split(split);
				if (str.Length < checkNum) Debug.LogError("Invalid Stream?");
			}
			return str;
		}
		
		static public string[] InvalidCheck(StreamReader s, int checkNum) {
			return InvalidCheck(s, checkNum, ';');
		}
		
		static public Vector3 ToVector3(StreamReader s) {
			Vector3 v = new Vector3();
			string[] str = InvalidCheck(s, 3);
			v.x = Convert.ToSingle(str[0]);
			v.y = Convert.ToSingle(str[1]);
			v.z = Convert.ToSingle(str[2]);
			return v;
		}
		
		static public Vector2 ToVector2(StreamReader s) {
			Vector2 v = new Vector2();
			string[] str = s.ReadLine().Split(',');
			v.x = Convert.ToSingle(str[0]);
			str = str[1].Split(';');
			v.y = Convert.ToSingle(str[0]);
			return v;
		}
		
		static public int ToInt(StreamReader s) {
			return ToInt(s, ';');
		}
		
		static public int ToInt(StreamReader s, char split) {
			string[] str = InvalidCheck(s, 1, split);
			int num = 0;
			try {
				num = Convert.ToInt32(str[0]); 
			}
			catch { 
				str = str[0].Split(';');
				num = Convert.ToInt32(str[0]);
			}
			return num;
		}
		
		static public float ToFloat(StreamReader s) {
			string[] str = InvalidCheck(s, 1);
			return Convert.ToSingle(str[0]);
		}
		
		static public Color ToColor(StreamReader s) {
			Color c = new Color();
			string[] str = InvalidCheck(s, 4);
			c.r = Convert.ToSingle(str[0]);
			c.g = Convert.ToSingle(str[1]);
			c.b = Convert.ToSingle(str[2]);
			c.a = Convert.ToSingle(str[3]);
			return c;
		}
		
		static public Color ToColor(StreamReader s, float alpha) {
			Color c = new Color();
			string[] str = InvalidCheck(s, 3);
			c.r = Convert.ToSingle(str[0]);
			c.g = Convert.ToSingle(str[1]);
			c.b = Convert.ToSingle(str[2]);
			c.a = alpha;
			return c;
		}
	}
	
	/// <summary>
	/// 頂点リスト 
	/// </summary>
	public class VertexList {
		private int vertexCount;
		public int VertexCount {
			get { return vertexCount; }
		}
		
		public Vector3[] vertex;
		
		public VertexList(StreamReader s) {
			vertexCount = Converter.ToInt(s);
			Debug.Log("VertexList.Length: " + vertexCount);
			
			vertex = new Vector3[vertexCount];
			for (int i = 0; i < vertexCount; i++)
				vertex[i] = Converter.ToVector3(s);
		}
	}
	
	/// <summary>
	/// メッシュリスト 
	/// ポリゴンを構成する頂点インデックスの集合 
	/// </summary>
	public class MeshList {
		private int meshCount;
		public int MeshCount {
			get { return meshCount; }
		}
		
		public List<int[]> mesh;
		
		public MeshList(StreamReader s, VertexList l) {
			meshCount = Converter.ToInt(s);
			Debug.Log("MeshList.Length: " + meshCount);
			
			// 頂点インデックスの保存 
			mesh = new List<int[]>();
			for (int i = 0; i < meshCount; i++) {
				// インデックスの抽出 
				string[] str = s.ReadLine().Split(';');
				mesh.Add(new int[Convert.ToInt32(str[0])]);
				
				// 数値の抽出
				str = str[1].Split(',');
				for (int j = 0; j < mesh[i].Length; j++) {
					try {
						mesh[i][j] = Convert.ToInt32(str[j]);
					} catch { 
						Debug.Log("Invalid MeshList: " + i);
					}
				}
			}
		}
	}
	
	/// <summary>
	/// 頂点マテリアルリスト 
	/// マテリアルを利用する頂点インデックスの集合 
	/// </summary>
	public class MeshMaterialList {
		int materialCount;
		public int MaterialCount {
			get { return materialCount; }
		}
		
		int polygonCount;
		public int PolygonCount {
			get { return polygonCount; }
		}
		
		public int[] materialIndex;
		public string[] materialReferens;
		
		public MeshMaterialList(StreamReader s) {
			materialCount = Converter.ToInt(s);
			Debug.Log("MeshMaterialList.Length: " + materialCount);
			polygonCount = Converter.ToInt(s);
			
			materialIndex = new int[polygonCount];
			for (int i = 0; i < polygonCount; i++) {
				materialIndex[i] = Converter.ToInt(s, ',');
			}
			
			materialReferens = new string[materialCount];
			for (int i = 0; i < materialCount; i++) {
				string[] str = s.ReadLine().Split('{');
				str = str[1].Split('}');
				materialReferens[i] = str[0];
			}
			
			string buf = s.ReadLine();
			if (!Regex.IsMatch(buf, "}"))
				throw new Exception("MeshMaterialList End?: " + buf);
		}
	}
	
	/// <summary>
	/// 法線リスト 
	/// </summary>
	public class MeshNormalList {
		int normalCount;
		public int NormalCount {
			get { return normalCount; }
		}
		
		public int NormalIndexCount {
			get { return normalIndex.Count; }
		}
		
		public Vector3[] normals;	// 法線データ 
		public List<int[]> normalIndex;	// どの頂点がどの法線データを参照しているか 
		
		public MeshNormalList(StreamReader s) {
			normalCount = Converter.ToInt(s);
			Debug.Log("MeshNormalList.Length: " + normalCount);
			
			// 法線の読み込み 
			normals = new Vector3[normalCount];
			for (int i = 0; i < normalCount; i++) {
				normals[i] = Converter.ToVector3(s);
			}
			
			// インデックスの読み込み 
			normalIndex = new List<int[]>();
			string read = s.ReadLine();
			read = s.ReadLine();	// カウントの読み捨て 
			while (!Regex.IsMatch(read, "}")) {
				string[] str = read.Split(';');
				int[] buf = new int[Convert.ToInt32(str[0])];
				str = str[1].Split(',');
				for (int i = 0; i < buf.Length; i++) {
					buf[i] = Convert.ToInt32(str[i]);
				}
				normalIndex.Add(buf);
				read = s.ReadLine();
			}
			Debug.Log("MeshNormalIndex.Length: " + normalIndex.Count);
		}
	}
	
	/// <summary>
	/// UVリスト 
	/// </summary>
	public class MeshTextureCoordList {
		int uvCount;
		public int UVCount {
			get { return uvCount; }
		}
		
		public Vector2[] uvs;
		
		public MeshTextureCoordList(StreamReader s) {
			uvCount = Converter.ToInt(s);
			Debug.Log("MeshTextureCoordList.Length: " + uvCount);
			
			uvs = new Vector2[uvCount];
			for (int i = 0; i < uvCount; i++) {
				uvs[i] = Converter.ToVector2(s);
			}
			s.ReadLine();
		}
	}
	
	public class MeshSection {
		private string name;
		public string Name {
			get { return name; }
		}
		
		public MeshList meshList;
		public VertexList vtxList;
		public MeshMaterialList matList;
		public MeshNormalList normList;
		public MeshTextureCoordList uvList;
		
		public MeshSection(StreamReader sr, string name) {
			this.name = name;
			vtxList = new VertexList(sr);
			meshList = new MeshList(sr, vtxList);
			
			// 他の何かがある場合 
			// 閉じるまで繰り返し 
			string read = sr.ReadLine();
			while (!Regex.IsMatch(read, "}")) {
				if (Regex.IsMatch(read, "MeshMaterialList") ){
					matList = new MeshMaterialList(sr);
				} else if (Regex.IsMatch(read, "MeshNormals")) {
					normList = new MeshNormalList(sr);
				} else if (Regex.IsMatch(read, "MeshTextureCoords")) {
					uvList = new MeshTextureCoordList(sr);
				}
				read = sr.ReadLine();
			}
		}
	}
	
	public class MaterialList {
		public int MaterialCount {
			get { return materials.Count; }
		}
		
		public List<Material> materials;
		
		public MaterialList(StreamReader s) {
			materials = new List<Material>();
		}
		
		public void AddMaterial(StreamReader s, string name) {
			materials.Add(new Material(s, name));
		}
	}
	
	/// <summary>
	/// マテリアル 
	/// </summary>
	public class Material {
		private string name;
		public string Name {
			get { return name; }
		}
		
		private Color diffuseColor;
		public Color DiffuseColor {
			get { return diffuseColor; }
		}
		
		private float specularity;
		public float Specularity {
			get { return specularity; }
		}
		
		private Color specularColor;
		public Color SpecularColor {
			get { return specularColor; }
		}
		
		private Color emissionColor;
		public Color EmissionColor {
			get { return emissionColor; }
		}
		
		private string textureFileName;
		public string TextureFileName {
			get { return textureFileName; }
		}
		
		public Material(StreamReader s, string name) {
			this.name = name;
			diffuseColor = Converter.ToColor(s);
			specularity = Converter.ToFloat(s);
			specularColor = Converter.ToColor(s, 1.0f);
			emissionColor = Converter.ToColor(s, 1.0f);
			string str = s.ReadLine();
			if (Regex.IsMatch(str, "TextureFilename")) {
				string[] buf = str.Split('"');
				textureFileName = buf[1];
			} else {
				textureFileName = "";
			}
		}
	}
}
