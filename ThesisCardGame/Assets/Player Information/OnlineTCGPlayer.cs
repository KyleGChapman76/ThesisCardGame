using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//class storing and communicating information about the player's status
//including life total, their library, and their hand
public class OnlineTCGPlayer : NetworkBehaviour, ICardGamePlayer
{
	public GameObject serverGameManager;

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

	[SyncVar]
	private bool hasPlayedAResourceThisTurn;

	private LocalGameManager clientGameManager;

	[SyncVar(hook = "IsMyTurnChanged")]
	private bool isMyTurn;

	private void Start()
	{
		if (isServer && isLocalPlayer)
		{
			Debug.Log("Server spawned player starts, instantiating game manager.");
			Instantiate(serverGameManager);
		}
	}

	/*** Getters and Setters ***/

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

	/*** Initialization Functions ***/

	public void InitializePlayer(Library library)
	{
		this.library = library;

		hand = new List<Card>();
		DrawLocalHand();

		if (!isServer)
		{
			CmdInitializeQualities();
		}
		else
		{
			InitializeQualities();
		}
	}

	private void InitializeQualities()
	{
		if (!isServer)
		{
			Debug.Log("Should only be run on the server.");
		}

		maxResourcesPerTurn = GameConstants.STARTING_MAX_RESOURCES_PER_TURN;
		currentResources = maxResourcesPerTurn;
	}

	private void DrawLocalHand()
	{
		Debug.Log("Drawing hand for player.");

		int handSize = GameConstants.STARTING_HAND_SIZE;
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

	/*** Actions ***/

	public void DrawCard()
	{
		Debug.Log("Player drawing card.");

		if (handCount >= GameConstants.MAX_HAND_SIZE)
		{
			Debug.Log("Can't draw a card, due to maximum hand size.");
			return;
		}

		Card cardDrawn;
		if (library.DrawCard(out cardDrawn))
		{
			hand.Add(cardDrawn);
			handCount++;
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
	}

	/*** Turn Structure Functions ***/

	public bool TryStartTurn()
	{
		if (isMyTurn)
		{
			Debug.LogError("Player trying to begin turn when it is already their turn.");
			return false;
		}

		//if on server, do server side turn code directly
		if (isServer)
		{
			ServerSideStartTurn();
		}
		//if not on the server, 
		else
		{
			CmdTurnChanged(true);
		}
		return true;
	}

	private void ServerSideStartTurn()
	{
		if (!isServer)
		{
			Debug.LogError("Server side start turn ran on client!");
			return;
		}

		hasPlayedAResourceThisTurn = false;
		ResetResources();
		DrawCard();
	}

	public bool TryEndTurn()
	{
		if (!isMyTurn)
		{
			Debug.LogError("Player trying to end turn when it isn't their turn.");
			return false;
		}

		//if on server, do server side turn code directly
		if (isServer)
		{
			ServerSideEndTurn();
        }
		//if not on the server, 
		else
		{
			CmdTurnChanged(false);
		}

		return true;
	}

	private void ServerSideEndTurn()
	{
		if (!isServer)
		{
			Debug.LogError("Server side start turn ran on client!");
			return;
		}
	}

	public bool TryPlayCard(Card card)
	{
		if (card == null)
		{
			Debug.Log("Can't play a null card.");
			return false;
		}

		if (!isMyTurn)
		{
			Debug.Log("Can't play a card when its not your turn.");
			return false;
		}

		if (card is ResourceCard)
		{
			Debug.Log("Player playing a resource card.");

			if (hasPlayedAResourceThisTurn)
			{
				Debug.Log("Player has already played a resource card this turn.");
				return false;
			}
        }
		else if (card is SpellCard)
		{
			SpellCard spellCard = (SpellCard)card;
			if (spellCard.BaseDefinition.ManaCost > currentResources)
			{
				Debug.Log("Not enough resources (current = " + currentResources.ToString() + ") to cast that spell with cost: " + spellCard.BaseDefinition.ManaCost.ToString());
				return false;
			}
		}

		if (isServer)
		{
			PlayCard(card.BaseDefinition, 0);
			RpcPlayerPlaysCard(card.BaseDefinition.CardID, 0);
		}
		else
		{
			CmdPlayerPlaysCard(card.BaseDefinition.CardID, 1);
		}

		return true;
	}

	//server side implementation
	private bool PlayCard(CardDefinition card, int playerNum)
	{
		if (card is ResourceCardDefinition)
		{
			Debug.Log("Player " + playerNum + " playing a resource card.");

			if (hasPlayedAResourceThisTurn)
			{
				Debug.Log("Player has already played a resource card this turn.");
				return false;
			}

			maxResourcesPerTurn += ((ResourceCardDefinition)card).ResourcesGiven;
		}
		else if (card is SpellCardDefinition)
		{
			SpellCardDefinition spellCard = (SpellCardDefinition)card;
			if (spellCard.ManaCost > currentResources)
			{
				Debug.Log("Player " + playerNum + " doesn't have enough resources (current = " + currentResources.ToString() + ") to cast that spell with cost: " + spellCard.ManaCost.ToString());
				return false;
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

		return true;
	}

	/*** RPCs and Commands ***/

	//client tells the server that it wants to be initialized
	[Command]
	private void CmdInitializeQualities()
	{
		InitializeQualities();
    }

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
		if (PlayCard(card, playerNum))
		{
			RpcPlayerPlaysCard(cardID, playerNum);
		}

		LocalGameManager.SingletonGameManager.PlayerStatusChanged();
	}

	//client tells the server that it wants to reset its resources (DEBUG)
	[Command]
	private void CmdPlayerResetsResources(int playerNum)
	{
		Debug.Log("Player " + playerNum + " resetting their resources.");
		currentResources = maxResourcesPerTurn;

		LocalGameManager.SingletonGameManager.PlayerStatusChanged();
	}

	//client tells the server that its hand size has changed
	[Command]
	private void CmdAnnounceHandSize(int handSize, int playerNum)
	{
		Debug.Log("Player " + playerNum + " hand size became " + handSize);
		handCount = handSize;

		LocalGameManager.SingletonGameManager.PlayerStatusChanged();
	}

	//client tells the server that it would like to change the turn
	[Command]
	private void CmdTurnChanged(bool turnBegan)
	{
		isMyTurn = turnBegan;

		if (isMyTurn)
		{
			ServerSideStartTurn();
		}
		else
		{
			ServerSideEndTurn();
		}

		LocalGameManager.SingletonGameManager.PlayerStatusChanged();
	}

	/*** SyncVar Hooks ***/

	private void CurrentResourcesChanged(int value)
	{
		currentResources = value;
		Debug.Log("CURRENT RESOURCES HOOK: " + value);
	}

	private void MaxResourcesPerTurnChanged(int value)
	{
		maxResourcesPerTurn = value;
		Debug.Log("MAX RESOURCES HOOK: " + value);
	}

	private void HandCountChanged(int value)
	{
		handCount = value;
		Debug.Log("HAND COUNT HOOK: " + value);
	}

	private void IsMyTurnChanged(bool value)
	{
		isMyTurn = value;
		Debug.Log("MY TURN HOOK: " + value);
	}
}
