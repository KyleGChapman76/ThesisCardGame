using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//deck of playing cards
//can contain only one copy of any individual card
public class Deck
{
	private SortedDictionary<Card, int> deck;
	private int minDeckSize;
	private int maxDeckSize;
	private int maxCopiesPerCard;

	private int cardCount;

	//initialize a deck with restrictions on copies per card and minimum/maximum deck size
	public Deck(int minDeckSize = 60, int maxDeckSize = 120, int maxCopiesPerCard = 4)
	{
		deck = new SortedDictionary<Card, int>();
		this.maxCopiesPerCard = maxCopiesPerCard;
		this.minDeckSize = minDeckSize;
		this.maxDeckSize = maxDeckSize;
		cardCount = 0;
    }

	//add some number of a card to the deck
	public void AddCard(Card card, int numberToAdd = 1)
	{
		if (numberToAdd <= 0)
		{
			Debug.Log("Tried to put a 0/negative number of a single card into the deck.");
			return;
		}

		if (deck.ContainsKey(card))
		{
			if ((deck[card] + numberToAdd) > maxCopiesPerCard)
			{
				Debug.Log("Putting that many cards in would go over the limit for a single card into the deck.");
				return;
			}
			else
			{
				deck[card] += numberToAdd;
				cardCount += numberToAdd;
			}
		}
		else
		{
			deck.Add(card, numberToAdd);
			cardCount += numberToAdd;
        }
    }

	//remove all copies of a card from the deck
	public void RemoveCardCompletely(Card card)
	{
		if (!deck.ContainsKey(card))
		{
			Debug.Log("Tried to remove a card from the deck that isn't in the deck.");
			return;
		}
		else
		{
			deck.Remove(card);
		}
	}

	//tries to export the cards in this deck as an array
	//returns true if the deck is in a valid state for exporting, and passes the array of cards into the out parameter
	//return false if the deck is in an invalid state for exporting, and passes null into the out parameter
	public bool ExportDeckToList(out List<Card> list)
	{
		if (cardCount < minDeckSize || cardCount > maxDeckSize)
		{
			Debug.Log("Deck not in valid size range.");
			list = null;
			return false;
		}
		else
		{
			list = new List<Card>();

			foreach (KeyValuePair<Card, int> kvp in deck)
			{
				for (int i = 0; i < kvp.Value; i++)
				{
					list.Add(kvp.Key);
				}
			}

			return true;
		}
	}
}
