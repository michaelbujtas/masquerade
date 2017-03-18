using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OurriTheNobleslayer : CardLogic, ICanKill
{
	public bool CanKill(Card other)
	{
		if (other.CardClass == CardClass.NOBLE ||
			other.CardClass == CardClass.KING ||
			other.CardClass == CardClass.QUEEN)
			return true;
		else
			return false;
	}
}
