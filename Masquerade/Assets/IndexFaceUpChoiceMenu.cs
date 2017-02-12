using UnityEngine;
using System.Collections.Generic;

public class IndexFaceUpChoiceMenu : MonoBehaviour {

	public GameTimer gameTimer;
	public GameObject SingleCardVisuals;
	public GameObject MultiCardVisuals;
	public CardRenderer FaceUp, FaceDown;
	public List<CardRenderer> MultipleCardCards;

	List<Choice> decisionQueue = new List<Choice>();


	Choice currentChoice
	{
		get
		{
			return decisionQueue.Count > 0 ? decisionQueue[0] : null;
		}
	}

	public delegate void HandleChoiceDelegate(bool[] choices);
	void Awake()
	{
		gameTimer = FindObjectOfType<GameTimer>();
	}


	public Choice GetChoice(byte[] indices, HandleChoiceDelegate handle)
	{
		Choice retVal = new Choice(indices, handle);
		gameTimer.AddMainTimerDelegate(
		   delegate
		   {
			   PurgeChoice(retVal);
		   });
		decisionQueue.Add(retVal);
		ShowChoice();
		return retVal;
	}


	public Choice GetChoice(byte index, HandleChoiceDelegate handle)
	{
		Choice retVal = new Choice(new byte[] { index }, handle);
		gameTimer.AddMainTimerDelegate(
		   delegate
		   {
			   PurgeChoice(retVal);
		   });
		decisionQueue.Add(retVal);
		ShowChoice();
		return retVal;
	}


	public void OnFaceUpButtonUI()
	{
		currentChoice.Facings[0] = true;
		Choose();
	}

	public void OnFaceDownButtonUI()
	{
		currentChoice.Facings[0] = false;
		Choose();
	}

	public void OnCardButtonUI(CardRenderer renderer)
	{
		for (int i = 0; i < currentChoice.Indices.Length; i++)
		{
			if(currentChoice.Indices[i] == renderer.Index)
			{
				currentChoice.Facings[i] = !currentChoice.Facings[i];
				renderer.RefreshCardImage(currentChoice.Facings[i]);
			}
		}
	}

	public void OnDoneButtonUI()
	{
		Choose();
	}

	void ShowChoice()
	{
		if (decisionQueue.Count == 0)
		{
			SingleCardVisuals.SetActive(false);
			MultiCardVisuals.SetActive(false);
		}
		else
		{

			Choice nextChoice = decisionQueue[0];

			if (nextChoice.Indices.Length == 1)
			{
				SingleCardVisuals.SetActive(true);
				MultiCardVisuals.SetActive(false);

				FaceUp.Index = nextChoice.Indices[0];
				FaceUp.RefreshCardImage(true);
				FaceDown.Index = nextChoice.Indices[0];
				FaceDown.RefreshCardImage(false);
			}
			else
			{
				SingleCardVisuals.SetActive(false);
				MultiCardVisuals.SetActive(true);

				for (int i = 0; i < MultipleCardCards.Count; i++)
				{
					if (i < nextChoice.Indices.Length)
					{
						MultipleCardCards[i].Index = nextChoice.Indices[i];
						MultipleCardCards[i].RefreshCardImage(nextChoice.Facings[i]);
					}
					else
					{
						MultipleCardCards[i].Index = CardIndex.EMPTY_SLOT;
						MultipleCardCards[i].RefreshCardImage();
					}

				}
			}
		}
	}

	void Choose()
	{
		Choice lastChoice = decisionQueue[0];
		decisionQueue.RemoveAt(0);
		lastChoice.Handle(lastChoice.Facings);
		ShowChoice();
	}


	public void PurgeChoice(Choice choice)
	{
		decisionQueue.Remove(choice);
		ShowChoice();
	}


	public class Choice
	{
		public Choice(byte[] indices, HandleChoiceDelegate handle)
		{
			Indices = indices;
			Facings = new bool[Indices.Length];
			for (int i = 0; i < Facings.Length; i++)
				Facings[i] = false;
			Handle = handle;
		}

		public byte[] Indices;
		public bool[] Facings;
		public HandleChoiceDelegate Handle;

	}
}
