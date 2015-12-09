using UnityEngine;
using System.Collections.Generic;
using BeardedManStudios.Network;
using DevConsole;
using System.Collections;
using AdvancedInspector;



[AdvancedInspector]
public class GameplayNetworking : SimpleNetworkedMonoBehavior
{
	[Inspect]
	public byte PlaceInTurnOrder = 0; //0-3, we don't handle players dropping very well, if someone drops someone gets skipped. This is a demo

	[Inspect]
	public byte MyPlayerNumber = 0; 

	[Inspect]
	public List<IndexHand> ClockwiseHands = new List<IndexHand>();

	List<IndexHand> UsedHands = new List<IndexHand>();

	[Inspect]
	public CardIndex TheCardIndex;

	[Inspect]
	public IndexDeck TheDeck;

	[Inspect]
	public IndexFaceUpChoiceMenu FaceUpChoiceMenu;
	[Inspect]
	public IndexActionChoiceMenu ActionChoiceMenu;
	[Inspect]
	public IndexCardChoiceMenu CardChoiceMenu;

	ResponseStore<bool> facingRequestResponses = new ResponseStore<bool>();
	ResponseStore<ActionDescriptor> actionRequestResponses = new ResponseStore<ActionDescriptor>();

	[Inspect]
	public NetworkingPlayer CurrentPlayer
	{
		get
		{
			if (OwningNetWorker.Connected)
			{
				if (OwningNetWorker.Players.Count > PlaceInTurnOrder)
				{
					return OwningNetWorker.Players[PlaceInTurnOrder];
				}
			}
			return null;
		}
	}

	public void AdvanceTurn()
	{
		if (OwningNetWorker != null && OwningNetWorker.Connected && OwningNetWorker.IsServer)
		{
			PlaceInTurnOrder++;
			if (PlaceInTurnOrder >= OwningNetWorker.Players.Count)
			{
				PlaceInTurnOrder = 0;
			}

			AuthoritativeRPC("AlertTurnRPC", OwningNetWorker, CurrentPlayer, false);
		}
	}

	public void RequestButtonClickUI() //Server
	{
		StartGame();
	}

	public void DrawButtonClickUI() //Server
	{
		if (CurrentPlayer != null && OwningNetWorker.IsServer)
			StartCoroutine(DrawCardCOR(PlaceInTurnOrder));
	}

	public void TakeActionButtonClickUI() //Server
	{
		if (CurrentPlayer != null && OwningNetWorker.IsServer)
			StartCoroutine(TakeActionCOR(PlaceInTurnOrder));
	}

	[BRPC]
	public void AlertTurnRPC() //Called by Server on Client
	{
		Console.Log("It's my turn!", Color.green);
	}

	protected override void NetworkStart()
	{
		base.NetworkStart();

		Console.Log("GameplayNetworking.NetworkStart()");
		gameObject.SetActive(OwningNetWorker.IsServer);

	}

	[BRPC]
	public void SetPlayerSlotIndexRPC(byte player, byte slot, byte index)
	{
		ClockwiseHands[player].SetIndex(slot, index);
	}

	[BRPC]
	public void ConfigureHandsRPC(bool fourPlayers, byte playerNumber)
	{
		if (fourPlayers)
		{
			//You on bottom, other players clockwise. If there's an empty seat, it will be player 4, to the left of player 1
			for (int i = playerNumber; i < 4; i++)
			{
				UsedHands.Add(ClockwiseHands[i]);
			}
			for (int i = 0; i < playerNumber; i++)
			{
				UsedHands.Add(ClockwiseHands[i]);
			}

		}
		else
		{
			//You on the bottom, them on top.
			if (playerNumber == 0)
			{
				UsedHands.Add(ClockwiseHands[0]);
				UsedHands.Add(ClockwiseHands[2]);
			}
			else
			{
				UsedHands.Add(ClockwiseHands[2]);
				UsedHands.Add(ClockwiseHands[0]);
			}

			UsedHands.Add(ClockwiseHands[3]);
			UsedHands.Add(ClockwiseHands[4]);


		}
	}

	//Draw a card system uses DrawCardCOR and the ChooseFacing RPCs
	public IEnumerator DrawCardCOR(byte playerIndex)
	{
		Console.Log("Starting Draw Coroutine.", Color.blue);
		byte cardIndex = TheDeck.Draw();
		Console.Log("Drew #" + cardIndex + ".", Color.blue);
		Response<bool> response = facingRequestResponses.Add();
		Console.Log("Sending RequestChooseFacingRPC", Color.blue);
		AuthoritativeRPC("RequestChooseFacingRPC", OwningNetWorker, GetPlayer(playerIndex), false, response.Index, cardIndex);
		int i = 0;
		while (!response.FlagCompleted)
		{
			i++;
			if (i > 60)
			{

				Console.Log("Waited 60 frames for response.", Color.blue);
				i = 0;
			}
			yield return null;
		}
		TheCardIndex.GetCard(cardIndex).IsFaceUp = response.Result;
		response.Recycle();
		Console.Log("Got a response. Calling AddCardToBoards and calling it a day.", Color.blue);
		AddCardToBoards(playerIndex, cardIndex);
		//No RPC to set facing on client machines yet exists. Right now all cards are face-up to the person who drew them
		//and face-down to everyone else.

	}

	[BRPC]
	void RequestChooseFacingRPC(byte choiceID, byte index)//Called by server on client
	{
		Console.Log("Recieved a request (#" + choiceID + ") to choose the facing of " + index + ".", Color.blue);
		FaceUpChoiceMenu.GetChoice(index,
			delegate (bool choice)
			{
				Console.Log("Facing Choice Delegate Hit (#" + choiceID + "). Sending ResponseChooseFacingRPC", Color.blue);
				RPC("ResponseChooseFacingRPC", NetworkReceivers.Server, choiceID, choice);
			}
			);
	}

	[BRPC]
	void ResponseChooseFacingRPC(byte choiceID, bool choice)
	{
		Console.Log("Recieved a response to my facing request (#" + choiceID + ") [" + choice + "].");
		Response<bool> response = facingRequestResponses.Responses[choiceID];
		if (response != null && response.FlagWaiting)
		{
			response.Fill(choice);
		}
	}

	//Action system uses TakeActionCOR and the TakeAction RPCs
	public IEnumerator TakeActionCOR(byte playerIndex)
	{
		Console.Log("Starting Take Action Coroutine.", Color.blue);

		Response<ActionDescriptor> response = actionRequestResponses.Add();
		Console.Log("Sending RequestTakeActionRPC", Color.blue);
		AuthoritativeRPC("RequestTakeActionRPC", OwningNetWorker, GetPlayer(playerIndex), false, response.Index);
		int framesWaited = 0;
		while (!response.FlagCompleted)
		{
			framesWaited++;
			if (framesWaited > 60)
			{

				Console.Log("Waited 60 frames for response.", Color.blue);
				framesWaited = 0;
			}
			yield return null;
		}
		Console.Log("Got a response. Right now, I'm not doing any gameplay with it.", Color.blue);
		switch (response.Result.Type)
		{
			case CardAction.FLIP:
				Console.Log("Flipping card #" + response.Result.ActorIndex, Color.blue);
				//This is where we would actually flip it
				break;
			case CardAction.ATTACK:
				Console.Log("Card #" + response.Result.ActorIndex + " attacks Card #" + response.Result.TargetIndex, Color.blue);
				//This is where we would actually attack
				break;
			case CardAction.ACTIVATE:
				Console.Log("Activating card #" + response.Result.ActorIndex, Color.blue);
				//This is where we would actually attack
				break;

		}

		response.Recycle();
		//No RPC to set facing on client machines yet exists. Right now all cards are face-up to the person who drew them
		//and face-down to everyone else.
	}

	[BRPC]
	void RequestTakeActionRPC(byte choiceID)
	{

		Console.Log("Recieved a request (#" + choiceID + ") to take an action.");
		/*ActionChoiceMenu.GetChoice(1, 
			delegate (CardAction choice)
			{
				Console.Log("Take Action Delegate Hit (#" + choiceID + "). Sending ResponseTakeActionRPC");
				byte sendChoiceID = choiceID;
				byte sendActorIndex = 1;
				byte sendActionType = (byte)choice;
				byte sendTargetIndex = 205;
				Console.Log(sendChoiceID, Color.cyan);
				Console.Log(sendActorIndex, Color.cyan);
				Console.Log(sendActionType, Color.cyan);
				Console.Log(sendTargetIndex, Color.cyan);

				RPC("ResponseTakeActionRPC", NetworkReceivers.Server, sendChoiceID, sendActorIndex, sendActionType, sendTargetIndex);
			}
			);*/


		CardChoiceMenu.GetChoice(ClockwiseHands[MyPlayerNumber].CardsOwned, Color.green,
			delegate (byte choice)
			{
				Console.Log("First Take Action Delegate Hit (#" + choiceID + "). Sending ResponseTakeActionRPC");
				byte sendChoiceID = choiceID;
				byte sendActorIndex = choice;
				byte sendActionType = 1;
				byte sendTargetIndex = 205;
				RPC("ResponseTakeActionRPC", NetworkReceivers.Server, sendChoiceID, sendActorIndex, sendActionType, sendTargetIndex);

			}

			);

	}

	[BRPC]
	void ResponseTakeActionRPC(byte choiceID, byte actorIndex, byte actionType, byte targetIndex)
	{

		Console.Log("Recieved a response to my Take Action request (#" + choiceID + ")");
		Console.Log(choiceID, Color.magenta);
		Console.Log(actorIndex, Color.magenta);
		Console.Log(actionType, Color.magenta);
		Console.Log(targetIndex, Color.magenta);
		Response<ActionDescriptor> response = actionRequestResponses.Responses[choiceID];
		if (response != null && response.FlagWaiting)
		{
			response.Fill(new ActionDescriptor(actorIndex, (CardAction)actionType, targetIndex));
		}
	}

	public NetworkingPlayer GetPlayer(byte playerIndex)
	{
			if (OwningNetWorker.Connected)
			{
				if (OwningNetWorker.Players.Count > playerIndex)
				{
					return OwningNetWorker.Players[playerIndex];
				}
			}
			return null;
	}

	public void AddCardToBoards(byte targetPlayer, byte targetIndex)
	{
		if (CurrentPlayer != null && OwningNetWorker.IsServer)
		{
			byte targetSlot = ClockwiseHands[targetPlayer].FirstOpenSlot; //UsedHands is super broken and I don't have time to fix it now

			foreach (NetworkingPlayer p in OwningNetWorker.Players)
			{
				if (p == GetPlayer(targetPlayer))
					AuthoritativeRPC("SetPlayerSlotIndexRPC", OwningNetWorker, p, true, targetPlayer, targetSlot, targetIndex);

				else
					AuthoritativeRPC("SetPlayerSlotIndexRPC", OwningNetWorker, p, false, targetPlayer, targetSlot, targetPlayer + 201);
			}
		}
	}

	public void StartGame()
	{
		Console.Log("Starting the game.");
		/*List<byte> baseNumbers = new List<byte>();
		for (byte i = 0; i < OwningNetWorker.Players.Count; i++)
			baseNumbers.Add(i);
		List<byte> shuffledNumbers = new List<byte>();
		while(baseNumbers.Count > 0)
		{
			int randomIndex = Random.Range(0, baseNumbers.Count);
			shuffledNumbers.Add(baseNumbers[randomIndex]);
			baseNumbers.RemoveAt(randomIndex);
		}

		bool fourPlayers = OwningNetWorker.Players.Count > 2;
		foreach (NetworkingPlayer p in OwningNetWorker.Players)
		{
			byte playerNumber = shuffledNumbers[0];
			AuthoritativeRPC("ConfigureHandsRPC", OwningNetWorker, p, playerNumber == 0, fourPlayers, playerNumber);
		}*/

		int i = 0;
		foreach (NetworkingPlayer p in OwningNetWorker.Players)
		{
			AuthoritativeRPC("SetPlayerNumberRPC", OwningNetWorker, p, false, i);
			i++;
        }
	}

	[BRPC]
	public void SetPlayerNumberRPC(byte number)
	{
		MyPlayerNumber = number;
		Console.Log("Setting my player number. I'm #" + MyPlayerNumber);
	}
	public void Start()
	{
		for (byte i = 0; i < ClockwiseHands.Count; i++)
			ClockwiseHands[i].PlayerNumber = i;
	}
}
