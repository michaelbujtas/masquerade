using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaryTheMute : CardLogic, IStaticEffect, IHasKeywords
{
	Dictionary<Card, Buff> buffs = new Dictionary<Card, Buff>();

	List<Keyword> keywords = new List<Keyword>() { Keyword.CANT_LOSE_TEXT };
	List<Keyword> IHasKeywords.GetKeywords()
	{
		return keywords;
	}


	void IStaticEffect.StaticEffect()
	{
		if (Card.IsAlive && Card.IsFaceUp)
		{

			foreach (Card c in Networking.GetCardsInPlay())
			{
				if(!c.HasKeyword(Keyword.CANT_LOSE_TEXT))
				{
					if (!buffs.ContainsKey(c))
					{
						Buff newBuff = c.AddKeywordBuff(this, Keyword.NO_TEXT);
						newBuff.Keywords.Add(Keyword.NO_TEXT);
						buffs.Add(c, newBuff);
					}

					if (!c.HasBuff(buffs[c]))
					{
						buffs[c] = c.AddKeywordBuff(this, Keyword.NO_TEXT);
					}
				}
				for(int i = 0; i < c.Buffs.Count; i++)
				{
					if(!c.Buffs[i].Source.Card.HasKeyword(Keyword.CANT_LOSE_TEXT))
					{
						c.RemoveBuff(c.Buffs[i]);
						i--;
					}
				}
			}
		}
		else
		{
			if (buffs.Count > 0)
			{
				foreach (KeyValuePair<Card, Buff> b in buffs)
					b.Key.RemoveBuff(b.Value);
				buffs.Clear();
			}
		}

	}
}
