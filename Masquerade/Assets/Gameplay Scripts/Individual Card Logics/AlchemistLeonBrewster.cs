using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlchemistLeonBrewster : CardLogic, IStaticEffect
{

	Dictionary<Card, Buff> buffs = new Dictionary<Card, Buff>();

	void IStaticEffect.StaticEffect()
	{
		if (Card.IsAlive && Card.IsFaceUp)
		{

			foreach (KeyValuePair<Card, Buff> b in buffs)
			{
				if (b.Key.Owner != Card.Owner)
				{
					b.Key.RemoveBuff(b.Value);
				}
			}

			foreach (byte b in Card.Owner.Hand.CardsOwned)
			{
				Card c = Card.Networking.TheCardIndex.GetCard(b);

				if (!buffs.ContainsKey(c))
				{
					buffs.Add(c, c.AddBuff(0, 2, true, false, this));
				}

				if (!c.HasBuff(buffs[c]))
					buffs[c] = c.AddBuff(0,2, true, false, this);
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