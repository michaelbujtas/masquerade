using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanteTheStud : CardLogic, IStaticEffect
{
	Card.Buff buff = null;
	void IStaticEffect.StaticEffect()
	{
		if(Card.IsAlive)
		{
			if(ThereAreOtherNobles)
			{
				if (!Card.HasBuff(buff))
				{
					buff = Card.AddBuff(3, 0, true, true);
					buff.Keywords.Add(Card.Keyword.CANT_BE_KILLED);
				}
			}
			else
			{
				Card.RemoveBuff(buff);
			}


		}
	}

	bool ThereAreOtherNobles
	{
		get
		{
			if (Card.Owner != null)
				foreach(MasqueradePlayer p in Card.Networking.MasqueradePlayers)
					foreach (byte b in p.Hand.CardsOwned)
					{
						Card c = Card.Networking.TheCardIndex.GetCard(b);

						if (c.IsFaceUp && c.CardClass == CardClass.NOBLE && c != Card)
							return true;
					}
			return false;
		}
	}
}
