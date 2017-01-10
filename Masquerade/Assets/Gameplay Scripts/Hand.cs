using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Hand : CardList {

	public List<Vector2> offsets = new List<Vector2>();

	public CardFactory factory;

	public void AddCard(CardRenderer card)
	{
		if(!Cards.Contains(card.Card))
		{
			Cards.Add(card.Card);
			card.rectTransform.parent = transform;

			PositionCards();
		}
	}

	public Card RemoveCard(CardRenderer card)
	{
		if(Cards.Contains(card.Card))
		{
			Cards.Remove(card.Card);
			PositionCards();
			return card.Card;
		}
		return null;
	}

	public void PositionCards()
	{
		for(int i = 0; i < Cards.Count && i < offsets.Count; i++)
		{
			Cards[i].Renderer.rectTransform.anchoredPosition = offsets[i];
		}
	}


	
}
