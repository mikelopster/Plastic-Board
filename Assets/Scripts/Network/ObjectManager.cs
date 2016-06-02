using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour {

	public Dictionary<string, ObjectPacket> table;


	// Initialize instance


	void Awake () {
		table = new Dictionary<string, ObjectPacket> ();
	}


	// Implement object methods


	public ObjectPacket Create () {
		ObjectPacket objectPacket = new ObjectPacket ();
		table [objectPacket.id] = objectPacket;
		return objectPacket;
	}

	public ObjectPacket Create (string id) {
		ObjectPacket objectPacket = new ObjectPacket ();
		objectPacket.id = id;
		table [id] = objectPacket;
		return objectPacket;
	}

	public Packet Pack (string id) {
		Packet packet = new Packet ();

		packet.id = table [id].id;
		packet.type = table [id].type;
		packet.message = table [id].Serialize ();

		return packet;
	}

	public ObjectPacket Get (string id) {
		return table [id];
	}

	public void Remove (string id) {
		table.Remove (id);
	}
}
