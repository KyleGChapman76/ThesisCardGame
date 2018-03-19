using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIOpponent : MonoBehaviour
{
	private OfflineTCGPlayer tcgPlayer;
	private GameUIManager uiManager;

	private void Start()
	{
		LocalGameManager.aiTurnBegins += AITurnBegins;
		tcgPlayer = GetComponent<OfflineTCGPlayer>();
    }

    private void AITurnBegins(GameUIManager gameManager)
	{
		Debug.Log("AI begins taking its turn.");

		this.uiManager = gameManager;

		StartCoroutine("MakeDecisions");
		StartCoroutine("EndTurnAfterOneMinute");
	}

	private IEnumerator MakeDecisions()
	{
		yield return new WaitForSeconds(Random.Range(2f, 3f));

		for (;;)
		{
			Debug.Log("AI making a decision.");

			//no decisions to make if hand empty
			if (tcgPlayer.GetHandCount() == 0)
			{
				break;
			}

			List<Card> hand = tcgPlayer.GetHand();

			Card cardToPlay = null;

			if (!tcgPlayer.HasPlayedAResourceThisTurn())
			{
				for (int i = 0; i < hand.Count && cardToPlay == null; i++)
				{
					Card card = hand[i];
					if (card is ResourceCard)
					{
						Debug.Log("AI decided to play a resource card.");
						cardToPlay = card;
                    }
				}
            }
		
			//if haven't decided on a resource to play, try and play a nonresource
			if(cardToPlay == null)
			{
				Dictionary<Card, float> possiblePlaysDebugInfo = new Dictionary<Card, float>();

				float maxStrength = -15;
				foreach (Card card in hand)
				{
					if (card is SpellCard)
					{
						SpellCard spellCard = (SpellCard)card;

						if (spellCard.BaseDefinition.ManaCost > tcgPlayer.GetCurrentResources())
						{
							continue;
						}

						float newEvaluation = evaluateStrengthOfCard(spellCard);

						possiblePlaysDebugInfo.Add(card, newEvaluation);

						if (newEvaluation > maxStrength)
						{
							cardToPlay = card;
                            maxStrength = newEvaluation;
                        }
					}
				}

				Debug.Log("AI chose between the following cards:");

				foreach (KeyValuePair<Card, float> possiblePlay in possiblePlaysDebugInfo)
				{
					Debug.Log(possiblePlay.Key.BaseDefinition.CardName + ": " + possiblePlay.Value);
				}
			}

			if (cardToPlay != null)
			{
				Debug.Log("AI tries to play " + cardToPlay.BaseDefinition.CardName);
				uiManager.TryPlayCard(cardToPlay);
			}
			else
			{
				yield return new WaitForSeconds(Random.Range(1.0f, 2.0f));
				PassTurnBack();
			}

			yield return new WaitForSeconds(Random.Range(.5f, 4.5f));
        }

		PassTurnBack();
    }

	//returns in integer from -5 to 5
	private float evaluateStrengthOfCard(SpellCard card)
	{
		float strengthOfCard = 0;

		float baseStrength = card.BaseDefinition.AICardStrength;
        float evaluationRandomness = Random.Range(-.1f, .1f);

		strengthOfCard = baseStrength + evaluateTempoChange(card) + evaluateCardAdvantageChange(card) + evaluationRandomness;

		return strengthOfCard;
	}

	private float evaluateTempoChange(SpellCard card)
	{
		if (card is CreatureCard)
		{
			CreatureCard creature = (CreatureCard)card;
			float powerToCMCRatio = creature.BaseDefinition.Power / creature.BaseDefinition.ManaCost;
			return powerToCMCRatio - (1 / Mathf.Sqrt(creature.BaseDefinition.ManaCost));
		}
		else if (card is SpellCard)
		{
			float baseTempoEvaluation = .05f;
			foreach (KeyValuePair<SpellEffect, int[]> pair in card.BaseDefinition.SpellEffects)
			{
				switch (pair.Key)
				{
					case SpellEffect.YOU_GAIN_LIFE:
						baseTempoEvaluation += .05f * pair.Value[0];
						break;
					case SpellEffect.OPPONENT_LOSE_LIFE:
						baseTempoEvaluation += .03f * pair.Value[0];
						break;
					case SpellEffect.YOU_DRAW_CARDS:
						baseTempoEvaluation += 0f;
						break;
					default:
						break;
				}
			}
			return baseTempoEvaluation;
        }

		return 0f;
	}

	private float evaluateCardAdvantageChange(SpellCard card)
	{
		if (card is CreatureCard)
		{
			return -.7f;
		}
		else if (card is SpellCard)
		{
			float baseCardAdvantageEvaluation = 1f;
			foreach (KeyValuePair<SpellEffect, int[]> pair in card.BaseDefinition.SpellEffects)
			{
				switch (pair.Key)
				{
					case SpellEffect.YOU_GAIN_LIFE:
						baseCardAdvantageEvaluation += .06f * pair.Value[0];
						break;
					case SpellEffect.OPPONENT_LOSE_LIFE:
						baseCardAdvantageEvaluation += .09f * pair.Value[0];
						break;
					case SpellEffect.YOU_DRAW_CARDS:
						baseCardAdvantageEvaluation += .2f * pair.Value[0];
						break;
					default:
						break;
				}
			}
			return baseCardAdvantageEvaluation;
        }

		return 0f;
	}

	private void PassTurnBack()
	{
		StopAllCoroutines();
		uiManager.TryEndTurn();
	}

	private IEnumerator EndTurnAfterOneMinute()
	{
		yield return new WaitForSeconds(60);

		Debug.Log("AI ends its turn.");

		PassTurnBack();
    }

}
