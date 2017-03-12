using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MistressEva : CardLogic, IAfterAttacking
{
	void IAfterAttacking.AfterAttacking(Card defender, System.Action callback)
	{
		byte defendingPlayerIndex = defender.LastOwner.PlayerIndex;

		List<byte> targets = new List<byte>();
		foreach (byte b in defender.LastOwner.Hand.CardsOwned)
		{
			Card c = Card.Networking.TheCardIndex.GetCard(b);
			if (c.IsFaceUp)
				targets.Add(b);
		}


		byte faceDownNumber = (byte)(CardIndex.PLAYER_1_FACEDOWN + defendingPlayerIndex);
		if (defender.LastOwner.Hand.HasFacedown)
			targets.Add(faceDownNumber);



		Card.Networking.StartCoroutine(Card.Networking.PickACardCOR(
			Card.LastOwner.PlayerIndex,
			targets.ToArray(),
			delegate (byte choice)
			{
				if (targets.Contains(choice))
				{
					//Steal the target
					if (choice == faceDownNumber)
						Card.Networking.GiveControl(Card.LastOwner.PlayerIndex, defender.LastOwner.Hand.RandomFaceDownCard);
					else
						Card.Networking.GiveControl(Card.LastOwner.PlayerIndex, choice);
					//If Eva's alive give her away
					if(Card.IsAlive)
						Card.Networking.GiveControl(defendingPlayerIndex, (byte)Card.Index);

					if (callback != null)
						callback();
				}
				else
				{
					//If somebody failed the sanity check they're probably cheating
					//The most reasonable thing to do is choose at random.

					byte randomChoice = targets[Random.Range(0, targets.Count)];


					//Steal the target
					if (randomChoice == faceDownNumber)
						Card.Networking.GiveControl(Card.LastOwner.PlayerIndex, defender.LastOwner.Hand.RandomFaceDownCard);
					else
						Card.Networking.GiveControl(Card.LastOwner.PlayerIndex, randomChoice);
					if (Card.IsAlive)
						Card.Networking.GiveControl(defendingPlayerIndex, (byte)Card.Index);


					if (callback != null)
						callback();
				}

			},
			null,
			new Color(.5f, .0f, .125f)
			));
	}
}