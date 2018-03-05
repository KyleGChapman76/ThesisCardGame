using System;

public class SpellCardDefinition : CardDefinition
{
	public int ManaCost
	{
		get
		{
			return manaCost;
		}
	}
	protected int manaCost;

	public string CardText
	{
		get
		{
			return cardText;
		}
	}
	protected string cardText;

	public SpellCardDefinition(string cardName, int manaCost, string cardText) : base(cardName)
	{
		this.manaCost = manaCost;
		this.cardText = cardText;
    }

	public override Card GetCardInstance()
	{
		return new SpellCard(this);
	}
}