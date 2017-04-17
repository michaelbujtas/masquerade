using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerChoiceMenu : MonoBehaviour {

	public GameTimer gameTimer;
	public GameObject Visuals;

	public NamedButton CancelButton;

	public PlayerIdentityRenderer[] IdentityRenderers;

	public List<Choice> decisionQueue = new List<Choice>();

	public delegate void HandleChoiceDelegate(byte choice);

	public delegate void HandleCancelDelegate();

	public void Awake()
	{
		gameTimer = FindObjectOfType<GameTimer>();


		for (int i = 0; i < IdentityRenderers.Length; i++)
		{
			//CardsInPlay[i].Background.gameObject.name = "SET LISTENER";
			PlayerIdentityRenderer capturedRenderer = IdentityRenderers[i];

			IdentityRenderers[i].Background.gameObject.GetComponent<Button>().onClick.AddListener(
				delegate {
				OnAnyCardButtonUI(capturedRenderer.Identity.Index);
				}
			);
		}


	}

	public Choice GetChoice(List<byte> options, Color highlight, HandleChoiceDelegate handle, HandleCancelDelegate cancel,  bool mainTimerBound = true)
	{
		Choice choice = new Choice(options, highlight, handle, cancel);

		if (mainTimerBound)
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

		}
	}


	void Highlight(List<byte> indices, Color color)
	{
		foreach (PlayerIdentityRenderer r in IdentityRenderers)
		{
			if (r.Identity != null)
			{
				if (indices.Contains((byte)r.Identity.Index))
				{
					r.Highlight(color);
				}
			}
		}
	}

	void Clear()
	{
		foreach (PlayerIdentityRenderer r in IdentityRenderers)
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

	public class Choice
	{

		public Choice(List<byte> options, Color highlight, HandleChoiceDelegate handle, HandleCancelDelegate cancel)
		{
			Options = options;
			Highlight = highlight;
			Handle = handle;
			Cancel = cancel;

		}
		public List<byte> Options;
		public Color Highlight;
		public HandleChoiceDelegate Handle;

		public HandleCancelDelegate Cancel;
	}
}
