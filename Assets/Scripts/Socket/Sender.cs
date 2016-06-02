using System;
using UnityEngine;

public class Sender : MonoBehaviour {

	public Socket socket;

	public enum Command {
		join,
		spawn,
		update,
		destroy
	}

	[Serializable]
	struct DataPacket {
		public int command;
		public Packet packet;
	}

	Action <Packet> onJoinCallback;
	Action <Packet> onSpawnCallback;
	Action <Packet> onUpdateCallback;
	Action <Packet> onDestroyCallback;


	// Start life cycle


	void Start () {
		socket.SetOnMessage (OnDataPacket);
	}


	// Implement packet methods


	public void SendPacket (Command command, Packet packet) {
		SendDataPacket (command, packet);
	}


	public void SetOnPacket (Command command, Action<Packet> callback) {
		switch (command) {

		case Command.join:
			onJoinCallback = callback;
			break;

		case Command.spawn:
			onSpawnCallback = callback;
			break;

		case Command.update:
			onUpdateCallback = callback;
			break;

		case Command.destroy:
			onDestroyCallback = callback;
			break;
		}
	}


	// Implement data packet functions


	void SendDataPacket (Command command, Packet packet) {

		DataPacket dataPacket = new DataPacket ();
		dataPacket.command = (int)command;
		dataPacket.packet = packet;

		string message = JsonUtility.ToJson (dataPacket);
		socket.SendMessage (message);
	}

	void OnDataPacket (string message) {

		DataPacket dataPacket = JsonUtility.FromJson<DataPacket> (message);
		Packet packet = dataPacket.packet;
		switch ((Command)dataPacket.command) {

		case Command.join:
			onJoinCallback (packet);
			break;

		case Command.spawn:
			onSpawnCallback (packet);
			break;

		case Command.update:
			onUpdateCallback (packet);
			break;

		case Command.destroy:
			onDestroyCallback (packet);
			break;
		}
	}
}
