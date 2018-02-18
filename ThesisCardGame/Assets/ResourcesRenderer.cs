using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourcesRenderer : MonoBehaviour
{
	public GameObject resourceStarPrefab;
	public Color fullStarColor;
	public Color usedStarColor;
	private GameObject[] resourceStars;

	public void Start()
	{
		resourceStars = new GameObject[TCGPlayer.MAX_MAX_RESOURCES_PER_TURN];
        for (int i = 0; i < TCGPlayer.MAX_MAX_RESOURCES_PER_TURN; i++)
		{
			GameObject newResourceStar = Instantiate(resourceStarPrefab);
			newResourceStar.transform.SetParent(gameObject.transform);
			newResourceStar.SetActive(false);
			newResourceStar.GetComponent<Image>().color = usedStarColor;
			resourceStars[i] = newResourceStar;
        }
	}
	
	public void RenderResources(int maxResourcesPerTurn, int currentResources)
	{
		for (int i = 0; i < resourceStars.Length; i++)
		{
			GameObject currentStar = resourceStars[i];
			if (i <= currentResources)
			{
				currentStar.SetActive(true);
				currentStar.GetComponent<Image>().color = fullStarColor;
            }
			else if (i <= maxResourcesPerTurn)
			{
				currentStar.SetActive(true);
				currentStar.GetComponent<Image>().color = usedStarColor;
			}
			else
			{
				currentStar.SetActive(false);
			}
		}
	}
}
