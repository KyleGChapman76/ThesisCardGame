using System;

public abstract class Card : IComparable
{
	protected int cardID;

	public Card(int cardID)
	{
		this.cardID = cardID;
    }

	public int CompareTo(object obj)
	{
		if (obj.GetType() == this.GetType())
		{
			Card otherCard = (Card)(obj);

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
}