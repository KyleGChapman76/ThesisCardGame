public class CreatureCard : SpellCard
{
	protected int power;
	protected int toughness;

	public CreatureCard(int cardID, int manaCost, int power, int toughness) : base(cardID, manaCost)
	{
		this.power = power;
		this.toughness = toughness;
	}
}