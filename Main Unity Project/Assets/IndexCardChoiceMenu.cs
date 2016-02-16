﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DevConsole;
using System.Linq;
using UnityEngine.UI;


public class IndexCardChoiceMenu : MonoBehaviour {

	public GameObject Visuals;

	public NamedButton PassButton, CancelButton;

	public List<CardRenderer> CardsInPlay = new List<CardRenderer>();

	public 
	List<Choice> decisionQueue = new List<Choice>();

	public delegate void HandleChoiceDelegate(byte choice);

	public delegate void HandleCancelDelegate();

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

	public Choice GetChoice(List<byte> options, Color highlight, HandleChoiceDelegate handle, HandleCancelDelegate cancel, HandleCancelDelegate pass)
	{
		Choice choice = new Choice(options, highlight, handle, cancel, pass);
        decisionQueue.Add(choice);
		ShowChoice();
		return choice;
	}
	public void PurgeChoice(Choice choice)
	{
		decisionQueue.Remove(choice);
		ShowChoice();
	}
	public void OnAnyCardButtonUI(byte index)
	{
		Console.Log("Card Button Clicked.");
		if (decisionQueue.Count > 0)
		{
			if (decisionQueue[0].Options.Contains(index))
			{
				Choose(index);
			}
			else
			{
				Console.Log(index + " is not a valid choice.");
				Console.Log(decisionQueue[0].Options.ToString());
			}
        }
	}


	public void OnCancelButtonUI()
	{
		Console.Log("Cancel Button Clicked.");
		if (decisionQueue.Count > 0)
		{
			if (decisionQueue[0].Cancel != null)
			{
				Cancel();
			}
			else
			{
				Console.Log("Can't cancel. (Why's the stinkin' button enabled then?)");
			}
		}
	}

	public void OnPassButtonUI()
	{
		Console.Log("Pass Button Clicked.");
		if (decisionQueue.Count > 0)
		{
			if (decisionQueue[0].Pass != null)
			{
				Pass();
			}
			else
			{
				Console.Log("Can't pass. (Why's the stinkin' button enabled then?)");
			}
		}
	}



	void ShowChoice()
	{
		if (decisionQueue.Count == 0)
		{
			Clear();
			Visuals.SetActive(false);
		}
		else
		{
			Visuals.SetActive(true);
			Choice nextChoice = decisionQueue[0];
            Highlight(nextChoice.Options, nextChoice.Highlight);

			if (nextChoice.Cancel != null)
			{
				CancelButton.Visuals.SetActive(true);
				CancelButton.SetHighlight(Color.blue);
			}
			else
			{
				CancelButton.Visuals.SetActive(false);
				CancelButton.ClearHighlight();
			}

			if (nextChoice.Pass != null)
			{
				PassButton.Visuals.SetActive(true);
				PassButton.SetHighlight(Color.red);
			}
			else
			{
				PassButton.Visuals.SetActive(false);
				PassButton.ClearHighlight();
			}

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
		Choice currentChoice = decisionQueue[0];
		decisionQueue.Remove(currentChoice);
        currentChoice.Handle(choice);
		ShowChoice();
	}

	void Cancel()
	{
		Console.Log("Canceling.");
		Choice currentChoice = decisionQueue[0];
		decisionQueue.Remove(currentChoice);
		currentChoice.Cancel();
		ShowChoice();
	}

	void Pass()
	{

		Console.Log("Pass.");
		Choice currentChoice = decisionQueue[0];
		decisionQueue.Remove(currentChoice);
		currentChoice.Pass();
		ShowChoice();
	}

	public class Choice
	{

		public Choice(List<byte> options, Color highlight, HandleChoiceDelegate handle, HandleCancelDelegate cancel, HandleCancelDelegate pass)
		{
			Options = options;
			Highlight = highlight;
			Handle = handle;
			Cancel = cancel;
			Pass = pass;
			
		}
		public List<byte> Options;
		public Color Highlight;
		public HandleChoiceDelegate Handle;

		public HandleCancelDelegate Cancel;

		public HandleCancelDelegate Pass;
	}
}