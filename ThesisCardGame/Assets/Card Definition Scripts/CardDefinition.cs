using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CardDefinition : IComparable
{
	public static int currentMaxCardID = 0;
	public static List<CardDefinition> listOfCardDefinitions;

	public int CardID
	{
		get
		{
			return cardID;
		}
	}
	protected int cardID;

	public string CardName
	{
		get
		{
			return cardName;
		}
	}
	protected string cardName;

	static CardDefinition()
	{
		listOfCardDefinitions = new List<CardDefinition>();
	}

	public CardDefinition(string cardName)
	{
		cardID = currentMaxCardID++;
		this.cardName = cardName;

		listOfCardDefinitions.Add(this);
    }

	public abstract Card GetCardInstance();

	public int CompareTo(object obj)
	{
		if (obj.GetType() == this.GetType())
		{
			CardDefinition otherCard = (CardDefinition)(obj);

			if (otherCard.cardID == cardID)
			{
				return 0;
			}
			else if (otherCard.cardID >= cardID)
			{
				return 1;
			}
			else
			{
				return -1;
			}
		}
		return -1;
	}

	public static CardDefinition GetCardDefinitionWithID(int id)
	{
		if (CardDefinition.listOfCardDefinitions[id].CardID == id)
		{
			return CardDefinition.listOfCardDefinitions[id];
		}
		else
		{
			Debug.LogError("Card in slot " + id + " does not have that ID.");
			return null;
		}
	}
}