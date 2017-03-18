using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DancerIsabella : CardLogic, IAfterAttacking
{
	public void AfterAttacking(Card defender, Action callback)
	{
		Card.Networking.ShuffleAway(defender);
		callback();
	}
}
