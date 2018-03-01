using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//class storing and communicating information about the player's status
//including life total, their library, and their hand
public class TCGPlayer : NetworkBehaviour
{
	//library management
	public const int STARTING_HAND_SIZE = 6;
	public const int MAX_HAND_SIZE = 8;
	public const int MAX_MAX_RESOURCES_PER_TURN = 12;
	public const int STARTING_MAX_RESOURCES_PER_TURN = 2;

	public Library Library
	{
		get
		{
			return library;
		}
		set
		{
			library = value;
			Debug.Log("Setting player library.");
        }
	}
	private Library library;

	public List<Card> Hand
	{
		get
		{
			if (hand == null)
				return null;
			return hand;
		}
	}
	private List<Card> hand;

	public int HandCount
	{
		get
		{
			return handCount;
        }
	}
	[SyncVar(hook = "HandCountChanged")]
	private int handCount;

	//resources mangement
	public int MaxResourcesPerTurn
	{
		get
		{
			return maxResourcesPerTurn;
		}
	}
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

	private bool multiplayerGame;
	private ClientSideGameManager clientGameManager;

	public void Update()
	{
		if (!multiplayerGame || isLocalPlayer)
		{
			if (Input.GetKeyUp(KeyCode.R))
			{
				Debug.Log("Resetting resources.");

				if (multiplayerGame)
				{
					CmdPlayerResetsResources(isLocalPlayer ? 0 : 1);
				}
				else
				{
					currentResources = maxResourcesPerTurn;
					clientGameManager.UpdateUI();
				}
			}
		}
	}

	public void InitializeSinglePlayer(ClientSideGameManager clientSide)
	{
		Debug.Log("Initializing single player.");

		hand = new List<Card>();
		DrawLocalHand();

		maxResourcesPerTurn = STARTING_MAX_RESOURCES_PER_TURN;
		currentResources = maxResourcesPerTurn;

		multiplayerGame = false;

		clientGameManager = clientSide;
	}

	public void InitializeLocalPlayer(ClientSideGameManager clientSide)
	{
		Debug.Log("Initializing player on " + (isServer ? " server." : " client."));

		hand = new List<Card>();
		DrawLocalHand();

		if (isServer)
		{
			maxResourcesPerTurn = STARTING_MAX_RESOURCES_PER_TURN;
			currentResources = maxResourcesPerTurn;
		}

		multiplayerGame = GameObject.FindObjectOfType<NetworkManager>() != null;

		clientGameManager = clientSide;
    }

	public void InitializeMultiplayerOpponent(ClientSideGameManager clientSide)
	{
		multiplayerGame = GameObject.FindObjectOfType<NetworkManager>() != null;
		clientGameManager = clientSide;

		if (isServer)
		{
			maxResourcesPerTurn = STARTING_MAX_RESOURCES_PER_TURN;
			currentResources = maxResourcesPerTurn;
		}
    }

	private void DrawLocalHand()
	{
		Debug.Log("Drawing hand for player.");

		int handSize = STARTING_HAND_SIZE;
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

		if (multiplayerGame && !isServer)
		{
			CmdAnnounceHandSize(hand.Count, 1);
		}
		else
		{
			handCount = hand.Count;
		}
	}

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

	//client side logic
	public void TryPlayCard(CardDefinition card)
	{
		if (card == null)
		{
			Debug.Log("Can't play a null card.");
			return;
		}

		if (multiplayerGame)
		{
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
		}
		else
		{
			PlayCard(card, 0);
		}
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
}
