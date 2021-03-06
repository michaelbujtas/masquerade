﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SorcerorWhitemane : CardLogic, IStartPhase {

	public override bool TriggerIsPlausible(MasqueradePlayer currentPlayer)
	{
		return currentPlayer == Card.Owner && Card.IsFaceUp && Card.IsAlive;
	}

	Buff buff = null;

	void IStartPhase.OnStartPhase(MasqueradePlayer currentPlayer, System.Action callback)
	{
		if (currentPlayer == Card.Owner && Card.IsFaceUp && Card.IsAlive)
		{
			if(!Card.HasBuff(buff))
				buff = Card.AddBuff(0, 0, true, true, this);


			List<byte> targets = new List<byte>();

			foreach (byte b in Card.Owner.Hand.CardsOwned)
			{
				Card c = Card.Networking.TheCardIndex.GetCard(b);
				if(!c.HasKeyword(Keyword.CANT_BE_DISCARDED))
				{
					targets.Add(b);
				}
			}
				Card.Networking.StartCoroutine(Card.Networking.PickACardCOR(
			Card.Owner.PlayerIndex,
			targets.ToArray(),
			delegate(byte choice)
			{
				if (targets.Contains(choice))
				{
					//Kill the card and give Whitemane +5
					buff.Attack = 5;
					buff.Defense = 5;

					Card.Networking.SyncCard(Card);


					Card foundCard = Networking.TheCardIndex.GetCard(choice);

					foundCard.StartCoroutine(foundCard.Flip(true,
						(b) =>
						{
							foundCard.Kill();
							callback();
						}));
				}
				else
				{
					//If somebody failed the sanity check they're probably cheating
					//The most reasonable thing to do is still just cancel


					buff.Attack = 0;
					buff.Defense = 0;
					Card.Networking.SyncCard(Card);
					callback();
				}

			},
			delegate
			{
				//Don't kill anybody

				buff.Attack = 0;
				buff.Defense = 0;

				Card.Networking.SyncCard(Card);

				callback();
			},
			new Color(0, .5f, 0)
			));
			
			CustomConsole.Log("It's his owner's start phase, and Sorceror Theodoric Whitemane is choosing someone to sacrifice.", UnityEngine.Color.green);
		}
	}



}
