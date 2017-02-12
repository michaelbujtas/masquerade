using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SorcerorWhitemane : CardLogic, IStartPhase {

	public override bool TriggerIsPlausible(MasqueradePlayer currentPlayer)
	{
		return currentPlayer == Card.Owner && Card.IsFaceUp && Card.IsAlive;
	}

	void IStartPhase.OnStartPhase(MasqueradePlayer currentPlayer, Response<bool> response)
	{
		if (currentPlayer == Card.Owner && Card.IsFaceUp && Card.IsAlive)
		{
			Card.Networking.StartCoroutine(Card.Networking.PickACardCOR(
				Card.Owner.PlayerIndex,
				Card.Owner.Hand.CardsOwned.ToArray(),
				delegate(byte choice)
				{
					//Kill the card and give Whitemane +5
					Card.AddBuff(5, 0, false, true);
					Card.Networking.TheCardIndex.GetCard(choice).Kill();
					response.Fill(true);

				},
				delegate
				{
					//Don't kill anybody
					response.Fill(true);
				},
				new Color(0, .5f, 0)
				));
			
			CustomConsole.Log("It's his owner's start phase, and Sorceror Theodoric Whitemane is choosing someone to sacrifice.", UnityEngine.Color.green);
		}
	}


}
