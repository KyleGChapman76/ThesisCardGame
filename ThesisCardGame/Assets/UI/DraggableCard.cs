using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public static DraggableCard cardBeingDragged;

	public Camera mainCamera;

	public Canvas canvas;
	public GameObject playerUIArea;
	private RectTransform thisRectTransform;
	private CanvasGroup thiscanvasGroup;

	public Card cardThisRenders;
	private GameUIManager gameUIManager;

	public Text cardName;
	public Text cardText;
	public Text cardCost;

	private int formerSiblingIndex = -1;

	public void Start()
	{
		mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		gameUIManager = GameObject.FindObjectOfType<GameUIManager>();
		thisRectTransform = GetComponent<RectTransform>();
		thiscanvasGroup = GetComponent<CanvasGroup>();
    }

	public void UpdateCardDisplay()
	{
		if (cardThisRenders == null)
		{
			Debug.LogError("Can't update card display when this draggable card has no reference to a card.");
		}
		cardName.text = cardThisRenders.BaseDefinition.CardName;
		cardText.text = (cardThisRenders is SpellCard) ? (((SpellCard)cardThisRenders).BaseDefinition.CardText).ToString() : "";
		cardCost.text = (cardThisRenders is SpellCard) ? (((SpellCard)cardThisRenders).BaseDefinition.ManaCost).ToString() : "";
		GetComponent<Image>().enabled = true;
    }

	public void OnBeginDrag(PointerEventData eventData)
	{
		//Debug.Log("Card is began being dragged.");
		cardBeingDragged = this;
		formerSiblingIndex = thisRectTransform.GetSiblingIndex();
        thisRectTransform.SetParent(canvas.transform);
		thiscanvasGroup.blocksRaycasts = false;
	}

	public void OnDrag(PointerEventData eventData)
	{
		Vector2 mouseScreenPosition = Input.mousePosition;
		Vector3 mouseViewportPosition = mainCamera.ScreenToViewportPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, 0));

		thisRectTransform.anchorMin = new Vector2(mouseViewportPosition.x, mouseViewportPosition.y);
		thisRectTransform.anchorMax = new Vector2(mouseViewportPosition.x, mouseViewportPosition.y);
		thisRectTransform.anchoredPosition = Vector2.zero;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		//Debug.Log("Card is stopped being dragged.");
		thisRectTransform.SetParent(playerUIArea.transform);
		thisRectTransform.SetSiblingIndex(formerSiblingIndex);
        thiscanvasGroup.blocksRaycasts = true;

		cardBeingDragged = null;
    }

	public void CardDroppedInPlayArea()
	{
		GetComponent<Image>().enabled = false;
		StartCoroutine("PlayCardOnceMovedBackSafely");
	}

	private IEnumerator PlayCardOnceMovedBackSafely()
	{
		while (cardBeingDragged)
		{
			yield return null;
		}

		if (!gameUIManager.TryPlayCard(cardThisRenders))
		{
			GetComponent<Image>().enabled = true;
		}
	}
}
