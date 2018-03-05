using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIOpponent : MonoBehaviour
{
	private OfflineTCGPlayer tcgPlayer;
	private ClientSideGameManager gameManager;

	private void Start()
	{
		ClientSideGameManager.aiTurnBegins += AITurnBegins;
		tcgPlayer = GetComponent<OfflineTCGPlayer>();
    }

    private void AITurnBegins(ClientSideGameManager gameManager)
	{
		Debug.Log("AI begins taking its turn.");

		this.gameManager = gameManager;
		tcgPlayer.StartTurn();

		StartCoroutine("EndTurnAfterFiveSeconds");
	}

	private IEnumerator EndTurnAfterFiveSeconds()
	{
		yield return new WaitForSeconds(5);

		Debug.Log("AI ends its turn.");

		gameManager.TryEndTurn();
    }

}
