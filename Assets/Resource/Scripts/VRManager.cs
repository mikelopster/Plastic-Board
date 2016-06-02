using UnityEngine;
using System.Collections;

public class VRManager : MonoBehaviour {

	bool m_vrMode = false;
	GameObject m_normalCamera;
	GameObject m_vrCamera;
	GameObject [] m_shadowPlanes;
	// Use this for initialization
	void Start () {
		m_normalCamera = GameObject.Find("Camera");
		m_vrCamera = GameObject.Find("OVRCameraController");
		m_vrCamera.SetActive(false);
		m_shadowPlanes = GameObject.FindGameObjectsWithTag("Plane");
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.V)) {
			m_vrMode = !m_vrMode;
			m_normalCamera.SetActive(!m_vrMode);
			m_vrCamera.SetActive(m_vrMode);
			for(int i = 0; i < m_shadowPlanes.Length; ++i) {
				m_shadowPlanes[i].SetActive(!m_vrMode);
			}
		}
	}
}
