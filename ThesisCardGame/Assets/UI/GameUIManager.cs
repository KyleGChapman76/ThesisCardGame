using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
	public LocalGameManager gameManager;

	public PlayerHandRenderer localHandRenderer;
	public OpponentHandRenderer opponentHandRenderer;

	public ResourcesRenderer localResourcesRenderer;
	public ResourcesRenderer opponentResourcesRenderer;

	public LifeTotalRenderer localLifeTotalRenderer;
	public LifeTotalRenderer opponentLifeTotalRenderer;

	public GameObject gameOverOverlay;
	public Text gameOverResultText;

	public Button endTurnButton;

 	public void UpdateUI(ICardGamePlayer localPlayer, ICardGamePlayer opponentPlayer)
	{
		if (localPlayer == null)
		{
			Debug.LogError("Can't show UI for null local player.");
			return;
		}

		if (opponentPlayer == null)
		{
			Debug.LogError("Can't show UI for null opponent.");
			return;
		}

		UpdateCardUI(localPlayer, opponentPlayer);
		UpdatePlayerResourcesUI(localPlayer, opponentPlayer);
		UpdatePlayerLifeUI(localPlayer, opponentPlayer);

		gameOverOverlay.SetActive(false);
    }

	public void UpdateCardUI(ICardGamePlayer localPlayer, ICardGamePlayer opponentPlayer)
	{
		if (localPlayer == null)
		{
			Debug.LogError("Can't show cards for null player.");
			return;
		}

		if (opponentPlayer == null)
		{
			Debug.LogError("Can't show cards for null opponent.");
			return;
		}

		if (localHandRenderer == null)
		{
			Debug.LogError("No access to local hand renderer component.");
		}
		else
		{
			localHandRenderer.RenderCards(localPlayer.GetHand());
		}

		if (opponentHandRenderer == null)
		{
			Debug.LogError("No access to opponent opponent renderer component.");
		}
		else
		{
			opponentHandRenderer.RenderCards(opponentPlayer.GetHandCount());
		}
	}

	public void UpdatePlayerResourcesUI(ICardGamePlayer localPlayer, ICardGamePlayer opponentPlayer)
	{
		if (localPlayer == null)
		{
			Debug.LogError("Can't show resources for null player.");
			return;
		}

		if (opponentPlayer == null)
		{
			Debug.LogError("Can't show resources for null opponent.");
			return;
		}

		if (localResourcesRenderer == null)
		{
			Debug.LogError("No access to local resources renderer.");
		}
		else
		{
			localResourcesRenderer.RenderResources(localPlayer.GetResourcesPerTurn(), localPlayer.GetCurrentResources());
		}

		if (opponentResourcesRenderer == null)
		{
			Debug.LogError("No access to opponent resources renderer.");
		}
		else
		{
			opponentResourcesRenderer.RenderResources(opponentPlayer.GetResourcesPerTurn(), opponentPlayer.GetCurrentResources());
		}
	}

	public void UpdatePlayerLifeUI(ICardGamePlayer localPlayer, ICardGamePlayer opponentPlayer)
	{
		if (localPlayer == null)
		{
			Debug.LogError("Can't show life total for null player.");
			return;
		}

		if (opponentPlayer == null)
		{
			Debug.LogError("Can't show life total for null opponent.");
			return;
		}

		if (localLifeTotalRenderer == null)
		{
			Debug.LogError("No access to local life total renderer.");
		}
		else
		{
			localLifeTotalRenderer.RenderLifeTotal(localPlayer.GetLifeTotal());
		}

		if (opponentLifeTotalRenderer == null)
		{
			Debug.LogError("No access to opponent life total renderer.");
		}
		else
		{
			opponentLifeTotalRenderer.RenderLifeTotal(opponentPlayer.GetLifeTotal());
		}
	}

	//try to begin the process of playing a card from the client
	public bool TryPlayCard(Card card)
	{
		return gameManager.TryPlayCard(card);
	}

	//try to begin the process of ending the turn from the client
	public void TryEndTurn()
	{
		gameManager.LocalEndTurn();
    }

	/*** Initializing UI for different turns ***/

	public void InitializeLocalTurnUI()
	{
		Debug.Log("Initializing UI for a local turn.");
		endTurnButton.interactable = true;
		localHandRenderer.SetCardsDraggable(true);
	}

	public void InitializeOpponentTurnUI()
	{
		Debug.Log("Initializing UI for the opponents turn.");
		endTurnButton.interactable = false;
		localHandRenderer.SetCardsDraggable(false);
	}

	internal void InitializeGameOverUI(bool weWon)
	{
		endTurnButton.interactable = false;
		localHandRenderer.SetCardsDraggable(false);

		gameOverOverlay.SetActive(true);
		if (weWon)
		{
			gameOverResultText.text = "You win!";
		}
		else
		{
			gameOverResultText.text = "You lost!";
		}
	}
}
