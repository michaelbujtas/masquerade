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

public interface IEndOfTurn
{
	void OnEndOfTurn(MasqueradePlayer turn);
}
