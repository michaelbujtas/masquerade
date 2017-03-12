using UnityEngine;
using System.Collections;
using BeardedManStudios.Network;
public class PlayerIdentity : NetworkedMonoBehavior {

	public string Name
	{
		get;
		private set;
	}

	public int PlayerNumber
	{
		get;
		private set;
	}


	PlayerIdentitySettings localSettingsObject;
	PlayerIdentitySettings LocalSettingsObject
	{
		get
		{
			if(localSettingsObject == null)
			{
				localSettingsObject = FindObjectOfType<PlayerIdentitySettings>();
			}
			return localSettingsObject;
		}
		set
		{
			localSettingsObject = value;
		}
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
		LocalSettingsObject.MyIdentity = this;
		LocalSettingsObject.RefreshSettings();
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
