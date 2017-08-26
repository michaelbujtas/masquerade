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
	/*
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
	}*/
}
