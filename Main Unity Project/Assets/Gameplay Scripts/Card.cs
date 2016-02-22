﻿using UnityEngine;
using System.Collections;
using AdvancedInspector;
using DevConsole;
using BeardedManStudios.Network;


[AdvancedInspector]
public class Card : SimpleNetworkedMonoBehavior
{
	//Debug junk
	[Inspect]
	int debugOut = 0;
	//Networking stuff attached to the card, set dynamically
	[Inspect]
	byte? index = null;

	public byte? Index
	{
		get
		{
			if (index == null)
				Console.LogError("Card index is not yet set.");
			return index;
		}
		set
		{
			if (index == null)
			{
				index = value;
			}
			else
			{
				Console.LogError("You're trying to reindex a card. You should never have to do that.");
			}
		}
	}

	public MasqueradePlayer Owner;

	public byte? OwnerIndex = null;

	public byte? CurrentSlot = null;

	//Actual card information

	[Inspect]
	public string CardName;
	[Inspect, Enum(true)]
	public CardClass CardClass;
	[Inspect]
	public int Attack;
	[Inspect]
	public FaceUpBonus AttackBonus;
	[Inspect]
	public int Defense;
	[Inspect]
	public FaceUpBonus DefenseBonus;

	bool isFaceUp = true;
	public bool IsFaceUp
	{
		get { return isFaceUp; }
		set { isFaceUp = value; }
	}

	bool isTapped = false;
	public bool IsTapped
	{
		get { return isTapped; }
		set { isTapped = value; }
	}

	[Inspect, CreateDerived]
	public CardLogic[] logic = new CardLogic[1];

	public CardLogic Logic
	{
		get
		{

			if (logic != null)
			{
				if (logic.Length > 1)
					Console.LogError(CardName + "has too many logics. I'm gonna use the first one, but you need to get your shit together.");
				if (logic.Length == 0)
					return null;
				return logic[0];
			}
			return null;
		}
		set
		{
			logic[0] = value;
		}
	}

	public bool CanAttack
	{
		get { return Attack > 0; }
	}

	public bool CanFlipDown
	{
		get { return isFaceUp == true; }
	}

	public bool CanFlipUp
	{
		get { return isFaceUp == false; }
	}

	public bool CanActivateAbility
	{
		get { return Logic != null && Logic is IActivatedAbility; }
	}



	public CardRenderer Renderer;
	public void LinkRenderer(CardRenderer renderer)
	{
		Renderer = renderer;
		//Renderer.Card = this;
	}

	public void LinkLogic(CardLogic logic)
	{
		Logic = logic;
		Logic.Card = this;
	}

	MasqueradeEngine engine;

	public MasqueradeEngine Engine
	{
		get
		{
			if (engine == null)
				engine = FindObjectOfType<MasqueradeEngine>();
			return engine;
		}
	}

	GameplayNetworking networking;

	public GameplayNetworking Networking
	{
		get
		{
			if (networking == null)
				networking = FindObjectOfType<GameplayNetworking>();
			return networking;
		}
	}


	public void Start()
	{
		if (Logic != null)
			Logic.Card = this;



	}

	public void Update()
	{
		debugOut = (int)(CardClass & CardClass.KING);
	}

	public void Copy(Card newCard)
	{
		isFaceUp = newCard.isFaceUp;
		Attack = newCard.Attack;
		Defense = newCard.Defense;
		AttackBonus = newCard.AttackBonus;
		DefenseBonus = newCard.DefenseBonus;
		CardName = newCard.CardName;


		if (newCard.Logic != null)
			Logic = newCard.Logic.Instantiate(gameObject, this) as CardLogic;

	}


	public int GetCombatAttack()
	{
		if (isFaceUp && AttackBonus == FaceUpBonus.FACE_UP ||
		   !isFaceUp && AttackBonus == FaceUpBonus.FACE_DOWN)
			return Attack * 2;

		return Attack;
	}

	public int GetCombatDefense()
	{
		if (isFaceUp && DefenseBonus == FaceUpBonus.FACE_UP ||
		   !isFaceUp && DefenseBonus == FaceUpBonus.FACE_DOWN)
			return Defense * 2;

		return Defense;
	}

	public bool FlipAction(bool shouldBeFaceUp)
	{
		IsTapped = true;
		return Flip(shouldBeFaceUp);
	}

	public void FlipAction()
	{
		IsTapped = true;
		Flip();
	}

	public void Flip()
	{
		IsFaceUp = !IsFaceUp;
		Sync();
		Networking.EnsureProperFlippedness(this);
	}

	public bool Flip(bool shouldBeFaceUp)
	{
		if (IsFaceUp == shouldBeFaceUp)
			return false;
		Flip();
		return true;
	}

	public void Untap()
	{
		if (isTapped)
		{
			isTapped = false;
			Sync();
		}
	}

	public void AttackAction(Card defender)
	{

		IsTapped = true;
		Sync();

		//Calculate Attack and Defense
		int attack = GetCombatAttack();
		int defense = defender.GetCombatDefense();


		Console.Log(CardName + " hit " + defender.CardName + " for " + attack + ".");
		Console.Log(defender.CardName + " blocked for " + defense + ".");

		//FlipUp
		Flip(true);
		defender.Flip(true);

		//Kill Loser
		if (attack >= defense)
		{
			defender.KillWithContext(this, DeathContext.DEFENDING);
		}
		else
		{
			KillWithContext(defender, DeathContext.ATTACKING);
		}

		//After Attacking
		if (Logic is IAfterAttacking)
			((IAfterAttacking)Logic).AfterAttacking(defender);

	}

	public void ActivateAction()
	{
		IsTapped = true;

		if (Logic is IActivatedAbility)
			((IActivatedAbility)Logic).ActivateAbility();

	}



	public void KillWithContext(Card killer, DeathContext context)
	{
		Console.Log(killer.CardName + " killed " + CardName + ". Context: " + context.ToString(), Color.red);

		if (Logic is IOnKilled)
			((IOnKilled)Logic).OnKilled(killer, context);

		Kill();
	}

	public void Kill()
	{
		Console.Log(CardName + " died.", Color.red);

		if (Logic is IOnDeath)
			((IOnDeath)Logic).OnDeath();

		Networking.SendToDiscard(this);
	}

	public void Sync()
	{
		if (IsFaceUp)
		{
			//Sync with everybody
			foreach (MasqueradePlayer p in Networking.MasqueradePlayers)
			{
				AuthoritativeRPC("SyncRPC", OwningNetWorker, p.NetworkingPlayer, false, IsFaceUp, IsTapped);
			}
		}
		else
		{
			//Sync with the owner
			AuthoritativeRPC("SyncRPC", OwningNetWorker, Owner.NetworkingPlayer, false, IsFaceUp, IsTapped);
		}
	}

	public void SyncFlip()
	{
		Sync();
		Networking.EnsureProperFlippedness(this);
	}

	[BRPC]
	void SyncRPC(bool shouldBeFaceup, bool shouldBeTapped)
	{
		IsFaceUp = shouldBeFaceup;
		IsTapped = shouldBeTapped;
	}

	public void ForgetHistory()
	{

		Console.Log(CardName + "is is forgetting its time on the board.");
		//Everything that can change about this card gets set back to it's default version. 

		//Forget our facing
		IsFaceUp = true;

		//Forget our tappedness
		IsTapped = false;

		//Forget basically everything else

		//Make sure everyone else knows who we are
		Sync();
		Networking.EnsureProperFlippedness(this);

		//Forget our position on the board
		Owner = null;
		OwnerIndex = null;
		CurrentSlot = null;
	}
}
