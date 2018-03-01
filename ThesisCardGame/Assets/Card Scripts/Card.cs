using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Card
{
	public CardDefinition BaseDefinition
	{
		get
		{
			return baseDefinition;
		}
	}
    protected CardDefinition baseDefinition;
}
