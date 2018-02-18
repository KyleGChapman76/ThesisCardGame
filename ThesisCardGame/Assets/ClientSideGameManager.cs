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

		Card card00 = new CreatureCard(0, 2, 2, 2);
		Card card01 = new CreatureCard(1, 4, 3, 4);
		Card card02 = new SpellCard(2, 3);

		Deck hardcodedDeck = new Deck(5, 8, 4);

		hardcodedDeck.AddCard(card00, 2);
		hardcodedDeck.AddCard(card01, 3);
		hardcodedDeck.AddCard(card02, 1);

		List<Card> cardList;

		if (!hardcodedDeck.ExportDeckToList(out cardList))
		{
			Debug.Log("Can't export hardcoded deck.");
			Application.Quit();
		}

		localPlayer.Library = new Library(cardList);
		localPlayer.InitializeLocalPlayer();

		UpdateCardShowingUI();
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

	public void TryPlayCard(Card card)
	{
		localPlayer.TryPlayCard(card);
	}
}
