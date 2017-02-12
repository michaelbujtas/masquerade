using UnityEngine;
using System.Collections;
using AdvancedInspector;
using BeardedManStudios.Network;
using System.Collections.Generic;

[AdvancedInspector]
public class Card : MonoBehaviour
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
				CustomConsole.LogError("Card index is not yet set.");
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
				CustomConsole.LogError("You're trying to reindex a card. You should never have to do that.");
			}
		}
	}

	public MasqueradePlayer Owner;

	public byte? CurrentSlot = null;

	//Actual card information

	[Inspect]
	public string CardName;
	[Inspect, Enum(false)]
	public CardClass CardClass;
	[Inspect]
	public int Attack;
	[Inspect]
	public FaceUpBonus AttackBonus;
	[Inspect]
	public int Defense;
	[Inspect]
	public FaceUpBonus DefenseBonus;
	[Inspect]
	public Sprite Art;
	[Inspect]
	public string RulesText = string.Empty;
	[Inspect]
	public string FlavorText = string.Empty;

	[Inspect]
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


	bool isAlive = false;
	public bool IsAlive
	{
		get { return isAlive; }
		set { isAlive = value; }
	}


	public List<Buff> Buffs = new List<Buff>();


	[Inspect]
	public CardLogic[] logic = new CardLogic[1];

	public CardLogic Logic
	{
		get
		{

			if (logic != null)
			{
				if (logic.Length > 1)
					CustomConsole.LogError(CardName + "has too many logics. I'm gonna use the first one, but you need to get your shit together.");
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
			CustomConsole.LogError("We can't copy cards with logics right now.");
		//	Logic = newCard.Logic.Instantiate(gameObject, this) as CardLogic;

	}


	public int GetCombatAttack(bool? hypotheticalFacing = null)
	{
		bool useFacing = false;
		if (hypotheticalFacing == null)
			useFacing = IsFaceUp;
		else
			useFacing = (bool)hypotheticalFacing;

		int modifiedAttack = Attack;

		foreach (Buff b in Buffs)
			if(b.BeforeBonus)
				modifiedAttack += b.Attack;

		if (useFacing && AttackBonus == FaceUpBonus.FACE_UP ||
		   !useFacing && AttackBonus == FaceUpBonus.FACE_DOWN)
				modifiedAttack *= 2;

		foreach (Buff b in Buffs)
			if (!b.BeforeBonus)
				modifiedAttack += b.Attack;

		return modifiedAttack;
	}

	public int GetCombatDefense(bool? hypotheticalFacing = null)
	{
		bool useFacing = false;
		if (hypotheticalFacing == null)
			useFacing = IsFaceUp;
		else
			useFacing = (bool)hypotheticalFacing;


		int modifiedDefense = Defense;

		foreach (Buff b in Buffs)
			modifiedDefense += b.Defense;

		if (useFacing && DefenseBonus == FaceUpBonus.FACE_UP ||
		   !useFacing && DefenseBonus == FaceUpBonus.FACE_DOWN)
			return Defense * 2;

		return Defense;
	}

	public IEnumerator FlipAction(bool shouldBeFaceUp, System.Action callback)
	{
		IsTapped = true;
		Networking.SyncCard(this);
		Flip(shouldBeFaceUp);
		yield return null;
		callback();
	}

	public void TapAction()
	{
		CustomConsole.Log("TAP ACTION!!!", Color.red);
		IsTapped = true;
		Networking.SyncCard(this);
	}

	public IEnumerator FlipAction(System.Action callback)
	{
		IsTapped = true;
		Networking.SyncCard(this);
		Flip();

		yield return null;
		callback();

	}

	public void Flip()
	{
		IsFaceUp = !IsFaceUp;
		Networking.SyncCard(this);
		Networking.EnsureProperFlippedness(this);
		Networking.StaticEffects();
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
			Networking.SyncCard(this);
		}
	}

	public IEnumerator AttackAction(Card defender, System.Action callback)
	{
		Color logColor = new Color(1, .5f, 0);
		IsTapped = true;
		Networking.SyncCard(this);

		//Calculate Attack and Defense
		int attack = GetCombatAttack();
		int defense = defender.GetCombatDefense();


		CustomConsole.Log(CardName + " hit " + defender.CardName + " for " + attack + ".", logColor);
		CustomConsole.Log(defender.CardName + " blocked for " + defense + ".", logColor);


		//FlipUp
		Flip(true);
		defender.Flip(true);

		//Kill Loser
		if (attack >= defense)
		{

			CustomConsole.Log("Defending player owns " + networking.UsedHands[defender.Owner.PlayerIndex].CardsOwned.Count + " cards.", logColor);

			if (networking.UsedHands[defender.Owner.PlayerIndex].CardsOwned.Count == 1)
			{
				bool BoardClearDrawFinished = false;

				StartCoroutine(networking.DrawCardCOR(Owner.PlayerIndex, 1, (coroutineReturn) => { BoardClearDrawFinished = true; }));

				while (!BoardClearDrawFinished)
					yield return null;
			}
			yield return null;
			defender.KillWithContext(this, DeathContext.DEFENDING);
		}
		else
		{
			this.KillWithContext(defender, DeathContext.ATTACKING);
		}

		CustomConsole.Log("AfterAttack isAlive status " + IsAlive);

		//After Attacking
		if (IsAlive)
			if (Logic is IAfterAttacking)
				((IAfterAttacking)Logic).AfterAttacking(defender);

		callback();

	}

	public IEnumerator ActivateAction(System.Action callback)
	{
		IsTapped = true;

		if (Logic is IActivatedAbility)
			((IActivatedAbility)Logic).ActivateAbility();

		callback();
		return null;

	}



	public void KillWithContext(Card killer, DeathContext context)
	{
		CustomConsole.Log(killer.CardName + " killed " + CardName + ". Context: " + context.ToString(), Color.red);
		if (IsAlive)
			if (Logic is IOnKilled)
				((IOnKilled)Logic).OnKilled(killer, context);

		Kill();
	}


	public void Kill()
	{
		CustomConsole.Log(CardName + " died.", Color.red);

		if (IsAlive)
			if (Logic is IOnDeath)
				((IOnDeath)Logic).OnDeath();

		IsAlive = false;
		Networking.SendToDiscard(this);
	}





	public void SyncFlip()
	{
		Networking.SyncCard(this);
		Networking.EnsureProperFlippedness(this);
	}


	public void ForgetHistory()
	{

		CustomConsole.Log(CardName + "is forgetting its time on the board.");
		//Everything that can change about this card gets set back to it's default version. 

		//Forget our facing
		IsFaceUp = true;

		//Forget our tappedness
		IsTapped = false;

		//Forget basically everything else
		Buffs.Clear();

		//Make sure everyone else knows who we are
		Networking.SyncCard(this);
		Networking.EnsureProperFlippedness(this);

		//Forget our position on the board
		Owner = null;
		CurrentSlot = null;
	}

	public Buff AddBuff(int attack, int defense, bool permanent, bool beforeBonus)
	{
		Buff buff = new Buff(attack, defense, permanent, beforeBonus);

		Buffs.Add(buff);

		Networking.SyncCard(this);
		return buff;
	}

	public void CleanupBuffs()
	{
		for (int i = 0; i < Buffs.Count; i++)
		{
			if (!Buffs[i].Permanent)
			{
				Buffs.RemoveAt(i);
				i--;
			}
		}
	}

	public int TotalAttackBuff
	{
		get
		{
			int retVal = 0;
			for (int i = 0; i < Buffs.Count; i++)
			{
				retVal += Buffs[i].Attack;
			}
			return retVal;
		}
	}

	public int TotalDefenseBuff
	{
		get
		{
			int retVal = 0;
			for (int i = 0; i < Buffs.Count; i++)
			{
				retVal += Buffs[i].Defense;
			}
			return retVal;
		}
	}

	public class Buff
	{
		public Buff(int attack, int defense, bool permanent, bool beforeBonus)
		{
			Attack = attack;
			Defense = defense;
			Permanent = permanent;
			BeforeBonus = beforeBonus;
		}
		public int Attack;
		public int Defense;
		public bool Permanent;
		public bool BeforeBonus;
	}
}
