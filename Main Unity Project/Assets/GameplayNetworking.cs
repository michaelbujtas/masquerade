using UnityEngine;
using System.Collections.Generic;
using BeardedManStudios.Network;
using DevConsole;
using System.Collections;
using AdvancedInspector;
using UnityEngine.UI;


[AdvancedInspector]
public class GameplayNetworking : SimpleNetworkedMonoBehavior
{
	[Inspect]
	public GameObject ServerControlPanel;


	[Inspect]
	public byte PlaceInTurnOrder = 0; //0-3, we don't handle players dropping very well, if someone drops someone gets skipped. This is a demo

	[Inspect]
	public byte MyPlayerNumber = 0;

	[Inspect]
	public List<IndexHand> ClockwiseHands = new List<IndexHand>();

	List<IndexHand> UsedHands = new List<IndexHand>();

	List<MasqueradePlayer> MasqueradePlayers = new List<MasqueradePlayer>();

	[Inspect]
	public CardIndex TheCardIndex;

	[Inspect]
	public IndexDeck TheDeck;

	[Inspect]
	public IndexDiscardPile TheDiscardPile;

	[Inspect]
	public IndexFaceUpChoiceMenu FaceUpChoiceMenu;
	[Inspect]
	public IndexActionChoiceMenu ActionChoiceMenu;
	[Inspect]
	public IndexCardChoiceMenu CardChoiceMenu;



	[Inspect]
	public NamedValueField CardsInDeckDisplay;

	[Inspect]
	public NamedValueField ActionsRemainingDisplay;



	ResponseStore<bool> facingRequestResponses = new ResponseStore<bool>();
	ResponseStore<ActionDescriptor> actionRequestResponses = new ResponseStore<ActionDescriptor>();
	ResponseStore<bool> coroutineReturns = new ResponseStore<bool>();

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

			UpdateWhoseTurn();
		}
	}

	public void RequestButtonClickUI() //Server
	{
		StartGame();
	}

	public void DrawButtonClickUI() //Server
	{
		if (CurrentPlayer != null && OwningNetWorker.IsServer)
		{

			StartCoroutine(DrawCardCOR(PlaceInTurnOrder, null));
		}
	}

	public void TakeActionButtonClickUI() //Server
	{
		if (CurrentPlayer != null && OwningNetWorker.IsServer)
		{
			StartCoroutine(TakeActionCOR(PlaceInTurnOrder, null));

		}
	}

	public void TakeTurnButtonClickUI() //Server
	{
		if (CurrentPlayer != null && OwningNetWorker.IsServer)
		{
			StartCoroutine(TakeTurnCOR(PlaceInTurnOrder, null));

		}
	}

	public void UpdateWhoseTurn()
	{

		if (OwningNetWorker != null && OwningNetWorker.Connected && OwningNetWorker.IsServer)
		{
			foreach (NetworkingPlayer p in OwningNetWorker.Players)
			{
				bool myTurn = (p == CurrentPlayer);
				AuthoritativeRPC("AlertTurnRPC", OwningNetWorker, p, false, myTurn, PlaceInTurnOrder);
			}
		}
	}

	[BRPC]
	public void AlertTurnRPC(bool isMyTurn, byte playerNumber) //Called by Server on Client
	{
		if (isMyTurn)
		{
			Console.Log("It's my turn!", Color.green);
			ActionsRemainingDisplay.SetColors(Color.white, Color.gray);
		}
		else
		{
			Console.Log("It's player " + playerNumber.ToString() + "'s turn. (Not mine.)", Color.green);
			ActionsRemainingDisplay.SetColors(Color.black, Color.gray);
		}
	}

	protected override void NetworkStart()
	{
		base.NetworkStart();

		Console.Log("GameplayNetworking.NetworkStart()");
		ServerControlPanel.SetActive(OwningNetWorker.IsServer);

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


	#region Draw Card
	//Draw a card system uses DrawCardCOR and the ChooseFacing RPCs
	public IEnumerator DrawCardCOR(byte playerIndex, byte? returnIndex)
	{
		Color logColor = Color.blue;
		Console.Log("Starting Draw Coroutine.", logColor);
		byte cardIndex = TheDeck.Draw();



		Console.Log("Sending UpdateCardsInDeckRPCs.", logColor);
		UpdateCardsInDeck(TheDeck.CardsRemaining);

		TheCardIndex.GetCard(cardIndex).Owner = MasqueradePlayers[playerIndex];

		Console.Log("Drew #" + cardIndex + ".", logColor);
		Response<bool> response = facingRequestResponses.Add();
		Console.Log("Sending RequestChooseFacingRPC", logColor);
		AuthoritativeRPC("RequestChooseFacingRPC", OwningNetWorker, GetPlayer(playerIndex), false, response.Index, cardIndex);
		int i = 0;
		while (!response.FlagCompleted)
		{
			i++;
			if (i > 60)
			{

				Console.Log("Waited 60 frames for response.", logColor);
				i = 0;
			}
			yield return null;
		}
		TheCardIndex.GetCard(cardIndex).IsFaceUp = response.Result;
		TheCardIndex.GetCard(cardIndex).Sync();
		response.Recycle();
		Console.Log("Got a response. Calling AddCardToBoards and calling it a day.", logColor);
		AddCardToBoards(playerIndex, cardIndex);

		if(returnIndex != null)
			coroutineReturns.Responses[(byte)returnIndex].Fill(true);
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
	#endregion

	#region Take Action
	//Action system uses TakeActionCOR and the TakeAction RPCs
	public IEnumerator TakeActionCOR(byte playerIndex, byte? returnIndex)
	{
		bool retVal = false;
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
		Console.Log("Got a response.", Color.blue);
		switch (response.Result.Type)
		{
			case CardAction.FLIP:
				Console.Log("Flipping card #" + response.Result.ActorIndex, Color.blue);
				TheCardIndex.GetCard(response.Result.ActorIndex).FlipAction();
				break;
			case CardAction.ATTACK:
				Console.Log("Card #" + response.Result.ActorIndex + " attacks Card #" + response.Result.TargetIndex, Color.blue);

				Card attacker = TheCardIndex.GetCard(response.Result.ActorIndex);
				Card defender = null;
				if (response.Result.TargetIndex < CardIndex.PLAYER_1_FACEDOWN)
				{
					defender = TheCardIndex.GetCard(response.Result.TargetIndex);
				}
				else if (response.Result.TargetIndex >= CardIndex.PLAYER_1_FACEDOWN && response.Result.TargetIndex <= CardIndex.PLAYER_4_FACEDOWN)
				{
					byte randomTargetIndex = ClockwiseHands[response.Result.TargetIndex - CardIndex.PLAYER_1_FACEDOWN].RandomFaceDownCard;

					defender = TheCardIndex.GetCard(randomTargetIndex);
				}



				if (attacker.Owner != defender.Owner)
					attacker.AttackAction(defender);

				break;
			case CardAction.ACTIVATE:
				Console.Log("Activating card #" + response.Result.ActorIndex, Color.blue);
				TheCardIndex.GetCard(response.Result.ActorIndex).ActivateAction();
				break;
			case CardAction.PASS:
				Console.Log("Passing Turn.", Color.blue);
				retVal = true;
				if (returnIndex == null)
					AdvanceTurn();
				break;

		}

		response.Recycle();


		if (returnIndex != null)
			coroutineReturns.Responses[(byte)returnIndex].Fill(retVal);
	}



	[BRPC]
	void RequestTakeActionRPC(byte choiceID)
	{

		Console.Log("Recieved a request (#" + choiceID + ") to take an action.");

		StartCoroutine(GenerateTakeActionResponseCOR(choiceID));
		

	}

	public IEnumerator GenerateTakeActionResponseCOR(byte choiceID)
	{
		Console.Log("Starting GenerateTakeActionResponseCOR.");

		bool canceled;

		byte sendChoiceID = choiceID;

		byte sendActorIndex;

		byte sendActionType;

		byte sendTargetIndex;

		do
		{

			sendActorIndex = CardIndex.EMPTY_SLOT;
			sendActionType = 0;
			sendTargetIndex = CardIndex.EMPTY_SLOT;

			bool passed = false;

			canceled = false;

			IndexCardChoiceMenu.Choice firstCardChoice = null;
			IndexCardChoiceMenu.Choice secondCardChoice = null;
			IndexActionChoiceMenu.Choice firstActionChoice = null;


			if (!passed)
			{
				firstCardChoice = CardChoiceMenu.GetChoice(ClockwiseHands[MyPlayerNumber].UntappedCards, Color.green,
				delegate (byte choice)
				{
					Console.Log("Got an actor. #" + choice);
					sendActorIndex = choice;
				},
				null,
				delegate
				{
					Console.Log("Passing turn.");
					sendActorIndex = CardIndex.EMPTY_SLOT;
					sendActionType = (byte)CardAction.PASS;
					sendTargetIndex = CardIndex.EMPTY_SLOT;
					passed = true;
				}
				);
			}


			while (!passed && sendActorIndex == CardIndex.EMPTY_SLOT)
				yield return null;



			if (!passed)
			{
				firstActionChoice = ActionChoiceMenu.GetChoice(sendActorIndex,
					delegate (CardAction choice)
					{
						Console.Log("Got a choice. " + choice.ToString());
						sendActionType = (byte)choice;
					}
					);

			}
			while (!passed && sendActionType == 0)
				yield return null;



			if (!passed)
			{
				bool needsATarget = sendActionType == (byte)CardAction.ATTACK;
				if (needsATarget)
				{


					List<byte> potentialTargets = new List<byte>();
					for (int i = 0; i < ClockwiseHands.Count; i++)
					{
						if (i != MyPlayerNumber)
							potentialTargets.AddRange(ClockwiseHands[i].CardsOpenToAttack);
					}
					secondCardChoice = CardChoiceMenu.GetChoice(potentialTargets, Color.red,
					   delegate (byte choice)
					   {
						   Console.Log("Got a target. #" + choice);
						   sendTargetIndex = choice;
					   },


						delegate
						{
							Console.LogError("Canceling attack.");
							canceled = true;
							passed = true;
						},
					   null);
				}

				while (!passed && needsATarget && sendTargetIndex == CardIndex.EMPTY_SLOT)
					yield return null;
			}

		} while (canceled == true);


		Console.Log("Got everything I need. Sending ResponseTakeActionRPC.");
		RPC("ResponseTakeActionRPC", NetworkReceivers.Server, sendChoiceID, sendActorIndex, sendActionType, sendTargetIndex);


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
	#endregion



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


	#region Add or Flip Card
	public void AddCardToBoards(byte targetPlayer, byte targetIndex)
	{

		if (CurrentPlayer != null && OwningNetWorker.IsServer)
		{
			Card card = TheCardIndex.GetCard(targetIndex);

			byte targetSlot = ClockwiseHands[targetPlayer].FirstOpenSlot; //UsedHands is super broken and I don't have time to fix it now

			card.CurrentSlot = targetSlot;

			AddCardToBoards(targetPlayer, targetIndex, targetSlot);
		}
	}

	public void AddCardToBoards(byte targetPlayer, byte targetIndex, byte targetSlot)
	{
		if (CurrentPlayer != null && OwningNetWorker.IsServer)
		{

			Card card = TheCardIndex.GetCard(targetIndex);

			foreach (NetworkingPlayer p in OwningNetWorker.Players)
			{
				if (p == GetPlayer(targetPlayer) || card.IsFaceUp)
					AuthoritativeRPC("SetPlayerSlotIndexRPC", OwningNetWorker, p, true, targetPlayer, targetSlot, targetIndex);

				else
					AuthoritativeRPC("SetPlayerSlotIndexRPC", OwningNetWorker, p, false, targetPlayer, targetSlot, (byte)(targetPlayer + CardIndex.PLAYER_1_FACEDOWN));
			}
		}
	}

	[BRPC]
	public void SetPlayerSlotIndexRPC(byte player, byte slot, byte index)
	{
		ClockwiseHands[player].SetIndex(slot, index);
	}

	public void EnsureProperFlippedness(Card card)
	{
		if (card.Index != null && card.CurrentSlot != null)
		{
			AddCardToBoards(card.Owner.PlayerIndex, (byte)card.Index, (byte)card.CurrentSlot);
		}
	}
	#endregion

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

		StartCoroutine(StartGameCOR());


	}

	IEnumerator StartGameCOR()
	{
		MasqueradePlayers.Clear();
		for (byte j = 0; j < OwningNetWorker.Players.Count; j++)
		{
			MasqueradePlayers.Add(new MasqueradePlayer(OwningNetWorker, j));
		}



		byte i = 0;
		foreach (NetworkingPlayer p in OwningNetWorker.Players)
		{
			AuthoritativeRPC("SetPlayerNumberRPC", OwningNetWorker, p, false, i);
			i++;
		}

		UpdateWhoseTurn();


		List<Response<bool>> allDrawResponses = new List<Response<bool>>();
		for (int j = 0; j < 2; j++)
		{

			i = 0;
			foreach (NetworkingPlayer p in OwningNetWorker.Players)
			{
				Response<bool> newResponse = coroutineReturns.Add();
				allDrawResponses.Add(newResponse);
				StartCoroutine(DrawCardCOR(i, newResponse.Index));
				i++;
			}
		}

		bool allPlayersFinished = false;

		while (!allPlayersFinished)
		{
			allPlayersFinished = true;
			foreach (Response<bool> r in allDrawResponses)
			{
				if (r.FlagWaiting)
				{
					allPlayersFinished = false;
					break;
				}
			}

			yield return null;
		}


		foreach (Response<bool> r in allDrawResponses)
			r.Recycle();

		bool gameIsOver = false;

		while (!gameIsOver)
		{
			Response<bool> turnResponse = coroutineReturns.Add();

			StartCoroutine(TakeTurnCOR(PlaceInTurnOrder, turnResponse.Index));

			while (turnResponse.FlagWaiting)
				yield return null;

			gameIsOver = turnResponse.Result;
			turnResponse.Recycle();
		}

	}

	[BRPC]
	public void SetPlayerNumberRPC(byte number)
	{
		MyPlayerNumber = number;
		Console.Log("Setting my player number. I'm #" + MyPlayerNumber);
	}

	#region Send To Discard
	public void SendToDiscard(Card card)
	{
		card.ForgetHistory();
		
		foreach (NetworkingPlayer p in OwningNetWorker.Players)
		{
			AuthoritativeRPC("SendToDiscardRPC", OwningNetWorker, p, false, card.Index);
		}
		SendToDiscardRPC((byte)card.Index);
	}

	[BRPC]
	void SendToDiscardRPC(byte cardIndex)
	{
		Console.Log("Sending " + TheCardIndex.GetCard(cardIndex).CardName + " to the discard.", Color.red);

		foreach (IndexHand h in ClockwiseHands)
		{
			h.RemoveIndex(cardIndex);
		}
		TheDiscardPile.AddIndex(cardIndex);

	}
	#endregion

	#region Update Display Values
	public void UpdateActionsLeft(int actionsLeft)
	{

		foreach (NetworkingPlayer p in OwningNetWorker.Players)
		{
			AuthoritativeRPC("UpdateActionsLeftRPC", OwningNetWorker, p, false, actionsLeft);
		}
		UpdateActionsLeftRPC(actionsLeft);
	}

	[BRPC]
	public void UpdateActionsLeftRPC(int actionsLeft)
	{
		ActionsRemainingDisplay.SetValue(actionsLeft);
	}

	public void UpdateCardsInDeck(int cardsInDeck)
	{

		foreach (NetworkingPlayer p in OwningNetWorker.Players)
		{
			AuthoritativeRPC("UpdateCardsInDeckRPC", OwningNetWorker, p, false, cardsInDeck);
		}
		UpdateCardsInDeckRPC(cardsInDeck);
	}


	[BRPC]
	public void UpdateCardsInDeckRPC(int cardsInDeck)
	{
		CardsInDeckDisplay.SetValue(cardsInDeck);
	}
	#endregion

	public void Start()
	{
		for (byte i = 0; i < ClockwiseHands.Count; i++)
			ClockwiseHands[i].PlayerNumber = i;
	}

	IEnumerator TakeTurnCOR(byte playerIndex, byte? returnIndex)
	{
		Response<bool> drawIsFinished = coroutineReturns.Add();

		StartCoroutine(DrawCardCOR(playerIndex, drawIsFinished.Index));

		while (drawIsFinished.FlagWaiting)
			yield return null;

		drawIsFinished.Recycle();


		int actionsLeft = 3;

		while(actionsLeft > 0)
		{
			UpdateActionsLeft(actionsLeft);

			Response<bool> actionIsFinished = coroutineReturns.Add();

			StartCoroutine(TakeActionCOR(playerIndex, actionIsFinished.Index));

			while (actionIsFinished.FlagWaiting)
				yield return null;

			if (actionIsFinished.Result)
			{
				actionsLeft = 0;
			}
			else
			{
				actionsLeft--;
			}

			actionIsFinished.Recycle();
		}

		AdvanceTurn();


		if (returnIndex != null)
			coroutineReturns.Responses[(byte)returnIndex].Fill(false); //Returning true will end the game
		
	}


}
