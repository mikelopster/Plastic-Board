using UnityEngine;
using System.Collections;

public class HandDrawCycle : MonoBehaviour {

	int m_cycleIndex = 0;
	float m_lastTriggerTime = 0.0f;
	const int m_cycleMax = 3;

	RiggedHandController m_riggedHandController;
	SkeletalHandController m_skeletalHandController;
	NonSkeletalHandController m_nonSkeletalHandController;

	void Start () {
		m_riggedHandController = GetComponent<RiggedHandController>();
		m_skeletalHandController = GetComponent<SkeletalHandController>();
		m_nonSkeletalHandController = GetComponent<NonSkeletalHandController>();
	}

	bool Toggled() {
		bool tappedThisFrame = false;
		for(int i = 0; i < Input.touchCount; ++i) {
			if (Input.GetTouch(i).phase == TouchPhase.Began) {
				tappedThisFrame = true;
				break;
			}
		}
		bool touchTrigger = (Input.touchCount > 2 && Time.time - m_lastTriggerTime > 0.4f && tappedThisFrame);
		bool toggled = Input.GetKeyDown(KeyCode.Space) || touchTrigger;
		if (toggled) {
			m_lastTriggerTime = Time.time;
		}
		return toggled;
	}
	
	// Update is called once per frame
	void Update () {
		if (Toggled()) {
			m_cycleIndex++;
			if (m_cycleIndex == m_cycleMax) {
				m_cycleIndex = 0;
			}
		}
		switch(m_cycleIndex) {
		case 0:
			m_riggedHandController.ShowHands(true);
			m_skeletalHandController.ShowHands(false);
			m_nonSkeletalHandController.ShowHands(false);
			break;
		case 1:
			m_riggedHandController.ShowHands(false);
			m_skeletalHandController.ShowHands(true);
			m_nonSkeletalHandController.ShowHands(false);
			break;
		case 2:
			m_riggedHandController.ShowHands(false);
			m_skeletalHandController.ShowHands(false);
			m_nonSkeletalHandController.ShowHands(true);
			break;
		}
	}
}
