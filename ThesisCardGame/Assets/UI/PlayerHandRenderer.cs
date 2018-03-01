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
		cardRenderObjects = new GameObject[TCGPlayer.MAX_HAND_SIZE];
		for (int i = 0; i < TCGPlayer.MAX_HAND_SIZE; i++)
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

		Debug.Log("Rendering cards.");
		for (int i = 0; i < TCGPlayer.MAX_HAND_SIZE; i++)
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
}
