using UnityEngine;
using System.Collections;

public class DiscardPile : CardList {
	public CardRenderer Renderer;

	public void Discard(Card card)
	{
		Cards.Add(card);
		Renderer.Card = card;
		Renderer.RefreshCardImage();
	}

	public void DiscardPileButton()
	{

	}

	public void Clear()
	{
		Cards.Clear();
		Renderer.Card = null;
		Renderer.RefreshCardImage();
		Renderer.NameText.text = "DISCARD";
	}
}
