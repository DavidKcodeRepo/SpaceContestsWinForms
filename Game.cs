using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Configuration;
using SpaceContest;
using System.Runtime.InteropServices.Marshalling;
using System.Drawing.Printing;
using SpaceContestsWinForms;

namespace SpaceContest;



/// <summary>
/// The game keeps track of the states of the decks of cards, hands and discard piles, and manages shuffles.
/// The game is the primary class that calls action from the player and updates the state of the game to the console.
/// </summary>
public class Game
{
	private ConsoleView consoleview;

	#region setup

	///Card decks
	List<Card> GameDeck { get; set; } = new List<Card>();
	List<Card> GameDiscardPile { get; set; } = new List<Card>();
	List<Card> ShopHand { get; set; } = new List<Card>();
	List<Card> RimBoyDeck { get; set; } = new List<Card>();

	List<Card> PlayerDeck { get; set; } = new List<Card>();
	List<Card> PlayerHand { get; set; } = new List<Card>();
	List<Card> PlayerShownCards { get; set; } = new List<Card>();
	List<Card> PlayerDiscardPile { get; set; } = new List<Card>();
	
	int PlayerResourceAvailable = 5;
	int PlayerAttackAvailable = 0;
	int ForceBalance = 4;
	int OpponenetBaseHitPoints = 8;

	public Faction PlayerFaction = Faction.Rebel; // TODO - remove when player.cs is active
	public Faction OpponentFaction;
	public bool IsPlayerTurn = true;

	List<Card> OpponentDeck { get; set; } = new List<Card>();
	List<Card> OpponenetHand { get; set; } = new List<Card>();
	List<Card> OpponentDiscardPile { get; set; } = new List<Card>();

	List<string> CommandStrings = new List<string>(Enum.GetNames(typeof(PlayerCommand)));

	int PlayerHandLength = 5;
	int ShopHandLength = 6;

	/// <summary>
	/// Constructor to build the game
	/// </summary>
	public Game(ConsoleView consoleView)
	{
		this.consoleview = consoleView;
	}

	public void GameStartup()
	{
		//Creates the decks of cards to GameDeck, PlayerDeck & OpponentDeck
		// & Deals first hand to ShopHand, playerHand, and OpponentHand

		consoleview.WriteLine("Let's play Star Cards!");
		//Console.WriteLine("this line");

		//(("Let's play some space contests"));
		consoleview.WriteLine("______________");
		consoleview.WriteLine("Getting the deck out the box...");
		string sourceFile = GlobalConfig.CardData.FullFilePath();
		List<string> sourceData = sourceFile.LoadFile();

		for (int i = 0; i < sourceData.Count; i++)
		{
			string line = sourceData[i];
			Card newCard = new Card(line);
			GameDeck.Add(newCard);
		}

		consoleview.WriteLine("Dealing cards to starting positions...");
		OpponentDeck.AddRange((GameDeck.GetRange(0, 10)));
		PlayerDeck.AddRange((GameDeck.GetRange(10, 10)));
		RimBoyDeck.AddRange((GameDeck.GetRange(20, 10)));

		GameDeck.RemoveRange(0, 30);

		consoleview.WriteLine("Shuffling the galaxy, player and opponent decks...");
		OpponentDeck = OpponentDeck.OrderBy(x => Random.Shared.Next()).ToList();
		PlayerDeck = PlayerDeck.OrderBy(x => Random.Shared.Next()).ToList();
		GameDeck = GameDeck.OrderBy(x => Random.Shared.Next()).ToList();

		consoleview.WriteLine("Dealing hand to player and opponent...");

		for (int i = 0; i < PlayerHandLength; i++)
		{
			PlayerDeck.First().IsShown = false;
			PlayerHand.Add(PlayerDeck.First());
			PlayerDeck.Remove(PlayerDeck.First());

			OpponentDeck.First().IsShown = false;
			OpponenetHand.Add(OpponentDeck.First());
			OpponentDeck.Remove(OpponentDeck.First());
		}

		consoleview.WriteLine("Dealing cards to the shop!...");
		for (int i = 0; i < ShopHandLength; i++)
		{
			ShopHand.Add(GameDeck.First());
			GameDeck.Remove(GameDeck.First());
		}
		consoleview.WriteLine("Hint: write 'help' for a list of commands");

		consoleview.WriteLine(" Player 1, it's your turn!");
		AssignOpponentFaction(PlayerFaction);
		IsPlayerTurn = true;
		consoleview.PromptPlayerForMove();
	}

	private void AssignOpponentFaction(Faction playerFaction)
	{
		if (playerFaction == Faction.Rebel) { OpponentFaction = Faction.Empire; }
		if (playerFaction == Faction.Empire) { OpponentFaction = Faction.Rebel; }
		return;
	}
	#endregion
	#region gameLoop

	bool IsGameOver = false;

	public void DoGameRound()
	{
		DiscardHand();
		DrawNewHand();
		IsPlayerTurn = false;
		DoOpponentTurn();
		IsPlayerTurn = true;
		return;
	}

	private void DoOpponentTurn()
	{
		//TODO - make the opponent do something...
		consoleview.WriteLine("The opponent is thinking!...");
		consoleview.WriteLine("but they don't really know how to play yet...");
		consoleview.WriteLine("The opponent ends turn");
		consoleview.PromptPlayerForMove();

		return;
	}

	public void DoPlayerGameMove(string userInput)
	{
		consoleview.WriteLine("\n");

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
					else { consoleview.WriteLine("\nI didn't understand. To show a Card from shop write \"showCard [int]\" or \"showCard [int] [int]...\" "); }
				}
				break;
			case "peekHand":
				DisplayCards(PlayerHand);
				break;
			case "peekShop":
				DisplayCards(ShopHand);
				break;
			case "peekDiscards":
				DisplayCards(PlayerDiscardPile);
				break;
			case "whereIsForce":
				ReportForce();
				break;
			case "buyCard":
				if(userInputParts.Length == 2)
				{
					if (int.TryParse(userInputParts[1], out int cardToBuy))
					{
						BuyCard(cardToBuy);
					}
					else { consoleview.WriteLine("\nI didn't understand. To buy a Card from shop write \"buyCard [int]\" "); }
				};
				break;
			case "endTurn":
				consoleview.WriteLine("You end your turn");
				DoGameRound();
				break;
			case "attackBase":
				if (userInputParts.Length < 2)
				{
					consoleview.WriteLine("\nI didn't understand. To attack the enemy base write \"attack Base [int]\" or \"attackBase [int] [int]...\" ");
				}
				for (int i = 1; i < userInputParts.Length; i++)
				{
					if (int.TryParse(userInputParts[i], out int cardToFight))
					{
						AttackBase(cardToFight - 1);
					}
					else { consoleview.WriteLine("\nI didn't understand. To attack the enemy base write \"attack Base [int]\" or \"attackBase [int] [int]...\" "); }
				}
				break;
			default:
				break;
		}
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
		Card card = PlayerHand[cardIndex];

		if (card.IsShown == true)
		{
			consoleview.WriteLine($"The {card.Name} is already shown!");
			return;
		}
		if (!(cardIndex >= 0 && cardIndex < PlayerHand.Count))
		{
			consoleview.WriteLine($"\n You don't have that card. You have {PlayerHand.Count} cards");
			return;
		}

		//update player resources
		if (card.ResourceValue > 0)
		{
			PlayerResourceAvailable += card.ResourceValue;
			consoleview.WriteLine($"{card.Name} gained {card.ResourceValue} for the {PlayerFaction} team. "
			+ $"\nYou now have {PlayerResourceAvailable} resources!");
		}
		//update force
		if (card.ForceValue > 0)
		{
			ForceChange(card.ForceValue);
			consoleview.WriteLine($"{card.Name} gained {card.ForceValue} for the {PlayerFaction} team. ");
			ReportForce();
		}
		//update attack
		if (card.AttackValue > 0)
		{
			PlayerAttackAvailable += card.AttackValue;
			consoleview.WriteLine($"{card.Name} is ready to fight! They can attack with {card.AttackValue} strength."
				+ $"\nYour team has {PlayerAttackAvailable} total attack strength.");
		}
		PlayerAttackAvailable += card.AttackValue;

		card.IsShown = true;
		return;
	}
	void Attack(int cardSlot)
	{
		throw new NotImplementedException(message: "attack not implemented yet");
	}

	void BuyCard(int indexToBuy)
	{
		if (indexToBuy > 0 && indexToBuy <= ShopHand.Count)
		{
			Card cardtoBuy = ShopHand[indexToBuy - 1];

			if ( cardtoBuy.Faction == OpponentFaction)
			{
				consoleview.WriteLine("This card is from the opponents faction! You can only ");
				return;
			}
			if (cardtoBuy.CardCost <= PlayerResourceAvailable)
			{
				ShopHand.RemoveAt(indexToBuy - 1);
				PlayerDiscardPile.Add(cardtoBuy);
				consoleview.WriteLine($"You bought {cardtoBuy.Name} for {cardtoBuy.CardCost}. The card is in your discard pile. \n");
				PlayerResourceAvailable -= cardtoBuy.CardCost;
				consoleview.WriteLine($"You have {PlayerResourceAvailable} resource remaining.\n");

				ShopHand.Insert(indexToBuy - 1, GameDeck.First());
				consoleview.WriteLine($"{GameDeck.First().Name} was added to the shop!\n");
				GameDeck.Remove(GameDeck.First());

				return;
			}
			else
			{
				consoleview.WriteLine($"You can't afford this card! You have {PlayerResourceAvailable} resource; this card costs {cardtoBuy.CardCost} resource");
				return;
			} 
		}
	}

	private void AttackBase(int cardIndex)
	{


		Card card = PlayerHand[cardIndex];

		if (card.IsShown == false)
		{
			consoleview.WriteLine($"You must first show card {cardIndex + 1} to use their attack ability!");
			return;
		}
		if (!(cardIndex >= 0 && cardIndex < PlayerHand.Count))
		{
			consoleview.WriteLine($"\n You don't have that card. You have {PlayerHand.Count} cards");
			return;
		}

		consoleview.WriteLine("You attack the enemy base!");
		//attack the base
		//TODO - implement capitalShip defence logic
		if (card.AttackValue > 0)
		{
			OpponenetBaseHitPoints -= card.AttackValue;
			consoleview.WriteLine($"{card.Name} attacked the Empire base with {card.AttackValue} strength.");
			if (OpponenetBaseHitPoints < 0)
			{
				consoleview.WriteLine($"You have destroyed the enemy base.");
				IsGameOver = true;
			}
			else
			{
				consoleview.WriteLine($"The Empire base was weakened and has {OpponenetBaseHitPoints} health left");
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

		consoleview.WriteLine($"\n");
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

		foreach (string line in lines) { consoleview.WriteLine(line); }
	}
	private void ReportForce()
	{
		if (ForceBalance > 0)
		{
			consoleview.WriteLine($"The force is with the rebels with +{Math.Abs(ForceBalance)}");
			return;
		}
		if (ForceBalance == 0)
		{
			consoleview.WriteLine("The force is neither with the Rebels or the Empire");
		}
		if (ForceBalance < 0)
		{
			consoleview.WriteLine($"The force is with the Empire with +{Math.Abs(ForceBalance)}");
		}
	}

	#endregion
	#region gameStateManagementHelpers
	void ForceChange(int delta)
	{
		ForceBalance += delta;
		Math.Clamp(ForceBalance, -4, 4);
	}

	void DiscardHand()
	{
		for (int i = 0; i< PlayerHand.Count; i++)
		{
			PlayerDiscardPile.AddRange(PlayerHand);
			PlayerHand.RemoveAll(item => true);
		}
	}
	void DrawNewHand()
	{
		consoleview.WriteLine("Drawing a new hand from player deck");


		for (int i = 0; i < 5 ; i++)
		{
			if (PlayerDeck.Count == 0)
			{
				consoleview.WriteLine("Player deck is empty...");
				ResetPlayerDeck();
			}
			Card card = PlayerDeck.First();
			card.IsShown = false;
			PlayerHand.Add(PlayerDeck.First());
			PlayerDeck.Remove(PlayerDeck.First());
		}

		consoleview.WriteLine("You have 5 cards in Hand");
	}

	private void ResetPlayerDeck()
	{
		consoleview.WriteLine("Shuffling discard pile to build the next player deck...");
		PlayerDiscardPile = PlayerDiscardPile.OrderBy(x => Random.Shared.Next()).ToList();

		for (int i = 0; i < PlayerDiscardPile.Count(); i++)
		{
			PlayerDeck.Add(PlayerDiscardPile.First());
			PlayerDiscardPile.Remove(PlayerDiscardPile.First());
		}
	}

	void GameOver()
	{
		throw new NotImplementedException(message: "game over not implemented; how did you even get here!");
	}
	#endregion
}