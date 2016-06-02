using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;

public class LeapLegacyHand : MonoBehaviour {

	public bool m_stale = false;
	public int m_handID = 0;
	Dictionary<int, GameObject> m_fingers = new Dictionary<int, GameObject>();
	Controller m_leapController;

	// Use this for initialization
	void Start () {
		m_leapController = new Controller();
	}

	public void Display(bool shouldShow) {
		foreach(KeyValuePair<int, GameObject> finger in m_fingers) {
			finger.Value.GetComponent<Renderer>().enabled = shouldShow;
			finger.Value.GetComponent<Collider>().enabled = shouldShow;
			finger.Value.GetComponent<TrailRenderer>().enabled = shouldShow;
		}
		GetComponent<Renderer>().enabled = shouldShow;
		//collider.enabled = shouldShow;
	}

	void OnDestroy() {
		foreach(KeyValuePair<int, GameObject> go in m_fingers) {
			Destroy(go.Value);
		}
	}

	// Update is called once per frame
	void Update () {
		// mark exising fingers as stale.
		foreach(KeyValuePair<int, GameObject> finger in m_fingers) {
			finger.Value.GetComponent<LeapLegacyFinger>().m_stale = true;
		}

		Hand h = m_leapController.Frame().Hand(m_handID);

		for (int i = 0; i < h.Fingers.Count; ++i) {
			GameObject finger;
			if (m_fingers.TryGetValue(h.Fingers[i].Id, out finger)) {
				finger.transform.position = h.Fingers[i].TipPosition.ToUnityScaled();
				finger.transform.forward = h.Fingers[i].Direction.ToUnity();
			} else {
				// else create new finger
				finger = Instantiate(Resources.Load("Prefabs/LeapLegacyFinger")) as GameObject;
				Vector3 s = finger.transform.localScale;
				s.z *= 3.5f;
				finger.transform.localScale = s;
				// push it into the dictionary.
				m_fingers.Add(h.Fingers[i].Id, finger);
			}

		}

	}
}
