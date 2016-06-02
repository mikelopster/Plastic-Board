using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectLoader : MonoBehaviour {

	public bool isLoaded;
	public Dictionary<string, byte[]> fileTable;

	void Awake () {
		isLoaded = true;
		gameObject.AddComponent<MeshFilter> ();
		gameObject.AddComponent<MeshRenderer> ();
	}

	public void Load (string oname, Dictionary<string, byte[]> fileTable) {
		if (!isLoaded)
			return;

		this.fileTable = fileTable;
		StartCoroutine (ConstructModel (oname));
	}

	IEnumerator ConstructModel (string oname) {

		isLoaded = false;

		MeshFilter filter = gameObject.GetComponent<MeshFilter> ();
		MeshRenderer renderer = gameObject.GetComponent<MeshRenderer> ();

		FileReader.ObjectFile obj = FileReader.ReadObjectFile (fileTable [oname]);
		FileReader.MaterialFile mtl = FileReader.ReadMaterialFile (fileTable [obj.mtllib]);

		filter.mesh = PopulateMesh (obj);
		renderer.materials = DefineMaterial (obj, mtl);

		isLoaded = true;
		yield return null;
	}

	Mesh PopulateMesh (FileReader.ObjectFile obj) {

		Mesh mesh = new Mesh ();

		List<int[]> triplets = new List<int[]> ();
		List<int> submeshes = new List<int> ();

		for (int i = 0; i < obj.f.Count; i += 1) {
			for (int j = 0; j < obj.f [i].Count; j += 1) {
				triplets.Add (obj.f [i] [j]);
			}
			submeshes.Add (obj.f [i].Count);
		}

		Vector3[] vertices = new Vector3[triplets.Count];
		Vector3[] normals = new Vector3[triplets.Count];
		Vector2[] uvs = new Vector2[triplets.Count];

		for (int i = 0; i < triplets.Count; i += 1) {
			vertices [i] = obj.v [triplets [i] [0] - 1];
			normals [i] = obj.vn [triplets [i] [2] - 1];
			if (triplets [i] [1] > 0)
				uvs [i] = obj.vt [triplets [i] [1] - 1];
		}

		mesh.name = obj.o;
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.uv = uvs;
		mesh.subMeshCount = submeshes.Count;

		int vertex = 0;
		for (int i = 0; i < submeshes.Count; i += 1) {
			int[] triangles = new int[submeshes [i]];
			for (int j = 0; j < submeshes [i]; j += 1) {
				triangles [j] = vertex;
				vertex += 1;
			}
			mesh.SetTriangles (triangles, i);
		}

		mesh.RecalculateBounds ();
		mesh.Optimize ();

		return mesh;
	}

	Material[] DefineMaterial (FileReader.ObjectFile obj, FileReader.MaterialFile mtl) {

		Material[] materials = new Material[obj.usemtl.Count];

		for (int i = 0; i < obj.usemtl.Count; i += 1) {
			int index = mtl.newmtl.IndexOf (obj.usemtl [i]);

			Texture2D texture = new Texture2D (1, 1);
			texture.LoadImage (fileTable [mtl.mapKd [index]]);

			materials [i] = new Material (Shader.Find ("Diffuse"));
			materials [i].name = mtl.newmtl [index];
			materials [i].mainTexture = texture;
		}

		return materials;
	}
}
