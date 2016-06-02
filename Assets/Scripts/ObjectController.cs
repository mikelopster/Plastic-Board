using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Vuforia;

public class ObjectController : MonoBehaviour {

	public Preview preview;
	public static ObjectController instance;
	public GameObject CameraObject,CubeObject,ImageTargetObject;
	public CardboardReticle cardR;
	public ObjectManager objectManager;
	public Sender sender;
	public Text isPreviewUIText,isMoveUIText,TargetObjectUIText,PositionUIText,RotationUIText;
	public GameObject particle;
 	
	GameObject TargetObject;
	Rigidbody TargetRigidbody;
	Renderer TargetRenderer;
	Dictionary<int,string> objectIdTable = new Dictionary<int,string>();
	Queue<GameObject> queue = new Queue<GameObject> ();

	// Use this for initialization
	bool isMove = false;
	bool isPreview = false;
	bool isShow = false;
	public bool isCheck = false;

	void Awake() {
		instance = this;
	}

	void Start () {
		isMove = false;
		isPreview = false;
		isShow = false;

		sender.SetOnPacket (Sender.Command.spawn, onCmdSpawn);
		sender.SetOnPacket (Sender.Command.update, onCmdUpdate);

	}
	
	// Update is called once per frame
	void Update () {
		Debug.Log ("IsMove: " + isMove.ToString ());

		if (TargetObject) {
			TargetObjectUIText.text = TargetObject.name;
			PositionUIText.text = TargetObject.transform.localPosition.ToString();
			RotationUIText.text = TargetObject.transform.localRotation.eulerAngles.ToString();
		} else {
			TargetObjectUIText.text = "none";
			PositionUIText.text = "0,0,0";
			RotationUIText.text = "0,0,0";
		}
		Debug.Log ("onCheck: " + isCheck);

		// Queue for hide renderer (Load Multiplayer)
		if (queue.Count > 0) {
			GameObject newObj = queue.Dequeue ();
			Renderer rend = newObj.GetComponent<Renderer> ();

			if (rend != null && !isCheck)
				rend.enabled = false;
			else {
				if(!isCheck)
					queue.Enqueue (newObj);
			}
				
		}

		// Keyboard Controller
		if (Input.GetKeyDown (KeyCode.Z))
			ChangeScale (1);
		else if (Input.GetKeyDown (KeyCode.X))
			ChangeScale (-1);
		else if (Input.GetKey (KeyCode.U))
			MoveTarget (new Vector3(0.2f,0,0));
		else if (Input.GetKey (KeyCode.I))
			MoveTarget (new Vector3(-0.2f,0,0));
		else if (Input.GetKey (KeyCode.O))
			MoveTarget (new Vector3(0,0.2f,0));
		else if (Input.GetKey (KeyCode.P))
			MoveTarget (new Vector3(0,-0.2f,0));
		else if (Input.GetKeyDown (KeyCode.R))
			ChangeRotation (1);
		else if (Input.GetKeyDown (KeyCode.E))
			ChangeRotation (-1);
		else if (Input.GetKeyDown (KeyCode.H))
			ReleaseTarget ();
		else if (Input.GetKeyDown (KeyCode.S)){
			if (!checkPreviewStatus ())
				onPreviewOpen ();
			else
				onPreviewClose ();
		}
		else if (Input.GetKeyDown (KeyCode.F))
			preview.Next ();
		else if (Input.GetKeyDown (KeyCode.G))
			preview.Previous ();
		else if (Input.GetKeyDown (KeyCode.Q))
			CreateObject ();
				
	}

	public void ChangeScale(int type) {
		if (TargetObject) {
			float scaleDist = 0.01f * type;
			TargetObject.transform.localScale = new Vector3 (TargetObject.transform.localScale.x + scaleDist,TargetObject.transform.localScale.y + scaleDist,TargetObject.transform.localScale.z + scaleDist);
			sender.SendPacket(Sender.Command.update,objectManager.Pack(objectIdTable [TargetObject.GetInstanceID()]));
		}
	}

	public void ChangeColor() {
		if (TargetObject) {
			TargetRenderer.material.color = new Color (Random.Range(0f,1f),Random.Range(0f,1f),Random.Range(0f,1f),1);
		}
	}

	public void ChangeRotation(int direction) {
		if (TargetObject) {
			Vector3 rot = TargetObject.transform.localRotation.eulerAngles;
			rot.y += (direction * 20);

			TargetObject.transform.localRotation = Quaternion.Euler(rot);
			sender.SendPacket(Sender.Command.update,objectManager.Pack(objectIdTable [TargetObject.GetInstanceID()]));
		}
	}

	public bool GetMove() {
		return isMove;
	}

	public void MoveTarget(Vector3 movement) {
		if (TargetObject) {
//			TargetObject.transform.position = new Vector3 (cardR.iPosition.x, cardR.iPosition.y + 0.5f, cardR.iPosition.z);
	
			TargetObject.transform.localPosition = new Vector3(TargetObject.transform.localPosition.x + (movement.x * 0.05f),TargetObject.transform.localPosition.y,TargetObject.transform.localPosition.z + (movement.y * 0.05f * -1));
			sender.SendPacket(Sender.Command.update,objectManager.Pack(objectIdTable [TargetObject.GetInstanceID()]));
		}
	}

	public void SetTarget(GameObject obj) {
		TargetObject = obj;

		TargetRenderer = TargetObject.GetComponent<Renderer> ();

		// Add Partical for knew that it was selected
		GameObject targetParticle = (GameObject)Instantiate(particle);
		targetParticle.transform.SetParent(TargetObject.transform);
		targetParticle.transform.localPosition = new Vector3 (0, 0, 0);
		targetParticle.transform.localScale = new Vector3 (0.1f, 0.1f, 0.1f);

//		ParticleSystem particleSys = TargetObject.transform.S


	}

	public void SetOnMove() {
		if(TargetObject)
		isMove = !isMove;
	}
		
	public void ReleaseTarget() {
		if (TargetObject) {
			isMove = false;
			isMoveUIText.text = "false";

//			Destroy (TargetObject.GetComponent<ParticleSystem> ());
			GameObject pt = GameObject.Find("Particle(Clone)");
			Destroy (pt);
//			particle.Stop();
			TargetObject = null;
			TargetRenderer = null;
		}

	}

	public GameObject GetTarget() {
		return TargetObject;
	}

	public void CreateObject() {
		
		onPreviewClose ();
		// Create Object on plane
		GameObject newObj = preview.GetCurrentObject ();
		newObj.transform.SetParent (ImageTargetObject.transform);
		newObj.tag = "Property";
		CapsuleCollider capsultCharacter = newObj.AddComponent<CapsuleCollider> ();
		capsultCharacter.radius = 2f;
		capsultCharacter.height = 4.5f;
		capsultCharacter.center = new Vector3(0,0,0);

		Vector3 chacPosition = new Vector3(cardR.iPosition.x,cardR.iPosition.y+0.5f,cardR.iPosition.z);
		newObj.transform.localScale = new Vector3 (0.2f, 0.2f, 0.2f);
		newObj.transform.localRotation = Quaternion.identity;
		newObj.transform.position = chacPosition;

		// Network 
		ObjectPacket objectPacket = objectManager.Create();
		objectPacket.type = newObj.name;
		objectPacket.gameObject = newObj;
		sender.SendPacket (Sender.Command.spawn, objectManager.Pack (objectPacket.id));

		objectIdTable [newObj.GetInstanceID()] = objectPacket.id;

		// 
	}
		

	public bool checkPreviewStatus() {
		return isPreview;
	}

	public void onPreviewOpen() {
		Debug.Log ("Open");
		isPreview = true;
		isPreviewUIText.text = "true";
		preview.Show ();
		ReleaseTarget ();
	}

	public void onPreviewClose() {
		Debug.Log ("Close");
		isPreview = false;
		isPreviewUIText.text = "false";
		preview.Hide ();
	}


	void onCmdSpawn(Packet packet) {
		Debug.Log ("Spawn Data:" + packet.id + ", " + packet.type + ", " + packet.message);

		// Create object from 
		ObjectPacket objectPacket = objectManager.Create (packet.id);
		GameObject newObj = preview.GetObject (packet.type);
		newObj.transform.SetParent (ImageTargetObject.transform);
		newObj.tag = "Property";
		CapsuleCollider capsultCharacter = newObj.AddComponent<CapsuleCollider> ();
		capsultCharacter.radius = 2f;
		capsultCharacter.height = 4.5f;
		capsultCharacter.center = new Vector3(0,0,0);

		objectPacket.gameObject = newObj;
		objectPacket.type = packet.type;
		objectPacket.Update (packet.message);

		objectIdTable [newObj.GetInstanceID()] = objectPacket.id;

		queue.Enqueue (newObj);
	}


	void onCmdUpdate(Packet packet) {
		ObjectPacket objectPacket = objectManager.Get (packet.id);
		objectPacket.Update (packet.message);
	}



}
