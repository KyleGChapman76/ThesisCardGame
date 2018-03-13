using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandRenderer : MonoBehaviour
{
	public Canvas parentCanvas;
	public GameObject cardRenderPrefab;

	private GameObject[] cardRenderObjects;

	[SerializeField]
    private bool cardsDraggable;

	public void InitializeCardRenderObjects()
	{
		//initialize card slots
		cardRenderObjects = new GameObject[GameConstants.MAX_HAND_SIZE];
		for (int i = 0; i < GameConstants.MAX_HAND_SIZE; i++)
		{
			GameObject newCardRenderer = Instantiate(cardRenderPrefab);
			newCardRenderer.transform.SetParent(this.gameObject.transform);

			cardRenderObjects[i] = newCardRenderer;

			DraggableCard dragableCardHandler = newCardRenderer.GetComponent<DraggableCard>();
			dragableCardHandler.canvas = parentCanvas;
			dragableCardHandler.playerUIArea = this.gameObject;
		}
	}

	public void RenderCards(List<Card> hand)
	{
		if (cardRenderObjects == null)
		{
			InitializeCardRenderObjects();
        }

		Debug.Log("Rendering " + hand.Count.ToString() + " cards for player.");
		for (int i = 0; i < GameConstants.MAX_HAND_SIZE; i++)
		{
			GameObject cardRenderObject = cardRenderObjects[i];

			if (i < hand.Count)
			{
				cardRenderObject.SetActive(true);
				DraggableCard dragableCardHandler = cardRenderObject.GetComponent<DraggableCard>();
				dragableCardHandler.cardThisRenders = hand[i];
				dragableCardHandler.UpdateCardDisplay();
				dragableCardHandler.enabled = cardsDraggable;
            }
			else
			{
				cardRenderObject.SetActive(false);
            }
        }
    }

	public void SetCardsDraggable(bool draggable)
	{
		if (cardRenderObjects == null)
		{
			Debug.LogError("Card render objects not initialized when trying to set cards draggable or not.");
			return;
		}

		cardsDraggable = draggable;

		//Debug.Log("Rendering " + hand.Count.ToString() + " cards for player.");
		for (int i = 0; i < GameConstants.MAX_HAND_SIZE; i++)
		{
			GameObject cardRenderObject = cardRenderObjects[i];
			DraggableCard dragableCardHandler = cardRenderObject.GetComponent<DraggableCard>();
			dragableCardHandler.enabled = cardsDraggable;
		}
	}
}
