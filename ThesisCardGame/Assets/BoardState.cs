using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardState : MonoBehaviour
{
	public int maxCreatureSlots;

	private Creature[] yourCreatures;
	private Creature[] opponentCreatures;

	public void Start()
	{
		yourCreatures = new Creature[maxCreatureSlots];
		opponentCreatures = new Creature[maxCreatureSlots];
	}
}
