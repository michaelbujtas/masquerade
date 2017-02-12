﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanceWymund : CardLogic, IStaticEffect {

	/*
	int ISpecialStats.GetBaseAttack()
	{
		List<Card> cardsInPlay = Card.Networking.GetCardsInPlay();

		bool isTurnedOn = false;

		foreach(Card c in cardsInPlay)
		{
			if (c.CardClass == CardClass.KING || c.CardClass == CardClass.QUEEN)
				isTurnedOn = true;
		}

		if (isTurnedOn)
			return Card.Attack + 7;
		else
			return Card.Attack;
	}

	int ISpecialStats.GetBaseDefense()
	{
		return Card.Defense;
	}*/


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

		if (buff == null)
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