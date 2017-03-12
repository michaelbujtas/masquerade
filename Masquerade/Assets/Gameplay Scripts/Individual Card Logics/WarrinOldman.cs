using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarrinOldman : CardLogic, IFlipEffect
{

	void IFlipEffect.OnFlip(bool flippedFaceUp, System.Action callback)
	{
		if (flippedFaceUp)
		{
			MasqueradePlayer owner = Card.LastOwner;


			List<byte> allCards = new List<byte>();
			foreach (MasqueradePlayer p in Card.Networking.MasqueradePlayers)
				allCards.AddRange(p.Hand.CardsOwned);

			for (int i = 0; i < allCards.Count; i++)
			{
				Card c = Card.Networking.TheCardIndex.GetCard(allCards[i]);
				if (c.IsFaceUp && c.CardClass == CardClass.SOLDIER)
					Card.Networking.GiveControl(Card.LastOwner.PlayerIndex, allCards[i]);
			}


		}
		if (callback != null)
			callback();
	}
}