using UnityEngine;
using System.Collections.Generic;

public class Deck : CardList {

	public List<CardList> Sources = new List<CardList>();
	public MasqueradeEngine Engine;

	public void Start()
	{
		ImportLists();
		Shuffle();
	}

	public void DrawButton()
	{
		Engine.DrawCard();
	}

	public void Shuffle()
	{
		List<Card> newOrder = new List<Card>();
		while(Cards.Count > 0)
		{
			int randomIndex = Random.Range(0, Cards.Count);
			newOrder.Add(Cards[randomIndex]);
			Cards.RemoveAt(randomIndex);
		}
		Cards = newOrder;
	}

	void ImportLists()
	{
		foreach(CardList list in Sources)
		{
			list.ScrapeCardsFromGameObject();
			ReferenceImport(list);
		}
	}
	

	public Card DrawCard()
	{
		if (Cards.Count > 0)
		{
			Card retVal = Cards[0];
			Cards.RemoveAt(0);
			return retVal;
		}
		CustomConsole.LogError("Drew a card when there were no cards. Returning null.");
		return null;
	}
	

}
