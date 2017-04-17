using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobTheButler : CardLogic, IEndPhase
{
	void IEndPhase.OnEndPhase(MasqueradePlayer turn, System.Action callback)
	{
		if(Card.IsFaceUp)
		{
			int cardsToDraw = 3 - turn.Hand.CardsOwned.Count;
			if (cardsToDraw > 0)
			{
				Networking.DrawCardFacing(turn.PlayerIndex, cardsToDraw, true);
			}
			callback();
		}
		else
		{
			callback();
		}
	}
}