using UnityEngine;
using System.Collections.Generic;
using DevConsole;
using System.Linq;


public class IndexHand : MonoBehaviour {

	public List<CardRenderer> Renderers;
	public byte PlayerNumber;

	public void Awake()
	{
		Renderers.AddRange(GetComponentsInChildren<CardRenderer>());
	}


	public void SetIndex(byte slot, byte index)
	{
		Renderers[slot].Index = index;
		Renderers[slot].RefreshCardImage();
	}

	public byte FirstOpenSlot
	{
		get
		{
			for (byte i = 0; i < Renderers.Count; i++)
				if (Renderers[i].Index == CardIndex.EMPTY_SLOT)
					return i;
			Console.LogError("ArgumentOutOfRangeException: No open slots in IndexHand.");
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
			return possibleCards[Random.Range(0, possibleCards.Count)];
		}
	}
}
