using UnityEngine;
using System.Collections.Generic;
using BeardedManStudios.Network;
using System.Collections;
using UnityEngine.UI;
using System.Linq;


public class GameplayNetworking : SimpleNetworkedMonoBehavior
{

	#region Properties
	public GameObject ServerControlPanel;


	public byte PlaceInTurnOrder = 0; //0-3, we don't handle players dropping very well, if someone drops someone gets skipped. This is a demo

	public byte MyPlayerNumber = 0;

	public List<IndexHand> ClockwiseHands = new List<IndexHand>();

	public List<IndexHand> UsedHands = new List<IndexHand>();

	public List<MasqueradePlayer> MasqueradePlayers = new List<MasqueradePlayer>();

	public List<PlayerIdentity> PlayerIdentities = new List<PlayerIdentity>();


	public CardIndex TheCardIndex;


	public IndexDeck TheDeck;


	public IndexDiscardPile TheDiscardPile;

	public IndexFaceUpChoiceMenu FaceUpChoiceMenu;

	public IndexActionChoiceMenu ActionChoiceMenu;

	public IndexCardChoiceMenu CardChoiceMenu;

	public PlayerChoiceMenu ThePlayerChoiceMenu;


	public EndGameScreen TheEndGameScreen;




	public NamedValueField CardsInDeckDisplay;


	public GameTimer Timer;


	public NamedValueField ActionsRemainingDisplay;


	ResponseStore<bool> facingRequestResponses = new ResponseStore<bool>();
	ResponseStore<bool[]> multiFacingRequestResponses = new ResponseStore<bool[]>();
	ResponseStore<ActionDescriptor> actionRequestResponses = new ResponseStore<ActionDescriptor>();
	ResponseStore<bool> coroutineReturns = new ResponseStore<bool>();
	ResponseStore<byte> cardRequestResponses = new ResponseStore<byte>();




	public MasqueradePlayer CurrentPlayer
	{
		get
		{
			if (MasqueradePlayers.Count > PlaceInTurnOrder)
			{
				return MasqueradePlayers[PlaceInTurnOrder];
			}
			return null;
		}
	}

	public MasqueradePlayer NextPlayer(MasqueradePlayer currentPlayer)
	{
		int nextIndex = currentPlayer.PlayerIndex + 1;
		if (nextIndex >= MasqueradePlayers.Count)
			nextIndex = 0;
		return MasqueradePlayers[nextIndex];
	}

	public MasqueradePlayer GetPlayer(int playerIndex)
	{
		while (MasqueradePlayers.Count <= playerIndex)
		{
			playerIndex -= MasqueradePlayers.Count;
		}

		if (MasqueradePlayers.Count > playerIndex)
		{
			return MasqueradePlayers[playerIndex];
		}

		return null;
	}

	public List<Card> GetCardsInPlay()
	{
			List<Card> retVal = new List<Card>();

			foreach (MasqueradePlayer p in MasqueradePlayers)
				foreach (byte b in p.Hand.CardsOwned)
					retVal.Add(TheCardIndex.GetCard(b));
			return retVal;
	}

	//This and Place in Turn Order are our only state variables
	bool gameIsOver = false;
	int actionsLeft = 0; //This is also a state variable :(

	#endregion

	#region UIClicks
	public void RequestButtonClickUI() //Server
	{
		StartGame();
	}

	public void DrawButtonClickUI() //Server
	{
		if (CurrentPlayer != null && OwningNetWorker.IsServer)
		{

			StartCoroutine(ChooseFacingCOR(TheDeck.Draw(), CurrentPlayer.PlayerIndex, false, null));
		}
	}

	public void TakeActionButtonClickUI() //Server
	{


	}



	public void TakeTurnButtonClickUI() //Server
	{
		if (CurrentPlayer != null && OwningNetWorker.IsServer)
		{
			StartCoroutine(TakeTurnCOR(CurrentPlayer.PlayerIndex, null));

		}
	}
	#endregion

	#region Advance or Take Turn
	public void AdvanceTurn()
	{
		if (OwningNetWorker != null && OwningNetWorker.Connected && OwningNetWorker.IsServer)
		{
			PlaceInTurnOrder++;
			if (PlaceInTurnOrder >= MasqueradePlayers.Count)
			{
				PlaceInTurnOrder = 0;
			}

			UpdateWhoseTurn();
		}
	}

	public void UpdateWhoseTurn()
	{

		if (OwningNetWorker != null && OwningNetWorker.Connected && OwningNetWorker.IsServer)
		{
			foreach (MasqueradePlayer p in MasqueradePlayers)
			{
				bool myTurn = (p == CurrentPlayer);
				AuthoritativeRPC("AlertTurnRPC", OwningNetWorker, p.NetworkingPlayer, false, myTurn, CurrentPlayer.PlayerIndex);
			}
		}
	}

	[BRPC]
	public void AlertTurnRPC(bool isMyTurn, byte playerNumber) //Called by Server on Client
	{
		if (isMyTurn)
		{
			CustomConsole.Log("It's my turn!", Color.green);
			ActionsRemainingDisplay.SetColors(Color.white, Color.gray);
		}
		else
		{
			CustomConsole.Log("It's player " + playerNumber.ToString() + "'s turn. (Not mine.)", Color.green);
			ActionsRemainingDisplay.SetColors(Color.black, Color.gray);
		}
	}

	IEnumerator TakeTurnCOR(byte playerIndex, byte? returnIndex)
	{

		bool bonusDrawFinished = true;
		bool drawIsFinished = false;
		actionsLeft = 3;
		Response<bool> actionIsFinished = null;
		//START PHASE

		//Start Timer
		{
			Timer.SetMainTimer(60, delegate
			{
				CustomConsole.Log("Player timed out. Ending their turn.");
				actionsLeft = 0;

			});
		}

		StaticEffects();




		Dictionary<byte, Trigger> StartPhaseTriggers = new Dictionary<byte, Trigger>();
		foreach(Card c in GetCardsInPlay())
		{
			if (c.Logic is IStartPhase)
			{
				if (c.Logic.TriggerIsPlausible(CurrentPlayer))
				{
					Trigger newTrigger = new Trigger(c.Logic);
					newTrigger.Resolution = () =>
					{
						((IStartPhase)newTrigger.Source).OnStartPhase(CurrentPlayer,
						() => newTrigger.PostResolution());
					};

					StartPhaseTriggers.Add((byte)c.Index, newTrigger);
				}
			}

			List<Buff> parasites = c.GetBuffs(Keyword.START_PHASE_PARASITE);
			foreach(Buff b in parasites)
			{
				Trigger newTrigger = new Trigger(b.Source);

				newTrigger.Resolution = () =>
				{
					((IStartPhaseParasite)newTrigger.Source).OnStartPhaseParasite(c,
					() => newTrigger.PostResolution());
				};

				StartPhaseTriggers.Add((byte)newTrigger.Source.Card.Index, newTrigger);
			}
				
				
		}


		Response<bool> startPhaseTriggersResponse = coroutineReturns.Add();

		StartCoroutine(HandleTriggerStack2COR(startPhaseTriggersResponse, StartPhaseTriggers));

		/*StartCoroutine(HandleTriggerStackCOR(playerIndex, startPhaseTriggersResponse, StartPhaseTriggers,
			delegate (CardLogic c, Response<bool> response)
			{
				((IStartPhase)c).OnStartPhase(CurrentPlayer, response);
			}
			));*/

		while (startPhaseTriggersResponse.FlagWaiting)
			yield return null;

		foreach (byte b in UsedHands[playerIndex].CardsOwned)
		{
			TheCardIndex.GetCard(b).Untap();
		}




		//DRAW PHASE
		StaticEffects();

		//Figure out if we get a draw phase
		bool willDrawThisTurn = true;
		foreach(byte b in UsedHands[playerIndex].CardsOwned)
		{
			Card c = TheCardIndex.GetCard(b);
			if (c.IsFaceUp && c.HasKeyword(Keyword.SKIP_DRAW_PHASE))
				willDrawThisTurn = false;
		}

		if(willDrawThisTurn)
		{
			//Bonus 
			if (UsedHands[playerIndex].CardsOwned.Count == 0)
			{
				bonusDrawFinished = false;
				StartCoroutine(DrawCardCOR(playerIndex, 1, delegate { bonusDrawFinished = true; }));
			}

			//Normal 
			StartCoroutine(DrawCardCOR(playerIndex, 1, delegate { drawIsFinished = true; }));
			while (!bonusDrawFinished || !drawIsFinished)
				yield return null;
		}
		



		//ACTION PHASE

		StaticEffects();

		while (actionsLeft > 0)
		{
			UpdateActionsLeft(actionsLeft);

			actionIsFinished = coroutineReturns.Add();

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

		//Discard

		StaticEffects();
		int discardsRequired = UsedHands[playerIndex].CardsOwned.Count - 6;

		if (discardsRequired > 0)
		{
			//DISCARD GOES HERE
		}

		//Cleanup



		StaticEffects();

		Dictionary<byte, Trigger> EndPhaseTriggers = new Dictionary<byte, Trigger>();
		foreach (Card c in GetCardsInPlay())
		{
			if (c.Logic is IEndPhase)
			{
				if (c.Logic.TriggerIsPlausible(CurrentPlayer))
				{
					Trigger newTrigger = new Trigger(c.Logic);
					newTrigger.Resolution = () =>
					{
						((IEndPhase)newTrigger.Source).OnEndPhase(CurrentPlayer,
						() => newTrigger.PostResolution());
					};

					EndPhaseTriggers.Add((byte)c.Index, newTrigger);
				}
			}
			
		}


		Response<bool> endPhaseTriggersResponse = coroutineReturns.Add();

		StartCoroutine(HandleTriggerStack2COR(endPhaseTriggersResponse, EndPhaseTriggers));

		while (endPhaseTriggersResponse.FlagWaiting)
			yield return null;

		StaticEffects();


		//Untap
		foreach (byte b in UsedHands[playerIndex].CardsOwned)
		{
			TheCardIndex.GetCard(b).Untap();
		}

		StaticEffects();
		if (returnIndex != null)
			coroutineReturns.Responses[(byte)returnIndex].Fill(false); //this return isn't used for anything anymore


	}
	#endregion

	#region Static Effects

	public void StaticEffects()
	{
		//APNAP order for cards in play
		for (int i = 0; i < MasqueradePlayers.Count; i++)
		{
			IndexHand apnapHand = GetPlayer(PlaceInTurnOrder + i).Hand;
			foreach (byte b in apnapHand.CardsOwned)
			{
				Card c = TheCardIndex.GetCard(b);

				if (c.GetCombatDefense() <= 0)
					c.Kill();

				if(c.Logic is IStaticEffect)
				{
					((IStaticEffect)c.Logic).StaticEffect();
				}
			}
		}

		//Random Order for everything else?
		


	}

	#endregion

	#region Trigger Stacking

	/*
	public IEnumerator HandleTriggerStackCOR(byte playerIndex, Response<bool> coroutineReturn, List<CardLogic> allLogics, System.Action<CardLogic, Response<bool>> onTrigger, bool mainTimerBound = true)
	{

		List<CardLogic> allPlausibleLogics = new List<CardLogic>();

		foreach (CardLogic c in allLogics)
			if (c.TriggerIsPlausible(MasqueradePlayers[playerIndex]))
				allPlausibleLogics.Add(c);

		bool dumpMode = false;


		GameTimer.TimerDelegate timeout;
		if(mainTimerBound)
		{
			timeout = Timer.AddMainTimerDelegate(delegate
			{
				CustomConsole.Log("Trigger stacking request out. Triggers in a random order.");
				dumpMode = true;
			});
		}
		else
		{
			timeout = Timer.AddSubTimerDelegate(delegate
			{
				CustomConsole.Log("Trigger stacking request out. Triggers in a random order.");
				dumpMode = true;
			});

		}




		while (allPlausibleLogics.Count > 0)
		{
			Response<byte> cardResponse = cardRequestResponses.Add();


			if (allPlausibleLogics.Count > 1 &! dumpMode)
			{
				byte[] allTargets = new byte[allPlausibleLogics.Count];
				for (int i = 0; i < allPlausibleLogics.Count; i++)
				{
					allTargets[i] = (byte)allPlausibleLogics[i].Card.Index;
				}


				AuthoritativeRPC("RequestChooseCardRPC", OwningNetWorker, MasqueradePlayers[playerIndex].NetworkingPlayer, false, cardResponse.Index, allTargets, false, mainTimerBound, new Color(0.75f, 0, 1));

				while (cardResponse.FlagWaiting)
				{
					if (dumpMode)
						cardResponse.Fill((byte)allPlausibleLogics[Random.Range(0, allPlausibleLogics.Count)].Card.Index);
					else
						yield return null;
				}
			}
			else
			{
				cardResponse.Fill((byte)allPlausibleLogics[Random.Range(0, allPlausibleLogics.Count)].Card.Index);
			}


			CardLogic foundLogic = TheCardIndex.GetCard(cardResponse.Result).Logic;


			Response<bool> response = coroutineReturns.Add();
			onTrigger(foundLogic, response);

			while (response.FlagWaiting)
				yield return null;


			allPlausibleLogics.Remove(foundLogic);
			allLogics.Remove(foundLogic);
			cardResponse.Recycle();
			response.Recycle();

		}
		if(mainTimerBound)
			Timer.RemoveMainTimerDelegate(timeout);
		else
			Timer.RemoveSubTimerDelegate(timeout);

		coroutineReturn.Fill(true);
		yield return null;
	}
	*/

	public IEnumerator HandleTriggerStack2COR(Response<bool> coroutineReturn, Dictionary<byte, Trigger> triggers)
	{

		MasqueradePlayer apnapPlayer = CurrentPlayer;
		bool dumpMode = false;
		GameTimer.TimerDelegate timeout = Timer.AddMainTimerDelegate(() => dumpMode = true);

		while (triggers.Count > 0)
		{
			//Get the current player's triggers
			Dictionary<byte,Trigger> currentPlayerTriggers = new Dictionary<byte, Trigger>();
			foreach (KeyValuePair<byte, Trigger> t in triggers)
			{
				if (t.Value.Source.Card.LastOwner == apnapPlayer)
					currentPlayerTriggers.Add(t.Key, t.Value);
			}

			List<byte> options = new List<byte>();
			foreach (KeyValuePair<byte, Trigger> t in currentPlayerTriggers)
				options.Add(t.Key);

			//Figure out if the current player has a decision to make
			if (currentPlayerTriggers.Count > 1)
			{
				//Are we choosing at random?
				if(dumpMode)
				{
					byte randomChoice = options[Random.Range(0, options.Count)];

					bool choiceDone = false;
					currentPlayerTriggers[randomChoice].Resolve(() => choiceDone = true);

					triggers.Remove(randomChoice);

					while (!choiceDone)
						yield return null;

				}
				else
				{
					//We have a decision to make


					bool choiceMade = false;
					StartCoroutine(PickACardCOR(
						apnapPlayer.PlayerIndex,
						options.ToArray(),
						(choice) =>
						{
							if (options.Contains(choice))
							{
								//Resolve that choice
								currentPlayerTriggers[choice].Resolve(() => choiceMade = true);
								//Remove it from the dictionary
								triggers.Remove(choice);

							}
							else
							{
								//We failed the sanity check. we'll just try again
								choiceMade = true;
							}
						},
						(() => dumpMode = true),
						Color.magenta,
						apnapPlayer == CurrentPlayer
						));

					while (!choiceMade && !dumpMode)
						yield return null;

				}

			}
			else
			{
				//We have one trigger
				if(currentPlayerTriggers.Count == 1)
				{
					//Resolve it
					byte choice = currentPlayerTriggers.First().Key;

					bool choiceDone = false;
					currentPlayerTriggers[choice].Resolve(() => choiceDone = true);
					//Remove it from the dictionary
					currentPlayerTriggers.Remove(choice);
					triggers.Remove(choice);

					while (!choiceDone)
						yield return null;
				}
				else //We have no triggers
				{
					MasqueradePlayer nextPlayer = NextPlayer(apnapPlayer);

					Timer.CancelSubTimer();
					if (nextPlayer != CurrentPlayer)
					{
						Timer.PauseMainTimer();
						Timer.RemoveMainTimerDelegate(timeout);
						Timer.RemoveSubTimerDelegate(timeout);
						timeout = Timer.StartSubTimer(15, () => dumpMode = true);
					}
					else
					{
						Timer.ResumeMainTimer();
						Timer.RemoveMainTimerDelegate(timeout);
						Timer.RemoveSubTimerDelegate(timeout);
						timeout = Timer.AddMainTimerDelegate(() => dumpMode = true);
					}
					apnapPlayer = nextPlayer;
					dumpMode = false;
				}
			}

		}
		Timer.RemoveMainTimerDelegate(timeout);
		Timer.RemoveSubTimerDelegate(timeout);
		Timer.CancelSubTimer();
		Timer.ResumeMainTimer();
		coroutineReturn.Fill(true);
	}

	#endregion

	#region Pick a Card

	public IEnumerator PickACardCOR(byte playerIndex, byte[] allChoices, System.Action<byte> onChoice, System.Action onCancel, Color color, bool mainTimerBound = true)
	{
		Response<byte> response = cardRequestResponses.Add();
		


		GameTimer.TimerDelegate timeout = delegate
		{
			CustomConsole.Log("Pick-A-Card request timed out. Cancel if I can, otherwise pick at random.");
			if (onCancel == null)
				response.Fill(allChoices[Random.Range(0, allChoices.Length)]);
			else
				response.Fill(CardIndex.CANCEL_CHOICE);
		};


		if (mainTimerBound)
		{
			Timer.AddMainTimerDelegate(timeout);
		}
		else
		{
			Timer.AddSubTimerDelegate(timeout);
		}
			AuthoritativeRPC("RequestChooseCardRPC", OwningNetWorker, MasqueradePlayers[playerIndex].NetworkingPlayer, false, response.Index, allChoices, onCancel != null, mainTimerBound, color);

			while (response.FlagWaiting)
				yield return null;

		if (response.Result == CardIndex.CANCEL_CHOICE && onCancel != null)
			onCancel();
		else
			onChoice(response.Result);

		response.Recycle();

		if(mainTimerBound)
		{
			Timer.RemoveMainTimerDelegate(timeout);
		}
		else
		{
			Timer.RemoveSubTimerDelegate(timeout);
		}
		yield return null;
	}

	[BRPC]
	void RequestChooseCardRPC(byte choiceID, byte[] cards, bool canCancel, bool mainTimerBound, Color color)
	{
		List<byte> options = new List<byte>();
		options.AddRange(cards);


		if (canCancel)
		{
			CardChoiceMenu.GetChoice(
				options,
				color,
				delegate (byte choice)
				{
					CustomConsole.Log("Being asked to pick a card.");
					RPC("ResponseChooseCardRPC", NetworkReceivers.Server, choiceID, choice);
				},
				delegate
				{
					RPC("ResponseChooseCardRPC", NetworkReceivers.Server, choiceID, CardIndex.CANCEL_CHOICE);
				},
				null,
				mainTimerBound
				);
		}
		else
		{

			CardChoiceMenu.GetChoice(
				options,
				color,
				delegate (byte choice)
				{
					CustomConsole.Log("Being asked to pick a card.");
					RPC("ResponseChooseCardRPC", NetworkReceivers.Server, choiceID, choice);
				},
				null,
				null,
				mainTimerBound
				);
		}
	}

	[BRPC]
	void ResponseChooseCardRPC(byte choiceID, byte choice)
	{
		cardRequestResponses.Responses[choiceID].Fill(choice);
	}

	#endregion

	#region Pick a Player


	public IEnumerator PickAPlayerCOR(byte playerIndex, byte[] allChoices, System.Action<byte> onChoice, System.Action onCancel, Color color, bool mainTimerBound = true)
	{
		Response<byte> response = cardRequestResponses.Add();



		GameTimer.TimerDelegate timeout = delegate
		{
			CustomConsole.Log("Pick-A-Player request timed out. Cancel if I can, otherwise pick at random.");
			if (onCancel == null)
				response.Fill(allChoices[Random.Range(0, allChoices.Length)]);
			else
				response.Fill(CardIndex.CANCEL_CHOICE);
		};


		if (mainTimerBound)
		{
			Timer.AddMainTimerDelegate(timeout);
		}
		else
		{
			Timer.AddSubTimerDelegate(timeout);
		}

		AuthoritativeRPC("RequestChoosePlayerRPC", OwningNetWorker, MasqueradePlayers[playerIndex].NetworkingPlayer, false, response.Index, allChoices, onCancel != null, mainTimerBound, color);

		while (response.FlagWaiting)
			yield return null;

		if (response.Result == CardIndex.CANCEL_CHOICE && onCancel != null)
			onCancel();
		else
			onChoice(response.Result);

		response.Recycle();

		if (mainTimerBound)
		{
			Timer.RemoveMainTimerDelegate(timeout);
		}
		else
		{
			Timer.RemoveSubTimerDelegate(timeout);
		}

		yield return null;
	}

	[BRPC]
	void RequestChoosePlayerRPC(byte choiceID, byte[] cards, bool canCancel, bool mainTimerBound, Color color)
	{
		List<byte> options = new List<byte>();
		options.AddRange(cards);

		CustomConsole.Log("Being asked to pick a player.");

		if (canCancel)
		{
			ThePlayerChoiceMenu.GetChoice(
				options,
				color,
				delegate (byte choice)
				{
					CustomConsole.Log("Choosing player: " + choice);
					RPC("ResponseChoosePlayerRPC", NetworkReceivers.Server, choiceID, choice);
				},
				delegate
				{
					RPC("ResponseChoosePlayerRPC", NetworkReceivers.Server, choiceID, CardIndex.CANCEL_CHOICE);
				},
				mainTimerBound
				);
		}
		else
		{

			ThePlayerChoiceMenu.GetChoice(
				options,
				color,
				delegate (byte choice)
				{
					CustomConsole.Log("Choosing player: " + choice);
					RPC("ResponseChoosePlayerRPC", NetworkReceivers.Server, choiceID, choice);
				},
				null,
				mainTimerBound
				);
		}
	}

	[BRPC]
	void ResponseChoosePlayerRPC(byte choiceID, byte choice)
	{
		cardRequestResponses.Responses[choiceID].Fill(choice);
	}
	#endregion

	#region Draw Card

	public void DrawCardFacing(byte playerIndex, int cardsToDraw = 1, bool faceUp = false)
	{
		for (int i = 0; i < cardsToDraw; i++)
		{
			byte cardIndex = TheDeck.Draw();
			Card card = TheCardIndex.GetCard(cardIndex);
			card.Owner = MasqueradePlayers[playerIndex];
			card.IsAlive = true;
			card.IsFaceUp = faceUp;
			UpdateCardsInDeck(TheDeck.CardsRemaining);
			AddCardToBoards(playerIndex, cardIndex);
		}

	}

	public IEnumerator DrawCardCOR(byte playerIndex, int cardsToDraw = 1, System.Action<bool[]> callback = null)
	{
		bool[] response = null;
		bool responded = false;

		if(cardsToDraw == 1)
		{
			byte cardIndex = TheDeck.Draw();

			TheCardIndex.GetCard(cardIndex).Owner = MasqueradePlayers[playerIndex];

			TheCardIndex.GetCard(cardIndex).IsAlive = true;

			StartCoroutine(ChooseFacingCOR(cardIndex, playerIndex, false, 
				delegate (bool retVal) {
					response = new bool[] {retVal};
					responded = true;
				}));
		}
		else
		{
			List<byte> allDraws = new List<byte>();
			for (int i = 0; i < cardsToDraw; i++)
			{
				byte currentDraw = TheDeck.Draw();
				TheCardIndex.GetCard(currentDraw).Owner = MasqueradePlayers[playerIndex];
				TheCardIndex.GetCard(currentDraw).IsAlive = true;
				allDraws.Add(currentDraw);
			}
			StartCoroutine(ChooseMultiFacingCOR(allDraws.ToArray(), playerIndex, false, delegate (bool[] retVal) { response = retVal; responded = true; }));
		}
		
		UpdateCardsInDeck(TheDeck.CardsRemaining);

		while (!responded)
			yield return null;

		if (callback != null)
			callback(response);


		yield break;

	}
	
	//Draw a card system uses DrawCardCOR and the ChooseFacing RPCs
	public IEnumerator ChooseFacingCOR(byte cardIndex, byte playerIndex, bool simultaneousDraw, System.Action<bool> callback = null)
	{
		Color logColor = Color.yellow;
		CustomConsole.Log("Starting Draw Coroutine.", logColor);

		Response<bool> response = facingRequestResponses.Add();

		GameTimer.TimerDelegate timeout = delegate
		{
			CustomConsole.Log("Facing request timed out. Placing facedown.");
			response.Fill(false);

		};

		if (Timer.MainTimerDone)
		{
			timeout();
		}
		else
		{
			//Start Timer
			Timer.AddMainTimerDelegate(timeout);
			CustomConsole.Log("Choosing the facing of #" + cardIndex + ".", logColor);
			CustomConsole.Log("Sending RequestChooseFacingRPC", logColor);
			AuthoritativeRPC("RequestChooseFacingRPC", OwningNetWorker, GetPlayer(playerIndex).NetworkingPlayer, false, response.Index, cardIndex);
			while (!response.FlagCompleted)
				yield return null;
		}

		if (simultaneousDraw)
			TheCardIndex.GetCard(cardIndex).IsFaceUp = false;
		else
			TheCardIndex.GetCard(cardIndex).IsFaceUp = response.Result;

		SyncCard(TheCardIndex.GetCard(cardIndex));
		EnsureProperFlippedness(TheCardIndex.GetCard(cardIndex));
		CustomConsole.Log("Got a response. Calling AddCardToBoards and calling it a day.", logColor);
		AddCardToBoards(playerIndex, cardIndex);

		if (simultaneousDraw)
			TheCardIndex.GetCard(cardIndex).IsFaceUp = response.Result;

		response.Recycle();

		if (callback != null)
			callback(response.Result);

		Timer.RemoveMainTimerDelegate(timeout);

	}

	public IEnumerator ChooseMultiFacingCOR(byte[] cardIndices, byte playerIndex, bool simultaneousDraw, System.Action<bool[]> callback = null)
	{
		Color logColor = Color.yellow;
		
		Response<bool[]> response = multiFacingRequestResponses.Add();

		GameTimer.TimerDelegate timeout = delegate
		{
			List<bool> falseFill = new List<bool>();
			for (int i = 0; i < cardIndices.Length; i++)
			{
				falseFill.Add(false);
			}
			response.Fill(falseFill.ToArray());
		};

		if (Timer.MainTimerDone)
		{
			timeout();
		}
		else
		{
			//Start Timer
			Timer.AddMainTimerDelegate(timeout);
			AuthoritativeRPC("RequestChooseMultiFacingRPC", OwningNetWorker, GetPlayer(playerIndex).NetworkingPlayer, false, response.Index, cardIndices);
			while (!response.FlagCompleted)
				yield return null;
		}


		for (int i = 0; i < cardIndices.Length; i++)
		{
			byte cardIndex = cardIndices[i];

			if (simultaneousDraw)
				TheCardIndex.GetCard(cardIndex).IsFaceUp = false;
			else
				TheCardIndex.GetCard(cardIndex).IsFaceUp = response.Result[i];

			SyncCard(TheCardIndex.GetCard(cardIndex));
			EnsureProperFlippedness(TheCardIndex.GetCard(cardIndex));

			AddCardToBoards(playerIndex, cardIndex);

			if (simultaneousDraw)
				TheCardIndex.GetCard(cardIndex).IsFaceUp = response.Result[i];
		}
		

		response.Recycle();

		if (callback != null)
			callback(response.Result);

		Timer.RemoveMainTimerDelegate(timeout);

	}


	[BRPC]
	void RequestChooseFacingRPC(byte choiceID, byte index)//Called by server on client
	{
		CustomConsole.Log("Recieved a request (#" + choiceID + ") to choose the facing of " + index + ".", Color.blue);
		FaceUpChoiceMenu.GetChoice(index,
			delegate (bool[] choices)
			{
				CustomConsole.Log("Facing Choice Delegate Hit (#" + choiceID + "). Sending ResponseChooseFacingRPC", Color.blue);
				RPC("ResponseChooseFacingRPC", NetworkReceivers.Server, choiceID, choices[0]);
			}
			);
	}

	[BRPC]
	void ResponseChooseFacingRPC(byte choiceID, bool choice)
	{
		CustomConsole.Log("Recieved a response to my facing request (#" + choiceID + ") [" + choice + "].");
		Response<bool> response = facingRequestResponses.Responses[choiceID];
		if (response != null && response.FlagWaiting)
		{
			response.Fill(choice);
		}
	}

	[BRPC]
	void RequestChooseMultiFacingRPC(byte choiceID, byte[] indices)
	{
		FaceUpChoiceMenu.GetChoice(indices,
			delegate (bool[] choices)
			{

				List<byte> byteChoices = new List<byte>();
				for (int i = 0; i < choices.Length; i++)
				{
					byteChoices.Add(choices[i] ? (byte)1 : (byte)0);
				}
				CustomConsole.Log("Facing Choice Delegate Hit (#" + choiceID + "). Sending ResponseChooseFacingRPC", Color.blue);
				RPC("ResponseChooseMultiFacingRPC", NetworkReceivers.Server, choiceID, byteChoices.ToArray());
			}
			);
	}

	[BRPC]
	void ResponseChooseMultiFacingRPC(byte choiceID, byte[] choices)
	{
		Response<bool[]> response = multiFacingRequestResponses.Responses[choiceID];
		if (response != null && response.FlagWaiting)
		{
			List<bool> boolChoices = new List<bool>();
			for (int i = 0; i < choices.Length; i++)
			{
				boolChoices.Add(choices[i] > 0);
			}

			bool[] boolArray = boolChoices.ToArray();
			response.Fill(boolArray);
		}
	}
	#endregion

	#region Take Action
	//Action system uses TakeActionCOR and the TakeAction RPCs
	public IEnumerator TakeActionCOR(byte playerIndex, byte? returnIndex)
	{
		bool retVal = false;
		Color logColor = new Color(0, 0.5f, 1);
		CustomConsole.Log("Starting Take Action Coroutine.", logColor);

		Response<ActionDescriptor> response = actionRequestResponses.Add();


		//Start Timer
		GameTimer.TimerDelegate timeout = Timer.AddMainTimerDelegate(delegate
		{
			CustomConsole.Log("Facing request timed out. Taking default actions.");
			response.Fill(new ActionDescriptor(0, CardAction.PASS, 0));

		});

		CustomConsole.Log("Sending RequestTakeActionRPC", logColor);
		AuthoritativeRPC("RequestTakeActionRPC", OwningNetWorker, GetPlayer(playerIndex).NetworkingPlayer, false, response.Index);

		while (!response.FlagCompleted)
			yield return null;


		CustomConsole.Log("Got a response.", logColor);
		bool actionFinished = false;

		switch (response.Result.Type)
		{
			case CardAction.FLIP:
				CustomConsole.Log("Flipping card #" + response.Result.ActorIndex, logColor);
				Card cardToFlip = TheCardIndex.GetCard(response.Result.ActorIndex);

				if(cardToFlip.Owner == CurrentPlayer)
				{
					actionFinished = false;
					StartCoroutine(cardToFlip.FlipAction(() => { actionFinished = true; }));
					while (!actionFinished)
						yield return null;

				}
				else
				{
					//Sanity check failed
				}

				break;
			case CardAction.ATTACK:
				CustomConsole.Log("Card #" + response.Result.ActorIndex + " attacks Card #" + response.Result.TargetIndex, logColor);

				Card attacker = TheCardIndex.GetCard(response.Result.ActorIndex);
				Card defender = null;
				if (response.Result.TargetIndex < CardIndex.PLAYER_1_FACEDOWN)
				{
					defender = TheCardIndex.GetCard(response.Result.TargetIndex);
				}
				else if (response.Result.TargetIndex >= CardIndex.PLAYER_1_FACEDOWN && response.Result.TargetIndex <= CardIndex.PLAYER_4_FACEDOWN)
				{

					byte randomTargetIndex = UsedHands[response.Result.TargetIndex - CardIndex.PLAYER_1_FACEDOWN].RandomFaceDownCard;

					defender = TheCardIndex.GetCard(randomTargetIndex);
				}

				bool sanityCheckPassed = true;
				if (attacker.Owner != CurrentPlayer)
					sanityCheckPassed = false;
				if (!defender.OpenToAttack())
					sanityCheckPassed = false;
				if (attacker.Owner == defender.Owner)
					sanityCheckPassed = false;

				if (sanityCheckPassed)
				{
					actionFinished = false;
					StartCoroutine(attacker.AttackAction(defender, () => { actionFinished = true; }));
					while (!actionFinished)
						yield return null;
				}
				else
				{
					//If we failed the sanity check right now you don't get your action back
				}

				break;
			case CardAction.ACTIVATE:
				CustomConsole.Log("Activating card #" + response.Result.ActorIndex, logColor);

				actionFinished = false;
				Card cardToActivate = TheCardIndex.GetCard(response.Result.ActorIndex);
				if (cardToActivate.Owner == CurrentPlayer)
				{
					StartCoroutine(cardToActivate.ActivateAction(() => { actionFinished = true; }));
					while (!actionFinished)
						yield return null;
				}
				else
				{
					//Sanity check failed
				}
				break;
			case CardAction.PASS:
				CustomConsole.Log("Passing Turn.", logColor);
				retVal = true;
				if (returnIndex == null)
					AdvanceTurn();
				break;

		}

		response.Recycle();

		StaticEffects();

		if (returnIndex != null)
			coroutineReturns.Responses[(byte)returnIndex].Fill(retVal);
		Timer.RemoveMainTimerDelegate(timeout);
	}



	[BRPC]
	void RequestTakeActionRPC(byte choiceID)
	{

		CustomConsole.Log("Recieved a request (#" + choiceID + ") to take an action.");

		StartCoroutine(GenerateTakeActionResponseCOR(choiceID));


	}

	public IEnumerator GenerateTakeActionResponseCOR(byte choiceID)
	{
		CustomConsole.Log("Starting GenerateTakeActionResponseCOR.");

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

				firstCardChoice = CardChoiceMenu.GetChoice(UsedHands[MyPlayerNumber].UntappedCards, Color.blue,
				delegate (byte choice)
				{
					CustomConsole.Log("Got an actor. #" + choice);
					sendActorIndex = choice;
				},
				null,
				delegate
				{
					CustomConsole.Log("Passing turn.");
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
						CustomConsole.Log("Got a choice. " + choice.ToString());
						sendActionType = (byte)choice;
					},
					delegate
					{
						CustomConsole.LogError("Canceling action.");
						canceled = true;
						passed = true;
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

					for (int i = 0; i < UsedHands.Count; i++)
					{
						byte b = (byte)i;
						if (i != MyPlayerNumber)
						{
							if (UsedHands[i].FaceUpCards.Count > 0)
								potentialTargets.AddRange(UsedHands[i].FaceUpCards);
							else
								potentialTargets.Add(((byte)(CardIndex.PLAYER_1_FACEDOWN + b)));
						}
					}

					secondCardChoice = CardChoiceMenu.GetChoice(potentialTargets, Color.yellow,
					   delegate (byte choice)
					   {
						   CustomConsole.Log("Got a target. #" + choice);
						   sendTargetIndex = choice;
					   },


						delegate
						{
							CustomConsole.LogError("Canceling attack.");
							canceled = true;
							passed = true;
						},
					   null);
				}

				while (!passed && needsATarget && sendTargetIndex == CardIndex.EMPTY_SLOT)
					yield return null;
			}

		} while (canceled == true);


		CustomConsole.Log("Got everything I need. Sending ResponseTakeActionRPC.");
		RPC("ResponseTakeActionRPC", NetworkReceivers.Server, sendChoiceID, sendActorIndex, sendActionType, sendTargetIndex);


	}

	[BRPC]
	void ResponseTakeActionRPC(byte choiceID, byte actorIndex, byte actionType, byte targetIndex)
	{

		CustomConsole.Log("Recieved a response to my Take Action request (#" + choiceID + ")");
		CustomConsole.Log(choiceID, Color.magenta);
		CustomConsole.Log(actorIndex, Color.magenta);
		CustomConsole.Log(actionType, Color.magenta);
		CustomConsole.Log(targetIndex, Color.magenta);
		Response<ActionDescriptor> response = actionRequestResponses.Responses[choiceID];
		if (response != null && response.FlagWaiting)
		{
			response.Fill(new ActionDescriptor(actorIndex, (CardAction)actionType, targetIndex));
		}
	}

	public void RefundAction()
	{
		actionsLeft++;
	}
	#endregion

	#region Add or Flip Card
	public void AddCardToBoards(byte targetPlayer, byte targetIndex)
	{

		if (CurrentPlayer != null && OwningNetWorker.IsServer)
		{
			Card card = TheCardIndex.GetCard(targetIndex);
			byte targetSlot = UsedHands[targetPlayer].FirstOpenSlot;

			card.CurrentSlot = targetSlot;

			AddCardToBoards(targetPlayer, targetIndex, targetSlot);
		}
	}

	public void AddCardToBoards(byte targetPlayer, byte targetIndex, byte targetSlot)
	{
		if (CurrentPlayer != null && OwningNetWorker.IsServer)
		{

			Card card = TheCardIndex.GetCard(targetIndex);
			SetPlayerSlotIndexRPC(targetPlayer, targetSlot, targetIndex);
			foreach (MasqueradePlayer p in MasqueradePlayers)
			{
				if (p == GetPlayer(targetPlayer) || card.IsFaceUp)
					AuthoritativeRPC("SetPlayerSlotIndexRPC", OwningNetWorker, p.NetworkingPlayer, false, targetPlayer, targetSlot, targetIndex);

				else
					AuthoritativeRPC("SetPlayerSlotIndexRPC", OwningNetWorker, p.NetworkingPlayer, false, targetPlayer, targetSlot, (byte)(targetPlayer + CardIndex.PLAYER_1_FACEDOWN));
			}

			StaticEffects();
		}
	}

	[BRPC]
	public void SetPlayerSlotIndexRPC(byte player, byte slot, byte index)
	{
		UsedHands[player].SetIndex(slot, index);
	}

	public void SyncCard(Card card)
	{
		if (card.IsFaceUp || card.Owner == null)
		{
			//Sync with everybody
			foreach (MasqueradePlayer p in MasqueradePlayers)
			{
				AuthoritativeRPC("SyncRPC", OwningNetWorker, p.NetworkingPlayer, false, 
					card.IsFaceUp,
					card.IsTapped,
					card.TotalAttackBuff,
					card.TotalDefenseBuff,
					card.Index);
			}
		}
		else
		{
			//Sync with the owner

			AuthoritativeRPC("SyncRPC", OwningNetWorker, card.Owner.NetworkingPlayer, false, 
				card.IsFaceUp,
				card.IsTapped,
				card.TotalAttackBuff,
				card.TotalDefenseBuff,
				card.Index);
		}
	}

	[BRPC]
	void SyncRPC(bool shouldBeFaceup, bool shouldBeTapped, int totalAttackBuff, int totalDefenseBuff, byte index)
	{
		Card card = TheCardIndex.GetCard(index);
		card.IsFaceUp = shouldBeFaceup;
		card.IsTapped = shouldBeTapped;
		card.Buffs.Clear();
		card.AddBuff(totalAttackBuff, totalDefenseBuff, false, false, null);
	}

	public void EnsureProperFlippedness(Card card)
	{
		if (card.Index != null && card.CurrentSlot != null)
		{
			AddCardToBoards(card.Owner.PlayerIndex, (byte)card.Index, (byte)card.CurrentSlot);
		}
	}

	public void GiveControl(byte targetPlayer, byte targetIndex)
	{
		if (CurrentPlayer != null && OwningNetWorker.IsServer)
		{
			Card card = TheCardIndex.GetCard(targetIndex);
			byte slotToNeutralize = (byte)card.CurrentSlot;


			card.Owner = null;
			card.CurrentSlot = null;
			
			NeutralizeCardRPC(targetIndex);

			foreach (MasqueradePlayer p in MasqueradePlayers)
			{
				if(p == card.LastOwner || card.IsFaceUp)
					AuthoritativeRPC("NeutralizeCardRPC", OwningNetWorker, p.NetworkingPlayer, false, targetIndex);
				else
					AuthoritativeRPC("NeutralizeFacedownRPC", OwningNetWorker, p.NetworkingPlayer, false, card.LastOwner.PlayerIndex, slotToNeutralize);

			}

			card.Owner = MasqueradePlayers[targetPlayer];

			AddCardToBoards(targetPlayer, targetIndex);
			card.SyncFlip();
		}
	}

	[BRPC]
	void NeutralizeCardRPC(byte cardIndex)
	{
		CustomConsole.Log("Neutralizing #" + cardIndex, Color.cyan);
		foreach (IndexHand h in UsedHands)
		{
			h.RemoveIndex(cardIndex);
		}
	}

	[BRPC]
	void NeutralizeFacedownRPC(byte playerIndex, byte slotIndex)
	{
		CustomConsole.Log("Neutralizing Slot#" + slotIndex, Color.cyan);
		UsedHands[playerIndex].RemoveFacedown(slotIndex);
	}




	#endregion

	#region Start Game
	public void StartGame()
	{
		CustomConsole.Log("Starting the game.");
		StartCoroutine(StartGameCOR());


	}

	IEnumerator StartGameCOR()
	{

		List<NetworkingPlayer> shuffledPlayers = new List<NetworkingPlayer>();
		shuffledPlayers.AddRange(OwningNetWorker.Players);
		shuffledPlayers = HelperFunctions.Shuffle(shuffledPlayers);


		MasqueradePlayers.Clear();
		for (byte j = 0; j < OwningNetWorker.Players.Count; j++)
		{
			NetworkingPlayer player = shuffledPlayers[j];
			byte positionInTurnOrder = j;

			MasqueradePlayers.Add(new MasqueradePlayer(player, positionInTurnOrder, (PlayerIdentity)player.PlayerObject, UsedHands[j]));
		}



		byte i = 0;
		bool fourPlayers = OwningNetWorker.Players.Count > 2;
		foreach (MasqueradePlayer p in MasqueradePlayers)
		{
			AuthoritativeRPC("ConfigureHandsRPC", OwningNetWorker, p.NetworkingPlayer, false, fourPlayers, i);
			i++;
		}

		foreach (MasqueradePlayer p in MasqueradePlayers)
		{
			RPC("LinkHandIdentityRPC", p.PlayerIndex, p.Identity.PlayerNumber);
		}

		foreach (MasqueradePlayer p in MasqueradePlayers)
		{
			p.Identity.SetIndex(p.PlayerIndex);
		}

		UpdateWhoseTurn();

		TheDeck.SetIndex(TheCardIndex);
		TheDeck.Shuffle();
		TheDeck.Shuffle(); //For Luck
		if (TheDeck.ShouldStack)
			TheDeck.Stack();

		Timer.SetMainTimer(15, delegate
		{
			CustomConsole.Log("Timeout during Start Game.");
		});

		List<bool> allDrawResponses = new List<bool>();
		List<Card> allDrawnCards = new List<Card>();

		/*for (int j = 0; j < 3; j++)
		{
			foreach (MasqueradePlayer p in MasqueradePlayers)
			{
				System.Action wrapper = () =>
				{
					int responseIndex = allDrawResponses.Count;
					allDrawResponses.Add(false);
					byte drawIndex = TheDeck.Draw();
					byte playerIndex = p.PlayerIndex;
					TheCardIndex.GetCard(drawIndex).Owner = MasqueradePlayers[playerIndex];

					TheCardIndex.GetCard(drawIndex).IsAlive = true;

					allDrawnCards.Add(TheCardIndex.GetCard(drawIndex));
					StartCoroutine(ChooseFacingCOR(drawIndex, p.PlayerIndex, true, delegate {
						allDrawResponses[responseIndex] = true;
					}));
				};
				wrapper();

			}
		}*/

		foreach (MasqueradePlayer p in MasqueradePlayers)
		{
			System.Action wrapper = () =>
			{
				int responseIndex = allDrawResponses.Count;
				allDrawResponses.Add(false);
				byte playerIndex = p.PlayerIndex;

				byte[] draws = new byte[3];

				for (int j = 0; j < draws.Length; j++)
				{
					byte drawIndex = TheDeck.Draw();

					TheCardIndex.GetCard(drawIndex).Owner = MasqueradePlayers[playerIndex];

					TheCardIndex.GetCard(drawIndex).IsAlive = true;

					allDrawnCards.Add(TheCardIndex.GetCard(drawIndex));

					draws[j] = drawIndex;
				}

				StartCoroutine(ChooseMultiFacingCOR(draws, p.PlayerIndex, true, delegate {
					allDrawResponses[responseIndex] = true;
				}));
			};
			wrapper();

		}

		while (allDrawResponses.Contains(false))

			yield return null;

		foreach (Card c in allDrawnCards)
			c.SyncFlip();



		while (!gameIsOver)
		{
			Response<bool> turnResponse = coroutineReturns.Add();

			StartCoroutine(TakeTurnCOR(CurrentPlayer.PlayerIndex, turnResponse.Index));

			while (turnResponse.FlagWaiting)
				yield return null;

			//gameIsOver = turnResponse.Result;
			turnResponse.Recycle();

			if (!gameIsOver)
				AdvanceTurn();
		}

		CustomConsole.Log("Game over!", Color.yellow);

	}


	[BRPC]
	public void ConfigureHandsRPC(bool fourPlayers, byte playerNumber)
	{
		CustomConsole.Log("Configuring hands. I'm playerNumber " + playerNumber);
		MyPlayerNumber = playerNumber;
		UsedHands.Clear();
		if (fourPlayers)
		{
			//You on bottom, other players clockwise. If there's an empty seat, it will be player 4, to the left of player 1


			for (int i = 0; i < 4; i++)
			{

				ClockwiseHands[HelperFunctions.MathMod(i - playerNumber, 4)].PlayerNumber = (byte)i;
				UsedHands.Add(ClockwiseHands[HelperFunctions.MathMod(i - playerNumber, 4)]);
			}

		}
		else
		{
			//You on the bottom, them on top.
			if (playerNumber == 0)
			{
				UsedHands.Add(ClockwiseHands[0]);
				UsedHands.Add(ClockwiseHands[2]);
				UsedHands.Add(ClockwiseHands[1]);
				UsedHands.Add(ClockwiseHands[3]);

				ClockwiseHands[0].PlayerNumber = 0;
				ClockwiseHands[1].PlayerNumber = 2;
				ClockwiseHands[2].PlayerNumber = 1;
				ClockwiseHands[3].PlayerNumber = 3;
			}
			else
			{
				UsedHands.Add(ClockwiseHands[2]);
				UsedHands.Add(ClockwiseHands[0]);
				UsedHands.Add(ClockwiseHands[3]);
				UsedHands.Add(ClockwiseHands[1]);

				ClockwiseHands[0].PlayerNumber = 2;
				ClockwiseHands[1].PlayerNumber = 0;
				ClockwiseHands[2].PlayerNumber = 3;
				ClockwiseHands[3].PlayerNumber = 1;
			}
		}


	}

	[BRPC]
	public void LinkHandIdentityRPC(byte hand, int identity)
	{
		CustomConsole.Log("LinkHandIdentityRPC");
		UsedHands[hand].AttachedIdentityRenderer.Identity = PlayerIdentities[identity];
		//UsedHands[hand].AttachedIdentityRenderer.Identity.Index = MasqueradePlayers[identity].PlayerIndex;

	}

	#endregion

	#region End Game
	public void EndGame(MasqueradePlayer winner)
	{
		CustomConsole.Log("EndGame hit. " + winner.Identity.name + " wins!");
		gameIsOver = true;

		foreach (MasqueradePlayer p in MasqueradePlayers)
		{
			foreach(byte index in p.Hand.CardsOwned)
			{
				Card c = TheCardIndex.GetCard(index);
				c.IsFaceUp = true;
				SyncCard(c);
				EnsureProperFlippedness(c);
			}

			if (p == winner)
				AuthoritativeRPC("ShowYouWon", OwningNetWorker, p.NetworkingPlayer, false, true);
			else
				AuthoritativeRPC("ShowYouWon", OwningNetWorker, p.NetworkingPlayer, false, false);

		}
	}

	[BRPC]
	public void ShowYouWon(bool didYouWin)
	{
		TheEndGameScreen.ShowYouWon(didYouWin);
	}

	#endregion

	#region Send To Discard
	public void SendToDiscard(Card card)
	{
		card.ForgetHistory();
		SendToDiscardRPC((byte)card.Index);


		foreach (MasqueradePlayer p in MasqueradePlayers)
		{
			AuthoritativeRPC("SendToDiscardRPC", OwningNetWorker, p.NetworkingPlayer, false, card.Index);
		}
	}

	[BRPC]
	void SendToDiscardRPC(byte cardIndex)
	{
		CustomConsole.Log("Sending " + TheCardIndex.GetCard(cardIndex).CardName + " to the discard.", Color.red);

		foreach (IndexHand h in UsedHands)
		{
			h.RemoveIndex(cardIndex);
		}
		TheDiscardPile.AddIndex(cardIndex);

	}


	#endregion

	#region ShuffleAway

	public void ShuffleAway(Card card)
	{
		card.ForgetHistory();
		TheDeck.ShuffleAway((byte)card.Index);

		ShuffleAwayRPC((byte)card.Index);


		foreach (MasqueradePlayer p in MasqueradePlayers)
		{
			AuthoritativeRPC("SendToDiscardRPC", OwningNetWorker, p.NetworkingPlayer, false, card.Index);
		}
	}

	[BRPC]
	void ShuffleAwayRPC(byte cardIndex)
	{
		CustomConsole.Log("Shuffling " + TheCardIndex.GetCard(cardIndex).CardName + " into the deck.", Color.cyan);

		foreach (IndexHand h in UsedHands)
		{
			h.RemoveIndex(cardIndex);
		}
		TheDiscardPile.RemoveIndex(cardIndex);

	}
	#endregion

	#region Update Display Values
	public void UpdateActionsLeft(int actionsLeft)
	{
		foreach (MasqueradePlayer p in MasqueradePlayers)
		{
			AuthoritativeRPC("UpdateActionsLeftRPC", OwningNetWorker, p.NetworkingPlayer, false, actionsLeft);
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
		foreach (MasqueradePlayer p in MasqueradePlayers)
		{
			AuthoritativeRPC("UpdateCardsInDeckRPC", OwningNetWorker, p.NetworkingPlayer, false, cardsInDeck);
		}
		UpdateCardsInDeckRPC(cardsInDeck);
	}


	[BRPC]
	public void UpdateCardsInDeckRPC(int cardsInDeck)
	{
		CardsInDeckDisplay.SetValue(cardsInDeck);
	}
	#endregion



	protected override void NetworkStart()
	{
		base.NetworkStart();

		CustomConsole.Log("GameplayNetworking.NetworkStart()");
		ServerControlPanel.SetActive(OwningNetWorker.IsServer);

	}

	public void Start()
	{
		for (byte i = 0; i < ClockwiseHands.Count; i++)
		{
			UsedHands.Add(ClockwiseHands[i]);
			ClockwiseHands[i].PlayerNumber = i;
		}

	}

	

}
