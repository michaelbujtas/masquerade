using UnityEngine;
using System.Collections;
using AdvancedInspector;

[AdvancedInspector]
public class CardLogic : ComponentMonoBehaviour {

	[Inspect]
	public Card Card;

	public void LinkCard(Card card)
	{
		card.LinkLogic(this);
	}

	
}
