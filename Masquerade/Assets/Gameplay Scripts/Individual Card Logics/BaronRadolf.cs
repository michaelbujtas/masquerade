using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaronRadolf : CardLogic, IAfterAttacking {
	public void AfterAttacking(Card defender, Action callback)
	{
		Card.StartCoroutine(Networking.DrawCardCOR(Card.LastOwner.PlayerIndex, 1, (a)=>callback()));
	}

}
