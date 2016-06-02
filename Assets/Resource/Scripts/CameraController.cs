using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	bool m_mirrored = false;
	Quaternion m_startingRot;
	Vector3 m_startingPos;
	Color m_gridGray;

	public bool IsMirrored() {
		return m_mirrored;
	}

	void Start() {

#if UNITY_ANDROID
		Screen.orientation = ScreenOrientation.Landscape;
#endif
		m_startingPos = transform.position;
		m_startingRot = transform.rotation;
	}

	void OnPreCull() {
		GetComponent<Camera>().ResetWorldToCameraMatrix();
		GetComponent<Camera>().ResetProjectionMatrix();

		if (m_mirrored) {
			GetComponent<Camera>().projectionMatrix = GetComponent<Camera>().projectionMatrix * Matrix4x4.Scale(new Vector3(-1, 1, 1));
		} else {
			GetComponent<Camera>().projectionMatrix = GetComponent<Camera>().projectionMatrix * Matrix4x4.Scale(new Vector3(1, 1, 1));
		}
	}

	void OnPreRender() {
		if (!m_mirrored) return;
		GL.SetRevertBackfacing(true);
	}

	void OnPostRender() {
		if (!m_mirrored) return;
		GL.SetRevertBackfacing(false);
	}

	void ProcessRotate(Vector2 delta) {

		Vector3 pivot = new Vector3(0.0f, 0.15f, 0.0f);
		if (m_mirrored) {
			delta.x *= -1.0f;
		}
		transform.RotateAround(pivot, Vector3.up, -delta.x);
		transform.RotateAround(pivot, transform.right, delta.y);
	}

	void ProcessTranslate(Vector2 averageDelta) {
		if (m_mirrored) averageDelta.x *= -1.0f;
		transform.position += transform.right * -averageDelta.x * 0.001f;
		transform.position += transform.up * -averageDelta.y * 0.001f;
	}

	void ProcessZoom(float zoomValue) {
		transform.position += transform.forward * zoomValue;
	}

	void Update () {

		if (Input.GetKeyDown(KeyCode.M)) {
			m_mirrored = !m_mirrored;

			Vector3 mirrorPos = Camera.main.transform.position;
			mirrorPos.z = -mirrorPos.z;
			Camera.main.transform.position = mirrorPos;
			Camera.main.transform.rotation = m_startingRot;
			if (m_mirrored) Camera.main.transform.RotateAround(Camera.main.transform.position, Vector3.up, 180.0f);
			GameObject.Find("Light").transform.RotateAround(Vector3.zero, Vector3.up, 180);
		}

		if (Input.touchCount == 1) {
			ProcessRotate(Input.GetTouch(0).deltaPosition * 0.08f);
		}
		if (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved) {
			Vector2 currentDelta = Input.GetTouch(0).position - Input.GetTouch(1).position;
			Vector2 previousDelta = ((Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition) - (Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition));
			float touchDelta = currentDelta.magnitude - previousDelta.magnitude;
			ProcessZoom(touchDelta * 0.01f);
		}

		if (Input.GetKeyDown(KeyCode.J)) {
			if (GetComponent<Camera>().backgroundColor == Color.white) {
				GetComponent<Camera>().backgroundColor = Color.black;
				GameObject [] planes = GameObject.FindGameObjectsWithTag("Plane");
				for(int i = 0; i < planes.Length; ++i) {
					planes[i].GetComponent<Renderer>().enabled = false;
				}
			} else {
				GetComponent<Camera>().backgroundColor = Color.white;
				GameObject [] planes = GameObject.FindGameObjectsWithTag("Plane");
				for(int i = 0; i < planes.Length; ++i) {
					planes[i].GetComponent<Renderer>().enabled = true;
				}
			}
		}

		ProcessZoom( Input.GetAxis("Mouse ScrollWheel") * 0.1f );

// for some reason android returns true
// on get mouse button...
#if !UNITY_ANDROID
		if (Input.GetMouseButton(0)) {
			ProcessRotate(-(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"))) * 3.0f);
		}
		if (Input.GetMouseButton(1) || Input.GetMouseButton(2)) {
			ProcessTranslate((new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"))) * 10.0f);
		}
#endif
	}
}
