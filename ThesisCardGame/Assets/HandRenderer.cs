using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandRenderer : MonoBehaviour
{
	public Canvas parentCanvas;
	public GameObject cardRenderPrefab;
	public GameObject faceDownCardRenderPrefab;
	private GameObject[] cardRenderObjects;

	public void RenderCards(List<Card> hand, bool faceUp = true)
	{
		cardRenderObjects = new GameObject[hand.Count];
		for (int i = 0; i < hand.Count; i++)
		{
			GameObject properPrefab = cardRenderPrefab;
			if (!faceUp)
				properPrefab = faceDownCardRenderPrefab;

			GameObject newCardRenderer = Instantiate(properPrefab);
			newCardRenderer.transform.SetParent(this.gameObject.transform);

			//TODO shape and transform cardRender objects to display nicely on screen
			newCardRenderer.GetComponent<RectTransform>().anchoredPosition = new Vector2(500-200f * i, 0f);

			cardRenderObjects[i] = newCardRenderer;

			DraggableCard dragableCardHandler = newCardRenderer.GetComponent<DraggableCard>();
			dragableCardHandler.canvas = parentCanvas;
			dragableCardHandler.cardThisRenders = hand[i];
        }
    }
}
