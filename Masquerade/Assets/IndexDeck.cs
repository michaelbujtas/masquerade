﻿using UnityEngine;
using System.Collections.Generic;

using AdvancedInspector;

[AdvancedInspector]
public class IndexDeck : MonoBehaviour {

	
	[Inspect]
	public List<byte> Indices = new List<byte>();

	[Inspect]
	public bool ShouldStack = false;

	[Inspect]
	public List<string> StackNames;

	CardIndex cardIndex;

	public void SetIndex(CardIndex index)
	{
		Indices.Clear();
		cardIndex = index;
		Indices.AddRange(cardIndex.AllCardIndices);

	}

	public void Shuffle()
	{
		List<byte> newOrder = new List<byte>();
		while (Indices.Count > 0)
		{
			int randomIndex = Random.Range(0, Indices.Count);
			newOrder.Add(Indices[randomIndex]);
			Indices.RemoveAt(randomIndex);
		}
		Indices = newOrder;
	}

	public void Stack()
	{
		for (int i = 0; i < StackNames.Count; i++)
		{
			string name = StackNames[i];
			for (int j = 0; j < Indices.Count; j++)
			{
				if (cardIndex.GetCard(Indices[j]).CardName.ToLower() == name.ToLower())
				{
					byte foundIndex = Indices[j];
					Indices.Remove(foundIndex);
					Indices.Insert(i, foundIndex);
				}
			}
		}
	}

	public byte Draw()
	{
		byte retVal = Indices[0];
		Indices.RemoveAt(0);
		return retVal;
	}


	public List<byte> TopCards(int count)
	{
		List<byte> retVal = new List<byte>();

		for (int i = 0; i < count; i++)
		{
			retVal.Add(Indices[i]);
		}

		return retVal;
	}

	public bool PullCard(byte index)
	{
		if(Indices.Contains(index))
		{
			Indices.Remove(index);
			return true;
		}
		else
		{
			return false;
		}
	}




	public void ShuffleAway(byte index)
	{
		Indices.Add(index);
		Shuffle();
	}

	public int CardsRemaining
	{
		get { return Indices.Count; }
	}
}
