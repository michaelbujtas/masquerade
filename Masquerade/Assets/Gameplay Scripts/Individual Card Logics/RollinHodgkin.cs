using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollinHodgkin : CardLogic, IStaticEffect, IAfterAttacking 
{
	Card.Buff buff = null;
	void IStaticEffect.StaticEffect()
	{
		if (Card.IsAlive)
		{
			if (Card.IsFaceUp)
			{
				if (buff != null)
				{
					Card.RemoveBuff(buff);
					buff = null;
				}
			}
			else
			{
				if (!Card.HasBuff(buff))
				{
					buff = Card.AddBuff(0, 0, true, true);
					buff.Keywords.Add(Card.Keyword.CANT_BE_KILLED);
				}
			}
		}
	}

	void IAfterAttacking.AfterAttacking(Card defender)
	{
		if(Card.IsAlive)
		{
			Card.Kill();
		}
	}

}
