using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//class storing and communicating information about the player's status
//including life total, their library, and their hand
public class OnlineTCGPlayer : NetworkBehaviour, ICardGamePlayer
{
	private Library library;

	private List<Card> hand;

	[SyncVar(hook = "HandCountChanged")]
	private int handCount;

	[SyncVar(hook = "MaxResourcesPerTurnChanged")]
	private int maxResourcesPerTurn;

	public int CurrentResources
	{
		get
		{
			return currentResources;
		}
	}
	[SyncVar(hook = "CurrentResourcesChanged")]
	private int currentResources;

	private bool hasPlayedAResourceThisTurn;

	private ClientSideGameManager clientGameManager;

	/*** Getters and Setters ***/

	public void SetLibrary(Library library)
	{
		this.library = library;
	}

	public Library GetLibrary()
	{
		return library;
	}

	public int GetCurrentResources()
	{
		return currentResources;
	}

	public int GetResourcesPerTurn()
	{
		return maxResourcesPerTurn;
	}

	public List<Card> GetHand()
	{
		return hand;
	}

	public int GetHandCount()
	{
		return handCount;
	}

	public void DrawCard()
	{
		Debug.Log("Player drawing card.");

		if (handCount >= ClientSideGameManager.MAX_HAND_SIZE)
		{
			Debug.Log("Can't draw a card, due to maximum hand size.");
			return;
		}

		Card cardDrawn;
		if (library.DrawCard(out cardDrawn))
		{
			hand.Add(cardDrawn);
			handCount++;
			clientGameManager.UpdateUI();
		}
		else
		{
			Debug.LogError("Could not draw card from library.");
		}
	}

	public void ResetResources()
	{
		Debug.Log("Resetting resources.");
		currentResources = maxResourcesPerTurn;
		clientGameManager.UpdateUI();
	}

	public void StartTurn()
	{
		hasPlayedAResourceThisTurn = false;
		ResetResources();
		DrawCard();
	}

	/*** Initialization Functions ***/

	public void InitializePlayer(ClientSideGameManager clientSide, bool isOpponent)
	{
		if (isOpponent)
		{
			InitializeMultiplayerOpponent(clientSide);
		}
		else
		{
			InitializeLocalPlayer(clientSide);
		}
	}

	private void InitializeLocalPlayer(ClientSideGameManager clientSide)
	{
		Debug.Log("Initializing player on " + (isServer ? " server." : " client."));

		hand = new List<Card>();
		DrawLocalHand();

		if (isServer)
		{
			maxResourcesPerTurn = ClientSideGameManager.STARTING_MAX_RESOURCES_PER_TURN;
			currentResources = maxResourcesPerTurn;
		}

		clientGameManager = clientSide;
    }

	private void InitializeMultiplayerOpponent(ClientSideGameManager clientSide)
	{
		Debug.Log("Initializing opponent on " + (isServer ? " server." : " client."));

		clientGameManager = clientSide;

		if (isServer)
		{
			maxResourcesPerTurn = ClientSideGameManager.STARTING_MAX_RESOURCES_PER_TURN;
			currentResources = maxResourcesPerTurn;
		}
    }

	private void DrawLocalHand()
	{
		Debug.Log("Drawing hand for player.");

		int handSize = ClientSideGameManager.STARTING_HAND_SIZE;
		if (hand.Count > 0)
		{
			handSize = hand.Count - 1;
		}
		for (int i = 0; i < handSize; i++)
		{
			Card cardDrawn;
			if (library.DrawCard(out cardDrawn))
			{
				hand.Add(cardDrawn);
			}
			else
			{
				Debug.LogError("Could not draw enough cards for hand.");
			}
		}

		if (isServer)
		{
			handCount = hand.Count;
		}
		else
		{
			CmdAnnounceHandSize(hand.Count, 1);
		}
	}

	public bool TryPlayCard(Card card)
	{
		//TODO
		return false;
	}

	//client side logic
	public bool TryPlayCard(CardDefinition card)
	{
		if (card == null)
		{
			Debug.Log("Can't play a null card.");
			return false;
		}

		bool cardPlayValid = false;
		if (card is ResourceCardDefinition)
		{
			//TODO check if the player can play a resource card this turn
			cardPlayValid = true;
		}
		else if (card is SpellCardDefinition)
		{
			SpellCardDefinition spellCard = (SpellCardDefinition)card;
			if (spellCard.ManaCost > currentResources)
			{
				Debug.Log("Not enough resources (current = " + currentResources.ToString() + ") to cast that spell with cost: " + spellCard.ManaCost.ToString());
			}
			else
			{
				cardPlayValid = true;
			}
		}

		if (cardPlayValid)
		{
			if (isServer)
			{
				RpcPlayerPlaysCard(card.CardID, 0);
			}
			else
			{
				CmdPlayerPlaysCard(card.CardID, 1);
			}
		}

		return true;
	}

	//server side checking and implementation
	private void PlayCard(CardDefinition card, int playerNum)
	{
		if (card is ResourceCardDefinition)
		{
			Debug.Log("Player " + playerNum + " playing a resource card.");

			//TODO check if the player can play a resource card this turn

			maxResourcesPerTurn += ((ResourceCardDefinition)card).ResourcesGiven;
		}
		else if (card is SpellCardDefinition)
		{
			SpellCardDefinition spellCard = (SpellCardDefinition)card;
			if (spellCard.ManaCost > currentResources)
			{
				Debug.Log("Player " + playerNum + " doesn't have enough resources (current = " + currentResources.ToString() + ") to cast that spell with cost: " + spellCard.ManaCost.ToString());
				return;
			}

			currentResources -= spellCard.ManaCost;

			if (card is CreatureCardDefinition)
			{
				Debug.Log("Player " + playerNum + " playing a creature card.");
				//TODO
			}
			else
			{
				Debug.Log("Player " + playerNum + " laying a spell card.");
				//TODO
			}
		}

		clientGameManager.UpdateUI();
	}

	/*** RPCs and Commands ***/

	//server tells the client that a card of a certain ID has been played
	[ClientRpc]
	private void RpcPlayerPlaysCard(int cardID, int playerNum)
	{
		CardDefinition card = CardDefinition.GetCardDefinitionWithID(cardID);
		Debug.Log("Player " + playerNum + " played card " + card.CardName + " (" + cardID + ").");
		
		//TODO animations and such
	}

	//client tells the server that it wants to play a card
	[Command]
	private void CmdPlayerPlaysCard(int cardID, int playerNum)
	{
		CardDefinition card = CardDefinition.GetCardDefinitionWithID(cardID);
		PlayCard(card, playerNum);
		RpcPlayerPlaysCard(cardID, playerNum);
    }

	//client tells the server that it has reset its resources
	[Command]
	private void CmdPlayerResetsResources(int playerNum)
	{
		Debug.Log("Player " + playerNum + " resetting their resources.");
		currentResources = maxResourcesPerTurn;
	}

	//client tells the server that its hand size has changed
	[Command]
	private void CmdAnnounceHandSize(int handSize, int playerNum)
	{
		Debug.Log("Player " + playerNum + " hand size became " + handSize);
		handCount = handSize;
	}

	/*** SyncVar Hooks ***/

	private void CurrentResourcesChanged(int value)
	{
		currentResources = value;
		Debug.Log("CURRENT RESOURCES HOOK: " + value);
		if (clientGameManager != null)
		{
			clientGameManager.UpdatePlayerResourcesUI();
		}
	}

	private void MaxResourcesPerTurnChanged(int value)
	{
		maxResourcesPerTurn = value;
		Debug.Log("MAX RESOURCES HOOK: " + value);
		if (clientGameManager != null)
		{
			clientGameManager.UpdatePlayerResourcesUI();
		}
	}

	private void HandCountChanged(int value)
	{
		handCount = value;
		Debug.Log("HAND COUNT HOOK: " + value);
		if (clientGameManager != null)
		{
			clientGameManager.UpdateCardShowingUI();
		}
	}
}
