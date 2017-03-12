using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollinHodgkin : CardLogic, IAfterAttacking 
{
	void IAfterAttacking.AfterAttacking(Card defender, System.Action callback)
	{
		if(Card.IsAlive)
		{
			Card.Kill();
		}
		if (callback != null)
			callback();
	}
	
}
