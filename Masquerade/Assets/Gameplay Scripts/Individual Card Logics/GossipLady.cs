using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GossipLady : CardLogic, IFlipEffect {

	void IFlipEffect.OnFlip(bool flippedFaceUp, System.Action callback)
	{
		Card.StartCoroutine(triggerCOR((() => callback())));
	}

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


	}
}
