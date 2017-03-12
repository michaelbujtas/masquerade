using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardsmanTerricIllbert : CardLogic, IAfterAttacking {

	void IAfterAttacking.AfterAttacking(Card defender, System.Action callback)
	{

		Card.StartCoroutine(triggerCOR(defender, (() => callback())));
	}

	IEnumerator triggerCOR(Card defender, System.Action callback)
	{
		
		//Not our turn, we have to do subtimer bullshit
		Card.Networking.Timer.PauseMainTimer();
		Card.Networking.Timer.StartSubTimer(15, (() =>
		{
			//Actual timeout is handled in the trigger but this probably isn't allowed to be null or something?
		}
		));

		List<CardLogic> triggers = new List<CardLogic>();

		foreach (byte b in defender.LastOwner.Hand.CardsOwned)
		{
			Card c = Card.Networking.TheCardIndex.GetCard(b);

			if (!c.IsFaceUp)
			{
				bool flipped = c.FlipNoTriggers(true);
				if (c.Logic is IFlipEffect)
					triggers.Add(c.Logic);
			}
		}

		Response<bool> corReturn = new Response<bool>(0);
		corReturn.Set();
		Card.Networking.StartCoroutine(Card.Networking.HandleTriggerStackCOR(
			defender.LastOwner.PlayerIndex,
			corReturn,
			triggers,
			(logic, response) =>
			{
				((IFlipEffect)logic).OnFlip(true, () =>
				{
					response.Fill(true);
				});
			},
			false
		));

		while (corReturn.FlagWaiting)
			yield return null;


		//Not our turn, we have to restart the main timer
		Card.Networking.Timer.ResumeMainTimer();
		Card.Networking.Timer.CancelSubTimer();

		callback();


	}
}

