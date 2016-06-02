using System;
using System.Collections;
using UnityEngine;

public class ObjectPacket {

	public string id;
	public string type;
	public GameObject gameObject;


	// Constructor


	public ObjectPacket () {
		id = Convert.ToBase64String (Guid.NewGuid ().ToByteArray ());
	}


	// Implement serialize / deserialize methods


	public string Serialize () {
		string message =
			
			Vector3S.ToString (gameObject.transform.localPosition) + ";" +
			Vector3S.ToString (gameObject.transform.localEulerAngles) + ";" +
			Vector3S.ToString (gameObject.transform.localScale);

		return message;
	}

	public void Update (string message) {
		string[] token = message.Split (';');

		Vector3 position = Vector3S.FromString (token [0]);
		Vector3 rotation = Vector3S.FromString (token [1]);
		Vector3 scale = Vector3S.FromString (token [2]);

		gameObject.transform.localPosition = position;
		gameObject.transform.localEulerAngles = rotation;
		gameObject.transform.localScale = scale;
	}
}
