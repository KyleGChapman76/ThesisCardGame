using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//class storing and communicating information about the player's status
//including life total, their library, and their hand
public class OnlineTCGPlayer : NetworkBehaviour, ICardGamePlayer
{
	public GameObject serverGameManagerPrefab;

	private Library library;

	private List<Card> hand;

	[SyncVar(hook = "HandCountChanged")]
	private int handCount;

	private Card cardTryingToPlay;

	[SyncVar(hook = "MaxResourcesPerTurnChanged")]
	private int maxResourcesPerTurn;

	[SyncVar(hook = "CurrentResourcesChanged")]
	private int currentResources;

	[SyncVar(hook = "HasPlayedAResourceThisTurnChanged")]
	private bool hasPlayedAResourceThisTurn;

	[SyncVar(hook = "LifetotalChanged")]
	private int lifeTotal;

	private void Start()
	{
		if (isServer && isLocalPlayer)
		{
			Debug.Log("Server spawned player starts, instantiating game manager.");
			GameObject serverGameManagerInstance = Instantiate(serverGameManagerPrefab);
			NetworkServer.Spawn(serverGameManagerInstance);
		}
	}

	private void Update()
	{
		if (!isLocalPlayer)
		{
			return;
		}

		//get debug data for local player
		if (Input.GetKeyUp(KeyCode.R))
		{
			if (isServer)
			{
				ResetResources();
			}
			else
			{
				CmdDebugResetResources();
			}
		}
		if (Input.GetKeyUp(KeyCode.D))
		{
			LocalSideDrawCard();
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

	public int GetLifeTotal()
	{
		return lifeTotal;
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
		library.Shuffle(LocalGameManager.rand);

		hand = new List<Card>();
		DrawLocalHand();

		if (!isServer)
		{
			CmdInitializeQualities();
		}
		else
		{
			ServerInitializeQualities();
		}
	}

	private void ServerInitializeQualities()
	{
		if (!isServer)
		{
			Debug.LogError("ServerInitiailizeQualities should only be run on the server.");
			return;
		}

		maxResourcesPerTurn = GameConstants.STARTING_MAX_RESOURCES_PER_TURN;
		currentResources = maxResourcesPerTurn;
		lifeTotal = GameConstants.STARTING_LIFE_TOTAL;
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

	/*** Server Side Interface ***/

	public void DrawCard()
	{
		if (!isServer)
		{
			Debug.LogError("Can't tell a player to draw a card except on the server.");
			return;
		}

		if (isLocalPlayer)
		{
			LocalSideDrawCard();
		}
		else
		{
			RpcDrawCard();
		}
	}

	public void ResetResources()
	{
		if (!isServer)
		{
			Debug.LogError("Can't tell a player to reset their resources except on the server.");
			return;
		}

		currentResources = maxResourcesPerTurn;
		hasPlayedAResourceThisTurn = false;
	}

	public bool PlayCard(Card card)
	{
		if (card == null)
		{
			Debug.Log("Can't play a null card.");
			return false;
		}

		//locally check if we are allowed to play the card
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

		cardTryingToPlay = card;
		if (isServer)
		{
			RpcPlayerSuccessfullyPlaysCard(card.BaseDefinition.CardID, true);
			return ServerSidePlayCard(card.BaseDefinition, true);
		}
		else
		{
			CmdPlayerPlaysCard(card.BaseDefinition.CardID, false);
		}

		return true;
	}

	public void ChangeLifeTotal(int lifeTotalChange)
	{
		if (!isServer)
		{
			Debug.LogError("Can't tell a player to change their life total except on the server.");
			return;
		}

		if (isServer)
		{
			ServerSideLifeTotalChange(lifeTotalChange);
		}
		else
		{
			CmdChangeLifeTotal(lifeTotalChange);
		}
	}

	/*** Server Side Actions ***/

	private bool ServerSidePlayCard(CardDefinition cardDefinition, bool playedByHost)
	{
		if (!isServer)
		{
			Debug.LogError("Server side play card ran on not the server.");
			return false;
		}

		if (cardDefinition is ResourceCardDefinition)
		{
			Debug.Log("Player playing a resource card.");

			if (hasPlayedAResourceThisTurn)
			{
				Debug.Log("Player has already played a resource card this turn.");
				return false;
			}

			maxResourcesPerTurn += ((ResourceCardDefinition)cardDefinition).ResourcesGiven;
			currentResources += ((ResourceCardDefinition)cardDefinition).ResourcesGiven;
			hasPlayedAResourceThisTurn = true;
        }
		else if (cardDefinition is SpellCardDefinition)
		{
			SpellCardDefinition spellCard = (SpellCardDefinition)cardDefinition;
			if (spellCard.ManaCost > currentResources)
			{
				Debug.Log("Player doesn't have enough resources (current = " + currentResources.ToString() + ") to cast that spell with cost: " + spellCard.ManaCost.ToString());
				return false;
			}

			currentResources -= spellCard.ManaCost;

			if (cardDefinition is CreatureCardDefinition)
			{
				Debug.Log("Player playing a creature card.");
				NetworkedGameManager.SingletonGameManager.PlayCreature((CreatureCardDefinition)cardDefinition, playedByHost);
			}
			else
			{
				Debug.Log("Player playing a spell card.");
				NetworkedGameManager.SingletonGameManager.PlaySpell((SpellCardDefinition)cardDefinition, playedByHost);
			}
		}

		if (isLocalPlayer)
		{
			LocalSideRemovedPlayedCardFromHand();
		}
		else
		{
			RpcRemovePlayedCardFromHand();
		}

		RpcPlayerSuccessfullyPlaysCard(cardDefinition.CardID, playedByHost);

		return true;
	}

	private void ServerSideLifeTotalChange(int lifeTotalChange)
	{
		if (!isServer)
		{
			Debug.LogError("Can't change life total except on server.");
			return;
        }
		lifeTotal += lifeTotalChange;

		if (lifeTotal <= 0)
		{
			Debug.Log("Player life total went below 0.");
			NetworkedGameManager.SingletonGameManager.ReportGameLoss(isLocalPlayer);
		}
	}

	/*** Local Side Actions ***/

	private void LocalSideDrawCard()
	{
		if (!isLocalPlayer)
		{
			Debug.LogError("Can't draw a card when not the local machine of this player.");
			return;
		}

		if (handCount >= GameConstants.MAX_HAND_SIZE)
		{
			Debug.Log("Can't draw a card, due to maximum hand size.");
			return;
		}

		Card cardDrawn;
		if (library.DrawCard(out cardDrawn))
		{
			hand.Add(cardDrawn);

			if (isServer)
			{
				handCount = hand.Count;
			}
			else
			{
				CmdAnnounceHandSize(hand.Count, 1);
			}
		}
		else
		{
			Debug.LogError("Could not draw card from library.");
		}
	}

	private void LocalSideRemovedPlayedCardFromHand()
	{
		if (!isLocalPlayer)
		{
			Debug.LogError("Can't run LocalSideRemovedPlayedCardFromHand not on local side.");
			return;
		}

		if (cardTryingToPlay == null)
		{
			Debug.LogError("Not trying to play a card currently, so can't removed a played card from our hand.");
			return;
		}

		hand.Remove(cardTryingToPlay);
		cardTryingToPlay = null;

		if (isServer)
		{
			handCount = hand.Count;
		}
		else
		{
			CmdAnnounceHandSize(hand.Count, 1);
		}

	}

	/*** RPCS ***/

	[ClientRpc]
	private void RpcDrawCard()
	{
		if (!isLocalPlayer)
		{
			Debug.Log("Can't draw a card if we aren't the local player.");
			return;
		}

		LocalSideDrawCard();
	}

	[ClientRpc]
	private void RpcRemovePlayedCardFromHand()
	{
		if (!isLocalPlayer)
		{
			Debug.Log("Can't removed a played card from our hand if we aren't the local player.");
			return;
		}

		LocalSideRemovedPlayedCardFromHand();
	}

	//server tells the client that a card of a certain ID has been played
	[ClientRpc]
	private void RpcPlayerSuccessfullyPlaysCard(int cardID, bool playedByHost)
	{
		CardDefinition card = CardDefinition.GetCardDefinitionWithID(cardID);

		NetworkedGameManager.SingletonGameManager.PlayerStatusChanged();
	}

	/*** Commands ***/

	//client tells the server that it wants to be initialized
	[Command]
	private void CmdInitializeQualities()
	{
		Debug.Log("Player wants their qualities initialized.");
		ServerInitializeQualities();
    }

	//client tells the server that it wants to play a card
	[Command]
	private void CmdPlayerPlaysCard(int cardID, bool playedByHost)
	{
		CardDefinition card = CardDefinition.GetCardDefinitionWithID(cardID);
		if (ServerSidePlayCard(card, playedByHost))
		{
			RpcPlayerSuccessfullyPlaysCard(cardID, playedByHost);
		}
	}

	//client tells the server that it wants to reset its resources (DEBUG)
	[Command]
	private void CmdDebugResetResources()
	{
		currentResources = maxResourcesPerTurn;
	}

	//client tells the server that its hand size has changed
	[Command]
	private void CmdAnnounceHandSize(int handSize, int playerNum)
	{
		handCount = handSize;
	}

	[Command]
	private void CmdChangeLifeTotal(int lifeTotalChange)
	{
		ServerSideLifeTotalChange(lifeTotalChange);
	}

	/*** SyncVar Hooks ***/

	private void CurrentResourcesChanged(int value)
	{
		currentResources = value;
		//Debug.Log("CURRENT RESOURCES HOOK: " + value);

		NetworkedGameManager.SingletonGameManager.PlayerStatusChanged();
	}

	private void MaxResourcesPerTurnChanged(int value)
	{
		maxResourcesPerTurn = value;
		//Debug.Log("MAX RESOURCES HOOK: " + value);

		NetworkedGameManager.SingletonGameManager.PlayerStatusChanged();
	}

	private void HasPlayedAResourceThisTurnChanged(bool value)
	{
		hasPlayedAResourceThisTurn = value;
		//Debug.Log("HAS PLAYED A RESOURCE THIS TURN HOOK: " + value);

		NetworkedGameManager.SingletonGameManager.PlayerStatusChanged();
	}

	private void HandCountChanged(int value)
	{
		handCount = value;
		//Debug.Log("HAND COUNT HOOK: " + value);

		NetworkedGameManager.SingletonGameManager.PlayerStatusChanged();
    }

	private void LifetotalChanged(int value)
	{
		lifeTotal = value;
		//Debug.Log("LIFE TOTAL HOOK: " + value);

		NetworkedGameManager.SingletonGameManager.PlayerStatusChanged();
	}
}
