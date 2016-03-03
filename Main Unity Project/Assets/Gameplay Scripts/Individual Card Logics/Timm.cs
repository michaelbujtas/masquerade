

public class Timm : CardLogic, IAfterAttacking
{
	void IAfterAttacking.AfterAttacking(Card defender)
	{
		Card.FlipAction(false);
		CustomConsole.Log("Timm should have just flipped down. His faceup status is " + Card.IsFaceUp, UnityEngine.Color.green);
	}

}
