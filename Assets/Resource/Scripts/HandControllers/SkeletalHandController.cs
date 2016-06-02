using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;

public class SkeletalHandController : MonoBehaviour {

	Controller m_leapController;
	GameObject TextUIPosition,TextUIForward;
	Dictionary<int, GameObject> m_hands = new Dictionary<int, GameObject>();

	float m_lastTriggerTime = 0.0f;

	bool m_enabled = false;

	PauseManager m_pauseManager;

	public bool IsEnabled() {
		return m_enabled;
	}

	public void ShowHands(bool shouldShow) {
		if (shouldShow) Enable();
		else Disable();
	}

	void Start () {
		m_leapController = new Controller();
		m_pauseManager = GameObject.Find("PauseManager").GetComponent<PauseManager>();
		TextUIPosition = GameObject.Find ("TextPosition");
		TextUIForward = GameObject.Find ("TextForward");
	}

	void Enable() {
		foreach(KeyValuePair<int, GameObject> h in m_hands) {
			h.Value.GetComponent<LeapHand>().Enable();
		}
	}

	void Disable() {
		foreach(KeyValuePair<int, GameObject> h in m_hands) {
			h.Value.GetComponent<LeapHand>().Disable();
		}
	}

	void Update () {
		
//		if (m_pauseManager.IsPaused()) return;

		Frame f = m_leapController.Frame();

		float thisPos = f.Hands.Count;

		TextUIPosition.GetComponent<GUIText> ().text = "thisPos: " + thisPos;

		// mark exising hands as stale.
		foreach(KeyValuePair<int, GameObject> h in m_hands) {
			h.Value.GetComponent<LeapHand>().m_stale = true;
		}

		// see what hands the leap sees and mark matching hands as not stale.
		for(int i = 0; i < f.Hands.Count; ++i) {
			GameObject hand;
			m_hands.TryGetValue(f.Hands[i].Id, out hand);

			
			// see if hand existed before
			if (hand != null) {
				// if it did then just update its position and joint positions.

				//hand.transform.position = f.Hands[i].PalmPosition.ToUnityScaled();
				hand.transform.position = f.Hands[i].PalmPosition.ToUnityScaled();
				hand.transform.forward = f.Hands[i].Direction.ToUnity();


				LeapHand leapHand = hand.GetComponent<LeapHand>();
				leapHand.m_stale = false;

				GameObject [] leapFingers = leapHand.GetFingers();


				for (int j = 0; j < leapFingers.Length; ++j) {
					LeapFinger finger = leapFingers[j].GetComponent<LeapFinger>();
					GameObject [] joints = finger.GetJoints();
					GameObject [] langes = finger.GetLanges();
					for(int k = 0; k < joints.Length; ++k) {
						Vector3 pos = f.Hands[i].Fingers[j].JointPosition((Finger.FingerJoint) k).ToUnityScaled();
						Vector3 nextPos = f.Hands[i].Fingers[j].JointPosition((Finger.FingerJoint) k + 1).ToUnityScaled();
						joints[k].transform.position = pos;
						Vector3 langePos = (pos + nextPos) * 0.5f;
						langes[k].transform.position = langePos;
						langes[k].transform.up = (nextPos - pos);
						Vector3 newScale = langes[k].transform.localScale;
						newScale.y = Mathf.Max(0.0f, (nextPos - pos).magnitude - 0.003f);
						langes[k].transform.localScale = newScale;
					}
					Vector3 tipPos;
					tipPos = f.Hands[i].Fingers[j].JointPosition(Finger.FingerJoint.JOINT_TIP).ToUnityScaled();
					finger.GetFingerTip().transform.position = tipPos;

					finger.m_fingerID = f.Hands[i].Fingers[j].Id;
				}

				leapHand.UpdateKnuckleLanges();

			} else {
				// else create new hand
				hand = Instantiate(Resources.Load("Prefabs/Hand")) as GameObject;
				// push it into the dictionary.
				m_hands.Add(f.Hands[i].Id, hand);
			}

		}

		// clear out stale hands.
		List<int> staleIDs = new List<int>();
		foreach(KeyValuePair<int, GameObject> h in m_hands) {
			if (h.Value.GetComponent<LeapHand>().m_stale) {
				Destroy(h.Value);
				// set for removal from dictionary.
				staleIDs.Add(h.Key);
			}
		}
		foreach(int id in staleIDs) {
			m_hands.Remove(id);
		}
	}
}
