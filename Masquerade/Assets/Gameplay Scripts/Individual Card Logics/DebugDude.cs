using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDude : CardLogic, IFlipEffect {



	void IFlipEffect.OnFlip(bool flippedFaceUp, System.Action callback)
	{
		Card.Networking.Timer.PauseMainTimer();
		Card.Networking.Timer.StartSubTimer(15, () =>
		{
			Card.Kill();
			Card.Networking.Timer.ResumeMainTimer();
		});

		if (callback != null)
			callback();
	}
}
