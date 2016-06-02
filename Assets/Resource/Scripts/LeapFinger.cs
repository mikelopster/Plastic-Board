using UnityEngine;
using System.Collections;

public class LeapFinger : MonoBehaviour {

	GameObject [] m_joints = new GameObject[3];
	GameObject [] m_langes = new GameObject[3];
	GameObject m_fingerTip;
	public Color m_trailColor;
	public int m_fingerID = 0;
	PauseManager m_pauseManager;

	public GameObject [] GetLanges() {
		return m_langes;
	}

	public GameObject [] GetJoints() {
		return m_joints;
	}

	public GameObject GetJoint(int id) {
		return m_joints[id];
	}

	public GameObject GetFingerTip() {
		return m_fingerTip;
	}

	public void Show(bool shouldShow) {
		for (int i = 0; i < m_joints.Length; ++i) {
			m_joints[i].GetComponent<Renderer>().enabled = shouldShow;
			m_joints[i].GetComponent<Collider>().enabled = shouldShow;
		}
		for (int i = 0; i < m_langes.Length; ++i) {
			m_langes[i].GetComponent<Renderer>().enabled = shouldShow;
		}
		m_fingerTip.GetComponent<Renderer>().enabled = shouldShow;
		m_fingerTip.GetComponent<Collider>().enabled = shouldShow;
		m_fingerTip.GetComponent<TrailRenderer>().enabled = shouldShow;
	}

	void OnDestroy() {
		foreach(GameObject go in m_joints) {
			Destroy(go);
		}
		foreach(GameObject go in m_langes) {
			Destroy(go);
		}
		Destroy(m_fingerTip);
	}

	void Start() {
		// init joints
		for (int i = 0; i < m_joints.Length; ++i) {
			m_joints[i] = Instantiate(Resources.Load("Prefabs/JointSphere")) as GameObject;
		}
		// init langes
		for (int i = 0; i < m_langes.Length; ++i) {
			m_langes[i] = Instantiate(Resources.Load("Prefabs/Lange")) as GameObject;
		}

		// init finger tip
		m_fingerTip = Instantiate(Resources.Load("Prefabs/TipSphere")) as GameObject;

		// set trail color
		m_fingerTip.GetComponent<TrailRenderer>().material.color = m_trailColor;

		m_pauseManager = GameObject.Find("PauseManager").GetComponent<PauseManager>();
	}

}
