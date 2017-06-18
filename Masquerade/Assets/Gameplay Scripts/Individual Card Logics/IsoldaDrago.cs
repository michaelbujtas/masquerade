using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsoldaDrago : CardLogic, IFlipEffect {
	public void OnFlip(bool flippedFaceUp, Action callback)
	{
		if(flippedFaceUp)
		{
			Card.StartCoroutine(triggerCOR(() => callback()));
		}
		else
		{
			callback();
		}
	}

	public IEnumerator triggerCOR(System.Action callback)
	{
		//If it's not my turn, pause the main timer and start a subtimer	
		if (Networking.CurrentPlayer != Card.LastOwner)
		{
			Timer.PauseMainTimer();
			Timer.StartSubTimer(15, (() =>{}));
		}
			

		//Find make a list of all face-up soldiers and nobles.

		List<Card> cardsInPlay = Networking.GetCardsInPlay();

		List<byte> targets = new List<byte>();

		foreach (Card c in cardsInPlay)
		{
			if(c.IsFaceUp)
			{
				if (c.CardClass == CardClass.SOLDIER || c.CardClass == CardClass.NOBLE)
				{
					targets.Add((byte)c.Index);
				}
			}
		}

		if(targets.Count > 0)
		{
			bool killDone = false;

			//Ask my last owner what nobel or soldier they'd like to see dead
			Card.StartCoroutine(Networking.PickACardCOR(
				Card.LastOwner.PlayerIndex,
				targets.ToArray(),
				(choiceIndex) => {
					Card victim = GetCard(choiceIndex);
					//Kill it (KillWithContext should handle all legality checks)
					if (targets.Contains((byte)victim.Index))
					{
						Card.StartCoroutine(victim.Flip(true, (b) =>
						{
							Networking.TheAnimationQueue.QueueThrowAnimationNetworked("KnifeThrow", (byte)Card.Index, (byte)victim.Index);
							//killDone = true;
							Card.StartCoroutine(victim.KillWithContext(Card, DeathContext.OTHER, (a) => { killDone = true; }));
						}));
					}
					else
					{
						//If you failed the sanity check you're probably cheating -- we'll choose at random
						int randomIndex = UnityEngine.Random.Range(0, targets.Count);
						Card randomVictim = GetCard(targets[randomIndex]);
						Card.StartCoroutine(randomVictim.Flip(true, (b) =>
						{
							Networking.TheAnimationQueue.QueueThrowAnimationNetworked("KnifeThrow", (byte)Card.Index, (byte)randomVictim.Index);
							Card.StartCoroutine(randomVictim.KillWithContext(Card, DeathContext.OTHER, (a) => { killDone = true; }));
						}));
					}
				},
				null,
				new Color(.15f, 0, .7f),
				Networking.CurrentPlayer == Card.LastOwner));


			while (!killDone)
				yield return null;

		}

		//If it's not my turn, kill the subtimer and resume the main timer
		if (Networking.CurrentPlayer != Card.LastOwner)
		{
			Timer.CancelSubTimer();
			Timer.ResumeMainTimer();
		}


		callback();
	}
}
