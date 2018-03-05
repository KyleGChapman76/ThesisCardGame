using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandRenderer : MonoBehaviour
{
	public Canvas parentCanvas;
	public GameObject cardRenderPrefab;

	private GameObject[] cardRenderObjects;

	private void InitializeCardSlots()
	{
		cardRenderObjects = new GameObject[ClientSideGameManager.MAX_HAND_SIZE];
		for (int i = 0; i < ClientSideGameManager.MAX_HAND_SIZE; i++)
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
			InitializeCardSlots();
		}

		Debug.Log("Rendering " + hand.Count.ToString() + " cards for player.");
		for (int i = 0; i < ClientSideGameManager.MAX_HAND_SIZE; i++)
		{
			GameObject cardRenderObject = cardRenderObjects[i];

			if (i < hand.Count)
			{
				cardRenderObject.SetActive(true);
				DraggableCard dragableCardHandler = cardRenderObject.GetComponent<DraggableCard>();
				dragableCardHandler.cardThisRenders = hand[i];
				dragableCardHandler.UpdateCardDisplay();
			}
			else
			{
				cardRenderObject.SetActive(false);
            }
        }
    }

	public void SetCardsDraggable(bool cardsDraggable)
	{
		if (cardRenderObjects == null)
		{
			Debug.LogError("Can't set the draggability of the player's cards when there are no card render objects.");
			return;
		}

		foreach (GameObject cardRenderObject in cardRenderObjects)
		{
			cardRenderObject.GetComponent<DraggableCard>().enabled = cardsDraggable;
        }

	}

}
