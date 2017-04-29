using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poisoner : CardLogic, IAfterAttacking, IStartPhaseParasite
{
	List<Buff> buffs = new List<Buff>();
	public void AfterAttacking(Card defender, Action callback)
	{
		Buff newBuff = defender.AddBuff(0, 0, true, false, this);
		newBuff.Keywords.Add(Keyword.START_PHASE_PARASITE);
		buffs.Add(newBuff);
		callback();
		
	}

	public void OnStartPhaseParasite(Card other, System.Action callback)
	{
		bool whiffed = true;

		for(int i = 0; i < buffs.Count; i++)
		{
			Buff b = buffs[i];
			if(b.Card.IsAlive)
			{
				if (b.Card == other)
				{
					whiffed = false;
					Card.StartCoroutine(b.Card.Flip(true, (actuallyFlipped) => {

						Card.StartCoroutine(b.Card.KillWithContext(Card, DeathContext.OTHER, (a) => callback()));

					}));

				}
			}
			else
			{
				buffs.Remove(b);
				i--;
			}
		}
		if (whiffed)
			callback();
	}
}
