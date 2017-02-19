using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanceWymund : CardLogic, IStaticEffect {

	Card.Buff buff = null;

	bool wasTurnedOn;

	void IStaticEffect.StaticEffect()
	{
		List<Card> cardsInPlay = Card.Networking.GetCardsInPlay();

		bool isTurnedOn = false;

		foreach (Card c in cardsInPlay)
		{
			if((c.IsFaceUp) && (c.CardClass == CardClass.KING || c.CardClass == CardClass.QUEEN))
				isTurnedOn = true;
		}


		if (!Card.HasBuff(buff))
			buff = Card.AddBuff(0, 0, true, true);

		if (isTurnedOn && Card.IsAlive)
			buff.Attack = 7;
		else
			buff.Attack = 0;

		if(wasTurnedOn != isTurnedOn)
		{
			Card.Networking.SyncCard(Card);
			wasTurnedOn = isTurnedOn;
		}

	}
}
