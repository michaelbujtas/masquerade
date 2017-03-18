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
		
		Dictionary<byte, Trigger> triggers = new Dictionary<byte, Trigger>();


		foreach (byte b in Card.LastOwner.Hand.CardsOwned)
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

		Response<bool> response = new Response<bool>(0);
		response.Set();

		Card.StartCoroutine(Networking.HandleTriggerStack2COR(response, triggers));

		while (response.FlagWaiting)
			yield return null;

		callback();


	}
}
