using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureCard : SpellCard
{
	public new CreatureCardDefinition BaseDefinition
	{
		get
		{
			return (CreatureCardDefinition)baseDefinition;
		}
	}

	public CreatureCard (CreatureCardDefinition creatureCardDefinition) : base(creatureCardDefinition)
	{
		baseDefinition = creatureCardDefinition;
	}
}
