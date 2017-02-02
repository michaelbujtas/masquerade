using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OlafTheGlutton : CardLogic, IStartPhase
{
	void IStartPhase.OnStartPhase(MasqueradePlayer currentPlayer, Response<bool> response)
	{
		if(currentPlayer == Card.Owner)
		{
			Card.Networking.StartCoroutine(Card.Networking.DrawCardCOR(currentPlayer.PlayerIndex, delegate
			{
				response.Fill(true);
			}));
			CustomConsole.Log("It's his owner's start phase, and Sir Olaf the Glutton is drawing a card.", UnityEngine.Color.green);
		}
	}



}
