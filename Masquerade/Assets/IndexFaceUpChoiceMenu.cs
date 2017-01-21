using UnityEngine;
using System.Collections.Generic;

public class IndexFaceUpChoiceMenu : MonoBehaviour {

	public GameObject Visuals;
	public CardRenderer FaceUp, FaceDown;

	List<Choice> decisionQueue = new List<Choice>();

	public delegate void HandleChoiceDelegate(bool choice);
	public Choice GetChoice(byte index, long expirationTicks, HandleChoiceDelegate handle)
	{
		Choice retVal = new Choice(index, expirationTicks, handle);
        decisionQueue.Add(retVal);
		ShowChoice();
		return retVal;
	}

	public void Update()
	{
		long ticksNow = System.DateTime.UtcNow.Ticks;
		for(int i = 0; i < decisionQueue.Count; i++)
		{
			if (decisionQueue[i].ExpirationTicks < ticksNow)
			{
				PurgeChoice(decisionQueue[i]);
				i--;
			}
		}
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

		public Choice(byte index, long expirationTicks, HandleChoiceDelegate handle)
		{
			Index = index;
			ExpirationTicks = expirationTicks;
			Handle = handle;
		}
		public byte Index;
		public long ExpirationTicks;
		public HandleChoiceDelegate Handle;

	}
}
