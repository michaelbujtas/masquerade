using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NickieTheShredder : CardLogic, IBeforeAttacking, IAfterAttacking
{
	Buff buff;

	public void BeforeAttacking(Card defender, Action callback)
	{
		buff = defender.AddBuff(0, 0, true, true, this);
		buff.Keywords.Add(Keyword.NO_BONUSES);
		callback();
	}

	public void AfterAttacking(Card defender, Action callback)
	{
		defender.RemoveBuff(buff);
		buff = null;
		callback();
	}

}
