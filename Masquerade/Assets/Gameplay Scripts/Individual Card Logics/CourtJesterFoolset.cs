using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourtJesterFoolset : CardLogic, IAfterAttacking
{

	void IAfterAttacking.AfterAttacking(Card defender, System.Action callback)
	{

		Card.StartCoroutine(triggerCOR(defender, (() => callback())));
	}

	IEnumerator triggerCOR(Card defender, System.Action callback)
	{


		//For each player in apnap order
		MasqueradePlayer currentPlayer = Networking.CurrentPlayer;
		for (int i = 0; i < Networking.MasqueradePlayers.Count; i++)
		{
			bool currentPlayerIsActive = currentPlayer == Networking.CurrentPlayer;

			if(!currentPlayerIsActive)
			{
				//Start Subtimer
				Timer.StartSubTimer(15, () => { });
			}
			List<byte> targets = new List<byte>();
			targets.AddRange(currentPlayer.Hand.CardsThatCanBeDiscarded);

			bool choiceMade = false;
			Card.StartCoroutine(Networking.PickACardCOR(currentPlayer.PlayerIndex, targets.ToArray(),
				(choice) =>
				{
					if (targets.Contains(choice))
					{
						//Discard it
						Networking.TheCardIndex.GetCard(choice).Kill();
						choiceMade = true;

					}
					else
					{
						//If we failed the sanity check choose at random;
						int randomChoice = Random.Range(0, targets.Count);

						Networking.TheCardIndex.GetCard((byte)randomChoice).Kill();
						choiceMade = true;
					}
				},
				null,
				new Color(0, .5f, .6f),
				currentPlayerIsActive
				));

			while (!choiceMade)
				yield return null;

			//Get rid of whatever timers we have
			if (currentPlayerIsActive)
			{
				Timer.PauseMainTimer();
			}
			else
			{
				Timer.CancelSubTimer();
			}

			currentPlayer = Networking.NextPlayer(currentPlayer);

		}



		Timer.ResumeMainTimer();

		callback();


	}
}