using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayArea : MonoBehaviour, IDropHandler
{
	public void OnDrop(PointerEventData eventData)
	{
		Debug.Log("Drop into play area.");
		if (DraggableCard.cardBeingDragged != null)
		{
			DraggableCard.cardBeingDragged.CardDroppedInPlayArea();
		}
	}
}
