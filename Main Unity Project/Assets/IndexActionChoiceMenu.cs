﻿using UnityEngine;
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


	List<Choice> decisionQueue = new List<Choice>();

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


	public Choice GetChoice(byte index, HandleChoiceDelegate handle)
	{
		Choice retVal = new Choice(index, handle);
        decisionQueue.Add(retVal);
		ShowChoice();
		return retVal;
	}

	public void PurgeChoice(Choice choice)
	{
		decisionQueue.Remove(choice);
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
			Choice nextChoice = decisionQueue[0];

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
		Choice currentChoice = decisionQueue[0];
		decisionQueue.RemoveAt(0);
		currentChoice.Handle(choice);
		ShowChoice();
	}

	public class Choice
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
