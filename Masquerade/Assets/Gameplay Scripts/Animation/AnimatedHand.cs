using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class AnimatedHand : MonoBehaviour {


	public float TopBottomPadding;
	public float EdgePadding;
	public float CardPadding;

	public byte PlayerNumber;
	public CardRenderer Prefab;
	public GameObject Discard;
	public Transform Deck;

	public int debug;

	float CardAspect = 5f / 7f;

	public float HandWidth
	{
		get
		{
			return rectTransform.sizeDelta.x;
		}
	}

	public float CardWidth
	{
		get
		{
			return (rectTransform.sizeDelta.y - TopBottomPadding * 2) * CardAspect; //five sevenths is the aspect ratio of the cards
		}
	}
	public int CardsInHand
	{
		get
		{
			return Renderers.Count;
		}
	}

	float TotalSpacedWidth
	{
		get
		{
			return CardsInHand * CardWidth + (CardsInHand - 1) * CardPadding + 2 * EdgePadding;
		}
	}

	bool OverlapMode
	{
		get
		{
			return TotalSpacedWidth > HandWidth;
		}
	}


	RectTransform rectTransform;

	List<CardRenderer> Renderers = new List<CardRenderer>();



	void Start() {
		rectTransform = GetComponent<RectTransform>();
		//DEBUG
		if (Renderers.Count == 0)
			Renderers.AddRange(GetComponentsInChildren<CardRenderer>());


	}

	void Update() {
		UpdateCardSizes();

		UpdateCardPositionsSlide();

		//Debug

		Debug.Log(CardWidth + ", " + TotalSpacedWidth + "/" + HandWidth + ", " + OverlapMode);

		if (Input.GetKeyDown(KeyCode.Space))
		{
			CardRenderer newCard = Instantiate(Renderers[0]);
			newCard.Index = (byte)Random.Range(0, 60);
			AddCardRenderer(newCard);
		}

		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			CardRenderer renderer = Renderers[Random.Range(0, Renderers.Count)];
			RemoveCardRenderer(renderer);
			Destroy(renderer.gameObject);
		}

		if (Input.GetKeyDown(KeyCode.Alpha1))
			DrawCard((byte)Random.Range(1, 60));
		if (Input.GetKeyDown(KeyCode.Alpha2))
			ReanimateCard((byte)Random.Range(1, 60));
		if (Input.GetKeyDown(KeyCode.Alpha3))
			DiscardCard((byte)debug);
		if (Input.GetKeyDown(KeyCode.Alpha4))
			ShuffleAwayCard((byte)debug);
	}

	public void AddCardRenderer(CardRenderer card)
	{
		Renderers.Add(card);
		card.transform.parent = transform;
	}

	public void RemoveCardRenderer(CardRenderer card)
	{
		Renderers.Remove(card);
	}

	void UpdateCardSizes()
	{

		for (int i = 0; i < CardsInHand; i++)
		{
			Renderers[i].rectTransform.sizeDelta = new Vector2(CardWidth, CardWidth / CardAspect);
		}
	}

	void UpdateCardPositionsInstant()
	{
		for (int i = 0; i < CardsInHand; i++)
		{
			Renderers[i].rectTransform.anchoredPosition = new Vector2(GetCardTargetXPosition(i), 0);

		}
	}

	void UpdateCardPositionsSlide()
	{
		for (int i = 0; i < CardsInHand; i++)
		{
			Renderers[i].SetMoveTarget(new Vector2(GetCardTargetXPosition(i), 0));

		}
	}

	float GetCardTargetXPosition(int slot)
	{
		if (!OverlapMode)
		{
			//We're in space mode
			return ((CardWidth + CardPadding) * slot) - ((HandWidth - CardWidth) / 2) + (HandWidth - TotalSpacedWidth) / 2 + EdgePadding;

		}
		else
		{
			//We're in overlapping mode
			return (CardWidth / 2f + EdgePadding) + ((HandWidth - (CardWidth + 2f * EdgePadding)) / (CardsInHand - 1f)) * slot - HandWidth / 2;
		}
	}

	CardRenderer GetRendererByIndex(byte index)
	{
		var matches =
				from r in Renderers
				where r.Card != null && r.Index == index
				select r;

		CardRenderer[] retArray = matches.ToArray();
		if(retArray.Length > 0)
			return retArray[Random.Range(0, retArray.Length)];
		return null;
	}

	//Hand Linq Queries
	public List<byte> CardsOpenToAttack
	{
		get
		{
			List<byte> retval = new List<byte>();
			var faceUpCards =
				from r in Renderers
				where r.Card != null && r.Card.IsFaceUp
				select r.Index;
			retval.AddRange(faceUpCards);
			retval.Remove(CardIndex.PLAYER_1_FACEDOWN);
			retval.Remove(CardIndex.PLAYER_2_FACEDOWN);
			retval.Remove(CardIndex.PLAYER_3_FACEDOWN);
			retval.Remove(CardIndex.PLAYER_4_FACEDOWN);
			if (retval.Count == 0/* && CardsOwned.Count > 0*/)
			{
				retval.Add((byte)(PlayerNumber + CardIndex.PLAYER_1_FACEDOWN));
			}

			return retval;
		}
	}
	public List<byte> FaceUpCards
	{
		get
		{
			List<byte> retval = new List<byte>();
			var faceUpCards =
				from r in Renderers
				where r.Card != null && r.Card.IsFaceUp
				select r.Index;
			retval.AddRange(faceUpCards);

			return retval;
		}
	}
	public bool HasFaceUpCards()
	{
		foreach (CardRenderer r in Renderers)
		{
			if (r.Card != null && r.Card.IsFaceUp)
			{
				return true;
			}
		}
		return false;
	}
	public List<byte> CardsOwned
	{
		get
		{
			List<byte> retval = new List<byte>();
			var nonBlankCards =
				from r in Renderers
				where r.Card != null
				select r.Index;
			retval.AddRange(nonBlankCards);
			return retval;
		}
	}
	public List<byte> CardsThatCanBeDiscarded
	{
		get
		{
			List<byte> retval = new List<byte>();
			var nonBlankCards =
				from r in Renderers
				where r.Card != null && !r.Card.HasKeyword(Keyword.CANT_BE_DISCARDED)
				select r.Index;
			retval.AddRange(nonBlankCards);
			return retval;
		}
	}
	public List<byte> UntappedCards
	{
		get
		{
			List<byte> retval = new List<byte>();
			var nonBlankCards =
				from r in Renderers
				where r.Card != null && !r.Card.IsTapped
				select r.Index;
			retval.AddRange(nonBlankCards);
			return retval;
		}
	}
	public byte RandomFaceDownCard
	{
		get
		{
			List<byte> possibleCards = new List<byte>();
			var faceDownCards =
				from r in Renderers
				where r.Card != null && !r.Card.IsFaceUp
				select r.Index;
			possibleCards.AddRange(faceDownCards);
			if (possibleCards.Count > 0)
				return possibleCards[Random.Range(0, possibleCards.Count)];
			return CardIndex.EMPTY_SLOT;
		}
	}
	public bool HasFacedown
	{
		get
		{
			var faceDownCards =
			from r in Renderers
			where r.Card != null && !r.Card.IsFaceUp
			select r.Index;

			return faceDownCards.Count() > 0;
		}
	}


	//Add and Remove Index
	public void DrawCard(byte index)
	{
		CardRenderer newCard = Instantiate(Prefab);
		newCard.Index = index;
		newCard.rectTransform.position = Deck.position;
		AddCardRenderer(newCard);
	}
	
	public void ReanimateCard(byte index)
	{
		CardRenderer newCard = Instantiate(Prefab);
		newCard.Index = index;
		newCard.rectTransform.position = Discard.transform.position;
		AddCardRenderer(newCard);
	}

	public void GainControlOfCard(byte index)
	{

	}

	public bool DiscardCard(byte index)
	{
		CardRenderer foundRenderer = GetRendererByIndex(index);
		if (foundRenderer != null)
		{
			RemoveCardRenderer(foundRenderer);
			foundRenderer.transform.parent = Discard.transform;
			foundRenderer.SetMoveTarget(Vector2.zero);
		
			return true;
		}
		return false;
	}

	public bool ShuffleAwayCard(byte index)
	{
		CardRenderer foundRenderer = GetRendererByIndex(index);
		if (foundRenderer != null)
		{
			RemoveCardRenderer(foundRenderer);

			foundRenderer.transform.parent = Deck.transform;
			foundRenderer.SetMoveTarget(Vector2.zero);

			return true;
		}
		return false;
	}

}
