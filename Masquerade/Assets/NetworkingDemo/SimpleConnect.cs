using UnityEngine;
using System.Collections;
using BeardedManStudios.Network;
using UnityEngine.UI;

public class SimpleConnect : SimpleNetworkedMonoBehavior
{

	public InputField ipField, portField;
	public Button hostButton, joinButton, disconnectButton;
	public bool WeAreHost
	{
		get;
		private set;
	}
	public bool IsConnected;
	public bool IsHeadless
	{
		get;
		private set;
	}

	void Awake()
	{
		IsHeadless = SystemInfo.graphicsDeviceID == 0;
	}

	// Use this for initialization
	void Start () {

		//disconnectButton.gameObject.SetActive(false);

		if(IsHeadless)
		{
			HostButtonClickUI();
		}
	}
	
	// Update is called once per frame
	void Update () {
		try
		{
			IsConnected = Networking.IsConnected(Port);
		}
		catch (System.Exception e)
		{
			CustomConsole.Log("Networking.IsConnected threw execption: " + e.GetType().ToString());
			IsConnected = false;
			//Jesus
		}
}

	public void HostButtonClickUI()
	{
		Networking.Disconnect();
		WeAreHost = true;
		Networking.Host(Port, Networking.TransportationProtocolType.UDP, 4);
		AddListeners();
	}

	public void JoinButtonClickUI()
	{
		Networking.Disconnect();
		WeAreHost = false;
		string ip = ipField.text;
		Networking.Connect(ip, Port, Networking.TransportationProtocolType.UDP, true);
		AddListeners();
	}

	public void DisconnectButtonClickUI()
	{
		Networking.Disconnect();
	}


	public void AddListeners()
	{
		NetWorker.playerConnected += UpdateIdentities;


		NetWorker.playerDisconnected += UpdateIdentities;


		NetWorker.connected += delegate
		{
			CustomConsole.Log("Connection Established");
			joinButton.gameObject.SetActive(false);
			hostButton.gameObject.SetActive(false);
			//disconnectButton.gameObject.SetActive(true);

		};

		NetWorker.disconnected += delegate
		{
			CustomConsole.Log("Connection Closed or Lost");
			joinButton.gameObject.SetActive(true);
			hostButton.gameObject.SetActive(true);
			//disconnectButton.gameObject.SetActive(false);
			WeAreHost = false;
		};
		
		

		NetWorker.serverDisconnected += delegate (string reason)
		{
			CustomConsole.Log("Server Disconnected for Reason: " + reason);
			joinButton.gameObject.SetActive(true);
			hostButton.gameObject.SetActive(true);
			//disconnectButton.gameObject.SetActive(false);
			WeAreHost = false;
		};

	}


	public ushort Port
	{
		get { return ((ushort)System.Convert.ToInt16(portField.text, 10)); }
	}

	public NetWorker NetWorker
	{
		get { return Networking.Sockets[Port]; }
	}

	public void UpdateIdentities(NetworkingPlayer player)
	{
		GameplayNetworking networking = FindObjectOfType<GameplayNetworking>();
		CustomConsole.Log("NetworkID " + player.NetworkId + " connected.");

		foreach (PlayerIdentity ident in networking.PlayerIdentities)
			ident.Clear();

		for (int i = 0; i < OwningNetWorker.Players.Count; i++)
			networking.PlayerIdentities[i].Setup(OwningNetWorker.Players[i], i);
	}

}
