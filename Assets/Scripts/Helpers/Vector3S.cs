using UnityEngine;

public static class Vector3S {

	public static string ToString (Vector3 vector) {
		return (
			vector.x.ToString ("N4") + "," +
			vector.y.ToString ("N4") + "," +
			vector.z.ToString ("N4"));
	}

	public static Vector3 FromString (string message) {
		string[] axis = message.Split (',');
		return new Vector3 (
			float.Parse (axis [0]),
			float.Parse (axis [1]),
			float.Parse (axis [2]));
	}
}
