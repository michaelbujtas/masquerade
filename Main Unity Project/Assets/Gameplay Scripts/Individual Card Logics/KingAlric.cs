
public class KingAlric : CardLogic, IOnKilled
{
	void IOnKilled.OnKilled(Card killer, DeathContext context)
	{

		CustomConsole.LogNetworked("The owner of " + killer.CardName + ", (Player #" + killer.Owner.PlayerIndex + ") wins!", UnityEngine.Color.yellow);
	}

}
