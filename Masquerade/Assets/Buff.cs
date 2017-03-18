using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff
{
	public Buff(int attack, int defense, bool permanent, bool beforeBonus, CardLogic source, Card card)
	{
		Attack = attack;
		Defense = defense;
		Permanent = permanent;
		BeforeBonus = beforeBonus;
		Source = source;
		Card = card;
	}

	public int Attack;
	public int Defense;
	public bool Permanent;
	public bool BeforeBonus;
	public CardLogic Source;
	public Card Card;

	public List<Keyword> Keywords = new List<Keyword>();
}

