using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DevConsole;
using System.Linq;
using UnityEngine.UI;


public class IndexCardChoiceMenu : MonoBehaviour {

	//public GameObject Visuals;

	public List<CardRenderer> CardsInPlay = new List<CardRenderer>();

	Queue<Choice> decisionQueue = new Queue<Choice>();

	public delegate void HandleChoiceDelegate(byte choice);

	public void Awake()
	{

		var notDummyCardRenderers =
			from r in FindObjectsOfType<CardRenderer>()
			where r.DummyRenderer == false
			select r;

		CardsInPlay.Clear();
		CardsInPlay.AddRange(notDummyCardRenderers);

		for(int i = 0; i < CardsInPlay.Count; i++)
		{
			CardsInPlay[i].Background.gameObject.name = "SET LISTENER";
			CardRenderer capturedRenderer = CardsInPlay[i];

			CardsInPlay[i].Background.GetComponent<Button>().onClick.AddListener(
				delegate {
					OnAnyCardButtonUI(capturedRenderer.Index);
				}
			);
		}


	}

	public void GetChoice(List<byte> options, Color highlight, HandleChoiceDelegate handle)
	{
		decisionQueue.Enqueue(new Choice(options, highlight, handle));
		ShowChoice();
	}

	public void OnAnyCardButtonUI(byte index)
	{
		Console.Log("Card Button Clicked.");
		if (decisionQueue.Count > 0)
		{
			if (decisionQueue.Peek().Options.Contains(index))
			{
				Choose(index);
			}
			else
			{
				Console.Log(index + " is not a valid choice.");
				Console.Log(decisionQueue.Peek().Options.ToString());
			}
        }
	}


	public void OnCancelButtonUI()
	{
		//Nothing yet
	}

	void ShowChoice()
	{
		if (decisionQueue.Count == 0)
		{
			Clear();
			//Visuals.SetActive(false);
		}
		else
		{
			//Visuals.SetActive(true);
			Choice nextChoice = decisionQueue.Peek();
			Highlight(nextChoice.Options, nextChoice.Highlight);

		}
	}


	void Highlight(List<byte> indices, Color color)
	{
		foreach (CardRenderer r in CardsInPlay)
		{
			if (indices.Contains(r.Index))
			{
				r.Highlight(color);
			}
		}
	}
	void Clear()
	{
		foreach (CardRenderer r in CardsInPlay)
		{
			r.Highlight(Color.clear);
		}
	}

	void Choose(byte choice)
	{
		Console.Log("Choosing Card.");
		Choice currentChoice = decisionQueue.Dequeue();
		currentChoice.Handle(choice);
		ShowChoice();
	}

	private class Choice
	{

		public Choice(List<byte> options, Color highlight, HandleChoiceDelegate handle)
		{
			Options = options;
			Highlight = highlight;
			Handle = handle;
		}
		public List<byte> Options;
		public Color Highlight;
		public HandleChoiceDelegate Handle;
	}
}
