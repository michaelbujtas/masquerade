using UnityEngine;
using System.Collections;
using BeardedManStudios.Network;
using System.Collections.Generic;

public class Card : MonoBehaviour
{
	//Networking stuff attached to the card, set dynamically
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



	MasqueradePlayer lastOwner;

	MasqueradePlayer owner;
	public MasqueradePlayer Owner
	{
		get
		{
			return owner;
		}
		set
		{
			owner = value;
			if(value != null)
				lastOwner = value;
		}
	}

	public MasqueradePlayer LastOwner
	{
		get
		{
			return lastOwner;
		}
	}

	public byte? CurrentSlot = null;

	//Actual card information

	public string CardName;
	public CardClass CardClass;
	public int Attack;
	public FaceUpBonus AttackBonus;
	public int Defense;
	public FaceUpBonus DefenseBonus;
	public Sprite Art;
	public string RulesText = string.Empty;
	public string FlavorText = string.Empty;

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


	public bool noText = false;
	public CardLogic[] logic = new CardLogic[1];

	public CardLogic Logic
	{
		get
		{

			if (logic != null)
			{
				if(noText)
				{
					return null;
				}
				else
				{
					if (logic.Length > 1)
						CustomConsole.LogError(CardName + "has too many logics. I'm gonna use the first one, but you need to get your shit together.");
					if (logic.Length == 0)
						return null;
					return logic[0];

				}
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
		get { return GetCombatAttack() > 0; }
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

		if (!HasKeyword(Keyword.NO_BONUSES))
		{
			if (useFacing && AttackBonus == FaceUpBonus.FACE_UP ||
		   !useFacing && AttackBonus == FaceUpBonus.FACE_DOWN)
				modifiedAttack *= 2;
		}

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
			if (b.BeforeBonus)
				modifiedDefense += b.Defense;

		if(!HasKeyword(Keyword.NO_BONUSES))
		{
			if (useFacing && DefenseBonus == FaceUpBonus.FACE_UP ||
			   !useFacing && DefenseBonus == FaceUpBonus.FACE_DOWN)
				modifiedDefense *= 2;
		}



		foreach (Buff b in Buffs)
			if (!b.BeforeBonus)
				modifiedDefense += b.Defense;

		return modifiedDefense;
	}

	public void TapAction()
	{
		CustomConsole.Log("TAP ACTION!!!", Color.red);
		IsTapped = true;
		Networking.SyncCard(this);
	}

	public IEnumerator FlipAction(bool shouldBeFaceUp, System.Action callback)
	{
		IsTapped = true;
		Networking.SyncCard(this);

		bool flipDone = false;
		StartCoroutine(Flip(shouldBeFaceUp, ((a) => flipDone = true)));

		while (!flipDone)
			yield return null;

		yield return null;
		callback();
	}

	public IEnumerator FlipAction(System.Action callback)
	{
		IsTapped = true;
		Networking.SyncCard(this);

		bool flipDone = false;
		StartCoroutine(Flip(() => flipDone = true));

		while(!flipDone)
			yield return null;

		callback();
	}

	public IEnumerator Flip(System.Action callback)
	{
		FlipNoTriggers();


		if (Logic is IFlipEffect)
		{
			bool flipDone = false;
			((IFlipEffect)Logic).OnFlip(IsFaceUp, () => flipDone = true);


			while (!flipDone)
				yield return null;
		}

		callback();
	}

	public IEnumerator Flip(bool shouldBeFaceUp, System.Action<bool> callback)
	{
		if (IsFaceUp == shouldBeFaceUp)
		{
			callback(false);
		}
		else
		{
			FlipNoTriggers();

			if (Logic is IFlipEffect)
			{
				bool flipDone = false;
				((IFlipEffect)Logic).OnFlip(IsFaceUp, () => flipDone = true);

				while (!flipDone)
					yield return null;
			}

			callback(true);
		}
	}

	public void FlipNoTriggers()
	{
		IsFaceUp = !IsFaceUp;
		SyncFlip();


		Networking.StaticEffects();
	}

	public bool FlipNoTriggers(bool shouldBeFaceUp)
	{
		if (IsFaceUp == shouldBeFaceUp)
			return false;
		FlipNoTriggers();
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


		//Before Attacking
		if (Logic is IBeforeAttacking)
		{
			bool beforeAttackDone = false;
			((IBeforeAttacking)Logic).BeforeAttacking(defender, (() => beforeAttackDone = true));
			while (!beforeAttackDone)
				yield return null;
		}

		//Calculate Attack and Defense
		int attack = GetCombatAttack();
		int defense = defender.GetCombatDefense();




		CustomConsole.Log(CardName + " hit " + defender.CardName + " for " + attack + ".", logColor);
		CustomConsole.Log(defender.CardName + " blocked for " + defense + ".", logColor);


		//FlipUp
		bool attackerFlipped = FlipNoTriggers(true);
		bool defenderFlipped = defender.FlipNoTriggers(true);


		bool attackerDied = false;
		bool defenderDied = false;

		//Kill Loser
		if (attack >= defense)
		{
			if (CanKill(defender) && defender.CanBeKilledBy(this))
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
				defenderDied = defender.KillWithContextNoTriggers(this, DeathContext.DEFENDING);
			}
		}
		else
		{
			attackerDied = this.KillWithContextNoTriggers(defender, DeathContext.ATTACKING);
		}

		CustomConsole.Log("AfterAttack isAlive status " + IsAlive);

		//Attacker triggers from the combat (only one should exist)



		//Flip Up
			if (Logic is IFlipEffect && attackerFlipped)
			{
				bool attackFlipDone = false;
				((IFlipEffect)Logic).OnFlip(true, (() => attackFlipDone = true));
				while (!attackFlipDone)
					yield return null;
		}

		//After Attacking
		if (Logic is IAfterAttacking)
		{
			bool afterAttackDone = false;
			((IAfterAttacking)Logic).AfterAttacking(defender, (() => afterAttackDone = true));
			while (!afterAttackDone)
				yield return null;
		}

		//Killed
		if(Logic is IOnKilled && attackerDied)
		{
			bool attackerDiedTriggerDone = false;
			((IOnKilled)Logic).OnKilled(defender, DeathContext.ATTACKING, (()=>attackerDiedTriggerDone = true));
			while (!attackerDiedTriggerDone)
				yield return null;
		}

		//Defender triggers from the combat (only one should exist)

		//Flip Up
		if (defender.Logic is IFlipEffect && defenderFlipped)
		{
			bool defenderFlipDone = false;
			((IFlipEffect)defender.Logic).OnFlip(true, (() => defenderFlipDone = true));
			while (!defenderFlipDone)
				yield return null;

		}

		//Killed
		if (defender.Logic is IOnKilled && defenderDied)
		{
			bool defenderDiedTriggerDone = false;
			((IOnKilled)defender.Logic).OnKilled(this, DeathContext.DEFENDING, (() => defenderDiedTriggerDone = true));
			while (!defenderDiedTriggerDone)
				yield return null;

		}

		if (callback!= null)
			callback();

	}

	public IEnumerator ActivateAction(System.Action callback)
	{
		IsTapped = true;
		Sync();

		bool actionDone = false;

		if (Logic is IActivatedAbility)
			((IActivatedAbility)Logic).ActivateAbility(()=> actionDone = true);
		else
			actionDone = true;

		while (!actionDone)
			yield return null;

		callback();

	}


	public bool CanBeKilledBy(Card other)
	{
		if (HasKeyword(Keyword.CANT_BE_KILLED))
			return false;

		return true;
	}

	public bool CanKill(Card other)
	{
		if (Logic is ICanKill)
			return ((ICanKill)Logic).CanKill(other);
		else
			return true;
	}

	public IEnumerator KillWithContext(Card killer, DeathContext context, System.Action<bool> callback)
	{
		//Check Context
		if (killer.CanKill(this) && this.CanBeKilledBy(killer))
		{
			
			if (IsAlive)
				if (Logic is IOnKilled)
				{
					bool onKilledFinished = false;
					((IOnKilled)Logic).OnKilled(killer, context, (()=>onKilledFinished = true));
					while (!onKilledFinished)
						yield return null;
				}

			Kill(false);
			CustomConsole.Log(killer.CardName + " killed " + CardName + ". Context: " + context.ToString(), Color.red);
			callback(true);
		}
		else
		{
			callback(false);
		}


	}


	public bool KillWithContextNoTriggers(Card killer, DeathContext context)
	{
		//Check Context
		if (killer.CanKill(this) && this.CanBeKilledBy(killer))
		{
			Kill(true);
			CustomConsole.Log(killer.CardName + " killed " + CardName + ". Context: " + context.ToString(), Color.red);
			return true;
		}
		return false;

	}

	public void Kill(bool noTriggers = false)
	{
		CustomConsole.Log(CardName + " died.", Color.red);

		/*
		if(!noTriggers)
		{
		if (IsAlive)
				if (Logic is IOnDeath)
					((IOnDeath)Logic).OnDeath();
		}*/

		if(IsAlive)
		{
			IsAlive = false;

			Networking.SendToDiscard(this);
		}
	}



	public void Sync()
	{
		Networking.SyncCard(this);
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

		
		IsAlive = false;

		//Forget basically everything else
		Buffs.Clear();

		//Make sure everyone else knows who we are
		Networking.SyncCard(this);
		Networking.EnsureProperFlippedness(this);

		//Forget our position on the board
		Owner = null;
		CurrentSlot = null;
	}

	public Buff AddBuff(int attack, int defense, bool permanent, bool beforeBonus, CardLogic source)
	{
		Buff buff = new Buff(attack, defense, permanent, beforeBonus, source, this);

		Buffs.Add(buff);
		

		Sync();
		return buff;
	}

	public Buff AddKeywordBuff(CardLogic source, Keyword keyword)
	{
		Buff buff = new Buff(0, 0, true, false, source, this);
		Buffs.Add(buff);
		buff.AddKeyword(keyword);
		Sync();
		return buff;


	}

	public List<Buff> GetBuffs(Keyword keyword)
	{
		List<Buff> retVal = new List<Buff>();

		for (int i = 0; i < Buffs.Count; i++)
		{
			if (Buffs[i].Keywords.Contains(keyword))
				retVal.Add(Buffs[i]);
		}

		return retVal;
	}

	public bool RemoveBuff(Buff buff)
	{
		bool retVal = Buffs.Remove(buff);
		if (!HasKeyword(Keyword.NO_TEXT))
			noText = false;
		Sync();
		return retVal;
	}

	public void CleanupBuffsEOT()
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

	public bool HasBuff(Buff buff)
	{
		if (buff == null)
			return false;
		return Buffs.Contains(buff);
	}

	public int TotalAttackBuff
	{
		get
		{
			int retVal = 0;
			for (int i = 0; i < Buffs.Count; i++)
			{
				retVal += Buffs[i].Attack;
				if (Buffs[i].BeforeBonus)
					if ((AttackBonus == FaceUpBonus.FACE_DOWN && !IsFaceUp) || (AttackBonus == FaceUpBonus.FACE_UP && IsFaceUp))
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
				if(Buffs[i].BeforeBonus)
					if((DefenseBonus == FaceUpBonus.FACE_DOWN && !IsFaceUp) || (DefenseBonus == FaceUpBonus.FACE_UP && IsFaceUp))
						retVal += Buffs[i].Defense;
			}
			return retVal;
		}
	}

	public bool HasKeyword(Keyword keyword)
	{
		if(Logic is IHasKeywords)
			if (((IHasKeywords)Logic).GetKeywords().Contains(keyword))
				return true;

		foreach (Buff b in Buffs)
			if (b.Keywords.Contains(keyword))
				return true;

		return false;
	}


	public bool OpenToAttack()
	{
		if (!isFaceUp)
		{
			foreach(byte b in Owner.Hand.CardsOwned)
			{
				Card c = Networking.TheCardIndex.GetCard(b);
				if (c.IsFaceUp)
					return false;
			}
		}
		return true;
	}


}
