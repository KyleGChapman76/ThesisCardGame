using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class LocalGameManager : MonoBehaviour, IGameManager
{
	public GameUIManager uiManager;

	public GameObject offlinePlayerPrefab;

	public static System.Random rand = new System.Random();

	public static event AITurnBegins aiTurnBegins;
	public delegate void AITurnBegins(GameUIManager gameManager);

	private OfflineTCGPlayer localPlayer;
	private OfflineTCGPlayer opponentPlayer;

	public static Library hardCodedLibrary;

	private bool isPlayersTurn;

	public static LocalGameManager SingletonGameManager;

	public NetworkedGameManager ServerGameManager
	{
		set
		{
			serverGameManager = value;
        }
	}
	private NetworkedGameManager serverGameManager;

	//start the local game logic
	//if find a network manager, wait for all players to load in to then initialize multiplayer logic
	//otherwise, setup players
	void Start ()
	{
		Debug.Log("GameManager began.");

		SingletonGameManager = this;

		InitializeCardDatabase();
		InitializeHardCodedLibrary();

		uiManager.gameManager = this;

		//if game is not multiplayer, spawn players and begin the UI
		if (!GameObject.FindObjectOfType<NetworkManager>())
		{
			StartCoroutine("InitializeGameAfterSmallTime");
        }
		//otherwise, do nothing!
	}

	//initialze the list of cards
	private void InitializeCardDatabase()
	{
		Debug.Log("Initialize cards database.");

		string XMLLocale = Path.Combine(Application.dataPath, "CardsWithAIData.xml");
		XNamespace nameSpace = "http://example.com/Cards";
		try
		{
			XDocument xmlDoc = XDocument.Load(XMLLocale);

			//get resource cards from file
			IEnumerable<XElement> resources =
			from item in xmlDoc.Root.Descendants(nameSpace + "Card")
			where item.Attribute("type").Value == "resource"
			select item;

			foreach (XElement resource in resources)
			{
				string resourceCardName = resource.Attribute("name").Value;
				int resourceValue = int.Parse(resource.Element(nameSpace + "resources").Value);
				int thresholdType = int.Parse(resource.Element(nameSpace + "threshold").Value);

				new ResourceCardDefinition(resourceCardName, resourceValue, thresholdType);
				Debug.Log("Loaded a resource card: " + resourceCardName + ", " + resourceValue.ToString() + ", " + thresholdType.ToString());
			}

			//get spell cards
			IEnumerable<XElement> spells =
			from item in xmlDoc.Root.Descendants(nameSpace + "Card")
			where item.Attribute("type").Value == "spell"
			select item;

			foreach (XElement spell in spells)
			{
				string spellCardName = spell.Attribute("name").Value;
				int spellCost = int.Parse(spell.Element(nameSpace + "cost").Value);
				string spellText = spell.Element(nameSpace + "text").Value;
				float cardStrength = float.Parse(spell.Attribute("strength").Value);

				IEnumerable<XElement> effects =
				from item in spell.Descendants(nameSpace + "Card")
				select item;

				Dictionary<SpellEffect, int[]> spellEffects = new Dictionary<SpellEffect, int[]>();
				foreach (XElement effect in effects)
				{
					string code = effect.Attribute("code").Value;
					SpellEffect spellEffect = SpellCardDefinition.StringToSpellEffect(code);

					IEnumerable<XElement> variables =
					from item in effect.Descendants(nameSpace + "variable")
					select item;

					List<int> variableValues = new List<int>();

					foreach (XElement variable in variables)
					{
						variableValues.Add(int.Parse(variable.Value));
                    }

					spellEffects.Add(spellEffect, variableValues.ToArray());
                }

				new SpellCardDefinition(spellCardName, spellCost, spellText, spellEffects, cardStrength);
				Debug.Log("Loaded a spell card: " + spellCardName + ", " + spellCost.ToString() + ", " + spellText);
			}

			//get creature cards
			IEnumerable<XElement> creatures =
			from item in xmlDoc.Root.Descendants(nameSpace + "Card")
			where item.Attribute("type").Value == "creature"
			select item;

			foreach (XElement creature in creatures)
			{
				string creatureCardName = creature.Attribute("name").Value;
				float cardStrength = float.Parse(creature.Attribute("strength").Value);
				int creatureCost = int.Parse(creature.Element(nameSpace + "cost").Value);
				int creaturePower = int.Parse(creature.Element(nameSpace + "power").Value);
				int creatureToughness = int.Parse(creature.Element(nameSpace + "toughness").Value);
				string creatureText = creature.Element(nameSpace + "text").Value;

				new CreatureCardDefinition(creatureCardName, creatureCost, creatureText, creaturePower, creatureToughness, cardStrength);
				Debug.Log("Loaded a creature card: " + creatureCardName + ", " + creatureCost.ToString() + ", " + creaturePower.ToString() + ", " + creatureToughness.ToString() + ", " + creatureText);
			}
		}
		catch (XmlException ex)
		{
			Debug.LogError("Could not load card game data.");
			Debug.LogError(ex.StackTrace);
			return;
		}
	}

	private IEnumerator InitializeGameAfterSmallTime()
	{
		yield return new WaitForSeconds(.1f);

		Debug.Log("Starting single player game.");
		GameObject singlePlayerObject = Instantiate(offlinePlayerPrefab);
		localPlayer = singlePlayerObject.GetComponent<OfflineTCGPlayer>();

		GameObject aiOpponentObject = Instantiate(offlinePlayerPrefab);
		opponentPlayer = aiOpponentObject.GetComponent<OfflineTCGPlayer>();

		localPlayer.InitializePlayer(hardCodedLibrary, false);
		opponentPlayer.InitializePlayer(hardCodedLibrary, true);

		isPlayersTurn = true;
		uiManager.UpdateUI(localPlayer, opponentPlayer);
		uiManager.InitializeLocalTurnUI();
	}

	private void InitializeHardCodedLibrary()
	{
		Debug.Log("Initializing hard coded library.");

		Deck hardcodedDeck = new Deck(30, 60);

		hardcodedDeck.AddCard(0, 8);
		hardcodedDeck.AddCard(1, 8);
		hardcodedDeck.AddCard(2, 4);
		hardcodedDeck.AddCard(3, 4);
		hardcodedDeck.AddCard(4, 4);
		hardcodedDeck.AddCard(5, 4);
		hardcodedDeck.AddCard(6, 4);
		hardcodedDeck.AddCard(7, 4);

		int[] cardList;

		if (!hardcodedDeck.ExportDeckToArray(out cardList))
		{
			Debug.LogError("Can't export hardcoded deck.");
			Application.Quit();
			return;
		}

		hardCodedLibrary = new Library(cardList);
	}

	private void Update()
	{
		//check for debug button presses
		if (Input.GetKeyUp(KeyCode.R) && localPlayer != null)
		{
			localPlayer.ResetResources();
		}

		if (Input.GetKeyUp(KeyCode.D) && localPlayer != null)
		{
			localPlayer.DrawCard();
		}
	}

	public bool LocalEndTurn()
	{
		if (serverGameManager)
		{
			return serverGameManager.LocalEndTurn();
        }

		if (isPlayersTurn)
		{
			isPlayersTurn = false;
			localPlayer.TryEndTurn();
			opponentPlayer.TryStartTurn();
			aiTurnBegins(uiManager);
			uiManager.InitializeOpponentTurnUI();
		}
		else
		{
			isPlayersTurn = true;
			opponentPlayer.TryEndTurn();
			localPlayer.TryStartTurn();
			uiManager.InitializeLocalTurnUI();
		}
		return true;
	}

	public bool TryPlayCard(Card card)
	{
		if (serverGameManager)
		{
			return serverGameManager.TryPlayCard(card);
		}
		if (isPlayersTurn)
		{
			return localPlayer.TryPlayCard(card);
		}
		else
		{
			return opponentPlayer.TryPlayCard(card);
		}
	}

	public void PlayerStatusChanged()
	{
		uiManager.UpdateUI(localPlayer, opponentPlayer);
	}

	public void ReportGameLoss(bool localPlayerLost)
	{
		uiManager.InitializeGameOverUI(!localPlayerLost);
    }
}
