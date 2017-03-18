using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyguardOgg : CardLogic, IAfterAttacking {

	Buff buff;

	int stacks = 0;

	void IAfterAttacking.AfterAttacking(Card defender, System.Action callback)
	{
		if (!Card.HasBuff(buff))
		{
			stacks = 0;
			buff = Card.AddBuff(0, 0, true, true, this);
		}

		stacks++;


		buff.Attack = stacks * -1;
		buff.Defense = stacks * -1;

		Card.Sync();
		if (callback != null)
			callback();
	}

}
