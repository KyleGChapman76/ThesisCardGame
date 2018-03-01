using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellCard : Card
{
	public new SpellCardDefinition BaseDefinition
	{
		get
		{
			return (SpellCardDefinition)baseDefinition;
		}
	}

	public SpellCard(SpellCardDefinition spellCardDefinition)
	{
		baseDefinition = spellCardDefinition;
	}
}
