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
	
	int PlayerResourceAvailable = 0;
	int PlayerAttackAvailable = 0;
	int ForceBalance = 4;


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
			PlayerHand.Add(PlayerDeck.First());
			PlayerDeck.Remove(PlayerDeck.First());

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
	}
	#endregion

	#region gameLoop

	bool gameOver = false;

	public void DoGameLoop(string userInput)
	{
		
		consoleview.WriteLine("What is your next move?");
		consoleview.WriteLine("--> ");

		if (gameOver == true) { GameOver(); return; }
		
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
				if (int.TryParse(userInputParts[1], out int cardToShow))
				{
					ShowCard(cardToShow);
				}
				else { consoleview.WriteLine("\nI didn't understand. To show a Card from shop write \"showCard [int]\" "); }
				break;
			case "peekShownCards":
				ShowHand(PlayerShownCards);
				break;
			case "peekHand":
				ShowHand(PlayerHand);
				break;
			case "peekShop":
				ShowHand(ShopHand);
				break;
			case "peekDiscards":
				ShowHand(PlayerDiscardPile);
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
			default:
				//Console.WriteLine("I don't know that command");
				break;
		}
	}

	private void ShowCard(int cardIndex)
	{
		if (cardIndex > 0 && cardIndex <= PlayerHand.Count )
		{
			Card card = PlayerHand[cardIndex - 1];

			//update player scores
			PlayerShownCards.Add(card);
			PlayerResourceAvailable += card.ResourceValue;
			//TODO implement subtleties of attack
			PlayerAttackAvailable += card.AttackValue;
			//TODO implement subtleties of force
			ForceChange(card.ForceValue);
			PlayerHand.RemoveAt(cardIndex);
			consoleview.WriteLine($"\n You showed the {card.Name}. You now have {PlayerResourceAvailable} resources!");

		}
		else
		{
			consoleview.WriteLine($"\n You don't have that card. You have {PlayerHand.Count} cards");
			return;
		}
	}

	void GameOver()
	{
		throw new NotImplementedException(message: "game over not implemented; how did you even get here!");
	}


	#endregion

	void ForceChange(int delta)
	{
		ForceBalance += delta;
		Math.Clamp(ForceBalance, -2, 2);
	}

	#region playerCommands

	void Attack(int cardSlot)
	{
		throw new NotImplementedException(message: "attack not implemented yet");
	}

	void BuyCard(int indexToBuy)
	{
		if (indexToBuy > 0 && indexToBuy <= ShopHand.Count)
		{
			Card cardtoBuy = ShopHand[indexToBuy - 1];
			if (cardtoBuy.CardCost <= PlayerResourceAvailable)
			{
				ShopHand.RemoveAt(indexToBuy - 1);
				PlayerDiscardPile.Add(cardtoBuy);
				consoleview.WriteLine($"You bought {cardtoBuy.Name} for {cardtoBuy.CardCost}. The card is in your discard pile. \n");
				PlayerResourceAvailable -= cardtoBuy.CardCost;
				consoleview.WriteLine($"You have {PlayerResourceAvailable} resource remaining.\n");

				ShopHand.Insert(indexToBuy - 1, GameDeck.First());
				GameDeck.Remove(GameDeck.First());
				consoleview.WriteLine($"A new card was added to the shop!\n");

				return;
			}
			else
			{
				consoleview.WriteLine($"You can't afford this card! You have {PlayerResourceAvailable} resource; this card costs {cardtoBuy.CardCost} resource");
				return;
			} 
		}
	}

	void ShowHand(List<Card> hand)
	{
		int totalAttack = 0;
		int totalResource = 0;

		foreach (Card card in hand)
		{
			totalAttack += card.AttackValue;
			totalResource += card.ResourceValue;
		}

		//TODO - make this reflect shop or playerhand
		consoleview.WriteLine($"\n");
		//consoleview.WriteLine($"\n You have {hand.Count} cards in your hand. They conceal attack value of {totalAttack} and a resourcevalue of {totalResource}");
		int cardNo = 0;
		int lineNo = 0;
		List<string> lines = new List<string>();

		foreach (Card card in hand)
		{
			using (StringReader reader = new StringReader(card.ToString()))
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

	void ShowHelp()
	{
		//Console.WriteLine("Here are the game commands");
		foreach (string com in CommandStrings) { /*Console.WriteLine(com);*/ }

		//TODO - write an overload for help(string) for each command
	}

	void AllIn()
	{
		throw new Exception(message: "All in is not implemented yet");
	}
	#endregion


}