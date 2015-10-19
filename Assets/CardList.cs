using UnityEngine;
using System.Collections.Generic;

public class CardList : MonoBehaviour {

	public List<Card> Cards = new List<Card>();


	public void CopyImport(CardList list)
	{
		foreach(Card c in list.Cards)
		{
			gameObject.AddComponent<Card>().Copy(c);
		}
		ScrapeCardsFromGameObject();
	}


	public void ReferenceImport(CardList list)
	{
		Cards.AddRange(list.Cards);
	}

	public void ScrapeCardsFromGameObject()
	{
		Cards.Clear();
		Cards.AddRange(GetComponents<Card>());
	}
}
