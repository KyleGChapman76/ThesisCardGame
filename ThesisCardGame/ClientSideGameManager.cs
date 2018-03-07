using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ClientSideGameManager : MonoBehaviour
{
	GameUIManager uiManager;
	public GameObject offlinePlayerPrefab;

	//start the local game logic
	//if find a network manager, wait for all players to load in to then initialize multiplayer logic
	//otherwise, setup players
	void Start ()
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
			uiManager = singlePlayerObject.GetComponent<OfflineTCGPlayer>();

			GameObject aiOpponentObject = Instantiate(offlinePlayerPrefab);
			opponentPlayer = aiOpponentObject.GetComponent<OfflineTCGPlayer>();

			multiplayerGame = false;
			InitializeLocalGame();
		}
	}
}
