using System;
using System.Collections.Generic;
using UnityEngine;

public class Library
{
	private List<Card> cards;

	public Library(int[] arrayOfCardDefIDs)
	{
		cards = new List<Card>();

		for (int i = 0; i < arrayOfCardDefIDs.Length; i++)
		{
			CardDefinition definition = CardDefinition.GetCardDefinitionWithID(arrayOfCardDefIDs[i]);
			Card instance = definition.GetCardInstance();
            cards.Add(instance);
        }
	}

	//Taken from https://stackoverflow.com/questions/273313/randomize-a-listt
	public void Shuffle(System.Random random)
	{
		int n = cards.Count;
		while (n > 1)
		{
			n--;
			int k = random.Next(n + 1);
			Card value = cards[k];
			cards[k] = cards[n];
			cards[n] = value;
		}
	}

	public bool DrawCard(out Card cardDrawn)
	{
		if (cards.Count == 0)
		{
			cardDrawn = null;
            return false;
		}

		Card topCard = cards[0];
		cards.RemoveAt(0);
		cardDrawn = topCard;
		return true;
    }
}