using System.Collections;

public class BlackWidow : CardLogic, IOnKilled
{
	void IOnKilled.OnKilled(Card killer, DeathContext context, System.Action callback)
	{
		if (context == DeathContext.DEFENDING)
			Card.StartCoroutine(doTriggerCOR(killer, context, (() => callback())));

	}

	IEnumerator doTriggerCOR(Card killer, DeathContext context, System.Action callback)
	{
		bool finished = false;

		killer.StartCoroutine(killer.KillWithContext(Card, DeathContext.OTHER, ((actuallyDied) => finished = true)));
		while (!finished)
			yield return null;

		callback();
	}

}
