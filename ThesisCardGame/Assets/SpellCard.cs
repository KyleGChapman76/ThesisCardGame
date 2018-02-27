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

	public SpellCard(int cardID, string cardName, int manaCost) : base(cardID, cardName)
	{
		this.manaCost = manaCost;
	}
}