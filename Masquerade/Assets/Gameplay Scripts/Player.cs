using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	public Hand Hand;
	public MasqueradeEngine Engine;

	int actionsLeft = 3;



	public void TakeTurn()
	{
		DrawPhase();
	}

	void DrawPhase()
	{
		/*if(Hand.Cards.Count == 0)
		{
			Console.Log("Player drew an extra card for having an empty board.", Color.green);
			DrawCard();
		}*/

		DrawCard();
	}

	void ActionPhase()
	{

		CustomConsole.Log(actionsLeft + " actions left.");
		if (actionsLeft > 0)
			Engine.Selector.Handle(new ActionRequest(this));
		else
			EndPhase();
	}

	void EndPhase()
	{
		actionsLeft = 3;
	}


	public void PlayCard(Card card, bool isFaceUp)
	{
		Engine.PlayCard(card, isFaceUp);
		ActionPhase();
	}

	void DrawCard()
	{
		Card newCard = Engine.DrawCard();
		if (newCard != null)
		{
			Hand.AddCard(newCard.Renderer);
			Engine.FaceUpChoiceMenu.Handle(new SingleFlipRequest(this, newCard));
		}
		else
		{
			CustomConsole.Log("Skipping some logic because the deck's empty.");
			ActionPhase();
		}
	}

	public void TakeAction(Card card, CardAction action)
	{
		actionsLeft--;
		switch (action)
		{
			case CardAction.ATTACK:
				CustomConsole.Log("Right now attacking ends the turn. I need to build a startling amount of infrastructure to fix this.");
				Engine.Attack(card);
				break;
			case CardAction.FLIP:
				if (card.IsFaceUp)
					Engine.FlipDown(card);
				else
					Engine.FlipUp(card);
				break;
			case CardAction.ACTIVATE:
				CustomConsole.Log("No activated abilities yet");
				break;
			/*case CardAction.CANCEL:
				actionsLeft++;
				break;*/
		}

		if (action == CardAction.ATTACK)
			EndPhase();
		else
			ActionPhase();

	}
}
