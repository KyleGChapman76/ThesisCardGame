using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameManager
{
	bool TryEndTurn();
	bool TryPlayCard(Card card);
}
