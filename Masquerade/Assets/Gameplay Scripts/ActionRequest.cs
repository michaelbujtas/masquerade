using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;


public class ActionRequest : CardSelectorRequest {

	Player player;
	Card card;
	public Card Card
	{
		get
		{
			return card;
		}
	}
	CardAction action;

	public ActionRequest(Player player)
	{
		this.player = player;
	}


	public override List<CardRenderer> GetList(List<CardRenderer> allRenderers)
	{
		var retVal =
			from renderer in allRenderers
			where player.Hand.Cards.Contains(renderer.Card)
			select renderer;


		return retVal.ToList();

	}



	public override void Fill(Card card)
	{
		this.card = card;
		player.Engine.CardOptionsMenu.Handle(this);
	}

	public void Fill(CardAction action)
	{
		this.action = action;

		player.TakeAction(card, action);

	}

	public override void Cancel()
	{
		Debug.Log("Attack cancelled.");
	}

	
}