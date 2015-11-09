public interface IAfterAttacking
{
	void AfterAttacking(Card defender);
}

public interface IOnKilled
{
	void OnKilled(Card killer);
}

public interface IActivatedAbility
{
	void ActivateAbility();
}

public interface IStateBasedEffect
{
	void OnGameStateChanged();
}
