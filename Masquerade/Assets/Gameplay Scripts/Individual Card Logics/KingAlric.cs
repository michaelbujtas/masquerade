using System.Collections.Generic;


public class KingAlric : CardLogic, IOnKilled, IEndPhase, IHasKeywords
{
	void IOnKilled.OnKilled(Card killer, DeathContext context)
	{
		CustomConsole.LogNetworked(killer.Owner.Identity.Name + " has killed the King! They win!", UnityEngine.Color.yellow);
		Card.Networking.EndGame(killer.Owner);
	}
	

	void IEndPhase.OnEndPhase(MasqueradePlayer turn)
	{
		if(Card.Networking.TheDeck.CardsRemaining == 0)
		{
			CustomConsole.LogNetworked("The deck has run out of cards. " + Card.Owner.Identity.Name + " controls the King! They win!" , UnityEngine.Color.yellow);
			Card.Networking.EndGame(Card.Owner);
		}

		if(Card.IsFaceUp)
		{

			foreach (byte b in Card.Owner.Hand.CardsOwned)
			{
				Card c = Card.Networking.TheCardIndex.GetCard(b);

				if (c.CardClass == CardClass.QUEEN && c.IsFaceUp)
				{
					CustomConsole.LogNetworked(Card.Owner.Identity.Name + " controls the King and Queen! They win!", UnityEngine.Color.yellow);
					Card.Networking.EndGame(Card.Owner);
				}
			}
		}
	}

	List<Card.Keyword> keywords = new List<Card.Keyword>() { Card.Keyword.CANT_BE_DISCARDED };
	List<Card.Keyword> IHasKeywords.GetKeywords()
	{
		return keywords;
	}


}
