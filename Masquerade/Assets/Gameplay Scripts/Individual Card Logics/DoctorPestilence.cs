using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoctorPestilence : CardLogic, IStartPhase, IHasKeywords
{

	List<Keyword> keywords = new List<Keyword>() { Keyword.SKIP_DRAW_PHASE };
	List<Keyword> IHasKeywords.GetKeywords()
	{
		return keywords;
	}

	void IStartPhase.OnStartPhase(MasqueradePlayer turn, System.Action callback)
	{
		if(Card.IsAlive && Card.IsFaceUp && turn == Card.Owner)
		{

			Card.StartCoroutine(triggerCOR(callback));

		}
		else
		{

			callback();
		}
	}

	IEnumerator triggerCOR(System.Action callback)
	{
		//Look at the to 2 cards
		List<byte> targets = Networking.TheDeck.TopCards(2);

		bool choiceMade = false;
		Card toGiveAway = null;
		Card toKeep = null;

		System.Action cancel = () =>
		{
			choiceMade = true;
			//Otherwise, discard the top 2 cards
			foreach (byte b in targets)
			{
				Networking.TheDeck.PullCard(b);
				Networking.SendToDiscard(Networking.TheCardIndex.GetCard(b));
			}

		};

		foreach(byte b in targets)
		{
			Card c = Networking.TheCardIndex.GetCard(b);
			if (c.HasKeyword(Keyword.CANT_BE_DISCARDED))
				cancel = null;
		}



		Card.StartCoroutine(Networking.PickACardCOR(
			Card.Owner.PlayerIndex,
			targets.ToArray(),
			(choice) =>
			{
				//You may choose 1 and give it to another player face-up

				Networking.TheDeck.PullCard(choice);
				toGiveAway = Networking.TheCardIndex.GetCard(choice);
				targets.Remove(choice);

				byte other = targets[0];

				Networking.TheDeck.PullCard(other);
				toKeep = Networking.TheCardIndex.GetCard(other);

				choiceMade = true;
			},
			cancel,
			Color.black,
			true));

		while (!choiceMade)
			yield return null;


		if(toGiveAway != null)
		{

			List<byte> playerTargets = new List<byte>();
			foreach (MasqueradePlayer m in Networking.MasqueradePlayers)
			{
				playerTargets.Add(m.Identity.Index);
			}
			playerTargets.Remove(Card.Owner.Identity.Index);
			bool playerChoiceMade = false;
			byte chosenPlayer = 205;

			Card.StartCoroutine(Networking.PickAPlayerCOR(
				Card.Owner.PlayerIndex,
				 playerTargets.ToArray(),
				 (choice) =>
				 {
					 chosenPlayer = choice;
					 playerChoiceMade = true;
				 },
				 null,
				 Color.black,
				 true));

			while (!playerChoiceMade)
				yield return null;


			toGiveAway.IsAlive = true;
			toGiveAway.IsFaceUp = true;
			toGiveAway.Owner = Networking.MasqueradePlayers[chosenPlayer];
			Networking.AddCardToBoards(chosenPlayer, (byte)toGiveAway.Index);

			toKeep.IsAlive = true;
			toKeep.Owner = Card.Owner;
			bool facingPicked = false;
			Card.StartCoroutine(Networking.ChooseFacingCOR(
				(byte)toKeep.Index,
				Card.Owner.PlayerIndex,
				false,
				(choice) =>
				{
					facingPicked = true;
				}));

			while (!facingPicked)
				yield return null;

		}





		callback();

	}
}
