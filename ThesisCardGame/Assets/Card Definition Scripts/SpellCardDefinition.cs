using System;
using System.Collections.Generic;

public class SpellCardDefinition : CardDefinition
{
	public int ManaCost
	{
		get
		{
			return manaCost;
		}
	}
	protected int manaCost;

	public string CardText
	{
		get
		{
			return cardText;
		}
	}
	protected string cardText;

	public Dictionary<SpellEffect, int[]> SpellEffects
	{
		get
		{
			return spellEffects;
		}
	}
	protected Dictionary<SpellEffect, int[]> spellEffects;

	public float AICardStrength
	{
		get
		{
			return aiCardStrength;
		}
	}
	private float aiCardStrength;

	public SpellCardDefinition(string cardName, int manaCost, string cardText, Dictionary<SpellEffect, int[]> spellEffects, float cardStrength) : base(cardName)
	{
		this.manaCost = manaCost;
		this.cardText = cardText;
		this.spellEffects = spellEffects;
		this.aiCardStrength = cardStrength;
	}

	public override Card GetCardInstance()
	{
		return new SpellCard(this);
	}

	public static SpellEffect StringToSpellEffect(string spellEffectCode)
	{
		switch (spellEffectCode)
		{
			case "gainlife":
				return SpellEffect.YOU_GAIN_LIFE;
			case "loselife":
				return SpellEffect.OPPONENT_LOSE_LIFE;
			case "drawcards":
				return SpellEffect.YOU_DRAW_CARDS;
		}
		return SpellEffect.NOTHING;
	}

	public static string SpellEffectToString(SpellEffect effect)
	{
		switch (effect)
		{
			case SpellEffect.YOU_GAIN_LIFE:
				return "gainlife";
			case SpellEffect.OPPONENT_LOSE_LIFE:
				return "loselife";
			case SpellEffect.YOU_DRAW_CARDS:
				return "drawcards";
			case SpellEffect.NOTHING:
				return "nothing";
		}
		return "error";
	}
}

public enum SpellEffect
{
	NOTHING,
	YOU_GAIN_LIFE,
	OPPONENT_LOSE_LIFE,
	YOU_DRAW_CARDS
}