using SpaceContestsWinForms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SpaceContest;

/// <summary>
/// The game keeps track of the states of the decks of cards, hands and discard piles, and manages shuffles.
/// The game is the primary class that calls action from the player and updates the state of the game to the console.
/// </summary>
public class Game
{
	private ConsoleView _consoleview;
	private Player _player;

	#region setup

	///Card decks
	List<Card> ShopDeck { get; set; } = new List<Card>();
	List<Card> ShopDiscardPile { get; set; } = new List<Card>();
	List<Card> ShopHand { get; set; } = new List<Card>();
	List<Card> OuterRimPilotDeck { get; set; } = new List<Card>();
	
	int ForceBalance = 4;
	int OpponenetBaseHitPoints = 8;

	public Faction OpponentFaction;
	public bool IsPlayerTurn = true;

	//TODO - refactor opponent into a class that extends player.cs
	List<Card> OpponentDeck { get; set; } = new List<Card>();
	List<Card> OpponentHand { get; set; } = new List<Card>();
	List<Card> OpponentDiscardPile { get; set; } = new List<Card>();

	List<string> CommandStrings = new List<string>(Enum.GetNames(typeof(PlayerCommand)));

	/// <summary>
	/// settings
	/// </summary>
	int GameHandCount = 5;
	int ShopHandCount = 6;

	/// <summary>
	/// Constructor to build the game
	/// </summary>
	public Game(ConsoleView consoleView)
	{
		this._consoleview = consoleView;
		_player = new Player(_consoleview);
	}

	public void GameStartup()
	{
		//Creates the decks of cards to GameDeck, PlayerDeck & OpponentDeck
		// & Deals first hand to ShopHand, playerHand, and OpponentHand

		List<string> TitleArt = new List<string> {

			"  ____                              ____               _              _        ",
			" / ___|  _ __    __ _   ___  ___   / ___| ___   _ __  | |_  ___  ___ | |_  ___ ",
			" \\___ \\ | '_ \\  / _` | / __|/ _ \\ | |    / _ \\ | '_ \\ | __|/ _ \\/ __|| __|/ __|",
			"  ___) || |_) || (_| || (__|  __/ | |___| (_) || | | || |_|  __/\\__ \\| |_ \\__ \\",
			" |____/ | .__/  \\__,_| \\___|\\___|  \\____|\\___/ |_| |_| \\__|\\___||___/ \\__||___/",
			"        |_|                                                                    "
			};

	foreach(string title in TitleArt)
		{
			_consoleview.WriteLine(title);
		}
		//Console.WriteLine("this line");

		//(("Let's play some space contests"));
		_consoleview.WriteLine("______________\n");
		_consoleview.WriteLine("Getting the deck out the box...");
		string sourceFile = GlobalConfig.CardData.FullFilePath();
		List<string> sourceData = sourceFile.LoadFile();

		for (int i = 0; i < sourceData.Count; i++)
		{
			string line = sourceData[i];
			Card newCard = new Card(line);
			ShopDeck.Add(newCard);
		}

		_consoleview.WriteLine("Dealing cards to starting positions...");
		OpponentDeck.AddRange((ShopDeck.GetRange(0, 10)));
		_player.Deck.AddRange((ShopDeck.GetRange(10, 10)));
		OuterRimPilotDeck.AddRange((ShopDeck.GetRange(20, 10)));

		ShopDeck.RemoveRange(0, 30);

		//set up factions
		_player.Faction = Faction.Rebel;
		if (_player.Faction == Faction.Rebel) { OpponentFaction = Faction.Empire; }
		if (_player.Faction == Faction.Empire) { OpponentFaction = Faction.Rebel; }

		//deal starter cards
		_consoleview.WriteLine("Shuffling the galaxy, player and opponent decks...");
		OpponentDeck = OpponentDeck.OrderBy(x => Random.Shared.Next()).ToList();
		_player.Deck = _player.Deck.OrderBy(x => Random.Shared.Next()).ToList();
		ShopDeck = ShopDeck.OrderBy(x => Random.Shared.Next()).ToList();

		_consoleview.WriteLine("Dealing hand to player and opponent...");

		//TODO - make choosing empire team an available choice

		//give out starter hands
		for (int i = 0; i < GameHandCount; i++)
		{
			_player.Deck.First().IsShown = false;
			_player.Hand.Add(_player.Deck.First());
			_player.Deck.Remove(_player.Deck.First());

			OpponentDeck.First().IsShown = false;
			OpponentHand.Add(OpponentDeck.First());
			OpponentDeck.Remove(OpponentDeck.First());
		}

		_consoleview.WriteLine("Dealing cards to the shop!...");
		for (int i = 0; i < ShopHandCount; i++)
		{
			ShopHand.Add(ShopDeck.First());
			ShopDeck.Remove(ShopDeck.First());
		}
		_consoleview.WriteLine("Hint: write 'help' for a list of commands");

		_consoleview.WriteLine(" Player 1, it's your turn!");
		
		//start first round		
		IsPlayerTurn = true;
		_consoleview.PromptPlayerForMove();
	}
	#endregion
	#region gameLoop

	bool IsGameOver = false;

	public void DoGameRound()
	{
		_player.DiscardHand();
		_player.DrawNewHand();
		IsPlayerTurn = false;
		DoOpponentTurn();
		IsPlayerTurn = true;
		return;
	}

	private void DoOpponentTurn()
	{
		//TODO - make the opponent do something...
		_consoleview.WriteLine("The opponent is thinking!...");
		_consoleview.WriteLine("but they don't really know how to play yet...");
		_consoleview.WriteLine("The opponent ends turn");
		_consoleview.PromptPlayerForMove();

		return;
	}

	public void DoPlayerGameMove(string userInput)
	{
		_consoleview.WriteLine("\n");

		if (IsGameOver == true) { GameOver(); return; }
		

		string[] userInputParts = userInput.Split(' ');

		switch (userInputParts[0])
		{
			//case "help":
			//	consoleview.WriteLine(" getting help...");

			//	ShowHelp();
			//	break;
			//case "attackEnemy":
			//	consoleview.WriteLine("Which card number would you like to attack the enemy?");
			//	int cardSlot = int.Parse(Console.ReadLine());
			//	//Console.WriteLine("you attack the enemy!");
			//	Attack(cardSlot);
			//	break;
			//case "goAllIn":
			//	//Console.WriteLine("You are going all in");
			//	AllIn();
			//	break;
			case "showCard":
				for (int i = 1; i < userInputParts.Length; i++) 
				{
					if (int.TryParse(userInputParts[i], out int cardToShow))
					{
						ShowCard(cardToShow - 1);
					}
					else { _consoleview.WriteLine("\nI didn't understand. To show a Card from shop write \"showCard [int]\" or \"showCard [int] [int]...\" "); }
				}
				break;
			case "peekHand":
				DisplayCards(_player.Hand);
				break;
			case "peekShop":
				DisplayCards(ShopHand);
				break;
			case "peekDiscards":
				DisplayCards(_player.DiscardPile);
				break;
			case "whereIsForce":
				ReportForce();
				break;
			case "buyCard":
				if(userInputParts.Length == 2)
				{
					if (int.TryParse(userInputParts[1], out int cardToBuy))
					{
						BuyCard(cardToBuy - 1);
					}
					else { _consoleview.WriteLine("\nI didn't understand. To buy a Card from shop write \"buyCard [int]\" "); }
				};
				break;
			case "endTurn":
				_consoleview.WriteLine("You end your turn");
				DoGameRound();
				break;
			case "attackBase":
				if (userInputParts.Length < 2)
				{
					_consoleview.WriteLine("\nI didn't understand. To attack the enemy base write \"attack Base [int]\" or \"attackBase [int] [int]...\" ");
				}
				for (int i = 1; i < userInputParts.Length; i++)
				{
					if (int.TryParse(userInputParts[i], out int cardToFight))
					{
						AttackBase(cardToFight - 1);
					}
					else { _consoleview.WriteLine("\nI didn't understand. To attack the enemy base write \"attack Base [int]\" or \"attackBase [int] [int]...\" "); }
				}
				break;
			case "useAbility":
				if (userInputParts.Length == 2)
				{
					if (int.TryParse(userInputParts[1], out int cardToAction))
					{
						UseAbility(cardToAction - 1);
					}
					else { _consoleview.WriteLine("\nI didn't understand. To use a Cards ability from shop write \"useAbility [int]\" "); }
				};
				break;
			default:
				break;
		}
	}

	private void UseAbility(int cardIndex)
	{
		Card card = _player.Hand[cardIndex];
		string condition = card.ConditionForAbilityBoon;
		string conditionMet = card.AbilityBoonConditionMet;
		string conditionNotMet = card.AbilityBoonConditionNotMet;

		if (card.IsShown == false)
		{
			_consoleview.WriteLine($"You must first show card {cardIndex + 1} to use their attack ability!");
			return;
		}
		if (!(cardIndex >= 0 && cardIndex < _player.Hand.Count))
		{
			_consoleview.WriteLine($"\n You don't have that card. You have {_player.Hand.Count} cards");
			return;
		}

		if (TestCondition(condition))
		{
			ActionBoon(conditionMet, card);
		}
        else
        {
			ActionBoon(conditionNotMet, card);
        }
		return;
    }

	private void ActionBoon(string boon, Card card)
	{
		switch (boon)
		{
			case "1A|1R|1F":
				string prompt = "Would you like 1 attack, 1 resource, or 1 force? Reply 'A', 'R' or 'F'.";
					_consoleview.RequestUserInput(Reward,prompt,card);
				break;
			case "something else":
				break;
			default:
				break;
		}
	}

	public void Reward(string userChoice, Card card)
	{
		switch (userChoice)
		{
			case "A":
				card.BonusAttackValue += 1;
				_consoleview.WriteLine($"{card.Name} now has an Attack Strength");
				break;
			case "R":
				_player.AttackAvailable += 1;
				_consoleview.WriteLine($"{card.Name} contributed resource, you now have {_player.ResourceAvailable} resources to spend");

				break;
			case "F":
				ForceChange(1);
				_consoleview.WriteLine($"{card.Name} has the force with them!");
				ReportForce();
				break;
		}
	}

	private bool TestCondition(string condition)
	{
		return true;
	}

	#endregion
	#region playerCommands

	void ShowHelp()
	{
		//Console.WriteLine("Here are the game commands");
		foreach (string com in CommandStrings) { /*Console.WriteLine(com);*/ }

		//TODO - write an overload for help(string) for each command
	}

	private void ShowCard(int cardIndex)
	{
		Card card = _player.Hand[cardIndex];

		if (card.IsShown == true)
		{
			_consoleview.WriteLine($"The {card.Name} is already shown!");
			return;
		}
		if (!(cardIndex >= 0 && cardIndex < _player.Hand.Count))
		{
			_consoleview.WriteLine($"\n You don't have that card. You have {_player.Hand.Count} cards");
			return;
		}

		//update player resources
		if (card.ResourceValue > 0)
		{
			_player.ResourceAvailable += card.ResourceValue;
			_consoleview.WriteLine($"{card.Name} gained {card.ResourceValue} for the {_player.Faction} team. "
			+ $"\nYou now have {_player.ResourceAvailable} resources!");
		}
		//update force
		if (card.ForceValue > 0)
		{
			ForceChange(card.ForceValue);
			_consoleview.WriteLine($"{card.Name} gained {card.ForceValue} for the {_player.Faction} team. ");
			ReportForce();
		}
		//update attack
		if (card.AttackValue > 0)
		{
			_player.AttackAvailable += card.AttackValue;
			_consoleview.WriteLine($"{card.Name} is ready to fight! They can attack with {card.AttackValue} strength."
				+ $"\nYour team has {_player.AttackAvailable} total attack strength.");
		}

		card.IsShown = true;
		return;
	}
	void Attack(int cardSlot)
	{
		throw new NotImplementedException(message: "attack not implemented yet");
	}

	void BuyCard(int indexToBuy)
	{
		if (indexToBuy > 0 && indexToBuy < ShopHand.Count)
		{
			Card cardtoBuy = ShopHand[indexToBuy];

			if ( cardtoBuy.Faction == OpponentFaction)
			{
				_consoleview.WriteLine("This card is from the opponents faction! You can only ");
				return;
			}
			if (cardtoBuy.CardCost <= _player.ResourceAvailable)
			{
				ShopHand.RemoveAt(indexToBuy - 1);
				_player.DiscardPile.Add(cardtoBuy);
				_consoleview.WriteLine($"You bought {cardtoBuy.Name} for {cardtoBuy.CardCost}. The card is in your discard pile. \n");
				_player.ResourceAvailable -= cardtoBuy.CardCost;
				_consoleview.WriteLine($"You have {_player.ResourceAvailable} resource remaining.\n");

				ShopHand.Insert(indexToBuy - 1, ShopDeck.First());
				_consoleview.WriteLine($"{ShopDeck.First().Name} was added to the shop!\n");
				ShopDeck.Remove(ShopDeck.First());

				return;
			}
			else
			{
				_consoleview.WriteLine($"You can't afford this card! You have {_player.ResourceAvailable} resource, " +
					$""+$"this card costs {cardtoBuy.CardCost} resource");
				return;
			} 
		}
	}

	private void AttackBase(int cardIndex)
	{


		Card card = _player.Hand[cardIndex];

		if (card.IsShown == false)
		{
			_consoleview.WriteLine($"You must first show card {cardIndex + 1} to use their attack ability!");
			return;
		}
		if (!(cardIndex >= 0 && cardIndex < _player.Hand.Count))
		{
			_consoleview.WriteLine($"\n You don't have that card. You have {_player.Hand.Count} cards");
			return;
		}

		_consoleview.WriteLine("You attack the enemy base!");
		//attack the base
		//TODO - implement capitalShip defence logic
		if (card.AttackValue > 0)
		{
			OpponenetBaseHitPoints -= card.AttackValue;
			_consoleview.WriteLine($"{card.Name} attacked the Empire base with {card.AttackValue} strength.");
			if (OpponenetBaseHitPoints < 0)
			{
				_consoleview.WriteLine($"You have destroyed the enemy base.");
				IsGameOver = true;
			}
			else
			{
				_consoleview.WriteLine($"The Empire base was weakened and has {OpponenetBaseHitPoints} health left");
			}
		}
	}
	#endregion
	#region ConsoleHelpers 
	void DisplayCards(List<Card> hand)
	{
		int totalAttack = 0;
		int totalResource = 0;

		foreach (Card card in hand)
		{
			totalAttack += card.AttackValue;
			totalResource += card.ResourceValue;
		}

		_consoleview.WriteLine($"\n");
		int cardNo = 0;
		int lineNo = 0;
		List<string> lines = new List<string>();

		foreach (Card card in hand)
		{
			using (StringReader reader = new StringReader(card.DisplayText(card.IsShown)))
			{
				string line = string.Empty;

				while (line != null)
				{
					line = reader.ReadLine();

					if (lineNo < lines.Count)
					{
						lines[lineNo] += "   " + line;
						lineNo++;
					}
					else { lines.Add(line); lineNo++; }
				}
			}
			lineNo = 0;
			cardNo++;
		}

		foreach (string line in lines) { _consoleview.WriteLine(line); }
	}
	private void ReportForce()
	{
		if (ForceBalance > 0)
		{
			_consoleview.WriteLine($"The force is with the rebels with +{Math.Abs(ForceBalance)}");
			return;
		}
		if (ForceBalance == 0)
		{
			_consoleview.WriteLine("The force is neither with the Rebels or the Empire");
		}
		if (ForceBalance < 0)
		{
			_consoleview.WriteLine($"The force is with the Empire with +{Math.Abs(ForceBalance)}");
		}
	}

	#endregion
	#region gameStateManagementHelpers
	void ForceChange(int delta)
	{
		ForceBalance += delta;
		ForceBalance = Math.Clamp(ForceBalance, -4, 4);
	}

	void GameOver()
	{
		throw new NotImplementedException(message: "game over not implemented; how did you even get here!");
	}
	#endregion
}