using UnityEngine;
using System.Collections;
using BeardedManStudios.Network;

public class Host : MonoBehaviour {

	public ushort port = 15937;
	public bool isConnected;
	// Use this for initialization
	void Start() {

		Networking.Host(port, Networking.TransportationProtocolType.UDP, 4);

		Networking.Sockets[port].connected += delegate
		{
			Debug.Log("Connection Established");
		};

		Networking.Sockets[port].disconnected += delegate
		{
			Debug.Log("Connection Closed or Lost");
		};



	}
	
	// Update is called once per frame
	void Update () {
		isConnected = Networking.IsConnected(port);
	}
}
