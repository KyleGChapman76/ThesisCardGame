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
	public HandRenderer localHandRenderer;
	public HandRenderer opponentHandRenderer;
	public ResourcesRenderer localResourcesRenderer;
	public ResourcesRenderer opponentResourcesRenderer;

	private Card[] cards;

	private void Start ()
	{
		Debug.Log("GameManager began.");

		//if multiplayer game, wait for players to load in
		if (GameObject.FindObjectOfType<NetworkManager>())
		{
			StartCoroutine("CheckForPlayersLoaded");
		}
		//otherwise, create a player to play the game
		else
		{
			GameObject localPlayerObject = Instantiate(playerPrefab);
            localPlayer = localPlayerObject.GetComponent<TCGPlayer>();
			InitializeLocalGame();
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

		//TODO unhardcode libraries

		Card card00 = new ResourceCard(0, "Resource 1", 1, 1);
        Card card01 = new CreatureCard(1, "Creature 1", 2, 2, 2);
		Card card02 = new CreatureCard(2, "Creature 2", 4, 3, 4);
		Card card03 = new SpellCard(3, "Spell 1", 1);
		Card card04 = new SpellCard(4, "Spell 2", 2);
		Card card05 = new SpellCard(5, "Spell 3", 3);

		cards = new Card[] { card00, card01, card02, card03, card04, card05};

		Deck hardcodedDeck = new Deck(5, 8, 4);

		hardcodedDeck.AddCard(card00, 1);
		hardcodedDeck.AddCard(card01, 1);
		hardcodedDeck.AddCard(card02, 1);
		hardcodedDeck.AddCard(card03, 1);
		hardcodedDeck.AddCard(card04, 1);
		hardcodedDeck.AddCard(card05, 1);

		List<Card> cardList;

		if (!hardcodedDeck.ExportDeckToList(out cardList))
		{
			Debug.Log("Can't export hardcoded deck.");
			Application.Quit();
		}

		localPlayer.Library = new Library(cardList);

		localPlayer.InitializeLocalPlayer(this);

		if (opponentPlayer != null)
		{
			opponentPlayer.InitializeOpponent(this);
		}

		UpdateUI();
    }

	public Card GetCardWithID(int id)
	{
		if (cards[id].CardID == id)
		{
			return cards[id];
		}
		else
		{
			Debug.Log("Card in slot " + id + " does not have that ID.");
			return null;
        }
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
			Debug.Log("Unable to update card showing UI for local player.");
		}
		else
		{
			localHandRenderer.RenderCards(localPlayer.Hand);
		}

		if (opponentHandRenderer == null || opponentPlayer == null || opponentPlayer.Hand == null)
		{
			Debug.Log("Unable to update card showing UI for opponent.");
		}
		else
		{
			opponentHandRenderer.RenderCards(opponentPlayer.Hand);
		}
	}

	public void UpdatePlayerResourcesUI()
	{
		if (localResourcesRenderer == null || localPlayer == null)
		{
			Debug.Log("Unable to update resources showing UI for local player.");
		}
		else
		{
			localResourcesRenderer.RenderResources(localPlayer.MaxResourcesPerTurn, localPlayer.CurrentResources);
        }

		if (localResourcesRenderer == null || opponentPlayer == null)
		{
			Debug.Log("Unable to update resources showing UI for opponent.");
		}
		else
		{
			opponentResourcesRenderer.RenderResources(opponentPlayer.MaxResourcesPerTurn, opponentPlayer.CurrentResources);
		}
	}

	public void TryPlayCard(Card card)
	{
		localPlayer.TryPlayCard(card);
	}
}
