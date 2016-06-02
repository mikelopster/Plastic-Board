using UnityEngine;
using System.Collections;

public class PauseManager : MonoBehaviour {

	Color m_pausedColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
	Color m_normalColor = new Color(0.95f, 0.95f, 0.95f, 1.0f);

	public bool IsPaused() {
		return m_paused;
	}

	bool m_paused = false;

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.P)) {
			m_paused = !m_paused;
		}
	}
}
