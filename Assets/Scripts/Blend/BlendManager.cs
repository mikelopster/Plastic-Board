using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendManager : MonoBehaviour {

	public string host;

	Dictionary<string, Blend> table;

	void Awake () {
		table = new Dictionary<string, Blend> ();
	}


	// Implement methods


	public void GetBlends (Action<Blend[]> callback) {
		StartCoroutine (LoadBlends (callback));
	}

	public void GetBlendFiles (string name, Action<Blend, Dictionary<string , byte[]>> callback) {
		StartCoroutine (LoadBlend (table [name], callback));
	}


	// Implement load coroutines


	IEnumerator LoadBlends (Action<Blend[]> callback) {

		WWW file = new WWW (host + "/api/blends");
		while (!file.isDone)
			yield return null;

		List<Blend> newBlends = new List<Blend> ();
		Blend[] blends = JsonUtility.FromJson<Blends> (file.text).blends;

		foreach (Blend blend in blends) {
			if (!table.ContainsKey (blend.name)) {
				newBlends.Add (blend);
				table.Add (blend.name, blend);
			}
		}

		callback (newBlends.ToArray ());
	}

	IEnumerator LoadBlend (Blend blend, Action<Blend, Dictionary<string , byte[]>> callback) {

		Dictionary<string, byte[]> files = new Dictionary<string, byte[]> ();

		foreach (string filename in blend.files) {

			WWW file = new WWW (host + "/api/blend?name=" + blend.name + "&file=" + filename);
			while (!file.isDone)
				yield return null;

			files [filename] = file.bytes;
		}

		callback (blend, files);
	}
}
