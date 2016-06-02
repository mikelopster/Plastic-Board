using UnityEngine;
using System.Collections;
using Leap;

public class LeapRiggedHand : MonoBehaviour {

	GameObject m_riggedHand = null;
	GameObject m_bones;

	Quaternion offsetRightArm = Quaternion.Euler(new Vector3(90, 90, 0));
	Quaternion offsetRightHand = Quaternion.Euler(new Vector3(90, 0, 0));
	
	Quaternion offsetLeftArm = Quaternion.Euler(new Vector3(-90, 90, 0));
	Quaternion offsetLeftHand = Quaternion.Euler(new Vector3(-90, 0, 0));

	bool m_isRightHand;

	public bool IsRight() {
		return m_isRightHand;
	}

	public bool m_stale = false;

	float m_distancePalmToWrist = 0.04f;
	public float m_handScaleFactor = 1.0f;

	public void InitializeHand(bool isRightHand) {
		if (isRightHand) {
			m_riggedHand = Instantiate(Resources.Load("Prefabs/RightRiggedHand")) as GameObject;
		} else {
			m_riggedHand = Instantiate(Resources.Load("Prefabs/LeftRiggedHand")) as GameObject;
		}
		m_isRightHand = isRightHand;
		m_bones = m_riggedHand.transform.Find("Bip01").Find("Bones").gameObject;
	}

	public void UpdateRig(Hand leapHand) {
		if (m_riggedHand == null) Debug.LogError("Rigged Hand is null, did you call InitializeHand?");
		// get the bones from the rig
		Transform [] rigBones = m_bones.GetComponent<SkinnedMeshRenderer>().bones;

		if (m_isRightHand) {
			UpdateHandRig(rigBones, "R", offsetRightArm, offsetRightHand, leapHand, Vector3.zero);
		} else {
			UpdateHandRig(rigBones, "L", offsetLeftArm, offsetLeftHand, leapHand, Vector3.zero);
		}
		m_stale = false;
		float scale = WristToMiddleKnuckle(leapHand) / 55.0f;
		m_riggedHand.transform.localScale = new Vector3(scale, scale, scale);
	}

	public void Draw(bool shouldDraw) {
		if (m_riggedHand == null) Debug.LogError("Rigged Hand is null, did you call InitializeHand?");
		m_bones.GetComponent<SkinnedMeshRenderer>().enabled = shouldDraw;
	}

	Transform FindBone(Transform [] array, string boneName) {
		for (int i = 0; i < array.Length; ++i) {
			if (array[i].name == boneName) return array[i];
		}
		Debug.LogError("Bone Not Found: " + boneName);
		return null;
	}

	void UpdateHandRig(Transform [] bones, string hand, Quaternion offsetArm, Quaternion offsetHand, Hand h, Vector3 handStart) {

		Quaternion handRot = Quaternion.identity;
		
		Transform handTransform = FindBone(bones, "Bip01 " + hand + " Hand");
		handTransform.rotation = Quaternion.LookRotation(h.Direction.ToUnity(), -h.PalmNormal.ToUnity()) * offsetArm * offsetHand;
		handRot = handTransform.rotation;
		handTransform.position = h.PalmPosition.ToUnityScaled() - h.Direction.ToUnity() * m_distancePalmToWrist;
		
		for (int i = 0; i < h.Fingers.Count; ++i) {
			Finger finger = h.Fingers[i];
			
			// get all the joint positions in unity space.
			Vector3 mcpPos = finger.JointPosition(Finger.FingerJoint.JOINT_MCP).ToUnityScaled();
			Vector3 pipPos = finger.JointPosition(Finger.FingerJoint.JOINT_PIP).ToUnityScaled();
			Vector3 dipPos = finger.JointPosition(Finger.FingerJoint.JOINT_DIP).ToUnityScaled();
			Vector3 tipPos = finger.JointPosition(Finger.FingerJoint.JOINT_TIP).ToUnityScaled();
			
			// compute finger joint rotations
			Transform mcp = FindBone(bones, "Bip01 " + hand + " Finger" + i);
			mcp.rotation = Quaternion.FromToRotation(h.Direction.ToUnity(), (pipPos - mcpPos).normalized);

			mcp.rotation *= handRot;

			Transform pip = FindBone(bones, "Bip01 " + hand + " Finger" + i + "1");
			pip.rotation = Quaternion.FromToRotation((pipPos - mcpPos).normalized, (dipPos - pipPos).normalized) * mcp.rotation;
			Transform dip = FindBone(bones, "Bip01 " + hand + " Finger" + i + "2");
			dip.rotation = Quaternion.FromToRotation((dipPos - pipPos).normalized, (tipPos - dipPos).normalized) * pip.rotation;
			
		}
	}

	float WristToMiddleKnuckle(Hand h) {
		return h.Fingers[2].Length;
	}

	void OnDestroy() {
		// InitializeHand might not have been called. Or the object is
		// destroyed already when the game quits.
		if (m_riggedHand != null) {
			Destroy(m_riggedHand);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
