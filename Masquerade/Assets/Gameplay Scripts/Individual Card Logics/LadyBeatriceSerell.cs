using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadyBeatriceSerell : CardLogic, IStartPhase
{

	Color serellGreen = new Color(0.5f, 1, 0.5f);

	void IStartPhase.OnStartPhase(MasqueradePlayer turn, System.Action callback)
	{
		if(turn == Card.Owner && Card.IsAlive)
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
		//Do I have other cards to sacrifice
		List<byte> sacTargets = new List<byte>();
		sacTargets.AddRange(Card.Owner.Hand.CardsThatCanBeDiscarded);
		sacTargets.Remove((byte)Card.Index);
		if(sacTargets.Count > 0)
		{
			//We'll be doing Beatrice Serell stuff today
			bool foundACard = false;
			Card.StartCoroutine(Networking.PickACardCOR(Card.Owner.PlayerIndex,
				sacTargets.ToArray(),
				(choice) =>
				{

					Card foundCard = Networking.TheCardIndex.GetCard(choice);

					foundCard.StartCoroutine(foundCard.Flip(true, 
						(b) =>
					{
						foundACard = true;
						foundCard.Kill();
					}));
				},
				null,
				serellGreen,
				true));

			while (!foundACard)
				yield return null;

			//Sacrifice finished, now give her away
			List<byte> targets = new List<byte>();
			foreach (MasqueradePlayer m in Networking.MasqueradePlayers)
			{
				targets.Add(m.Identity.Index);
			}
			targets.Remove(Card.Owner.Identity.Index);

			bool foundAPlayer = false;
			Card.StartCoroutine(Networking.PickAPlayerCOR(
				Card.Owner.PlayerIndex,
				targets.ToArray(),
				(choice) =>
				{
					foundAPlayer = true;
					Networking.GiveControl(choice, (byte)Card.Index);
				},
				null,
				serellGreen,
				true));

			while (!foundAPlayer)
				yield return null;

		}
		else
		{
			//We'll be doing her default behavior

			//Currently she dies

			Card.Kill();
		}

		yield return null;
		callback();
	}


}
