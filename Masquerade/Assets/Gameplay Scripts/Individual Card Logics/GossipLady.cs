using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GossipLady : CardLogic, IFlipEffect {

	void IFlipEffect.OnFlip(bool flippedFaceUp, System.Action callback)
	{
		if (flippedFaceUp)
			Card.StartCoroutine(triggerCOR((() => callback())));
		else
			callback();
	}


	IEnumerator triggerCOR(System.Action callback)
	{
		//Flip every card
		Dictionary<byte, Trigger> triggers = new Dictionary<byte, Trigger>();

		foreach (MasqueradePlayer m in Card.Networking.MasqueradePlayers)
		{
			foreach (byte b in m.Hand.CardsOwned)
			{
				Card c = Card.Networking.TheCardIndex.GetCard(b);

				if (!c.IsFaceUp)
				{
					bool flipped = c.FlipNoTriggers(true);
					if (flipped)
					{
						if (c.Logic is IFlipEffect)
						{
							if (c.Logic.TriggerIsPlausible(Networking.CurrentPlayer))
							{
								Trigger newTrigger = new Trigger(c.Logic);
								newTrigger.Resolution = () =>
								{
									((IFlipEffect)newTrigger.Source).OnFlip(true, () => newTrigger.PostResolution());
								};
								triggers.Add((byte)c.Index, newTrigger);
							}
						}
					}

				}
			}
		}
		Response<bool> response = new Response<bool>(0);
		response.Set();

		Card.StartCoroutine(Networking.HandleTriggerStack2COR(response, triggers));

		while (response.FlagWaiting)
			yield return null;

		callback();

	}



	/*
	IEnumerator triggerCOR(System.Action callback)
	{
		
		//Flip every card
		Dictionary<MasqueradePlayer, List<CardLogic>> triggersByPlayer = new Dictionary<MasqueradePlayer, List<CardLogic>>();

		foreach(MasqueradePlayer m in Card.Networking.MasqueradePlayers)
		{
			List<CardLogic> triggers = new List<CardLogic>();
			triggersByPlayer.Add(m, triggers);
			foreach (byte b in m.Hand.CardsOwned)
			{
				Card c = Card.Networking.TheCardIndex.GetCard(b);

				if (!c.IsFaceUp)
				{
					bool flipped = c.FlipNoTriggers(true);
					if (c.Logic is IFlipEffect)
						triggers.Add(c.Logic);
				}
			}
		}


		//Resolve triggers in APNAP order

		byte currentlyResolving = Card.Networking.CurrentPlayer.PlayerIndex;

		for (int i = 0; i < Card.Networking.MasqueradePlayers.Count; i++)
		{
			List<CardLogic> triggers = triggersByPlayer[Card.Networking.MasqueradePlayers[currentlyResolving]];


			if(currentlyResolving != Card.Networking.CurrentPlayer.PlayerIndex)
			{
				//Not our turn and we need to start a subtimer
				if(!Card.Networking.Timer.MainTimerPaused)
					Card.Networking.Timer.PauseMainTimer();

				Card.Networking.Timer.StartSubTimer(15, (() =>
				{
					//Nothing to do here
				}
				));
			}

			//Have that player stack their triggers
			Response<bool> corReturn = new Response<bool>(0);
			corReturn.Set();
			Card.Networking.StartCoroutine(Card.Networking.HandleTriggerStackCOR(
				currentlyResolving,
				corReturn,
				triggers,
				(logic, response) =>
				{
					((IFlipEffect)logic).OnFlip(true, () =>
					{
						response.Fill(true);
					});
				},
				currentlyResolving == Card.Networking.CurrentPlayer.PlayerIndex
			));

			//Wait
			while (corReturn.FlagWaiting)
				yield return null;


			//Get rid of the subtimer
			if (currentlyResolving != Card.Networking.CurrentPlayer.PlayerIndex)
			{
				Card.Networking.Timer.CancelSubTimer();
			}


			currentlyResolving++;
			if (currentlyResolving >= Card.Networking.MasqueradePlayers.Count)
				currentlyResolving = 0;

			yield return null;
		}


		//Resume main timer
		Card.Networking.Timer.ResumeMainTimer();


		callback();


	}*/
}
