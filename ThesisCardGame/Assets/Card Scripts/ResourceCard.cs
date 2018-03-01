using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCard : Card
{
	public new ResourceCardDefinition BaseDefinition
	{
		get
		{
			return (ResourceCardDefinition)baseDefinition;
		}
	}

	public ResourceCard(ResourceCardDefinition resourceCardDefinition)
	{
		baseDefinition = resourceCardDefinition;
	}
}
