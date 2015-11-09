using System.Collections.Generic;

public abstract class CardSelectorRequest {

    public abstract List<CardRenderer> GetList(List<CardRenderer> allRenderers);
	public abstract void Fill(Card card);
	public abstract void Cancel();
	public bool CanBeCancelled = true;


}
