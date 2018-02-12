using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableCard : MonoBehaviour, IPointerDownHandler
{
	public Camera mainCamera;
	private GameObject dragDropCardRender;
	private RectTransform dragDropCardRenderRectTransform;
	public Canvas canvas;
	public MouseHover playAreaMouseOver;

	public float dragDropSizeModifier;
	public float dragDropTransparency;

	public Card cardThisRenders;
	private TCGGameManager tcgGameManager;

	public void Start()
	{
		//create the DragDrop card render as a copy of this
		dragDropCardRender = Instantiate(this.gameObject);
		dragDropCardRender.transform.SetParent(canvas.transform);
		
		//disable or modify some aspects of the dragdrop clone
		Destroy(dragDropCardRender.GetComponent<DraggableCard>());
		Image dragDropImage = dragDropCardRender.GetComponent<Image>();
		dragDropImage.raycastTarget = false;
		dragDropImage.color = new Color(dragDropImage.color.r, dragDropImage.color.g, dragDropImage.color.b, dragDropTransparency);

		//set DragDrop recttransform settings
		dragDropCardRenderRectTransform = dragDropCardRender.GetComponent<RectTransform>();
		dragDropCardRenderRectTransform.sizeDelta *= dragDropSizeModifier;

		//disable dragdrop render by default
		dragDropCardRender.SetActive(false);

		mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		playAreaMouseOver = GameObject.FindGameObjectWithTag("PlayArea").GetComponent<MouseHover>();
		tcgGameManager = GameObject.FindObjectOfType<TCGGameManager>();
    }

	public void OnPointerDown(PointerEventData eventData)
	{
		dragDropCardRender.SetActive(true);

		StopCoroutine("UpdateDraggableCard");
		StartCoroutine("UpdateDraggableCard");
	}

	private IEnumerator UpdateDraggableCard()
	{
		for (;;)
		{
			Vector2 mouseScreenPosition = Input.mousePosition;
			Vector3 mouseViewportPosition = mainCamera.ScreenToViewportPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, 0));

			dragDropCardRenderRectTransform.anchorMin = new Vector2(mouseViewportPosition.x, mouseViewportPosition.y);
			dragDropCardRenderRectTransform.anchorMax = new Vector2(mouseViewportPosition.x, mouseViewportPosition.y);
			dragDropCardRenderRectTransform.anchoredPosition = Vector2.zero;

			//if mouse no longer held down, stop dragging
			if (!Input.GetMouseButton(0))
			{
				dragDropCardRender.SetActive(false);

				//card has been dragged into play area
				if (playAreaMouseOver.isHoveringOverThis)
				{
					tcgGameManager.TryPlayCard(cardThisRenders);
                }

				break;
			}

			yield return null;
		}
    }

}
