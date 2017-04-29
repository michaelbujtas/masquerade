using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class CardViewer : MonoBehaviour {

	public IndexCardChoiceMenu cardsInPlaySelector;

	public List<GameObject> otherScreens = new List<GameObject>();

	public GameObject visuals;


	public CollectionViewer collectionViewer;
	public CardRenderer cardRenderer;
	public TextMeshProUGUI rulesText;
	public TextMeshProUGUI flavorText;


	List<CardRenderer> CardsInPlay = new List<CardRenderer>();
	CardIndex CardIndex;
	IndexDiscardPile DiscardPile;


	public void Awake()
	{
		CardIndex = FindObjectOfType<CardIndex>();
		DiscardPile = FindObjectOfType<IndexDiscardPile>();

		var notDummyCardRenderers =
			from r in FindObjectsOfType<CardRenderer>()
			where r.DummyRenderer == false
			select r;

		CardsInPlay.Clear();
		CardsInPlay.AddRange(notDummyCardRenderers);

		for (int i = 0; i < CardsInPlay.Count; i++)
		{
			CardRenderer capturedRenderer = CardsInPlay[i];

			CardsInPlay[i].Background.GetComponent<RightClickListener>().rightClick.AddListener(
				delegate {
					OnAnyCardButtonUI(capturedRenderer.Index, true);
				}
			);
		}

		collectionViewer.OnAnyCardClickedDelegate = (b) => {
			OnAnyCardButtonUI(b, false);
		};
	}

	// Update is called once per frame
	void Update () {
		if(visuals.activeInHierarchy)
		{
			foreach (GameObject s in otherScreens)
				if (s.activeInHierarchy)
					visuals.SetActive(false);

			if (Input.GetKeyDown(KeyCode.Mouse1))
				visuals.SetActive(false);
		}
	}

	public void OnAnyCardButtonUI(byte index, bool hideOptions = true)
	{

		Card c = CardIndex.GetCard(index);

		if (c != null)
		{
			visuals.SetActive(true);

			if (hideOptions)
				collectionViewer.gameObject.SetActive(false);

			cardRenderer.Index = index;
			cardRenderer.SetFacing(c.IsFaceUp);
			cardRenderer.RefreshCardImage();

			rulesText.text = c.RulesText;
			flavorText.text = c.FlavorText;

		}



	}

	public void OnDeckUI()
	{
		List<byte> cardsUnaccountedFor = new List<byte>();
		cardsUnaccountedFor.AddRange(CardIndex.AllCardIndices);
		
		foreach(CardRenderer r in CardsInPlay)
		{
			cardsUnaccountedFor.Remove(r.Index);
		}

		foreach (byte b in DiscardPile.Contents)
		{
			cardsUnaccountedFor.Remove(b);
		}


		OnAnyCardButtonUI(cardsUnaccountedFor[Random.Range(0, cardsUnaccountedFor.Count)], false);
		collectionViewer.gameObject.SetActive(true);
		collectionViewer.SetValues(cardsUnaccountedFor);


	}

	public void OnDiscardUI()
	{
		OnAnyCardButtonUI(DiscardPile.TopCard, false);
		collectionViewer.gameObject.SetActive(true);
		collectionViewer.SetValues(DiscardPile.Contents);
	}



}
