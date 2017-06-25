using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Network;

public class AnimationQueue : SimpleNetworkedMonoBehavior
{

	IndexDiscardPile Discard;
	NamedValueField Deck;
	GameplayNetworking Networking;
	MultiWidthDriver GraphicsSettings;

	public float AnimationSpeed = 1;
	public bool Playing = false;

	Dictionary<string, Animation> prefabs = new Dictionary<string, Animation>();

	List<System.Action> animationQueue = new List<System.Action>();

	List<CardRenderer> cardsInPlay = new List<CardRenderer>();

	List<Vector2> slotPositions = new List<Vector2>();



	public int debug1, debug2;

	void Start() {
		Object[] allPrefabs = Resources.LoadAll("Animation Assets/Prefabs");

		for (int i = 0; i < allPrefabs.Length; i++)
		{
			GameObject prefab = (GameObject)allPrefabs[i];

			prefabs.Add(prefab.name, prefab.GetComponent<Animation>());
			Debug.Log("found a thing");
		}


		Networking = FindObjectOfType<GameplayNetworking>();


		Discard = Networking.TheDiscardPile;
		Deck = Networking.CardsInDeckDisplay;
		GraphicsSettings = FindObjectOfType<MultiWidthDriver>();


		for (int i = 0; i < Networking.ClockwiseHands.Count; i++)
		{
			cardsInPlay.AddRange(Networking.ClockwiseHands[i].Renderers);
		}


		slotPositions.Add(DeckPosition);
		slotPositions.Add(DiscardPosition);

		for (int i = 0; i < cardsInPlay.Count; i++)
		{
			slotPositions.Add(cardsInPlay[i].rectTransform.position);
		}
	}

	void Update() {

		if (Input.GetKeyDown(KeyCode.Space))
		{
			//QueueThrowAnimationNetworked("KnifeThrow", (byte)debug1, (byte)debug2);

			byte cardIndex = Networking.TheDeck.Draw();
			Networking.TheCardIndex.GetCard(cardIndex).IsFaceUp = false;
			Networking.TheCardIndex.GetCard(cardIndex).SyncFlip();
			QueueDrawAnimationNetworked((byte)Random.Range(0, 2), (byte)Random.Range(0, 6), cardIndex);
		}

	}


	Vector2 GetCardPosition(byte index)
	{
		foreach (CardRenderer r in cardsInPlay)
		{
			if (r.Index == index)
				return r.rectTransform.position;
		}

		if (Discard.Contents.Contains(index))
			return DiscardPosition;

		return DeckPosition;
	}

	Vector2 GetSlotPosition(byte player, byte slot)
	{
		return Networking.UsedHands[player].Renderers[slot].rectTransform.position;
	}

	Vector2 DiscardPosition
	{
		get
		{
			return Discard.Renderer.rectTransform.position;
		}
	}

	Vector2 DeckPosition
	{
		get
		{
			return Deck.Background.rectTransform.position;
		}
	}

	public void QueueThrowAnimationNetworked(string name, byte originIndex, byte destinationIndex)
	{
		foreach (MasqueradePlayer p in Networking.MasqueradePlayers)
		{
			AuthoritativeRPC("QueueThrowAnimationRPC", OwningNetWorker, p.NetworkingPlayer, false, name, originIndex, destinationIndex);
		}
	}

	[BRPC]
	void QueueThrowAnimationRPC(string name, byte originIndex, byte destinationIndex)
	{
		QueueTweenAnimation(name, GetCardPosition(originIndex), GetCardPosition(destinationIndex));
	}

	public void QueueTweenAnimation(string name, Vector2 origin, Vector2 destination)
	{
		CustomConsole.Log("Queue Tween Animation");
		Animation anim = prefabs[name];
		if (anim is TweenAnimation)
		{
			animationQueue.Add(() =>
			{
				CustomConsole.Log("Play Tween Animation");
				TweenAnimation tweenAnim = Instantiate(anim.gameObject, transform).GetComponent<TweenAnimation>();
				tweenAnim.Setup(origin, destination, AnimationSpeed,
					() => {
						PlayNextAnimation();
					});
			});
			ResumePlayback();
		}
		else
		{
			Debug.Log("not a tweenAnimation");
		}
	}


	public void QueueFakeCardAnimationNetworked(byte cardIndex, byte destinationIndex)
	{
		foreach (MasqueradePlayer p in Networking.MasqueradePlayers)
		{
			AuthoritativeRPC("QueueFakeCardAnimationRPC", OwningNetWorker, p.NetworkingPlayer, false,  cardIndex, destinationIndex);
		}
	}

	[BRPC]
	void QueueFakeCardAnimationRPC(byte cardIndex, byte destinationIndex)
	{
		QueueFakeCardAnimation(cardIndex, slotPositions[destinationIndex]);
	}

	public void QueueFakeCardAnimation(byte index, Vector2 destination)
	{
		QueueFakeCardAnimation(index, GetCardPosition(index), destination);
	}

	public void QueueFakeCardAnimation(byte index, Vector2 origin, Vector2 destination, System.Action pre = null, System.Action post = null)
	{
		CustomConsole.Log("Queue Fake Card Animation");

		
		Animation anim = prefabs["FakeCard"];
		animationQueue.Add(() =>
		{
			CustomConsole.Log("Play Fake Card Animation");
			if(pre != null)
				pre();

			CardRenderer renderer = Instantiate(anim.gameObject, transform).GetComponent<CardRenderer>();
			renderer.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, GraphicsSettings.CardHeight);
			renderer.Index = index;
			renderer.RefreshCardImage();

			TweenAnimation tweenAnim = renderer.GetComponent<TweenAnimation>();

			tweenAnim.Setup(origin, destination, AnimationSpeed,
					() => {
						if (post != null)
							post();
						PlayNextAnimation();
					});

		});
		ResumePlayback();

	}

	public void QueueDiscardAnimationNetworked(byte index)
	{
		foreach (MasqueradePlayer p in Networking.MasqueradePlayers)
		{
			AuthoritativeRPC("QueueDiscardAnimationRPC", OwningNetWorker, p.NetworkingPlayer, false, index);
		}
	}

	[BRPC]
	void QueueDiscardAnimationRPC(byte index)
	{
		QueueDiscardAnimation(index);
	}

	public void QueueDiscardAnimation(byte index)
	{
		CustomConsole.Log("Queue Discard Animation");

		System.Action Pre = () => {
			foreach (IndexHand h in Networking.UsedHands)
			{
				h.RemoveIndex(index);
			}
		};

		System.Action Post = () => {
			Networking.TheDiscardPile.AddIndex(index);

		};

		QueueFakeCardAnimation(index, GetCardPosition(index), DiscardPosition, Pre, Post);
		


	}



	public void QueueShuffleAwayAnimationNetworked(byte index)
	{
		foreach (MasqueradePlayer p in Networking.MasqueradePlayers)
		{
			AuthoritativeRPC("QueueShuffleAwayAnimationRPC", OwningNetWorker, p.NetworkingPlayer, false, index);
		}
	}

	[BRPC]
	void QueueShuffleAwayAnimationRPC(byte index)
	{
		QueueShuffleAwayAnimation(index);
	}

	public void QueueShuffleAwayAnimation(byte index)
	{
		CustomConsole.Log("Queue Discard Animation");

		System.Action Pre = () => {
			foreach (IndexHand h in Networking.UsedHands)
			{
				h.RemoveIndex(index);
			}
			Networking.TheDiscardPile.RemoveIndex(index);
		};

		System.Action Post = () => {
			//Increase the deck by 1

		};

		QueueFakeCardAnimation(index, GetCardPosition(index), DeckPosition, Pre, Post);



	}


	public void QueueTakeControlAnimationNetworked(byte index, byte oldOwner, byte newOwner)
	{
		Card c = Networking.TheCardIndex.GetCard(index);
		foreach (MasqueradePlayer p in Networking.MasqueradePlayers)
		{
			if(c.IsFaceUp)
			{
				//Do a normal slide from A to B
			}
			else
			{
				if(p.PlayerIndex == oldOwner)
				{
					//Do a normal slide that turns facedown
				}
				else if(p.PlayerIndex == newOwner)
				{
					//Do a normal slide from a facedown slot
				}
				else
				{
					//Do a slide from a facedown slot to another facedown slot
				}
			}
		}
	}

	[BRPC]
	void QueueTakeControlAnimationRPC(byte index, byte oldOwner, byte oldSlot, byte newOwner, byte newSlot)
	{
		QueueTakeControlAnimation(index, oldOwner, oldSlot, newOwner, newSlot);
	}

	public void QueueTakeControlAnimation(byte index, byte oldOwner, byte oldSlot, byte newOwner, byte newSlot)
	{

		System.Action Pre = () => {
			//Neutralize the old slot

		};

		System.Action Post = () => {
			//If the card is facedown and leaving my control for someone else's, place a facedown index instead
			if(Networking.MyPlayerNumber == oldOwner && Networking.MyPlayerNumber != newOwner && !Networking.TheCardIndex.GetCard(index).IsFaceUp)
			{

				Networking.UsedHands[newOwner].SetIndex(newSlot, (byte)(CardIndex.PLAYER_1_FACEDOWN + newOwner));
			}
			//Otherwise place a normal index
			Networking.UsedHands[newOwner].SetIndex(newSlot, index);

		};

		QueueFakeCardAnimation(index, GetSlotPosition(oldOwner, oldSlot), GetSlotPosition(newOwner, newSlot), Pre, Post);



	}



	public void QueueDrawAnimationNetworked(byte player, byte slot, byte index, bool reanimateMode = false)
	{
		Card c = Networking.TheCardIndex.GetCard(index);

		foreach (MasqueradePlayer p in Networking.MasqueradePlayers)
		{
			
				if (c.IsFaceUp || player == p.PlayerIndex)
					AuthoritativeRPC("QueueDrawAnimationRPC", OwningNetWorker, p.NetworkingPlayer, false, player, slot, index, reanimateMode);
				else
					AuthoritativeRPC("QueueDrawAnimationRPC", OwningNetWorker, p.NetworkingPlayer, false, player, slot, (byte)(CardIndex.PLAYER_1_FACEDOWN + player), reanimateMode);
			
		}
	}

	[BRPC]
	public void QueueDrawAnimationRPC(byte player, byte slot, byte index, bool reanimateMode)
	{
		QueueDrawAnimation(player, slot, index, reanimateMode);
	}

	public void QueueDrawAnimation(byte player, byte slot, byte index, bool reanimateMode = false)
	{
		Card c = Networking.TheCardIndex.GetCard(index);
		System.Action Pre = () => {
			//Reduce the deck by 1
			

		};

		System.Action Post = () => {

			Networking.UsedHands[player].SetIndex(slot, index);
			/*
			if (reanimateMode)
			{
				if (!c.IsFaceUp || player == Networking.MyPlayerNumber)
					Networking.UsedHands[player].SetIndex(slot, index);
				else
					Networking.UsedHands[player].SetIndex(slot, (byte)(CardIndex.PLAYER_1_FACEDOWN + player));

			}
			else
			{

			}*/

		};

		QueueFakeCardAnimation(index, reanimateMode ? DiscardPosition: DeckPosition, GetSlotPosition(player, slot), Pre, Post);
	}



	public void ResumePlayback()
	{
		if (!Playing)
			PlayNextAnimation();
	}

	public void PlayNextAnimation()
	{
		if(animationQueue.Count > 0)
		{
			Playing = true;
			System.Action nextAnim = animationQueue[0];
			animationQueue.RemoveAt(0);
			nextAnim();
		}
		else
		{
			Playing = false;
		}
	}
}
