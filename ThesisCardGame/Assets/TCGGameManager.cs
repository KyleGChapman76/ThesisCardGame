using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TCGGameManager : MonoBehaviour
{
	public GameObject playerPrefab;
	public TCGPlayer localPlayer;
	public TCGPlayer opponentPlayer;

	private void Start ()
	{
		Debug.Log("GameManager began.");

		//if multiplayer game, wait for players to load in
		if (GameObject.FindObjectOfType<NetworkManager>())
		{
			StartCoroutine("CheckForPlayersLoaded");
		}
		else
		{
			GameObject localPlayerObject = Instantiate(playerPrefab);
            localPlayer = localPlayerObject.GetComponent<TCGPlayer>();
			Destroy(localPlayerObject.GetComponent<NetworkIdentity>());
			InitializeGame();
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

				InitializeGame();
				break;
			}
			else
			{
				yield return null;
			}
		}
	}

	private void InitializeGame()
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
	}

	public void UpdateEnemyHandUI()
	{
		//TODO
	}

	public void TryPlayCard(Card card)
	{
		if (card == null)
		{
			Debug.Log("Can't play a null card.");
			return;
		}

		if (card is ResourceCard)
		{
			Debug.Log("Local player playing a resource card.");
			//TODO
		}
		else if (card is SpellCard)
		{
			if (card is CreatureCard)
			{
				Debug.Log("Local player playing a creature card.");
				//TODO
			}
			else
			{
				Debug.Log("Local player playing a spell card.");
				//TODO
			}
		}
	}
}
