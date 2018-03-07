using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCardDefinition : CardDefinition
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

	public ResourceCardDefinition(string cardName, int resourcesGiven, int thresholdType) : base(cardName, 100)
	{
		this.resourcesGiven = resourcesGiven;
		this.thresholdType = thresholdType;
    }

	public override Card GetCardInstance()
	{
		return new ResourceCard(this);
	}
}
