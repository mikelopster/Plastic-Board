using UnityEngine;
using System.Collections;

public class Spinner : MonoBehaviour {

	public float speed;

	void Update () {
		transform.RotateAround (
			transform.position, transform.up,
			speed * Time.deltaTime);
	}
}
