using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ClientSideGameManager : MonoBehaviour
{
	public const int STARTING_HAND_SIZE = 6;
	public const int MAX_HAND_SIZE = 8;
	public const int MAX_MAX_RESOURCES_PER_TURN = 12;
	public const int STARTING_MAX_RESOURCES_PER_TURN = 2;

	public GameObject offlinePlayerPrefab;

	private bool multiplayerGame;

	public ICardGamePlayer localPlayer;
	public ICardGamePlayer opponentPlayer;

	public PlayerHandRenderer localHandRenderer;
	public OpponentHandRenderer opponentHandRenderer;
	public ResourcesRenderer localResourcesRenderer;
	public ResourcesRenderer opponentResourcesRenderer;

	public static System.Random rand = new System.Random();

	public Button endTurnButton;

	public static event AITurnBegins aiTurnBegins;
	public delegate void AITurnBegins(ClientSideGameManager gameManager);

	//0 = local player, or single player in SP
	//1 = opponent player, or AI oppponent in SP
	public int currentTurn;

	private void Start()
	{
		Debug.Log("GameManager began.");

		//if multiplayer game, wait for players to load in
		if (GameObject.FindObjectOfType<NetworkManager>())
		{
			StartCoroutine("CheckForPlayersLoaded");
		}
		//otherwise, create a player to play the game, and an AI to face them
		else
		{
			GameObject singlePlayerObject = Instantiate(offlinePlayerPrefab);
			localPlayer = singlePlayerObject.GetComponent<OfflineTCGPlayer>();

			GameObject aiOpponentObject = Instantiate(offlinePlayerPrefab);
			opponentPlayer = aiOpponentObject.GetComponent<OfflineTCGPlayer>();

			multiplayerGame = false;
			InitializeLocalGame();
		}
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.R) && localPlayer != null)
		{
			localPlayer.ResetResources();
		}

		if (Input.GetKeyUp(KeyCode.D) && localPlayer != null)
		{
			localPlayer.DrawCard();
		}
	}

	private IEnumerator CheckForPlayersLoaded()
	{
		for (;;)
		{
			OnlineTCGPlayer[] players = GameObject.FindObjectsOfType<OnlineTCGPlayer>();
			if (players.Length == 2)
			{
				if (players[0].gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					localPlayer = players[0];
					opponentPlayer = players[1];
				}
				else
				{
					localPlayer = players[1];
					opponentPlayer = players[0];
				}

				multiplayerGame = true;
				InitializeLocalGame();

				break;
			}
			else
			{
				yield return null;
			}
		}
	}

	private void InitializeLocalGame()
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

		Library localLibrary = new Library(cardList);

		if (multiplayerGame)
		{
			localPlayer.SetLibrary(localLibrary);
			localPlayer.InitializePlayer(this, false);
			opponentPlayer.InitializePlayer(this, true);
		}
		else
		{
			localPlayer.SetLibrary(localLibrary);
			localPlayer.InitializePlayer(this, false);

			opponentPlayer.SetLibrary(new Library(cardList));
			opponentPlayer.InitializePlayer(this, true);
		}

		UpdateUI();
	}

	public void UpdateUI()
	{
		UpdateCardShowingUI();
		UpdatePlayerResourcesUI();
	}

	public void UpdateCardShowingUI()
	{
		if (localHandRenderer == null || localPlayer == null || localPlayer.GetHand() == null)
		{
			Debug.LogError("Unable to update card showing UI for local player.");
		}
		else
		{
			localHandRenderer.RenderCards(localPlayer.GetHand());
		}

		if (opponentHandRenderer == null || opponentPlayer == null)
		{
			Debug.LogError("Unable to update card showing UI for opponent.");
		}
		else
		{
			opponentHandRenderer.RenderCards(opponentPlayer.GetHandCount());
		}
	}

	public void UpdatePlayerResourcesUI()
	{
		if (localResourcesRenderer == null || localPlayer == null)
		{
			Debug.LogError("Unable to update resources showing UI for local player.");
		}
		else
		{
			localResourcesRenderer.RenderResources(localPlayer.GetResourcesPerTurn(), localPlayer.GetCurrentResources());
		}

		if (localResourcesRenderer == null || opponentPlayer == null)
		{
			Debug.LogError("Unable to update resources showing UI for opponent.");
		}
		else
		{
			opponentResourcesRenderer.RenderResources(opponentPlayer.GetResourcesPerTurn(), opponentPlayer.GetCurrentResources());
		}
	}

	//try to begin the process of playing a card from the client
	public bool TryPlayCard(Card card)
	{
		return localPlayer.TryPlayCard(card);
	}

	public void TryBeginTurn()
	{
		if (multiplayerGame)
		{
			if (currentTurn == 0)
			{
				Debug.LogError("Client trying to begin turn when it is already the local players turn.");
				return;
			}

			currentTurn = 0;
			InitializeLocalTurnUI();
		}
		else
		{
			Debug.LogError("TryBeginTurn should never be ran in a singleplayer game.");
            return;
		}
	}

	//try to begin the process of ending the turn from the client
	public void TryEndTurn()
	{
		if (multiplayerGame)
		{
			if (currentTurn == 0)
			{
				if (localPlayer.TryEndTurn() && opponentPlayer.TryStartTurn())
				{
					currentTurn = 1;
					InitializeOpponentTurnUI();
                }
			}
			else
			{
				Debug.LogError("Client trying to end turn when it isn't the local players turn.");
				return;
			}
		}
		else
		{
			if (currentTurn == 0)
			{
				if (localPlayer.TryEndTurn())
				{
					currentTurn = 1;
					InitializeAITurnUI();
					aiTurnBegins(this);
				}
			}
			else
			{
				if (localPlayer.TryStartTurn())
				{
					currentTurn = 0;
					InitializeSingleTurnUI();
				}
			}
        }
	}

	/*** Initializing Turns ***/

	public void InitializeLocalTurnUI()
	{
		Debug.Log("Initializing UI for a local turn.");
		endTurnButton.interactable = true;
		localHandRenderer.cardsDraggable = true;
	}

	public void InitializeOpponentTurnUI()
	{
		Debug.Log("Initializing UI for the opponents turn.");
		endTurnButton.interactable = false;
		localHandRenderer.cardsDraggable = false;
	}

	public void InitializeSingleTurnUI()
	{
		Debug.Log("Initializing UI for a singleplayer turn.");
		endTurnButton.interactable = true;
		localHandRenderer.cardsDraggable = true;
	}

	public void InitializeAITurnUI()
	{
		Debug.Log("Initializing UI foran AI controlled turn.");
		endTurnButton.interactable = false;
		localHandRenderer.cardsDraggable = false;
	}
}
