using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkedGameManager : NetworkBehaviour, IGameManager
{
	public GameUIManager uiManager;

	private OnlineTCGPlayer localPlayer;
	private OnlineTCGPlayer opponentPlayer;

	public static NetworkedGameManager SingletonGameManager;

	public void Start()
	{
		StartCoroutine("CheckForPlayersLoaded");
		uiManager = GameObject.FindObjectOfType<GameUIManager>();
		SingletonGameManager = this;
		GameObject.FindObjectOfType<LocalGameManager>().ServerGameManager = this;
	}

	//wait until two players have loaded into the game
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

				localPlayer.InitializePlayer(LocalGameManager.hardCodedLibrary);

				uiManager.UpdateUI(localPlayer, opponentPlayer);

				break;
			}
			else
			{
				yield return null;
			}
		}
	}

	public bool TryEndTurn()
	{
		return localPlayer.TryEndTurn();
	}

	public bool TryPlayCard(Card card)
	{
		return localPlayer.TryPlayCard(card);
	}
}
