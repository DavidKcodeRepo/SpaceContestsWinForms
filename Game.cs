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
/// The game keeps track of the states of the decks of cards, 
/// hands and discard piles, and also calls actions to change these according to the player
/// </summary>
/// 
class Game
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
	List<Card> PlayerDiscardPile { get; set; } = new List<Card>();

	List<Card> OpponentDeck { get; set; } = new List<Card>();
	List<Card> OpponenetHand { get; set; } = new List<Card>();
	List<Card> OpponentDiscardPile { get; set; } = new List<Card>();

	List<string> CommandStrings = new List<string>(Enum.GetNames(typeof(Commands)));

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
		consoleview.WriteLine(" -> Write 'help' for a list of commands");
		consoleview.WriteLine(" Player 1, it's your turn!");
		consoleview.WriteLine("What is your next move?");
	}
	#endregion

	#region gameLoop

	bool gameOver = false;

	public void DoGameLoop(string userInput)
	{
		if (gameOver == true) { GameOver(); return; }

		switch (userInput)
		{
			case "help":
				consoleview.WriteLine(" getting help...");

				ShowHelp();
				break;
			case "attackEnemy":
				consoleview.WriteLine("Which card number would you like to attack the enemy?");
				int cardSlot = int.Parse(Console.ReadLine());
				//Console.WriteLine("you attack the enemy!");
				Attack(cardSlot);
				break;
			case "goAllIn":
				//Console.WriteLine("You are going all in");
				AllIn();
				break;
			case "peekHand":
				Peek(PlayerHand);
				break;
			case "peekShop":
				Peek(ShopHand);
				break;
			case "buyCard":
				//Console.WriteLine("Which card number would you like to buy?");
				int shopCardSlot = int.Parse(Console.ReadLine());
				//Console.WriteLine("Which of your cards are you paying in for this?");
				int playerCardSlot = int.Parse(Console.ReadLine());
				BuyCard(shopCardSlot, playerCardSlot);
				break;
			default:
				//Console.WriteLine("I don't know that command");
				break;
		}
	}

	void GameOver()
	{
		throw new NotImplementedException(message: "game over not implemented; how did you get here!");
	}


	#endregion


	#region playerCommands

	public enum Commands
	{
		attackEnemy,
		drawCard,
		peekHand,
		peekShop,
		buyCard,
		help,
		goAllIn

		//DiscardCard,
		//Purchase (shopcard),
		//Discard (handcard),
		//Reward (shopcard),
		//UserAbiltiy(handcard),
		//TargetBase(),
		//ChooseNewBase(),
		//ShuffleDeck(),
		//FishDiscard(),
		//CountEnemyCards(),
		//PeekShop(),

	}

	void Attack(int cardSlot)
	{
		throw new NotImplementedException(message: "attack not implemented yet");
	}

	void BuyCard(int shopCardSlot, int playerCardSlot)
	{
		if (ShopHand[shopCardSlot].CardCost <= PlayerHand[playerCardSlot].CardCost)
		{
			Card drawnCard = ShopHand[shopCardSlot];
			ShopHand.RemoveAt(shopCardSlot);
			PlayerHand.Add(drawnCard);
			// fix this
			return;
		}
		else
		{
			//Console.WriteLine("You can't afford this card!");
			return;
		}
	}

	void ShowHand(List<Card> hand)
	{
		int cardNo = 0;
		int lineNo = 0;
		foreach (Card card in hand)
		{
			using (StringReader reader = new StringReader(card.ToString()))
			{
				string line = string.Empty;
				do
				{
					line = reader.ReadLine();
					if (line != null)
					{
						//Console.SetCursorPosition(cardNo * 30, lineNo);
						//Console.Write(line);
						lineNo++;
					}

				} while (line != null);
			}
			lineNo = 0;
			cardNo++;
		}
		//Console.WriteLine("");
	}

	void Peek(List<Card> hand)
	{
		//Console.WriteLine($"This hand has {hand.Count} cards.");
		ShowHand(hand);
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