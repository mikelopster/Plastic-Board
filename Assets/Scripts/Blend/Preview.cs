using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Preview : MonoBehaviour {

	public float timeout;
	public BlendManager blendManager;
	public GameObject previewObject;

	bool isOpen;
	int maxIndex;
	int currentIndex;
	List<string> blendList;


	// Initialize


	void Awake () {
		maxIndex = 0;
		currentIndex = 0;
		blendList = new List<string> ();
	}

	void Start () {
		Fetch ();
		previewObject.SetActive (false);
	}


	// Implement methods


	public void Show () {
		if (isOpen)
			return;
		isOpen = true;

		Fetch ();
		Attach (blendList [currentIndex], previewObject);
		previewObject.SetActive (true);
	}

	public void Hide () {
		if (!isOpen)
			return;
		isOpen = false;

		previewObject.SetActive (false);
	}

	public void Next () {
		if (!isOpen || maxIndex == 0)
			return;
		
		currentIndex = (currentIndex + 1) % maxIndex;
		Attach (blendList [currentIndex], previewObject);
	}

	public void Previous () {
		if (!isOpen || maxIndex == 0)
			return;

		currentIndex = (currentIndex - 1 + maxIndex) % maxIndex;
		Attach (blendList [currentIndex], previewObject);
	}

	public GameObject GetCurrentObject () {
		GameObject gameObject = new GameObject ();
		Attach (blendList [currentIndex], gameObject);
		gameObject.name = blendList [currentIndex];
		return gameObject;
	}

	public GameObject GetObject (string name) {
		GameObject gameObject = new GameObject ();
		int index = blendList.IndexOf (name);

		if (index > -1) {
			Attach (blendList [index], gameObject);
		} else {
			StartCoroutine (WaitAndAttach (gameObject, name));
		}

		gameObject.name = name;
		return gameObject;
	}


	// Wait and Attach once


	IEnumerator WaitAndAttach (GameObject gameObject, string name) {
		yield return new WaitForSeconds (timeout);

		int index = blendList.IndexOf (name);
		Attach (blendList [index], gameObject);
	}
		
	// Implement function
	void Fetch () {
		blendManager.GetBlends ((Blend[] blends) => {
			
			foreach (Blend blend in blends)
				blendList.Add (blend.name);

			maxIndex = blendList.Count;
		});
	}

	void Attach (string name, GameObject gameObject) {

		ObjectLoader loader = gameObject.AddComponent <ObjectLoader> ();
		blendManager.GetBlendFiles (name,
			(Blend blend, Dictionary<string, byte[]> fileTable) => {
				loader.Load (blend.obj, fileTable);
			});
	}
}
