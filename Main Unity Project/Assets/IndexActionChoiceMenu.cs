using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class IndexActionChoiceMenu : MonoBehaviour
{
	public CardIndex Index;

	public GameObject Visuals;

	public CardRenderer cardRenderer;
	public Button attack;
	public Button flipUp;
	public Button flipDown;
	public Button ability;
	public Button back;


	Queue<Choice> decisionQueue = new Queue<Choice>();

	public delegate void HandleChoiceDelegate(CardAction choice);


	public void AttackButtonUI()
	{
		Choose(CardAction.ATTACK);
	}
	public void FlipButtonUI()
	{
		Choose(CardAction.FLIP);
	}
	public void AbilityButtonUI()
	{
		Choose(CardAction.ACTIVATE);
	}

	public void BackButtonUI()
	{
		//Cancel isn't supported yet
		//Choose(CardAction.CANCEL);
	}


	public void GetChoice(byte index, HandleChoiceDelegate handle)
	{
		decisionQueue.Enqueue(new Choice(index, handle));
		ShowChoice();
	}

	void ShowChoice()
	{
		if (decisionQueue.Count == 0)
		{
			Visuals.SetActive(false);
		}
		else
		{

			Visuals.SetActive(true);
			Choice nextChoice = decisionQueue.Peek();

			cardRenderer.Index = nextChoice.Index;
			cardRenderer.RefreshCardImage();

			Card newCard = Index.GetCard(nextChoice.Index);

			attack.gameObject.SetActive(newCard.CanAttack);
			flipUp.gameObject.SetActive(newCard.CanFlipUp);
			flipDown.gameObject.SetActive(newCard.CanFlipDown);
			ability.gameObject.SetActive(newCard.CanActivateAbility);
			back.gameObject.SetActive(false);

		}
	}

	void Choose(CardAction choice)
	{
		Choice currentChoice = decisionQueue.Dequeue();
		currentChoice.Handle(choice);
		ShowChoice();
	}

	private class Choice
	{

		public Choice(byte index, HandleChoiceDelegate handle)
		{
			Index = index;
			Handle = handle;
		}
		public byte Index;
		public HandleChoiceDelegate Handle;
	}
}

