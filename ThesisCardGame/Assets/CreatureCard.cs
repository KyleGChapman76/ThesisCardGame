public class CreatureCard : SpellCard
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

	public CreatureCard(int cardID, int manaCost, int power, int toughness) : base(cardID, manaCost)
	{
		this.power = power;
		this.toughness = toughness;
	}
}