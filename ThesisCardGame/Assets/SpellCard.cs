public class SpellCard : Card
{
	protected int manaCost;

	public SpellCard(int cardID, int manaCost) : base(cardID)
	{
		this.manaCost = manaCost;
	}
}