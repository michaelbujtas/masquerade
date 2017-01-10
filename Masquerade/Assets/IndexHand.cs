﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class IndexHand : MonoBehaviour {

	public List<CardRenderer> Renderers;
	public byte PlayerNumber;
	public PlayerIdentityRenderer AttachedIdentityRenderer;


	public bool KillSlotZero = false;

	public void Awake()
	{
		if(Renderers.Count == 0)
			Renderers.AddRange(GetComponentsInChildren<CardRenderer>());
	}

	public void Update()
	{
		if(KillSlotZero)
		{
			KillSlotZero = false;
			CustomConsole.Log("Slot 0 contains #" + GetIndex(0), Color.yellow);
			RemoveIndex(GetIndex(0));
		}
	}


	public void SetIndex(byte slot, byte index)
	{
		Renderers[slot].Index = index;
		Renderers[slot].RefreshCardImage();
	}

	public byte GetIndex(byte slot)
	{
		return Renderers[slot].Index;
	}

	public int? GetSlot(byte index)
	{
		for (int i = 0; i < Renderers.Count; i++)
		{
			if (Renderers[i].Index == index)
				return i;
        }
		return null;
	} 
	public byte FirstOpenSlot
	{
		get
		{
			for (byte i = 0; i < Renderers.Count; i++)
				if (Renderers[i].Index == CardIndex.EMPTY_SLOT)
					return i;
			CustomConsole.LogError("ArgumentOutOfRangeException: No open slots in IndexHand.");
			throw new System.NotFiniteNumberException();
		}
	}

	public List<byte> CardsOpenToAttack
	{
		get
		{
			List<byte> retval = new List<byte>();
			var faceUpCards =
				from r in Renderers
				where r.Card != null && r.Card.IsFaceUp
				select r.Index;
			retval.AddRange(faceUpCards);
			if(retval.Count == 0/* && CardsOwned.Count > 0*/)
			{
				retval.Add((byte)(PlayerNumber + CardIndex.PLAYER_1_FACEDOWN));
			}
			return retval;
		}
	}

	public List<byte> CardsOwned
	{
		get
		{
			List<byte> retval = new List<byte>();
			var nonBlankCards =
				from r in Renderers
				where r.Card != null
				select r.Index;
			retval.AddRange(nonBlankCards);
			return retval;
		}
	}

	public List<byte> UntappedCards
	{
		get
		{
			List<byte> retval = new List<byte>();
			var nonBlankCards =
				from r in Renderers
				where r.Card != null && !r.Card.IsTapped
				select r.Index;
			retval.AddRange(nonBlankCards);
			return retval;
		}
	}
	public byte RandomFaceDownCard
	{
		get
		{
			List<byte> possibleCards = new List<byte>();
			var faceDownCards =
				from r in Renderers
				where r.Card != null && !r.Card.IsFaceUp
				select r.Index;
			possibleCards.AddRange(faceDownCards);
			if(possibleCards.Count > 0)
				return possibleCards[Random.Range(0, possibleCards.Count)];
			return CardIndex.EMPTY_SLOT;
		}
	}

	public bool RemoveIndex(byte index)
	{
		if(CardsOwned.Contains(index))
		{
			int slot = (int)GetSlot(index);
            Renderers[slot].Index = CardIndex.EMPTY_SLOT;
			Renderers[slot].RefreshCardImage();
			return true;
		}
		return false;
	}
}