using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface ICardGamePlayer
{
	int GetCurrentResources();
	int GetResourcesPerTurn();
	int GetLifeTotal();

	List<Card> GetHand();
	int GetHandCount();
	void DrawCard();
}
