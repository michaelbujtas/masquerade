

public class Timm : CardLogic, IAfterAttacking
{
	void IAfterAttacking.AfterAttacking(Card defender, System.Action callback)
	{
		if(Card.IsAlive)
		{
			Card.StartCoroutine(Card.Flip(false,
				((a) =>
				{
					CustomConsole.Log("Timm should have just flipped down. His faceup status is " + Card.IsFaceUp, UnityEngine.Color.green);

					callback();
				})));


		}
		else
		{
			callback();
		}
	}

}
