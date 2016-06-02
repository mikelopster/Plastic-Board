using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;

public class NonSkeletalHandController : MonoBehaviour {

	Controller m_leapController;
	Dictionary<int, GameObject> m_hands = new Dictionary<int, GameObject>();
	PauseManager m_pauseManager;

	public void ShowHands(bool shouldShow) {
		foreach(KeyValuePair<int, GameObject> h in m_hands) {
			h.Value.GetComponent<LeapLegacyHand>().Display(shouldShow);
		}
	}

	// Use this for initialization
	void Start () {
		m_leapController = new Controller();
	}
	
	// Update is called once per frame
	void Update () {
		// mark exising hands as stale.
		foreach(KeyValuePair<int, GameObject> h in m_hands) {
			h.Value.GetComponent<LeapLegacyHand>().m_stale = true;
		}

		Frame f = m_leapController.Frame();

		// see what hands the leap sees and mark matching hands as not stale.
		for(int i = 0; i < f.Hands.Count; ++i) {
			GameObject hand;
			m_hands.TryGetValue(f.Hands[i].Id, out hand);
			
			// see if hand existed before
			if (hand != null) {
				// if it did then just update its position and joint positions.
				
				hand.transform.position = f.Hands[i].PalmPosition.ToUnityScaled();
				hand.transform.forward = f.Hands[i].Direction.ToUnity();
				LeapLegacyHand leapHand = hand.GetComponent<LeapLegacyHand>();
				leapHand.m_stale = false;

			} else {
				// else create new hand
				hand = Instantiate(Resources.Load("Prefabs/LeapLegacyHand")) as GameObject;
				hand.GetComponent<LeapLegacyHand>().m_handID = f.Hands[i].Id;
				// push it into the dictionary.
				m_hands.Add(f.Hands[i].Id, hand);
			}
		}

		// clear out stale hands.
		List<int> staleIDs = new List<int>();
		foreach(KeyValuePair<int, GameObject> h in m_hands) {
			if (h.Value.GetComponent<LeapLegacyHand>().m_stale) {
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
