using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravekeeperAnathema : CardLogic, IActivatedAbility
{
	void IActivatedAbility.ActivateAbility(System.Action callback)
	{

		if(Card.IsFaceUp && Card.IsAlive)
			Card.StartCoroutine(triggerCOR(callback));
		else
		{
			Networking.RefundAction();
			Card.IsTapped = false;
			Card.Sync();
			callback();
		}

	}


	IEnumerator triggerCOR(System.Action callback)
	{
		List<byte> cardsInHand = new List<byte>();
		cardsInHand.AddRange(Card.Owner.Hand.CardsThatCanBeDiscarded);

		bool cancelled = false;
		byte cardIndexToSac = CardIndex.EMPTY_SLOT;
		byte cardIndexToReanimate = CardIndex.EMPTY_SLOT;


		Card.StartCoroutine(Networking.PickACardCOR(Card.Owner.PlayerIndex,
			cardsInHand.ToArray(),
			(choice) =>
			{
				if (cardsInHand.Contains(choice))
					cardIndexToSac = choice;
				else
					cancelled = true;
			},
			() =>
			{
				cancelled = true;
				Networking.RefundAction();
				Card.IsTapped = false;
				Card.Sync();
			},
			Color.green,
			true));



		while (!cancelled && cardIndexToSac == CardIndex.EMPTY_SLOT)
			yield return null;

		CustomConsole.Log("Anathema is past the sac. Cancelled = " + cancelled);


		if(!cancelled)
		{
			Card cardToSac = Networking.TheCardIndex.GetCard(cardIndexToSac);

			bool flipDone = false;

			cardToSac.StartCoroutine(cardToSac.Flip(true, (b) => flipDone = true));

			while (!flipDone)
				yield return null;

			CustomConsole.Log("Anathema is past the flip. Cancelled = " + cancelled);

			cardToSac.Kill();


			List<byte> cardsInGraveyard = new List<byte>();
			cardsInGraveyard.AddRange(Networking.TheDiscardPile.Contents);
			cardsInGraveyard.Remove(cardIndexToSac);

			if(cardsInGraveyard.Count < 1)
			{
				cancelled = true;
			}
			else
			{
				Card.StartCoroutine(Networking.PickACardCOR(Card.Owner.PlayerIndex,
				cardsInGraveyard.ToArray(),
				(choice) =>
				{
					if (cardsInGraveyard.Contains(choice))
						cardIndexToReanimate = choice;
					else
						cancelled = true;
				},

				() =>
				{
					cancelled = true;
				},
				Color.green,
				true));

				while (!cancelled && cardIndexToReanimate == CardIndex.EMPTY_SLOT)
					yield return null;


				CustomConsole.Log("Anathema is past the reanimate choice. Cancelled = " + cancelled);
			}

		}


		if (!cancelled)
		{
			Networking.TheDiscardPile.RemoveIndex(cardIndexToReanimate);

			Networking.TheCardIndex.GetCard(cardIndexToReanimate).Owner = Card.Owner;

			Networking.TheCardIndex.GetCard(cardIndexToReanimate).IsAlive = true;

			bool facingPickedDone = false;
			Card.StartCoroutine(Networking.ChooseFacingCOR(cardIndexToReanimate, Card.Owner.PlayerIndex, false, (facingChoice) =>
			{
				facingPickedDone = true;
			}));
			while (!facingPickedDone)
				yield return null;
		}


		callback();

	}

}
