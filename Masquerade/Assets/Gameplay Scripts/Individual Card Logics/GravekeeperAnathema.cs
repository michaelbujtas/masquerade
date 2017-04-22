using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravekeeperAnathema : CardLogic, IActivatedAbility
{
	void IActivatedAbility.ActivateAbility(System.Action callback)
	{
		List<byte> cardsInGraveyard = new List<byte>();
		cardsInGraveyard.AddRange(Networking.TheDiscardPile.Contents);
		Card.StartCoroutine(Networking.PickACardCOR(Card.Owner.PlayerIndex,
			cardsInGraveyard.ToArray(),
			(choice) =>
			{
				Networking.TheDiscardPile.RemoveIndex(choice);

				Networking.TheCardIndex.GetCard(choice).Owner = Card.Owner;

				Networking.TheCardIndex.GetCard(choice).IsAlive = true;
				Card.StartCoroutine(Networking.ChooseFacingCOR(choice, Card.Owner.PlayerIndex, false, (facingChoice) =>
				{
					callback();
				}));
			},
			() =>
			{
				Networking.RefundAction();
				Card.IsTapped = false;
				Card.Sync();
				callback();
			},
			Color.green,
			true));


	}
}
