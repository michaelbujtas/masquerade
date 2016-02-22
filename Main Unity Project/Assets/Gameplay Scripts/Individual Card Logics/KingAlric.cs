using DevConsole;

public class KingAlric : CardLogic, IOnKilled
{
	void IOnKilled.OnKilled(Card killer, DeathContext context)
	{

		Console.NetworkedLog("The owner of " + killer.CardName + ", (Player #" + killer.Owner.PlayerIndex + ") wins!", UnityEngine.Color.yellow);
	}

}
