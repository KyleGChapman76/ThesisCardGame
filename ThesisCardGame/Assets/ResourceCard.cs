using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCard : Card
{
	public int ResourcesGiven
	{
		get
		{
			return resourcesGiven;
		}
	}
	protected int resourcesGiven;

	public int ThresholdType
	{
		get
		{
			return thresholdType;
		}
	}
	protected int thresholdType;

	public ResourceCard(int cardID, string cardName, int resourcesGiven, int thresholdType) : base(cardID, cardName)
	{
		this.resourcesGiven = resourcesGiven;
		this.thresholdType = thresholdType;
    }
}
