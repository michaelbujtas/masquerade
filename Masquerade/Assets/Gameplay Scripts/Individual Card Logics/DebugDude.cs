using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDude : CardLogic, IStartPhase {
	public void OnStartPhase(MasqueradePlayer turn, Action callback)
	{
		if (turn == Card.LastOwner)
			Card.AddBuff(2, 0, false, true, this);
		else
			Card.AddBuff(0, 2, false, true, this);
		callback();

	}

}
