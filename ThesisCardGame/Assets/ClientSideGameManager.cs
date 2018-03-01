using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ClientSideGameManager : MonoBehaviour
{
	public GameObject playerPrefab;
	public TCGPlayer localPlayer;
	public TCGPlayer opponentPlayer;
	public PlayerHandRenderer localHandRenderer;
	public OpponentHandRenderer opponentHandRenderer;
	public ResourcesRenderer localResourcesRenderer;
	public ResourcesRenderer opponentResourcesRenderer;

	private void Start ()
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
			GameObject localPlayerObject = Instantiate(playerPrefab);
            localPlayer = localPlayerObject.GetComponent<TCGPlayer>();

			GameObject aiOpponentObject = Instantiate(playerPrefab);
			opponentPlayer = aiOpponentObject.GetComponent<TCGPlayer>();
			InitializeLocalGame(false);
		}
    }

	private IEnumerator CheckForPlayersLoaded()
	{
		for (;;)
		{
			TCGPlayer[] players = GameObject.FindObjectsOfType<TCGPlayer>();
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

				InitializeLocalGame(true);
				break;
			}
			else
			{
				yield return null;
			}
		}
	}

	private void InitializeLocalGame(bool isMultiplayer)
	{
		Debug.Log("Beginning the game.");

		//define all game cards
		CardDefinition card00 = new ResourceCardDefinition("Resource 1", 1, 1);
        CardDefinition card01 = new CreatureCardDefinition("Creature 1", 2, 2, 2);
		CardDefinition card02 = new CreatureCardDefinition("Creature 2", 4, 3, 4);
		CardDefinition card03 = new SpellCardDefinition("Spell 1", 1);
		CardDefinition card04 = new SpellCardDefinition("Spell 2", 2);
		CardDefinition card05 = new SpellCardDefinition("Spell 3", 3);

		Deck hardcodedDeck = new Deck(5, 8, 4);

		hardcodedDeck.AddCard(0, 1);
		hardcodedDeck.AddCard(1, 1);
		hardcodedDeck.AddCard(2, 1);
		hardcodedDeck.AddCard(3, 1);
		hardcodedDeck.AddCard(4, 1);
		hardcodedDeck.AddCard(5, 1);

		int[] cardList;

		if (!hardcodedDeck.ExportDeckToArray(out cardList))
		{
			Debug.LogError("Can't export hardcoded deck.");
			Application.Quit();
		}

		localPlayer.Library = new Library(cardList);
		localPlayer.InitializeLocalPlayer(this);

		if (isMultiplayer)
		{
			opponentPlayer.InitializeMultiplayerOpponent(this);
		}
		else
		{
			opponentPlayer.Library = new Library(cardList);
			opponentPlayer.InitializeSinglePlayer(this);
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
		if (localHandRenderer == null || localPlayer == null || localPlayer.Hand == null)
		{
			Debug.LogError("Unable to update card showing UI for local player.");
		}
		else
		{
			localHandRenderer.RenderCards(localPlayer.Hand);
		}

		if (opponentHandRenderer == null || opponentPlayer == null)
		{
			Debug.LogError("Unable to update card showing UI for opponent.");
		}
		else
		{
			opponentHandRenderer.RenderCards(opponentPlayer.HandCount);
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
			localResourcesRenderer.RenderResources(localPlayer.MaxResourcesPerTurn, localPlayer.CurrentResources);
        }

		if (localResourcesRenderer == null || opponentPlayer == null)
		{
			Debug.LogError("Unable to update resources showing UI for opponent.");
		}
		else
		{
			opponentResourcesRenderer.RenderResources(opponentPlayer.MaxResourcesPerTurn, opponentPlayer.CurrentResources);
		}
	}

	public void TryPlayCard(Card card)
	{
		localPlayer.TryPlayCard(card.BaseDefinition);
	}
}
