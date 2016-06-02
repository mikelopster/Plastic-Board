using UnityEngine;	
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Leap;

using System;
using System.Linq;
using System.IO;

public class RiggedHandController : MonoBehaviour {

	// for object
	public GameObject CardboardRaticleObject;
	public Text HandTextUI,GestureTextUI,HandXTextUI,HandYTextUI,PalmTextUI,PitchTextUI,PitchEachTextUI;

	// Variable For Cardboard Controller
	StereoController CardboardStereo;

	// Variable For Hand Controller
	Dictionary<int, GameObject> m_hands = new Dictionary<int, GameObject>();
	Controller m_leapController;
	SkeletalHandController m_skeletalDrawer;
	PauseManager m_pauseManager;
	GameObject TextUIPosition,TextUICheck;
	Frame f;
	CardboardReticle cardR;
	Vector3 old_movement;

	// Array Direction 
	int max_count = 40;
	int gesture_count = 0;
	float[] x_direction;
	float[] y_direction;
	double[] pinch_distances;
	double[] pan_areas;
	int index_direction = 0;
	double finger_x_max = 0, finger_x_min = 0, finger_y_max = 0, finger_y_min = 0;
	double[] fingers_tippoint_x;
	double[] fingers_tippoint_y;
	double pitch_dist;

	// Use this for initialization
	void Start () {

		// initial direction detect
		x_direction = new float[max_count];
		pinch_distances = new double[max_count];
		y_direction = new float[max_count];
		pan_areas = new double[max_count];
		fingers_tippoint_x = new double[5];
		fingers_tippoint_y = new double[5];

		old_movement = Vector3.zero;

		m_skeletalDrawer = GetComponent<SkeletalHandController>();
		cardR = CardboardRaticleObject.GetComponent<CardboardReticle> ();

		try {
			m_leapController = new Controller();

		} catch (Exception e) {
			Debug.Log(e);
		}
	}

	Hand FindFrontLeftHand(Frame f) {
		Hand h = null;
		float compVal = -float.MaxValue;
		for (int i = 0; i < f.Hands.Count; ++i) {
			if (f.Hands[i].IsLeft && f.Hands[i].PalmPosition.ToUnityScaled().z > compVal) {
				compVal = f.Hands[i].PalmPosition.ToUnityScaled().z;
				h = f.Hands[i];
			}
		}
		return h;
	}
	
	Hand FindFrontRightHand(Frame f) {
		Hand h = null;
		float compVal = -float.MaxValue;
		for (int i = 0; i < f.Hands.Count; ++i) {
			if (f.Hands[i].IsRight && f.Hands[i].PalmPosition.ToUnityScaled().z > compVal) {
				compVal = f.Hands[i].PalmPosition.ToUnityScaled().z;
				h = f.Hands[i];
			}
		}
		return h;
	}

	public void ShowHands(bool shouldShow) {
		foreach(KeyValuePair<int, GameObject> h in m_hands) {
			h.Value.GetComponent<LeapRiggedHand>().Draw(shouldShow);
		}

	}

	int CheckSwipeVertical(int index_direction) {

		int current = index_direction;

		int control = 0;
		float diff_x = x_direction [current] - x_direction.Max ();


		// swipe left, right
		if (diff_x < -110f) {
			HandXTextUI.text = diff_x.ToString ();
			control = 1;
		}

		return control;
	}

	int CheckSwipeHorizontal(int index_direction) {
		int current = index_direction;

		int control = 0;
		float diff_y = y_direction [current] - y_direction.Max();
			
		if (diff_y < -90f) {
			HandYTextUI.text = diff_y.ToString ();
			control = 1;
		}

		return control;
	}

	int CheckPinch(int index_direction) {

		int current = index_direction;
		int control = 0;

		double first_position = pinch_distances [current];
		double last_position_up = pinch_distances.Max();
		double last_position_down = pinch_distances.Min();

		if (last_position_down == 0)
			return control;

		double max_distance_in = first_position - last_position_up;
		double max_distance_out = first_position - last_position_down;

		PitchTextUI.text = Math.Round(max_distance_in,0).ToString() + " - " + Math.Round(max_distance_out,0).ToString();

		if (max_distance_in < -35f && max_distance_out > 20f) {
			control = 0;
		} else if (max_distance_in < -35f) {
			PitchEachTextUI.text = max_distance_in.ToString ();
			control = -1;
		} else if (max_distance_out > 20f) {
			PitchEachTextUI.text = max_distance_out.ToString ();
			control = 1;
		}

//		PitchTextUI.text = max_distance_in.ToString() + ", " + max_distance_out.ToString ();
//		PitchTextUI.text = max_distance_in.ToString ();

		return control;
		
	}

	int CheckPan(int index_direction) {

		double first_position = pan_areas [index_direction];
		double last_position = pan_areas.Min ();
		double diff = first_position - last_position;

		int control = 0;

		if (last_position == 0) {
			return control;
		}

		PalmTextUI.text = diff.ToString ();
			
		if (diff > 4500) {
			
			control = 1;
		}

		return control;
	}

	// Update is called once per frame
	void FixedUpdate () {

		if (m_leapController == null || !m_leapController.IsConnected) return;

		// mark exising hands as stale.
		foreach (KeyValuePair<int, GameObject> h in m_hands) {
			h.Value.GetComponent<LeapRiggedHand> ().m_stale = true;
		}

		f = m_leapController.Frame();

		HandTextUI.text = f.Hands.Count.ToString();

		// New Gesture
		if (f.Hands.Count >= 1) {
			Hand currentHand = f.Hands [0];

			// Collect Finger Position 
			for (int i = 0; i < 5; i++) {
				fingers_tippoint_x [i] = currentHand.Fingers [i].TipPosition.x;
				fingers_tippoint_y [i] = currentHand.Fingers [i].TipPosition.y;
			}

			pitch_dist = Math.Sqrt (Math.Pow (fingers_tippoint_x [0] - fingers_tippoint_x [1], 2) + Math.Pow (fingers_tippoint_y [0] - fingers_tippoint_y [1], 2));
			finger_x_max = fingers_tippoint_x.Max ();
			finger_x_min = fingers_tippoint_x.Min ();
			finger_y_max = fingers_tippoint_y.Max ();
			finger_y_min = fingers_tippoint_y.Min ();

			string gestured = GestureTextUI.text;

			// for Circle, Tap Gesture
//			for (int j = 0; j < f.Gestures().Count; j++) {
//				if (f.Gestures () [j].Type == Gesture.GestureType.TYPE_CIRCLE) {
//					ObjectController.instance.ReleaseTarget ();
//					gestured = "Circle";
//				}
//			}

			// Get position, distance
			x_direction [index_direction] = currentHand.PalmPosition.x;
			y_direction [index_direction] = currentHand.PalmPosition.y;
			pinch_distances [index_direction] = pitch_dist;
			pan_areas [index_direction] = (finger_x_max - finger_x_min) * (finger_y_max - finger_y_min);

			// Check Gesture
			int gesVertical = CheckSwipeVertical (index_direction);
			int gesHorizontal = CheckSwipeHorizontal (index_direction);
			int pinch = CheckPinch (index_direction);
			int pan = CheckPan (index_direction);
					
			// Update index
			index_direction = (index_direction + 1) % max_count;

	
			if (ObjectController.instance.GetMove ()) {
				// Move object to eye position
				Vector3 new_movement = currentHand.PalmPosition.ToUnityScaled() - (new Vector3 (0,0.2f,0));
				ObjectController.instance.MoveTarget(new_movement);
			}

			// Swipe Down
			if (pan == 1 && !ObjectController.instance.checkPreviewStatus ()) {
				ObjectController.instance.onPreviewOpen ();
				gestured = "Pan" + (++gesture_count).ToString();
				Array.Clear (pan_areas, 0, max_count);
			} else if (gesVertical == 1) {

				if (ObjectController.instance.checkPreviewStatus ())
					ObjectController.instance.preview.Next ();
				else
					ObjectController.instance.ChangeRotation (gesVertical);

				gestured = "Swipe Right" + (++gesture_count).ToString();
				Array.Clear (x_direction, 0, max_count);
			} else if (gesHorizontal == 1) {
				
				if (!ObjectController.instance.checkPreviewStatus ()) {
					if (ObjectController.instance.GetTarget () != null) {
						ObjectController.instance.SetOnMove ();
					} 
				} else {
					ObjectController.instance.CreateObject ();
				}

				gestured = "Swipe Down" + (++gesture_count).ToString();
				Array.Clear (y_direction, 0, max_count);

			} else if (pinch != 0) { 
				
				ObjectController.instance.ChangeScale (pinch);

				if (pinch == 1)
					gestured = "Pinch Zoom Up" + (++gesture_count).ToString();
				else
					gestured = "Pinch Zoom Down" + (++gesture_count).ToString();
				
				Array.Clear (pinch_distances, 0, max_count);
			} else {
				gesture_count = 0;
			}
				
				
			GestureTextUI.text = gestured;
			HandTextUI.text = HandTextUI.text + " " + gestured;

		}


		// clear out stale hands.
		List<int> staleIDs = new List<int>();
		foreach(KeyValuePair<int, GameObject> h in m_hands) {
			if (h.Value.GetComponent<LeapRiggedHand>().m_stale) {
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

