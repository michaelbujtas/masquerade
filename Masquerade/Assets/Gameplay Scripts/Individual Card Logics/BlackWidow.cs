public class BlackWidow : CardLogic, IOnKilled
{
	void IOnKilled.OnKilled(Card killer, DeathContext context)
	{
		if (context == DeathContext.DEFENDING)
			killer.KillWithContext(Card, DeathContext.OTHER);
	}

}
