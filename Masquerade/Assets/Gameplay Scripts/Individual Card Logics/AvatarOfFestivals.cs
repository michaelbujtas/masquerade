using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarOfFestivals : CardLogic, IFlipEffect {

	void IFlipEffect.OnFlip(bool flippedFaceUp, System.Action callback)
	{
		if (flippedFaceUp)
		{
			Card.StartCoroutine(triggerCOR(() => callback()));
		}
		else
		{
			callback();
		}
	}

	IEnumerator triggerCOR(System.Action callback)
	{	
			MasqueradePlayer owner = Card.LastOwner;

			if (Card.IsAlive)
				Card.Kill();


			List<byte> allCards = new List<byte>();
			foreach (MasqueradePlayer p in Card.Networking.MasqueradePlayers)
				allCards.AddRange(p.Hand.CardsOwned);

			for (int i = 0; i < allCards.Count; i++)
			{
				//If we had flip-down triggers, this would not stack them properly. We do not.
				bool flipDone = false;

				Card.StartCoroutine(Card.Networking.TheCardIndex.GetCard(allCards[i]).Flip(false, ((a) => flipDone = true)));
				while (!flipDone)
					yield return null;
			}

			List<byte> shuffledCards = new List<byte>();


			while (allCards.Count > 0)
			{
				int randomIndex = Random.Range(0, allCards.Count);
				shuffledCards.Add(allCards[randomIndex]);
				allCards.RemoveAt(randomIndex);
			}

			byte dealIndex = owner.PlayerIndex;

			for (int i = 0; i < shuffledCards.Count; i++)
			{
				Card.Networking.GiveControl(dealIndex, shuffledCards[i]);
				dealIndex++;
				if (dealIndex >= Card.Networking.MasqueradePlayers.Count)
					dealIndex = 0;
			}

		if (callback != null)
			callback();
	}
}
