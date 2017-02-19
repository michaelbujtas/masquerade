using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalfRicket : CardLogic, IStaticEffect {

	Card.Buff buff = null;
	int lastAttack = 0;
	void IStaticEffect.StaticEffect()
	{
		if(Card.IsAlive)
		{
			if (!Card.HasBuff(buff))
				buff = Card.AddBuff(0, 0, true, true);

			lastAttack = buff.Attack;
			buff.Attack = Card.Owner.Hand.CardsOwned.Count * 2;

			if (lastAttack != buff.Attack)
				Card.Sync();
		}
	}
}
