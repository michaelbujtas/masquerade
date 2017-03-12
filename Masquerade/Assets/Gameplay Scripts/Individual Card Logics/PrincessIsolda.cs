using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrincessIsolda : CardLogic, IFlipEffect
{

	void IFlipEffect.OnFlip(bool flippedFaceUp, System.Action callback)
	{
		if (Card.IsAlive && flippedFaceUp == true)
		{

		}
	}
}