using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class IndexActionChoiceMenu : MonoBehaviour
{
	public CardIndex Index;

	public GameObject Visuals;

	public CardRenderer cardRenderer;
	public TextMeshProUGUI rulesText;
	public TextMeshProUGUI flavorText;
	public Button attack;
	public Button flipUp;
	public Button flipDown;
	public Button ability;
	public Button back;


	List<Choice> decisionQueue = new List<Choice>();

	public delegate void HandleChoiceDelegate(CardAction choice);

	public delegate void HandleCancelDelegate();
	void Awake()
	{
		//Index = FindObjectOfType<GameplayNetworking>().TheCardIndex;
	}
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
		Cancel();
	}


	public Choice GetChoice(byte index, long expirationTicks, HandleChoiceDelegate handle, HandleCancelDelegate cancel)
	{
		Choice retVal = new Choice(index, expirationTicks, handle, cancel);
        decisionQueue.Add(retVal);
		ShowChoice();
		return retVal;
	}

	public void Update()
	{
		long ticksNow = System.DateTime.UtcNow.Ticks;
		for (int i = 0; i < decisionQueue.Count; i++)
		{
			if (decisionQueue[i].ExpirationTicks < ticksNow)
			{
				PurgeChoice(decisionQueue[i]);
				i--;
			}
		}
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
			back.gameObject.SetActive(nextChoice.Cancel != null);

			rulesText.SetText(newCard.RulesText);
			flavorText.SetText(newCard.FlavorText);

		}
	}

	void Choose(CardAction choice)
	{
		Choice currentChoice = decisionQueue[0];
		decisionQueue.RemoveAt(0);
		currentChoice.Handle(choice);
		ShowChoice();
	}

	void Cancel()
	{
		Choice currentChoice = decisionQueue[0];
		decisionQueue.RemoveAt(0);
		currentChoice.Cancel();
		ShowChoice();
	}

	public class Choice
	{

		public Choice(byte index, long expirationTicks, HandleChoiceDelegate handle, HandleCancelDelegate cancel)
		{
			Index = index;
			ExpirationTicks = expirationTicks;
			Handle = handle;
			Cancel = cancel;
		}

		public byte Index;
		public long ExpirationTicks;
		public HandleChoiceDelegate Handle;
		public HandleCancelDelegate Cancel;
	}
}

