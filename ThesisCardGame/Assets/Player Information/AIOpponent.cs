using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIOpponent : MonoBehaviour
{
	private OfflineTCGPlayer tcgPlayer;
	private GameUIManager uiManager;

	private void Start()
	{
		LocalGameManager.aiTurnBegins += AITurnBegins;
		tcgPlayer = GetComponent<OfflineTCGPlayer>();
    }

    private void AITurnBegins(GameUIManager gameManager)
	{
		Debug.Log("AI begins taking its turn.");

		this.uiManager = gameManager;

		StartCoroutine("EndTurnAfterFiveSeconds");
	}

	private IEnumerator EndTurnAfterFiveSeconds()
	{
		yield return new WaitForSeconds(5);

		Debug.Log("AI ends its turn.");

		uiManager.TryEndTurn();
    }

}
