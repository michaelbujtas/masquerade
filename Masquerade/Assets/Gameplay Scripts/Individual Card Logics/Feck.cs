using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feck : CardLogic, IOnKilled {

	
	void IOnKilled.OnKilled(Card killer, DeathContext context, System.Action callback)
	{
		Card.StartCoroutine(triggerCOR((() => callback())));
	}

	IEnumerator triggerCOR(System.Action callback)
	{
		if (Card.LastOwner == Card.Networking.CurrentPlayer)
		{
			//It's our turn and we don't have to mess with subtimers at all.
		}
		else
		{
			//Not our turn, we have to do subtimer bullshit
			Card.Networking.Timer.PauseMainTimer();
			Card.Networking.Timer.StartSubTimer(15, (() =>
			{
				//What do we even do when Feck times out
				//I think... nothing, here? 
			}
			));
		}

		List<CardLogic> triggers = new List<CardLogic>();

		foreach (byte b in Card.LastOwner.Hand.CardsOwned)
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
			Card.LastOwner.PlayerIndex, 
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


		if (Card.LastOwner == Card.Networking.CurrentPlayer)
		{
			//It's our turn and we don't have to mess with subtimers at all.
		}
		else
		{
			//Not our turn, we have to restart the main timer

			Card.Networking.Timer.CancelSubTimer();
			Card.Networking.Timer.ResumeMainTimer();
		}

		callback();

		
	}
}
