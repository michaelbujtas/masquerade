using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BeardedManStudios.Network;
using AdvancedInspector;

[AdvancedInspector]
public class MasqueradePlayer {

	[Inspect]
	public NetworkingPlayer NetworkingPlayer
	{
		get;
		private set;
	}

	[Inspect]
	public byte PlayerIndex
	{
		get;
		private set;
	}

	[Inspect]
	public PlayerIdentity Identity
	{
		get;
		private set;
	}

	[Inspect]
	public IndexHand Hand
	{
		get;
		private set;
	}

	public MasqueradePlayer(NetworkingPlayer netWorker, byte index, PlayerIdentity identity, IndexHand hand)
	{
		PlayerIndex = index;

		NetworkingPlayer = netWorker;

		Identity = identity;

		Hand = hand;
	
		
	}
}
