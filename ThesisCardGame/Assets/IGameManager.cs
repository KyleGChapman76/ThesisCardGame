using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameManager
{
	bool LocalEndTurn();
	bool TryPlayCard(Card card);
}
