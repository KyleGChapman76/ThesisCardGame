using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface ICardGamePlayer
{
	void InitializePlayer(ClientSideGameManager clientSide, bool isOpponent);
	bool TryPlayCard(Card card);
	void SetLibrary(Library library);
	Library GetLibrary();
	int GetCurrentResources();
	int GetResourcesPerTurn();
	void ResetResources();
	void StartTurn();
	List<Card> GetHand();
	int GetHandCount();
	void DrawCard();
}
