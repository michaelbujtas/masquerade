using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BeardedManStudios.Network;

public class MasqueradePlayer {


	public NetworkingPlayer NetworkingPlayer
	{
		get;
		private set;
	}

	public byte PlayerIndex
	{
		get;
		private set;
	}

	public MasqueradePlayer(NetworkingPlayer netWorker, byte index)
	{
		PlayerIndex = index;

		NetworkingPlayer = netWorker;
		
	}
}
