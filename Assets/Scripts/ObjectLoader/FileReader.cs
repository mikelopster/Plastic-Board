using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileReader {

	public struct ObjectFile {
		public string o;
		public string mtllib;
		public List<string> usemtl;
		public List<Vector3> v;
		public List<Vector3> vn;
		public List<Vector2> vt;
		public List<List<int[]>> f;
	}

	public struct MaterialFile {
		public List<string> newmtl;
		public List<string> mapKd;
	}

	public static ObjectFile ReadObjectFile (byte[] bytes) {

		ObjectFile obj = new ObjectFile ();
		StreamReader reader = new StreamReader (new MemoryStream (bytes));

		obj.usemtl = new List<string> ();
		obj.v = new List<Vector3> ();
		obj.vn = new List<Vector3> ();
		obj.vt = new List<Vector2> ();
		obj.f = new List<List<int[]>> ();

		string line;
		while ((line = reader.ReadLine ()) != null) {
			if (line == "" || line.StartsWith ("#"))
				continue;

			string[] token = line.Split (' ');
			switch (token [0]) {

			case ("o"):
				obj.o = token [1];
				break;
			case ("mtllib"):
				obj.mtllib = token [1];
				break;
			case ("usemtl"):
				obj.usemtl.Add (token [1]);
				obj.f.Add (new List<int[]> ());
				break;
			case ("v"):
				obj.v.Add (new Vector3 (
					float.Parse (token [1]),
					float.Parse (token [2]),
					float.Parse (token [3])));
				break;
			case ("vn"):
				obj.vn.Add (new Vector3 (
					float.Parse (token [1]),
					float.Parse (token [2]),
					float.Parse (token [3])));
				break;
			case ("vt"):
				obj.vt.Add (new Vector3 (
					float.Parse (token [1]),
					float.Parse (token [2])));
				break;
			case ("f"):
				for (int i = 1; i < 4; i += 1) {
					int[] triplet = Array.ConvertAll (token [i].Split ('/'), x => {
						if (String.IsNullOrEmpty (x))
							return 0;
						return int.Parse (x);
					});
					obj.f [obj.f.Count - 1].Add (triplet);
				}
				break;
			}
		}

		return obj;
	}

	public static MaterialFile ReadMaterialFile (byte[] bytes) {

		MaterialFile mtl = new MaterialFile ();
		StreamReader reader = new StreamReader (new MemoryStream (bytes));

		mtl.newmtl = new List<string> ();
		mtl.mapKd = new List<string> ();

		string line;
		while ((line = reader.ReadLine ()) != null) {
			if (line == "" || line.StartsWith ("#"))
				continue;

			string[] token = line.Split (' ');
			switch (token [0]) {

			case ("newmtl"):
				mtl.newmtl.Add (token [1]);
				break;
			case ("map_Kd"):
				mtl.mapKd.Add (token [1]);
				break;
			}
		}

		return mtl;
	}
}
