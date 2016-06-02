using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class Socket : MonoBehaviour {

	public string url;
	public float delay;
	public bool isReady;

	WebSocket ws;
	Queue<string> messageQueue;
	Action<string> onMessageCallback;


	// Start life cycle


	void Awake () {
		ws = new WebSocket (url);
		ws.OnOpen += OnOpen;
		ws.OnClose += OnClose;
		ws.OnError += OnError;
		ws.OnMessage += OnMessage;

		messageQueue = new Queue<string> ();
	}

	void Start () {
		StartCoroutine (Connect ());
	}

	void Update () {
		if (messageQueue.Count > 0)
			onMessageCallback (messageQueue.Dequeue ());
	}


	// Implement message methods


	public void SendMessage (string message) {
		ws.SendAsync (message, OnComplete);
	}

	public void SetOnMessage (Action<string> callback) {
		onMessageCallback = callback;
	}


	// Keep connection alive


	IEnumerator Connect () {
		while (true) {
			if (!ws.IsAlive)
				ws.ConnectAsync ();
			yield return new WaitForSeconds (delay);
		}
	}


	// Implement event listeners


	void OnOpen (object sender, EventArgs e) {
		isReady = true;
		Debug.Log ("[Socket] Connection open");
	}

	void OnClose (object sender, CloseEventArgs e) {
		isReady = false;
		Debug.Log ("[Socket] Connection close");
	}

	void OnError (object send, ErrorEventArgs e) {
		isReady = false;
		Debug.Log ("[Socket] Connection error");
	}

	void OnComplete (bool completed) {
		if (completed)
			return;
		Debug.Log ("[Socket] Sending error");
	}

	void OnMessage (object sender, MessageEventArgs e) {
		messageQueue.Enqueue (e.Data);
	}
}
