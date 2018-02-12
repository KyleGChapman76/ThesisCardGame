using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCard : Card
{
	int resourcesGiven;
	int thresholdType;

	public ResourceCard(int cardID, int resourcesGiven, int thresholdType) : base(cardID)
	{
		this.resourcesGiven = resourcesGiven;
		this.thresholdType = thresholdType;
    }
}
