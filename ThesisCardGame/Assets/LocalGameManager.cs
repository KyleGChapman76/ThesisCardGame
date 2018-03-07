using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LocalGameManager : MonoBehaviour, IGameManager
{
	public GameUIManager uiManager;

	public GameObject offlinePlayerPrefab;

	public static System.Random rand = new System.Random();

	public static event AITurnBegins aiTurnBegins;
	public delegate void AITurnBegins(GameUIManager gameManager);

	private OfflineTCGPlayer localPlayer;
	private OfflineTCGPlayer opponentPlayer;

	public static Library hardCodedLibrary;

	private bool isPlayersTurn;

	public static LocalGameManager SingletonGameManager;

	public NetworkedGameManager ServerGameManager
	{
		set
		{
			serverGameManager = value;
        }
	}
	private NetworkedGameManager serverGameManager;

	//start the local game logic
	//if find a network manager, wait for all players to load in to then initialize multiplayer logic
	//otherwise, setup players
	void Start ()
	{
		Debug.Log("GameManager began.");

		SingletonGameManager = this;

		InitializeCardsDatabase();
		InitializeHardCodedLibrary();

		//if multiplayer game, wait for players to load in
		if (!GameObject.FindObjectOfType<NetworkManager>())
		{
			GameObject singlePlayerObject = Instantiate(offlinePlayerPrefab);
			localPlayer = singlePlayerObject.GetComponent<OfflineTCGPlayer>();

			GameObject aiOpponentObject = Instantiate(offlinePlayerPrefab);
			opponentPlayer = aiOpponentObject.GetComponent<OfflineTCGPlayer>();

			localPlayer.InitializePlayer(hardCodedLibrary);
			opponentPlayer.InitializeOpponent(hardCodedLibrary);

			isPlayersTurn = true;

			uiManager.gameManager = this;
			uiManager.UpdateUI(localPlayer, opponentPlayer);
			uiManager.InitializeLocalTurnUI();
        }
	}

	//initialze the list of cards
	private void InitializeCardsDatabase()
	{
		Debug.Log("Beginning the game.");

		//define all game cards
		new ResourceCardDefinition("Resource 1", 1, 1);
		new ResourceCardDefinition("Resource 2", 1, 2);
		new CreatureCardDefinition("Creature 1", 2, "Flight", 2, 2);
		new CreatureCardDefinition("Creature 2", 4, "Overrun", 3, 4);
		new CreatureCardDefinition("Creature 3", 3, "Speed", 2, 3);
		new SpellCardDefinition("Spell 1", 1, "Destroy target creature.");
		new SpellCardDefinition("Spell 2", 2, "You gain 4 life.");
		new SpellCardDefinition("Spell 3", 3, "Target opponent loses 4 life.");
	}

	private void InitializeHardCodedLibrary()
	{
		Deck hardcodedDeck = new Deck(30, 60, 4);

		hardcodedDeck.AddCard(0, 4);
		hardcodedDeck.AddCard(1, 4);
		hardcodedDeck.AddCard(2, 4);
		hardcodedDeck.AddCard(3, 4);
		hardcodedDeck.AddCard(4, 4);
		hardcodedDeck.AddCard(5, 4);
		hardcodedDeck.AddCard(6, 4);
		hardcodedDeck.AddCard(7, 4);

		int[] cardList;

		if (!hardcodedDeck.ExportDeckToArray(out cardList))
		{
			Debug.LogError("Can't export hardcoded deck.");
			Application.Quit();
			return;
		}

		hardCodedLibrary = new Library(cardList);
	}

	private void Update()
	{
		//check for debug button presses
		if (Input.GetKeyUp(KeyCode.R) && localPlayer != null)
		{
			localPlayer.ResetResources();
		}

		if (Input.GetKeyUp(KeyCode.D) && localPlayer != null)
		{
			localPlayer.DrawCard();
		}
	}

	public bool TryEndTurn()
	{
		if (serverGameManager)
		{
			return serverGameManager.TryEndTurn();
        }

		if (isPlayersTurn)
		{
			isPlayersTurn = false;
			localPlayer.TryEndTurn();
			opponentPlayer.TryStartTurn();
			aiTurnBegins(uiManager);
			uiManager.InitializeOpponentTurnUI();
		}
		else
		{
			isPlayersTurn = true;
			opponentPlayer.TryEndTurn();
			localPlayer.TryStartTurn();
			uiManager.InitializeLocalTurnUI();
		}
		return true;
	}

	public bool TryPlayCard(Card card)
	{
		if (serverGameManager)
		{
			return serverGameManager.TryPlayCard(card);
		}
		return localPlayer.TryPlayCard(card);
	}

	public void PlayerStatusChanged()
	{
		uiManager.UpdateUI(localPlayer, opponentPlayer);
	}
}
