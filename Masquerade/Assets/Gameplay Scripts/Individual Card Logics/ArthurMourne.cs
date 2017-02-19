using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArthurMourne : CardLogic, IStaticEffect
{
	Card theKing = null;
	Card.Buff buff = null;
	void IStaticEffect.StaticEffect()
	{
		if (Card.IsAlive && Card.IsFaceUp)
		{
			if(theKing == null)
			{
				if (Card.Owner != null)
				{
					foreach (MasqueradePlayer p in Card.Networking.MasqueradePlayers)
					{
						foreach (byte b in p.Hand.CardsOwned)
						{
							Card c = Card.Networking.TheCardIndex.GetCard(b);

							if (c.CardClass == CardClass.KING)
								theKing = c;
						}
					}
				}
			}

			if(theKing != null)
			{
				if (!theKing.HasBuff(buff))
				{
					buff = theKing.AddBuff(0, 0, true, true);
					buff.Keywords.Add(Card.Keyword.CANT_BE_KILLED);
				}
			}
			
		}
		else
		{

			if (theKing != null && buff != null)
			{
				theKing.RemoveBuff(buff);
				buff = null;
			}
		}
	}

}
