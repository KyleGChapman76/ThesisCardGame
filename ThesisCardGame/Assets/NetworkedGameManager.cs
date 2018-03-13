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

	[SyncVar(hook = "IsHostsTurnChanged")]
	private bool isHostsTurn;

	private ClientRequests clientRequests;

	public void Start()
	{
		Debug.Log("Starting networked game manager.");
		
		uiManager = GameObject.FindObjectOfType<GameUIManager>();
		SingletonGameManager = this;
		GameObject.FindObjectOfType<LocalGameManager>().ServerGameManager = this;

		StartCoroutine("CheckForPlayersLoaded");
	}

	//wait until two players have loaded into the game
	private IEnumerator CheckForPlayersLoaded()
	{
		for (;;)
		{
			OnlineTCGPlayer[] players = GameObject.FindObjectsOfType<OnlineTCGPlayer>();
			if (players.Length == 2)
			{
				Debug.Log("Two players loaded in.");
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

				if (isServer)
				{
					isHostsTurn = true;
					opponentPlayer.GetComponent<ClientRequests>().localCopyOfNetworkedGameManager = this;
				}
				else
				{
					clientRequests = localPlayer.GetComponent<ClientRequests>();
				}

				UpdateLocalUI();

				break;
			}
			else
			{
				Debug.Log("Checking for players loaded.");
				yield return null;
			}
		}
	}

	private bool IsOurTurn()
	{
		if (isServer && isHostsTurn)
			return true;

		if (!isServer && !isHostsTurn)
			return true;

		return false;
	}

	private void UpdateLocalUI()
	{
		if (IsOurTurn())
		{
			uiManager.InitializeLocalTurnUI();
		}
		else
		{
			uiManager.InitializeOpponentTurnUI();
		}
	}

	public bool LocalEndTurn()
	{
		if (localPlayer == null)
		{
			Debug.LogError("Networked game manager doesn't have reference to local player.");
			return false;
		}

		if (!IsOurTurn())
		{
			Debug.LogError("Can't end local players turn when its not local players turn.");
			return false;
		}

		//if on server, do server side turn code directly
		if (isServer)
		{
			ServerSideEndTurn();
		}
		//if not on the server, send command to end turn
		else
		{
			clientRequests.CmdRequestTurnEnd();
		}

		return true;
	}

	public bool ClientRequestEndTurn()
	{
		if (opponentPlayer == null)
		{
			Debug.LogError("Networked game manager doesn't have reference to opponent player.");
			return false;
		}

		if (IsOurTurn())
		{
			Debug.LogError("Can't end opponents turn when it its our local players turn.");
			return false;
		}

		ServerSideEndTurn();

		return true;
	}

	private void ServerSideEndTurn()
	{
		Debug.Log("Server side ending turn code.");

		if (isHostsTurn)
		{
			Debug.Log("Starting the turn for the client.");
			isHostsTurn = false;

			opponentPlayer.DrawCard();
			opponentPlayer.ResetResources();

			UpdateLocalUI();
        }
		else
		{
			Debug.Log("Starting the turn for the host.");
			isHostsTurn = true;

			localPlayer.DrawCard();
			localPlayer.ResetResources();

			UpdateLocalUI();
		}
	}

	public bool TryPlayCard(Card card)
	{
		if (localPlayer == null)
		{
			Debug.LogError("Networked game manager doesn't have reference to local player.");
			return false;
		}

		if (!IsOurTurn())
		{
			Debug.LogError("Can't play card when its not your turn.");
			return false;
		}

		return localPlayer.PlayCard(card);
	}

	public void PlayCreature(CreatureCardDefinition creatureCardDefinition, bool playedByLocalPlayer)
	{
		throw new NotImplementedException();
	}

	public void PlaySpell(SpellCardDefinition spellCardDefinition, bool playedByLocalPlayer)
    {
		Debug.Log("Executing effects of spell card: " + spellCardDefinition.CardName);
		//TODO
		if (playedByLocalPlayer)
		{
			opponentPlayer.ChangeLifeTotal(-10);
		}
		else
		{
			localPlayer.ChangeLifeTotal(-10);
		}
	}

	/*** Notifications From Model ***/

	public void PlayerStatusChanged()
	{
		uiManager.UpdateUI(localPlayer, opponentPlayer);
    }

	public void ReportGameLoss(bool hostLost)
	{
		if (!isServer)
		{
			Debug.LogError("Can't report loss when not on the server.");
			return;
		}

		bool weWon = hostLost != isServer;
		uiManager.InitializeGameOverUI(weWon);

		RpcReportGameLoss(hostLost);
		StartCoroutine("DestroyGameManagerAfterShortTime");
    }

	private IEnumerator DestroyGameManagerAfterShortTime()
	{
		yield return new WaitForSeconds(.5f);
		Network.Destroy(gameObject);
	}

	/*** RPCs and Commands ***/

	[ClientRpc]
	private void RpcReportGameLoss(bool hostLost)
	{
		bool weWon = hostLost != isServer;
        uiManager.InitializeGameOverUI(weWon);
	}

	/*** SyncVar Hooks ***/
	private void IsHostsTurnChanged(bool value)
	{
		isHostsTurn = value;

		UpdateLocalUI();

		Debug.Log("ISHOSTSTURN HOOK: " + value.ToString());
	}
}
