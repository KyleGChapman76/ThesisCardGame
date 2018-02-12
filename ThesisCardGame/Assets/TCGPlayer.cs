using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//class storing information about the player's status
//including life total, their library, and their hand
public class TCGPlayer : MonoBehaviour
{
	//library management
	public const int STARTING_HAND_SIZE = 6;
	public const int MAX_HAND_SIZE = 8;

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
	private List<Card> hand;

	private HandRenderer myHandRenderer;

	//resources mangement
	private int maxResourcesPerTurn;
	private int currentResources;

	public void InitializeLocalPlayer()
	{
		Debug.Log("Initializing player.");

		hand = new List<Card>();
		DrawLocalHand();
		
		myHandRenderer = GameObject.FindGameObjectWithTag("PlayerUIArea").GetComponent<HandRenderer>();
		UpdateCardShowingUI();
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

	public void UpdateCardShowingUI()
	{
		if (myHandRenderer == null)
		{
			Debug.Log("Unable to update card showing UI since don't have access to the PlayerUIArea.");
			return;
		}

		myHandRenderer.RenderCards(hand);
    }
}
