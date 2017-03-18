using UnityEngine;
using System.Collections;
//using AdvancedInspector;

//[AdvancedInspector]
public class CardLogic //: ComponentMonoBehaviour
{

	//[Inspect]
	public Card Card;

	public void LinkCard(Card card)
	{
		card.LinkLogic(this);
	}

	public virtual bool TriggerIsPlausible(MasqueradePlayer currentPlayer)
	{
		return true;
	}

	protected GameplayNetworking Networking
	{
		get { return Card.Networking; }
	}

	protected GameTimer Timer
	{
		get { return Card.Networking.Timer; }
	}

	protected Card GetCard(byte index)
	{
		return Card.Networking.TheCardIndex.GetCard(index);
	}
}
