using UnityEngine;
using System.Collections;
using BeardedManStudios.Network;
using AdvancedInspector;

[AdvancedInspector]
public class PlayerIdentity : NetworkedMonoBehavior {

	[Inspect]
	public string Name
	{
		get;
		private set;
	}

	[Inspect]
	public int PlayerNumber
	{
		get;
		private set;
	}


	PlayerIdentitySettings localSettingsObject;
	

	void Awake()
	{
		localSettingsObject = FindObjectOfType<PlayerIdentitySettings>();
	}


	public void Setup(NetworkingPlayer owner, int number)
	{
		if(OwningNetWorker.IsServer && OwningNetWorker.Connected)
		{
			owner.SetMyBehavior(this);
			RPC("SetPlayerNumberRPC", number);
			AuthoritativeRPC("ConnectSettingsObjectRPC", OwningNetWorker, owner, false);
		}
	}


	[BRPC]
	public void ConnectSettingsObjectRPC()
	{
		localSettingsObject.MyIdentity = this;
		localSettingsObject.RefreshSettings();
	}

	public void SetName(string name)
	{
		RPC("SetNameRPC", NetworkReceivers.AllBuffered, name);
	}


	[BRPC]
	void SetNameRPC(string name)
	{
		Name = name;
	}

	public void Clear()
	{
		RPC("ClearRPC");
	}

	[BRPC]
	void ClearRPC()
	{
		Name = "";
	}


	[BRPC]
	void SetPlayerNumberRPC(int number)
	{
		PlayerNumber = number;
	}


}
