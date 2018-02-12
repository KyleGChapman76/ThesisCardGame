using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//taken from https://gamedev.stackexchange.com/questions/125986/how-can-i-get-ui-element-over-which-pointer-is-in-unity3d
public class MouseHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[HideInInspector]
	public bool isHoveringOverThis;

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (eventData.pointerCurrentRaycast.gameObject != null)
		{
			isHoveringOverThis = true;
        }
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		isHoveringOverThis = false;
    }
}
