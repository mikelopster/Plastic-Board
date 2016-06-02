using UnityEngine;
using System.Collections;
using Leap;

public class MouseHoverInfo : MonoBehaviour {

	bool m_drawHoverInfo = false;
	Vector3 m_drawInfoPosition;
	public GUIStyle m_guiStyle;
	string m_debugString;

	void OnGUI() {
		if (m_drawHoverInfo == false) return;
		GUI.Label(new Rect(m_drawInfoPosition.x + 10.0f, UnityEngine.Screen.height - m_drawInfoPosition.y, 290, 30), m_debugString, m_guiStyle);
	}

	void Update () {
		// get mouse screen position and ray that casts from it to the world.
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		// perform a ray cast and see if we hit anything.
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit)) {
			m_drawHoverInfo = true;
			m_drawInfoPosition = Input.mousePosition;
			Vector3 posInMM = hit.transform.position * 1000.0f;
			m_debugString = "Position: (" + posInMM.x.ToString("F4") + ", " + 
				                            posInMM.y.ToString("F4") + ", " + 
					                        posInMM.z.ToString("F4") + ")";

		} else {
			m_drawHoverInfo = false;
		}

	}
}
