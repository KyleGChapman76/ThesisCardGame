using System;
using System.Collections.Generic;

public class Library
{
	private List<Card> cards;

	public Library(List<Card> cards)
	{
		this.cards = cards;
	}

	//Taken from https://stackoverflow.com/questions/273313/randomize-a-listt
	public void Shuffle(Random random)
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

	public Card DrawCard()
	{
		Card topCard = cards[0];
		cards.RemoveAt(0);
		return topCard;
    }
}