using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//deck of playing cards
//can contain only one copy of any individual card
public class Deck
{
	private SortedDictionary<int, int> deck;
	private int minDeckSize;
	private int maxDeckSize;

	private int cardCount;

	//initialize a deck with restrictions on copies per card and minimum/maximum deck size
	public Deck(int minDeckSize = 60, int maxDeckSize = 120)
	{
		deck = new SortedDictionary<int, int>();
		this.minDeckSize = minDeckSize;
		this.maxDeckSize = maxDeckSize;
		cardCount = 0;
    }

	//add some number of a card to the deck
	public void AddCard(int cardID, int numberToAdd = 1)
	{
		if (numberToAdd <= 0)
		{
			Debug.LogError("Tried to put a 0/negative number of a single card into the deck.");
			return;
		}
		
		int maxCopiesOfCard = CardDefinition.GetCardDefinitionWithID(cardID).MaxCopiesInDeck;

		if (deck.ContainsKey(cardID))
		{
            if ((deck[cardID] + numberToAdd) > maxCopiesOfCard)
			{
				Debug.LogError("Putting " + numberToAdd + " cards in would make the current number " + deck[cardID].ToString() +  " go over the limit for that card: " + maxCopiesOfCard.ToString());
                return;
			}
			else
			{
				deck[cardID] += numberToAdd;
				cardCount += numberToAdd;
			}
		}
		else
		{
			if (numberToAdd > maxCopiesOfCard)
			{
				Debug.LogError("Putting " + numberToAdd + " cards in would go over the limit for that card: " + maxCopiesOfCard.ToString());
				return;
			}
			else
			{
				deck.Add(cardID, numberToAdd);
				cardCount += numberToAdd;
			}
        }
    }

	//remove all copies of a card from the deck
	public void RemoveCardCompletely(int cardID)
	{
		if (!deck.ContainsKey(cardID))
		{
			Debug.LogError("Tried to remove a card from the deck that isn't in the deck.");
			return;
		}
		else
		{
			cardCount -= deck[cardID];
            deck.Remove(cardID);
		}
	}

	//tries to export the cards in this deck as an array
	//returns true if the deck is in a valid state for exporting, and passes the array of cards into the out parameter
	//return false if the deck is in an invalid state for exporting, and passes null into the out parameter
	public bool ExportDeckToArray(out int[] deckArray)
	{
		if (cardCount < minDeckSize || cardCount > maxDeckSize)
		{
			Debug.Log("Deck not in valid size range for export. Is size " + cardCount + " and needs to be within " + minDeckSize + " to " + maxDeckSize + ".");
			deckArray = null;
			return false;
		}
		else
		{
			deckArray = new int[cardCount];
			int exportIndex = 0;

			foreach (KeyValuePair<int, int> kvp in this.deck)
			{
				for (int i = 0; i < kvp.Value; i++)
				{
					deckArray[exportIndex] = kvp.Key;
					exportIndex++;
                }
			}

			return true;
		}
	}
}
