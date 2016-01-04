using UnityEngine;
using DevConsole;
using System.Collections;

public class MasqueradeEngine : MonoBehaviour {

	public Deck Deck;
	public DiscardPile DiscardPile;
	public CardFactory Factory;
	public CardSelector Selector;
	public CardOptionsMenu CardOptionsMenu;
	public FaceUpChoiceMenu FaceUpChoiceMenu;
	public Player Player1, Player2;


	public void FlipUp(Card card)
	{
		card.IsFaceUp = true;
		card.Renderer.RefreshCardImage();
	}

	public void FlipDown(Card card)
	{
		card.IsFaceUp = false;
		card.Renderer.RefreshCardImage();
	}

	public void Attack(Card attacker)
	{
		Selector.Handle(new SelectDefenderRequest(this, attacker));
	}

	public void Attack(Card attacker, Card defender)
	{

		int attack = attacker.GetCombatAttack();
		int defense = defender.GetCombatDefense();

		FlipUp(attacker);
		FlipUp(defender);

		Console.Log(attacker.CardName + " hit " + defender.CardName + " for " + attack + ".");
		Console.Log(defender.CardName + " blocked for " + defense + ".");
		if (attack >= defense)
			Kill(attacker, defender);
		else
			Kill(defender, attacker);

		if (attacker.Logic is IAfterAttacking)
			((IAfterAttacking)attacker.Logic).AfterAttacking(defender);


	}

	public void Kill(Card killer, Card victim)
	{
		Console.Log(killer.CardName + " killed " + victim.CardName, Color.red);
		//if (victim.Logic is IOnKilled)
		//	((IOnKilled)victim.Logic).OnKilled(killer);
		Player1.Hand.RemoveCard(victim.Renderer);
		DiscardPile.Discard(victim);
		victim.Renderer.Destroy();

	}

	public void PlayCard(Card card, bool facing) //Called once a card's facing and things are set. 
	{
		card.IsFaceUp = facing;
		card.Renderer.gameObject.SetActive(true);
		card.Renderer.RefreshCardImage();
	}

	public Card DrawCard() //Called to draw a card from the deck
	{

		if (Deck.Cards.Count == 0)
		{
			Deck.ReferenceImport(DiscardPile);
			Deck.Shuffle();
			DiscardPile.Clear();
		}


		if (Deck.Cards.Count > 0)
		{
			Card newCard = Deck.DrawCard();
			Factory.MakeCard(newCard);
			return newCard;
		}
		else
		{
			Console.Log("Can't draw because we're out of cards.");
			return null;
		}

	}

}
