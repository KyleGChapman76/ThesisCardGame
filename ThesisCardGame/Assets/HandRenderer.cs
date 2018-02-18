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
		Debug.Log("Rendering cards.");
		cardRenderObjects = new GameObject[hand.Count];
		for (int i = 0; i < hand.Count; i++)
		{
			GameObject properPrefab = cardRenderPrefab;
			if (!faceUp)
				properPrefab = faceDownCardRenderPrefab;

			GameObject newCardRenderer = Instantiate(properPrefab);
			newCardRenderer.transform.SetParent(this.gameObject.transform);

			cardRenderObjects[i] = newCardRenderer;

			DraggableCard dragableCardHandler = newCardRenderer.GetComponent<DraggableCard>();
			dragableCardHandler.canvas = parentCanvas;
			dragableCardHandler.playerUIArea = this.gameObject;
			dragableCardHandler.cardThisRenders = hand[i];
        }
    }
}
