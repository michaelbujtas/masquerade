using UnityEngine;
using System.Collections.Generic;

public class IndexFaceUpChoiceMenu : MonoBehaviour {

	public GameObject Visuals;
	public CardRenderer FaceUp, FaceDown;

	Queue<Choice> decisionQueue = new Queue<Choice>();

	public delegate void HandleChoiceDelegate(bool choice);
	public void GetChoice(byte index, HandleChoiceDelegate handle)
	{
		decisionQueue.Enqueue(new Choice(index, handle));
		ShowChoice();
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
			Choice nextChoice = decisionQueue.Peek();

			FaceUp.Index = nextChoice.Index;
			FaceUp.RefreshCardImage();
			FaceUp.SetFacing(true);
			FaceDown.Index = nextChoice.Index;
			FaceDown.RefreshCardImage();
			FaceDown.SetFacing(false);
		}
	}

	void Choose(bool choice)
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
