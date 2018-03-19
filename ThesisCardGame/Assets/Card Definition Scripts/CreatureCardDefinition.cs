using System.Collections.Generic;

public class CreatureCardDefinition : SpellCardDefinition
{
	public int Power
	{
		get
		{
			return power;
		}
	}
	protected int power;

	public int Toughness
	{
		get
		{
			return toughness;
		}
	}
	protected int toughness;

	public CreatureCardDefinition(string cardName, int manaCost, string cardText, int power, int toughness, float cardStrength) : base(cardName, manaCost, cardText, null, cardStrength)
	{
		this.power = power;
		this.toughness = toughness;
	}

	public override Card GetCardInstance()
	{
		return new CreatureCard(this);
	}
}