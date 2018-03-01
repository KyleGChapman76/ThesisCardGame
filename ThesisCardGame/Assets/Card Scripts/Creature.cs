public class Creature
{
	protected int power;
	protected int toughness;

	private int damageMarked;

	CreatureCardDefinition card;

	public Creature(int power, int toughness)
	{
		this.power = power;
		this.toughness = toughness;
    }
}