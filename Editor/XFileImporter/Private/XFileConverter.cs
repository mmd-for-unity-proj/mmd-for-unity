using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System;

namespace xfile {
	public class XFileConverter {
		string filePath;
		string folderPath;
		string fileName;
		string localPath;
		StreamReader sr;
		
		MeshSection meshSection;
		MaterialList matList;
		
		// フォルダのパス取得 
		// 最後にスラッシュアリ
		private string GetFolderPath() {
			string[] str = filePath.Split('/');
			string result = "";
			for (int i = 0; i < str.Length-1; i++)
				result += str[i] + "/";
			return result;
		}
		
		private string GetFileName() {
			string[] str = filePath.Split('/');
			return str[str.Length-1];
		}
		
		// 1行1行解釈してクラスを作ってく 
		private void Parser(string s) {
			if (Regex.IsMatch(s, "Material ")) {
				s = Regex.Replace(s, "Material ", "");	// マッチさせる前に邪魔なのは消す 
				Match m = Regex.Match(s, "\\w+");
				matList.AddMaterial(sr, m.Value);
			} else if (Regex.IsMatch(s, "Mesh ")) {
				s = Regex.Replace(s, "Mesh ", "");
				Match m = Regex.Match(s, "\\w+");
				Debug.Log(m.Value);
				meshSection = new MeshSection(sr, m.Value);
			}
		}
		
		private void CreateFolder() {
			string buf = folderPath;
			buf.Replace("/", "\\");
			System.IO.Directory.CreateDirectory(buf + "Materials");	// とりあえず、マテリアルのフォルダ作成 
		}
		
		public XFileConverter(UnityEngine.Object xfile) {
			filePath = UnityEditor.AssetDatabase.GetAssetPath(xfile);
			folderPath = GetFolderPath();
			fileName = GetFileName();
			
			CreateFolder();
			sr = new StreamReader(filePath);
			matList = new MaterialList(sr);
			
			while (!sr.EndOfStream) {
				Parser(sr.ReadLine());
			}
		}
		
		private void EntryVerticesForMesh(Mesh mesh) {
			if (meshSection.vtxList.vertex.Length > 65000)
				throw new Exception("A mesh may not have more than 65000 vertices.");
			mesh.vertices = meshSection.vtxList.vertex;
		}
		
		private void EntryUVForMesh(Mesh mesh) {
			mesh.uv = meshSection.uvList.uvs;
		}
		
		// サブメッシュの登録 
		private void EntrySubMeshForMesh(Mesh mesh) {
			MeshList meshList = meshSection.meshList;
			MeshMaterialList matList = meshSection.matList;
			mesh.subMeshCount = matList.MaterialCount;	// サブメッシュの数をここで設定
			
			/*
			for (int i = 0; i < meshList.MeshCount; i++) {
				mesh.SetTriangles(meshList.mesh[i], matList.materialIndex[i]);
			}*/
			
			for (int i = 0; i < matList.MaterialCount; i++) {
				List<int> submesh = new List<int>();
				for (int j = 0; j < meshList.MeshCount; j++) {
					if (i == matList.materialIndex[j]) {
						foreach (int num in meshList.mesh[j])
							submesh.Add(num);
					}
				}
				int[] buf = new int[submesh.Count];
				submesh.CopyTo(buf);
				mesh.SetTriangles(buf, i);
			}
		}
		
		public UnityEngine.Object CreatePrefab() {
			string path = folderPath + fileName.Split('.')[0] + ".prefab";
			return PrefabUtility.CreateEmptyPrefab(path);
		}
		
		private void EntryNormal(Mesh mesh) {
			mesh.normals = meshSection.normList.normals;
		}
		
		// メッシュの生成 
		public Mesh CreateMesh() {
			Mesh mesh = new Mesh();
			EntryVerticesForMesh(mesh);
			EntryUVForMesh(mesh);
			EntrySubMeshForMesh(mesh);
			EntryNormal(mesh);
			AssetDatabase.CreateAsset(mesh, folderPath + fileName.Split('.')[0] + ".asset");
			return mesh;
		}
		
		// マテリアルの登録 
		private UnityEngine.Material EntryMaterial(int i) {
			UnityEngine.Material mat = new UnityEngine.Material(Shader.Find("VertexLit"));
			Material source = matList.materials[i];
			Texture tex = null;
			
			// テクスチャを貼る 
			if (source.TextureFileName != "") {
				tex = AssetDatabase.LoadAssetAtPath(folderPath + source.TextureFileName, typeof(Texture)) as Texture;
				mat.mainTexture = tex;
				mat.SetTextureScale("_MainTex", new Vector2(1, -1));
			}
			
			mat.color = source.DiffuseColor;
			mat.SetColor("_SpecColor", source.SpecularColor);
			mat.SetColor("_Emission", source.EmissionColor);
			mat.SetFloat("_Shiness", source.Specularity);
			mat.name = this.fileName + "_" + source.Name;
			
			AssetDatabase.CreateAsset(mat, folderPath + "Materials/" + mat.name + ".asset");
			return mat;
		}
		
		public UnityEngine.Material[] CreateMaterials() {
			UnityEngine.Material[] material = new UnityEngine.Material[matList.MaterialCount];
			for (int i = 0; i < matList.MaterialCount; i++) {
				material[i] = EntryMaterial(i);
			}
			return material;
		}
		
		public void ReplacePrefab(UnityEngine.Object prefab, Mesh mesh, UnityEngine.Material[] materials) {
			GameObject obj = new GameObject(fileName.Split('.')[0]);
			MeshFilter filter = obj.AddComponent<MeshFilter>();
			filter.mesh = mesh;
			MeshRenderer mren = obj.AddComponent<MeshRenderer>();
			mren.sharedMaterials = materials;
			PrefabUtility.ReplacePrefab(obj, prefab);
		}
	}
}
