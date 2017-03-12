using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueenBea : CardLogic, IOnKilled
{
	void IOnKilled.OnKilled(Card killer, DeathContext context, System.Action callback)
	{
		if(context == DeathContext.DEFENDING)
		{
			foreach (byte b in killer.LastOwner.Hand.CardsOwned)
			{
				Card.Networking.GiveControl(Card.LastOwner.PlayerIndex, b);
			}
		}
		callback();
	}
}
