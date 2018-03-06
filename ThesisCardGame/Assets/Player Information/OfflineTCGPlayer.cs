using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//class storing and communicating information about the player's status
//including life total, their library, and their hand
public class OfflineTCGPlayer : MonoBehaviour, ICardGamePlayer
{
	//library management
	public const int STARTING_HAND_SIZE = 6;
	public const int MAX_HAND_SIZE = 8;
	public const int MAX_MAX_RESOURCES_PER_TURN = 12;
	public const int STARTING_MAX_RESOURCES_PER_TURN = 2;

	private Library library;

	private List<Card> hand;

	private int handCount;

	private int maxResourcesPerTurn;
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

		if (handCount >= MAX_HAND_SIZE)
		{
			Debug.Log("Can't draw a card, due to maximum hand size.");
			return;
		}

		Card cardDrawn;
		if (library.DrawCard(out cardDrawn))
		{
			hand.Add(cardDrawn);
			handCount = hand.Count;
			clientGameManager.UpdateUI();
		}
		else
		{
			Debug.LogError("Could not draw card.");
		}
	}

	public void ResetResources()
	{
		Debug.Log("Resetting resources.");
		currentResources = maxResourcesPerTurn;
		clientGameManager.UpdateUI();
	}

	/*** Turn Structure Functions ***/

	public bool TryStartTurn()
	{
		hasPlayedAResourceThisTurn = false;
		ResetResources();
		DrawCard();

		return true;
    }

	public bool TryEndTurn()
	{
		//TODO
		return true;
	}

	/*** Initialization Functions ***/

	public void InitializePlayer(ClientSideGameManager clientSide, bool isOpponent)
	{
		if (library == null)
		{
			Debug.LogError("Can't initialize offline player when library hasn't been set!");
			return;
		}

		library.Shuffle(ClientSideGameManager.rand);

		if (isOpponent)
		{
			InitializeAIOpponent(clientSide);
		}
		else
		{
			InitializeSinglePlayer(clientSide);
		}
	}

	private void InitializeSinglePlayer(ClientSideGameManager clientSide)
	{
		Debug.Log("Initializing single player.");

		hand = new List<Card>();
		DrawLocalHand();

		maxResourcesPerTurn = STARTING_MAX_RESOURCES_PER_TURN;
		currentResources = maxResourcesPerTurn;

		clientGameManager = clientSide;
	}

	private void InitializeAIOpponent(ClientSideGameManager clientSide)
	{
		Debug.Log("Initializing AI opponent (except not really).");

		hand = new List<Card>();
		DrawLocalHand();

		maxResourcesPerTurn = STARTING_MAX_RESOURCES_PER_TURN;
		currentResources = maxResourcesPerTurn;

		clientGameManager = clientSide;

		gameObject.AddComponent<AIOpponent>();
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

		handCount = hand.Count;
	}

	//client side logic
	public bool TryPlayCard(Card card)
	{
		if (card == null)
		{
			Debug.Log("Can't play a null card.");
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

			maxResourcesPerTurn += ((ResourceCard)card).BaseDefinition.ResourcesGiven;
			hasPlayedAResourceThisTurn = true;

			hand.Remove(card);
			handCount = hand.Count;
		}
		else if (card is SpellCard)
		{
			SpellCard spellCard = (SpellCard)card;
			if (spellCard.BaseDefinition.ManaCost > currentResources)
			{
				Debug.Log("Player doesn't have enough resources (current = " + currentResources.ToString() + ") to cast that spell with cost: " + spellCard.BaseDefinition.ManaCost.ToString());
				return false;
			}

			currentResources -= spellCard.BaseDefinition.ManaCost;

			if (card is CreatureCard)
			{
				Debug.Log("Player playing a creature card.");
				//TODO

				hand.Remove(card);
				handCount = hand.Count;
			}
			else
			{
				Debug.Log("Player playing a spell card.");
				//TODO

				hand.Remove(card);
				handCount = hand.Count;
			}
		}

		clientGameManager.UpdateUI();
		return true;
	}
}
