public class SpellCard : Card
{
	public int ManaCost
	{
		get
		{
			return manaCost;
		}
	}
	protected int manaCost;

	public SpellCard(int cardID, int manaCost) : base(cardID)
	{
		this.manaCost = manaCost;
	}
}