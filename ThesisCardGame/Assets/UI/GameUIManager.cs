using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
	public IGameManager gameManager;

	public PlayerHandRenderer localHandRenderer;
	public OpponentHandRenderer opponentHandRenderer;
	public ResourcesRenderer localResourcesRenderer;
	public ResourcesRenderer opponentResourcesRenderer;

	public Button endTurnButton;

 	public void UpdateUI(ICardGamePlayer localPlayer, ICardGamePlayer opponentPlayer)
	{
		if (localPlayer == null)
		{
			Debug.LogError("Can't show UI for null player.");
			return;
		}

		if (opponentPlayer == null)
		{
			Debug.LogError("Can't show UI for null opponent.");
			return;
		}

		UpdateCardShowingUI(localPlayer, opponentPlayer);
		UpdatePlayerResourcesUI(localPlayer, opponentPlayer);
	}

	public void UpdateCardShowingUI(ICardGamePlayer localPlayer, ICardGamePlayer opponentPlayer)
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
			Debug.LogError("UNo access to local resources renderer.");
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

	//try to begin the process of playing a card from the client
	public bool TryPlayCard(Card card)
	{
		return gameManager.TryPlayCard(card);
	}

	//try to begin the process of ending the turn from the client
	public void TryEndTurn()
	{
		gameManager.TryEndTurn();
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
}
