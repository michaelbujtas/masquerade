using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;


public class IndexCardChoiceMenu : MonoBehaviour {
	public GameTimer gameTimer;
	public GameObject Visuals;

	public NamedButton PassButton, CancelButton;
	public CollectionViewer CardsNotInPlay;


	public List<AnimatedHand> AnimatedHands = new List<AnimatedHand>();

	//public List<CardRenderer> CardsInPlay = new List<CardRenderer>();
	public List<CardRenderer> CardsInPlay
	{
		get
		{
			List<CardRenderer> retVal = new List<CardRenderer>();
			foreach(AnimatedHand h in AnimatedHands)
			{
				retVal.AddRange(h.CardRenderers);
			}
			return retVal;
		}
	}

	public List<Choice> decisionQueue = new List<Choice>();

	public delegate void HandleChoiceDelegate(byte choice);

	public delegate void HandleCancelDelegate();

	GameplayNetworking networking;

	public void Awake()
	{
		networking = FindObjectOfType<GameplayNetworking>();
		AnimatedHands.AddRange(networking.ClockwiseHands);

		gameTimer = FindObjectOfType<GameTimer>();

		var notDummyCardRenderers =
			from r in FindObjectsOfType<CardRenderer>()
			where r.DummyRenderer == false
			select r;

		CardsInPlay.Clear();
		CardsInPlay.AddRange(notDummyCardRenderers);

		for(int i = 0; i < CardsInPlay.Count; i++)
		{
			//CardsInPlay[i].Background.gameObject.name = "SET LISTENER";
			CardRenderer capturedRenderer = CardsInPlay[i];

			CardsInPlay[i].Background.GetComponent<Button>().onClick.AddListener(
				delegate {
					OnAnyCardButtonUI(capturedRenderer.Index);
				}
			);
		}

		CardsNotInPlay.OnAnyCardClickedDelegate = (index) => { OnAnyCardButtonUI(index); };


	}

	public Choice GetChoice(List<byte> options, Color highlight, HandleChoiceDelegate handle, HandleCancelDelegate cancel, HandleCancelDelegate pass, bool mainTimerBound = true)
	{
		Choice choice = new Choice(options, highlight, handle, cancel, pass);

		if(mainTimerBound)
		{
			gameTimer.AddMainTimerDelegate(
				delegate
				{
					PurgeChoice(choice);
				});
		}
		else
		{
			gameTimer.AddSubTimerDelegate(
				delegate
				{
					PurgeChoice(choice);
				});
		}

		decisionQueue.Add(choice);
		ShowChoice();
		return choice;
	}

	
	public void PurgeChoice(Choice choice)
	{
		decisionQueue.Remove(choice);
		ShowChoice();
	}

	public Choice CurrentChoice
	{
		get
		{
			if (decisionQueue.Count > 0)
				return decisionQueue[0];
			return null;
		}

	}
	public void OnAnyCardButtonUI(byte index)
	{
		CustomConsole.Log("Card Button Clicked.");
		if (decisionQueue.Count > 0)
		{
			if (decisionQueue[0].Options.Contains(index))
			{
				Choose(index);
			}
			else
			{
				CustomConsole.Log(index + " is not a valid choice.");
				CustomConsole.Log(decisionQueue[0].Options.ToString());
			}
        }
	}


	public void OnCancelButtonUI()
	{
		CustomConsole.Log("Cancel Button Clicked.");
		if (decisionQueue.Count > 0)
		{
			if (decisionQueue[0].Cancel != null)
			{
				Cancel();
			}
			else
			{
				CustomConsole.Log("Can't cancel. (Why's the stinkin' button enabled then?)");
			}
		}
	}

	public void OnPassButtonUI()
	{
		CustomConsole.Log("Pass Button Clicked.");
		if (decisionQueue.Count > 0)
		{
			if (decisionQueue[0].Pass != null)
			{
				Pass();
			}
			else
			{
				CustomConsole.Log("Can't pass. (Why's the stinkin' button enabled then?)");
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
			List<byte> cardsNotOnBoard = HighlightChoicesOnBoard(nextChoice.Options, nextChoice.Highlight);
			if(cardsNotOnBoard.Count == 0)
			{
				CardsNotInPlay.gameObject.SetActive(false);
			}
			else
			{
				CardsNotInPlay.gameObject.SetActive(true);
				CardsNotInPlay.SetValues(cardsNotOnBoard);
			}

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


	List<byte> HighlightChoicesOnBoard(List<byte> indices, Color color)
	{
		List<byte> retVal = new List<byte>();
		retVal.AddRange(indices);
		foreach (CardRenderer r in CardsInPlay)
		{
			if (indices.Contains(r.Index))
			{
				r.Highlight(color);
				retVal.Remove(r.Index);
			}
		}

		for (int i = 0; i < retVal.Count; i++)
		{
			if(retVal[i]>=200)
			{
				retVal.RemoveAt(i);
				i--;
			}
		}
		return retVal;
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
		CustomConsole.Log("Choosing Card.");
		Choice currentChoice = decisionQueue[0];
		decisionQueue.Remove(currentChoice);
        currentChoice.Handle(choice);
		ShowChoice();
	}

	void Cancel()
	{
		CustomConsole.Log("Canceling.");
		Choice currentChoice = decisionQueue[0];
		decisionQueue.Remove(currentChoice);
		currentChoice.Cancel();
		ShowChoice();
	}

	void Pass()
	{

		CustomConsole.Log("Pass.");
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
