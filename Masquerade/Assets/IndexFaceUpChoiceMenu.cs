using UnityEngine;
using System.Collections.Generic;

public class IndexFaceUpChoiceMenu : MonoBehaviour {

	public GameObject Visuals;
	public CardRenderer FaceUp, FaceDown;

	List<Choice> decisionQueue = new List<Choice>();

	public delegate void HandleChoiceDelegate(bool choice);
	public Choice GetChoice(byte index, HandleChoiceDelegate handle)
	{
		Choice retVal = new Choice(index, handle);
        decisionQueue.Add(retVal);
		ShowChoice();
		return retVal;
	}

	public void OnFaceUpButtonUI()
	{
		Choose(true);
	}
	public void OnFaceDownButtonUI()
	{
		Choose(false);
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

			FaceUp.Index = nextChoice.Index;
			FaceUp.RefreshCardImage(true);
			FaceDown.Index = nextChoice.Index;
			FaceDown.RefreshCardImage(false);
		}
	}

	void Choose(bool choice)
	{
		Choice currentChoice = decisionQueue[0];
		decisionQueue.RemoveAt(0);
		currentChoice.Handle(choice);
		ShowChoice();
	}


	public void PurgeChoice(Choice choice)
	{
		decisionQueue.Remove(choice);
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
