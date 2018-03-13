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

	//initialze the list of cards
	static LocalGameManager()
	{
		Debug.Log("Initialize cards database.");

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

	//start the local game logic
	//if find a network manager, wait for all players to load in to then initialize multiplayer logic
	//otherwise, setup players
	void Start ()
	{
		Debug.Log("GameManager began.");

		SingletonGameManager = this;

		InitializeHardCodedLibrary();

		uiManager.gameManager = this;

		//if game is not multiplayer, spawn players and begin the UI
		if (!GameObject.FindObjectOfType<NetworkManager>())
		{
			StartCoroutine("InitializeGameAfterSmallTime");
        }
		//otherwise, do nothing!
	}

	private IEnumerator InitializeGameAfterSmallTime()
	{
		yield return new WaitForSeconds(.1f);

		Debug.Log("Starting single player game.");
		GameObject singlePlayerObject = Instantiate(offlinePlayerPrefab);
		localPlayer = singlePlayerObject.GetComponent<OfflineTCGPlayer>();

		GameObject aiOpponentObject = Instantiate(offlinePlayerPrefab);
		opponentPlayer = aiOpponentObject.GetComponent<OfflineTCGPlayer>();

		localPlayer.InitializePlayer(hardCodedLibrary, false);
		opponentPlayer.InitializePlayer(hardCodedLibrary, true);

		isPlayersTurn = true;
		uiManager.UpdateUI(localPlayer, opponentPlayer);
		uiManager.InitializeLocalTurnUI();
	}

	private void InitializeHardCodedLibrary()
	{
		Debug.Log("Initializing hard coded library.");

		Deck hardcodedDeck = new Deck(30, 60);

		hardcodedDeck.AddCard(0, 8);
		hardcodedDeck.AddCard(1, 8);
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

	public bool LocalEndTurn()
	{
		if (serverGameManager)
		{
			return serverGameManager.LocalEndTurn();
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

	public void ReportGameLoss(bool localPlayerLost)
	{
		uiManager.InitializeGameOverUI(!localPlayerLost);
    }
}
