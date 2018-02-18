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
			return new List<Card>(hand);
		}
	}
	private List<Card> hand;

	//resources mangement
	public int MaxResourcesPerTurn
	{
		get
		{
			return maxResourcesPerTurn;
		}
	}
	[SyncVar]
	private int maxResourcesPerTurn;

	public int CurrentResources
	{
		get
		{
			return currentResources;
		}
	}
	[SyncVar]
	private int currentResources;

	private bool multiplayerGame;
	private ClientSideGameManager clientGameManager;

	public void InitializeLocalPlayer()
	{
		Debug.Log("Initializing player on " + (isServer ? " server." : " client."));

		hand = new List<Card>();
		DrawLocalHand();

		maxResourcesPerTurn = 10;
		currentResources = 4;

		multiplayerGame = GameObject.FindObjectOfType<NetworkManager>() != null;

		clientGameManager = GameObject.FindObjectOfType<ClientSideGameManager>();
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
			hand.Add(library.DrawCard());
		}
	}

	public void TryPlayCard(Card card)
	{
		if (card == null)
		{
			Debug.Log("Can't play a null card.");
			return;
		}

		if (multiplayerGame)
		{
			if (isServer)
			{
				RpcPlayerPlaysSpell(0);
			}
			else
			{
				CmdPlayerPlaysSpell(1);
			}
		}

		if (card is ResourceCard)
		{
			Debug.Log("Local player playing a resource card.");
			//TODO
		}
		else if (card is SpellCard)
		{
			SpellCard spellCard = (SpellCard)card;
			if (spellCard.ManaCost > currentResources)
			{
				Debug.Log("Not enough resources to cast that spell.");
				return;
			}

			currentResources -= spellCard.ManaCost;

			if (card is CreatureCard)
			{
				Debug.Log("Local player playing a creature card.");
				//TODO
			}
			else
			{
				Debug.Log("Local player playing a spell card.");
				//TODO
			}
		}

		clientGameManager.UpdateUI();
    }

	[ClientRpc]
	private void RpcPlayerPlaysSpell(int playerNum)
	{
		Debug.Log("Player " + playerNum + " played a spell.");
	}

	[Command]
	private void CmdPlayerPlaysSpell(int playerNum)
	{
		RpcPlayerPlaysSpell(playerNum);
    }
}
