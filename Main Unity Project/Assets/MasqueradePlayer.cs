using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DevConsole;
using BeardedManStudios.Network;

public class MasqueradePlayer {


	public NetworkingPlayer Player
	{
		get;
		private set;
	}
	public byte PlayerIndex
	{
		get;
		private set;
	}

	public MasqueradePlayer(NetWorker netWorker, byte index)
	{
		PlayerIndex = index;

		if (netWorker.Players.Count > index)
		{
			Player = netWorker.Players[index];
		}
		else
		{
			Player = null;
		}
	}
}
