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


			foreach (Card c in Networking.GetCardsInPlay())
			{
				if (c.IsFaceUp && c.CardClass == CardClass.SOLDIER && c.Owner != owner)
					Card.Networking.GiveControl(owner.PlayerIndex, (byte)c.Index);
			}


		}
		if (callback != null)
			callback();
	}
}