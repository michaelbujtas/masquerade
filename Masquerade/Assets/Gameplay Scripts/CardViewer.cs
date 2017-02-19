using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class CardViewer : MonoBehaviour {

	public List<GameObject> otherScreens = new List<GameObject>();

	public GameObject visuals;

	public CardRenderer cardRenderer;
	public TextMeshProUGUI rulesText;
	public TextMeshProUGUI flavorText;

	List<CardRenderer> CardsInPlay = new List<CardRenderer>();
	CardIndex CardIndex;


	public void Awake()
	{
		CardIndex = FindObjectOfType<CardIndex>();

		var notDummyCardRenderers =
			from r in FindObjectsOfType<CardRenderer>()
			where r.DummyRenderer == false
			select r;

		CardsInPlay.Clear();
		CardsInPlay.AddRange(notDummyCardRenderers);

		for (int i = 0; i < CardsInPlay.Count; i++)
		{
			CardRenderer capturedRenderer = CardsInPlay[i];

			CardsInPlay[i].Background.GetComponent<Button>().onClick.AddListener(
				delegate {
					OnAnyCardButtonUI(capturedRenderer.Index);
				}
			);
		}
	}

	// Update is called once per frame
	void Update () {
		if(visuals.activeInHierarchy)
		{
			foreach (GameObject s in otherScreens)
				if (s.activeInHierarchy)
					visuals.SetActive(false);

			if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1))
				visuals.SetActive(false);
		}
	}

	public void OnAnyCardButtonUI(byte index)
	{
		Card c = CardIndex.GetCard(index);

		visuals.SetActive(true);

		cardRenderer.Index = index;
		cardRenderer.SetFacing(c.IsFaceUp);
		cardRenderer.RefreshCardImage();

		rulesText.text = c.RulesText;
		flavorText.text = c.FlavorText;


	}

}
