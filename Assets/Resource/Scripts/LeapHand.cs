using UnityEngine;
using System.Collections;
using Leap;

public class LeapHand : MonoBehaviour {

	GameObject [] m_fingers = new GameObject[5];
	GameObject [] m_knuckleLanges = new GameObject[6];
	GameObject m_wristJoint;

	bool m_initialized = false;

	public bool m_stale = false;

	public void UpdateKnuckleLanges() {
		for (int i = 0; i < m_fingers.Length - 1; ++i) {
			Vector3 start = m_fingers[i].GetComponent<LeapFinger>().GetJoint(0).transform.position;
			Vector3 end = m_fingers[i + 1].GetComponent<LeapFinger>().GetJoint(0).transform.position;
			m_knuckleLanges[i].transform.position = (start + end) * 0.5f;
			m_knuckleLanges[i].transform.up = (start - end);
			Vector3 newScale = m_knuckleLanges[i].transform.localScale;
			newScale.y = Mathf.Max(0.0f, (start - end).magnitude - 0.003f);
			m_knuckleLanges[i].transform.localScale = newScale;
		}
		// get the last joint by reflecting the mcb bone of the thumb
		// across the hand direction.
		Vector3 d = transform.position - m_fingers[0].GetComponent<LeapFinger>().GetJoint(0).transform.position;
		Vector3 n = -transform.forward;
		Vector3 r = d - (2 * Vector3.Dot(d, n)) * n;
		m_wristJoint.transform.position = transform.position + r;
		Vector3 pinkymcb = m_fingers[4].GetComponent<LeapFinger>().GetJoint(0).transform.position;
		m_knuckleLanges[4].transform.position = (pinkymcb + m_wristJoint.transform.position) * 0.5f;
		m_knuckleLanges[4].transform.up = (pinkymcb - m_wristJoint.transform.position);
		Vector3 s = m_knuckleLanges[4].transform.localScale;
		s.y = Mathf.Max(0.0f, (pinkymcb - m_wristJoint.transform.position).magnitude);
		m_knuckleLanges[4].transform.localScale = s;

		Vector3 thumbmcb = m_fingers[0].GetComponent<LeapFinger>().GetJoint(0).transform.position;
		m_knuckleLanges[5].transform.position = (m_wristJoint.transform.position + thumbmcb) * 0.5f;
		m_knuckleLanges[5].transform.up = (m_wristJoint.transform.position - thumbmcb);
		s = m_knuckleLanges[5].transform.localScale;
		s.y = Mathf.Max(0.0f, (m_wristJoint.transform.position - thumbmcb).magnitude);
		m_knuckleLanges[5].transform.localScale = s;
	}

	public GameObject [] GetFingers() {
		return m_fingers;
	}

	public void Enable() {
		if (!m_initialized) return;
		for(int i = 0; i < m_fingers.Length; ++i) {
			m_fingers[i].GetComponent<LeapFinger>().Show(true);
		}
		for (int i = 0; i < m_knuckleLanges.Length; ++i) {
			m_knuckleLanges[i].GetComponent<Renderer>().enabled = true;
		}
		m_wristJoint.GetComponent<Renderer>().enabled = true;
		m_wristJoint.GetComponent<Collider>().enabled = true;
		GetComponent<Renderer>().enabled = true;
		GetComponent<Collider>().enabled = true;
	}

	public void Disable() {
		if (!m_initialized) return;
		for(int i = 0; i < m_fingers.Length; ++i) {
			m_fingers[i].GetComponent<LeapFinger>().Show(false);
		}
		for (int i = 0; i < m_knuckleLanges.Length; ++i) {
			m_knuckleLanges[i].GetComponent<Renderer>().enabled = false;
		}
		m_wristJoint.GetComponent<Renderer>().enabled = false;
		m_wristJoint.GetComponent<Collider>().enabled = false;
		GetComponent<Renderer>().enabled = false;
		GetComponent<Collider>().enabled = false;
	}

	void OnDestroy() {
		foreach(GameObject go in m_fingers) {
			Destroy(go);
		}
		foreach(GameObject go in m_knuckleLanges) {
			Destroy(go);
		}
		Destroy(m_wristJoint);
	}
	
	void Start () {
		// initialize the fingers.

		for (int i = 0; i < m_fingers.Length; ++i) {
			m_fingers[i] = Instantiate(Resources.Load("Prefabs/Finger")) as GameObject;
		}
		// initialize knuckle langes
		for (int i = 0; i < m_knuckleLanges.Length; ++i) {
			m_knuckleLanges[i] = Instantiate(Resources.Load("Prefabs/Lange")) as GameObject;
		}
		m_wristJoint = Instantiate(Resources.Load("Prefabs/JointSphere")) as GameObject;
		m_initialized = true;
	}

}
