public interface IAfterAttacking
{
	void AfterAttacking(Card defender);
}

public interface IOnKilled
{
	void OnKilled(Card killer, DeathContext context);
}
public interface IOnDeath
{
	void OnDeath();
}

public interface IActivatedAbility
{
	void ActivateAbility();
}

public interface IEndPhase
{
	void OnEndPhase(MasqueradePlayer turn);
}

public interface IStartPhase
{
	void OnStartPhase(MasqueradePlayer turn, Response<bool> response);
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
