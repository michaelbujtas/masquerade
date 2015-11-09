using UnityEngine;
using System.Collections;

public class SingleFlipRequest {

	Player player;
	public Card card;

	public SingleFlipRequest(Player player, Card card)
	{

		this.player = player;
		this.card = card;
	}

	public void Fill(bool faceUp)
	{
		player.PlayCard(card, faceUp);
	}
}
