public class BlackWidow : CardLogic, IOnKilled
{
	void IOnKilled.OnKilled(Card killer)
	{
		Card.Engine.Kill(Card, killer);
	}

}
