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


	public int debug1, debug2;

	void Start() {
		Object[] allPrefabs = Resources.LoadAll("Animation Assets/Prefabs");

		for (int i = 0; i < allPrefabs.Length; i++)
		{
			GameObject prefab = (GameObject)allPrefabs[i];

			prefabs.Add(prefab.name, prefab.GetComponent<Animation>());
			Debug.Log("found a thing");
		}

		CardRenderer[] cardRenderers = FindObjectsOfType<CardRenderer>();
		for (int i = 0; i < cardRenderers.Length; i++)
		{
			if (!cardRenderers[i].DummyRenderer)
				cardsInPlay.Add(cardRenderers[i]);
		}

		Networking = FindObjectOfType<GameplayNetworking>();
		Discard = Networking.TheDiscardPile;
		Deck = Networking.CardsInDeckDisplay;
		GraphicsSettings = FindObjectOfType<MultiWidthDriver>();

	}

	void Update() {

		if (Input.GetKeyDown(KeyCode.Space))
		{
			//QueueThrowAnimationNetworked("KnifeThrow", (byte)debug1, (byte)debug2);

			QueueFakeCardAnimation((byte)debug1, Vector2.zero);
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
		Animation anim = prefabs[name];
		if (anim is TweenAnimation)
		{
			animationQueue.Add(() =>
			{
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

	public void QueueFakeCardAnimation(byte index, Vector2 destination)
	{
		QueueFakeCardAnimation(index, GetCardPosition(index), destination);
	}


	public void QueueFakeCardAnimation(byte index, Vector2 origin, Vector2 destination)
	{
		CardRenderer extantCard = Networking.TheCardIndex.GetCard(index).Renderer;

		Animation anim = prefabs["FakeCard"];
		animationQueue.Add(() =>
		{
			CardRenderer renderer = Instantiate(anim.gameObject, transform).GetComponent<CardRenderer>();
			renderer.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, GraphicsSettings.CardHeight);
			renderer.Index = index;
			renderer.RefreshCardImage();

			TweenAnimation tweenAnim = renderer.GetComponent<TweenAnimation>();

			tweenAnim.Setup(origin, destination, AnimationSpeed,
					() => {
						PlayNextAnimation();
					});

		});
		ResumePlayback();

	}

	public void QueueDiscardAnimation(byte index)
	{
		animationQueue.Add(() =>
		{
			//See if we can even find that index
			foreach (CardRenderer r in cardsInPlay)
			{
				if (r.Index == index)
				{
					//If we can, get rid of it and make a fake version going to the discard
					Vector2 origin = GetCardPosition(index);

					foreach (IndexHand h in Networking.UsedHands)
					{
						h.RemoveIndex(index);
					}

					QueueFakeCardAnimation(index, origin, DiscardPosition);

					//Once it gets there, add it to the discard pile
					//NOT FIXED YET
				}
			}
		});
		ResumePlayback();




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
