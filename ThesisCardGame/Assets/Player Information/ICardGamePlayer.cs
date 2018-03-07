using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface ICardGamePlayer
{
	int GetCurrentResources();
	int GetResourcesPerTurn();
	void ResetResources();

	List<Card> GetHand();
	int GetHandCount();
	void DrawCard();

	bool TryPlayCard(Card card);

	bool TryStartTurn();
	bool TryEndTurn();
}
