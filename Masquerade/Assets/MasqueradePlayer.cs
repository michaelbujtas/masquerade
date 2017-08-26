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

	public PlayerIdentity Identity
	{
		get;
		private set;
	}

	public AnimatedHand Hand
	{
		get;
		private set;
	}

	public MasqueradePlayer(NetworkingPlayer netWorker, byte index, PlayerIdentity identity, AnimatedHand hand)
	{
		PlayerIndex = index;

		NetworkingPlayer = netWorker;

		Identity = identity;

		Hand = hand;
	
		
	}
}
