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

	public SpellCardDefinition(string cardName, int manaCost) : base(cardName)
	{
		this.manaCost = manaCost;
	}

	public override Card GetCardInstance()
	{
		return new SpellCard(this);
	}
}