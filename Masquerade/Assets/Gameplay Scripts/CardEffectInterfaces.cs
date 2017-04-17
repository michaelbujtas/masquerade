using System.Collections.Generic;

public interface IAfterAttacking
{
	void AfterAttacking(Card defender, System.Action callback);
}

public interface IBeforeAttacking
{
	void BeforeAttacking(Card defender, System.Action callback);
}

public interface IOnKilled
{
	void OnKilled(Card killer, DeathContext context, System.Action callback);
}

/*public interface IOnDeath
{
	void OnDeath();
}*/

public interface IActivatedAbility
{
	void ActivateAbility();
}

public interface IEndPhase
{
	void OnEndPhase(MasqueradePlayer turn, System.Action callback);
}

public interface IStartPhase
{
	void OnStartPhase(MasqueradePlayer turn, System.Action callback);
}

public interface ISpecialStats
{
	int GetBaseAttack();
	int GetBaseDefense();
}

public interface IStaticEffect
{
	void StaticEffect();
}

//This is for keywords that are on all the time. 
//Specifically, it exists for things that have keywords when they're not in play, like the King.
//Most things that only have keywords sometimes grant them as a self-buff instead.
//Keywords exist in buffs at all because Arthur gives one to the King.
//There is no good reason to use self-buffs instead of this, or the other way around. Both work.
public interface IHasKeywords
{
	List<Keyword> GetKeywords();
}

public interface IFlipEffect
{
	void OnFlip(bool flippedFaceUp, System.Action callback);
}

public interface ICanKill
{
	bool CanKill(Card other);
}

public interface IStartPhaseParasite
{
	void OnStartPhaseParasite(Card other, System.Action callback);
}



