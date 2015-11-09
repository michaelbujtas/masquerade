using UnityEngine;
using System.Collections;

using BeardedManStudios.Network;

public class Client : MonoBehaviour
{
	public string ip = "127.0.0.1";
	public ushort port = 15937;
	public bool isConnected;

	// Use this for initialization
	void Start ()
	{
		Networking.Connect(ip, port, Networking.TransportationProtocolType.UDP, true);

		Networking.Sockets[port].connected += delegate
		{
			Debug.Log("(Client) Connection Established");
		};

		Networking.Sockets[port].disconnected += delegate
		{
			Debug.Log("(Client) Connection Closed or Lost");
		};

		Networking.Sockets[port].serverDisconnected += delegate(string reason)
		{
			Debug.Log("(Client) Server Disconnected for Reason: " + reason);
		};

	}
	
	// Update is called once per frame
	void Update () {

		isConnected = Networking.IsConnected(port);
	}
}
