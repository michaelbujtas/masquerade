using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OlafTheGlutton : CardLogic, IStartPhase
{
	public override bool TriggerIsPlausible(MasqueradePlayer currentPlayer)
	{
		return currentPlayer == Card.Owner && Card.IsFaceUp && Card.IsAlive;
	}

	void IStartPhase.OnStartPhase(MasqueradePlayer currentPlayer, Response<bool> response)
	{
		if(currentPlayer == Card.Owner && Card.IsFaceUp && Card.IsAlive)
		{
			Card.Networking.StartCoroutine(Card.Networking.DrawCardCOR(currentPlayer.PlayerIndex, 1, delegate
			{
				response.Fill(true);
			}));
			CustomConsole.Log("It's his owner's start phase, and Sir Olaf the Glutton is drawing a card.", UnityEngine.Color.green);
		}
		response.Fill(false);
	}



}
