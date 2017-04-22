using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : CardLogic, IFlipEffect
{
	void IFlipEffect.OnFlip(bool flippedFaceUp, Action callback)
	{
		if(flippedFaceUp)
		{
			List<byte> cardsInGraveyard = new List<byte>();
			cardsInGraveyard.AddRange(Networking.TheDiscardPile.Contents);
			cardsInGraveyard.Remove((byte)Card.Index);

			Card.StartCoroutine(Networking.PickACardCOR(Card.LastOwner.PlayerIndex,
				cardsInGraveyard.ToArray(),
				(choice) =>
				{
					Networking.TheDiscardPile.RemoveIndex(choice);

					Card foundCard = Networking.TheCardIndex.GetCard(choice);
					foundCard.Owner = Card.LastOwner;

					foundCard.IsAlive = true;
					foundCard.IsFaceUp = true;

					Networking.AddCardToBoards(Card.LastOwner.PlayerIndex, choice);
					callback();

				},

				() =>
				{

					callback();
				},
				Color.green,
				true));
		}
		

	}
}