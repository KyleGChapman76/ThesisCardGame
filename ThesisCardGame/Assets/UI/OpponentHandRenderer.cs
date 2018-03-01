﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentHandRenderer : MonoBehaviour
{
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
		}
	}

	public void RenderCards(int handCount)
	{
		if (cardRenderObjects == null)
		{
			InitializeCardSlots();
		}

		Debug.Log("Rendering cards.");
		for (int i = 0; i < TCGPlayer.MAX_HAND_SIZE; i++)
		{
			GameObject cardRenderObject = cardRenderObjects[i];

			if (i < handCount)
			{
				cardRenderObject.SetActive(true);
			}
			else
			{
				cardRenderObject.SetActive(false);
            }
        }
    }
}
