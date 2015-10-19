using DevConsole;

public class Timm : CardLogic, IAfterAttacking
{
	void IAfterAttacking.AfterAttacking(Card defender)
	{
		if (Card.Renderer != null) //This is the worst way I can possibly think of to identify if Timm died
		{
			Card.Engine.FlipDown(Card);
			Console.Log("Timm should have just flipped down. His faceup status is " + Card.IsFaceUp, UnityEngine.Color.green);
		}
	}

}
