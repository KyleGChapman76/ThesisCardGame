public class Creature
{
	protected int power;
	protected int toughness;

	private int damageMarked;

	CreatureCard card;

	public Creature(int power, int toughness)
	{
		this.power = power;
		this.toughness = toughness;
    }
}