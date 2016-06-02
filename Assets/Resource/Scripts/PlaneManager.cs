using UnityEngine;
using System.Collections;

public class PlaneManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.H)) {
			GetComponent<Renderer>().enabled = !GetComponent<Renderer>().enabled;
		}
	}
}
