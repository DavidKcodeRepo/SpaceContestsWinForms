using SpaceContestsWinForms;
using System;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
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
		//reset non-persistant player stats, non persistant cards effects are reset at drawNewHand
		_player.ExilesAvailable = 0;
		_player.FreePurchasesAvailable = 0;
		_player.ResourceAvailable = 0;
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
		string userErrorPrompt;

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
				userErrorPrompt = "\nI didn't understand. To show a Card from shop write \"showCard [int]\" or \"showCard [int] [int]...\" ";

                for (int i = 1; i < userInputParts.Length; i++) 
				{
					if (int.TryParse(userInputParts[i], out int cardToShow))
					{
						ShowCard(cardToShow - 1);
					}
					else { _consoleview.WriteLine(userErrorPrompt); }
				}
				break;
			case "peekHand":
				DisplayCards(_player.Hand);
				break;
			case "peekShop":
				ReportResource(_player);
				DisplayCards(ShopHand);
				break;
			case "peekDiscards":
				DisplayCardsShort(_player.DiscardPile);
				break;
            case "peekShopDiscards":
                DisplayCardsShort(ShopDiscardPile);
                break;
            case "reportForce":
				ReportForce();
				break;
			case "buyCard":
				userErrorPrompt = "\nI didn't understand. To buy a Card from shop write \"buyCard [int]\" ";

                if (userInputParts.Length == 2)
				{
					if (int.TryParse(userInputParts[1], out int cardToBuy))
					{
						BuyCard(cardToBuy - 1);
					}
					else { _consoleview.WriteLine(userErrorPrompt); }
				};
				break;
            case "buyCardFree":
                userErrorPrompt = "\nI didn't understand. To buy a Card for free from shop write \"buyCardFree [int]\" ";

                if (userInputParts.Length == 2)
                {
                    if (int.TryParse(userInputParts[1], out int cardToBuy))
                    {
                        BuyCardFree(cardToBuy - 1);
                    }
                    else { _consoleview.WriteLine(userErrorPrompt); }
                };
                break;
            case "endTurn":
				_consoleview.WriteLine("You end your turn");
				_player.ResourceAvailable = 0;
				_player.AttackAvailable = 0;
				DoGameRound();
				break;
			case "attackBase":
				userErrorPrompt = "\nI didn't understand. To attack the enemy base write \\\"attackBase [int]\\\" or \\\"attackBase [int] [int]...\\\"";

                if (userInputParts.Length < 2)
				{
					_consoleview.WriteLine(userErrorPrompt);
				}
				for (int i = 1; i < userInputParts.Length; i++)
				{
					if (int.TryParse(userInputParts[i], out int cardToFight))
					{
						AttackBase(cardToFight - 1);
					}
					else { _consoleview.WriteLine(userErrorPrompt); }
				}
				break;
            case "attackShop":
				userErrorPrompt = "\nI didn't understand. To attack a card in the shop base write \"attackShop [target:int] [attackCard:int]\"" +
                                  "\nor \"attackShop [target:int] [attackCard:int] [attackCard:int]...\" ";
                if (userInputParts.Length < 3)
                {
                    _consoleview.WriteLine(userErrorPrompt);
                }

				if (int.TryParse(userInputParts[1], out int targetIndex))
				{
					;
				}
				else { _consoleview.WriteLine(userErrorPrompt); }

				List<int> attackerIndexes = new List<int>();
                for (int i = 2; i < userInputParts.Length; i++)
                {
                    if (int.TryParse(userInputParts[i], out int attackerIndex))
					{
                        attackerIndexes.Add(attackerIndex);
                    }
                    else { _consoleview.WriteLine(userErrorPrompt); }
                }
				AttackCard(targetIndex, attackerIndexes);

                break;
            case "useAbility":
				userErrorPrompt = "\nI didn't understand. To use a Cards ability from shop write \"useAbility [int]\" ";

                if (userInputParts.Length == 2)
				{
					if (int.TryParse(userInputParts[1], out int cardToAction))
					{
						UseAbility(cardToAction - 1);
					}
					else { _consoleview.WriteLine(userErrorPrompt); }
				};
				break;
			case "exile":
                userErrorPrompt = "\nI didn't understand. To exile a Card from hand write \"exile [int]\" ";

                if (userInputParts.Length == 2)
                {
                    if (int.TryParse(userInputParts[1], out int index))
                    {
                        ExileHand(index - 1);
                    }
                    else { _consoleview.WriteLine(userErrorPrompt); }
                };
                break;
            case "exileDiscard":
                userErrorPrompt = "\nI didn't understand. To exile a Card from discard write \"exileDiscard [int]\" ";

                if (userInputParts.Length == 2)
                {
                    if (int.TryParse(userInputParts[1], out int index))
                    {
                        ExileDiscard(index - 1);
                    }
                    else { _consoleview.WriteLine(userErrorPrompt); }
                };
                break;

            default:
				break;
		}
	}


    #endregion
    #region CardEffects
    private void ActionBoon(string boon, Card card)
    {
		//where player must decide
		if (boon.Contains("|"))
		{
			int optionCount = boon.Split('|').Count();
			string optionText = "Reply";
			for(int i = 0; i < optionCount; i++) { optionText.Concat($"'{i}',"); }

			string prompt = $"That ability comes with choices. {card.Name} has ability: {card.Ability}. Which choice would you like? {optionText}";
			_consoleview.RequestUserInput(PerkOption, prompt, card);
			return;
		}
		//otherwise, keep going
		else Perk(boon, card);
		return;
    }

    public void PerkOption(string usersChoiceMessage, Card card)
    {
        //TODO parse fail?

        int choice;
        int.TryParse(usersChoiceMessage, out choice);
        choice = choice - 1;

        string options = "";
        if (TestCondition(card.ConditionForAbilityBoon))
        {
            options = card.AbilityBoonConditionMet;
        }
        else
        {
            options = card.AbilityBoonConditionMet;
        }

        string option = options.Split('|')[choice];

        Perk(option, card);
    }

    public void Perk(string award, Card card)
    {
        Regex rewardPattern = new Regex(@"(\d+)([A-Z])");
        MatchCollection rewardMatches = rewardPattern.Matches(award);

        Dictionary<char, int> rewardDictionary = new Dictionary<char, int>();

        foreach (Match match in rewardMatches)
        {
            int count = int.Parse(match.Groups[1].Value);
            char rewardType = match.Groups[2].Value[0];
            rewardDictionary[rewardType] = count;
        }

        foreach (var kvp in rewardDictionary)
        {

            switch (kvp.Key.ToString())
            {
                case "A":
                    card.BonusAttackValue += kvp.Value;
                    _consoleview.WriteLine($"{card.Name} now has {kvp.Value} Attack Strength");
                    break;
                case "B":
                    OpponentBaseHealthChange(kvp.Value * -1);
                    _consoleview.WriteLine($"{card.Name} Damaged the enemy base!");
                    ReportOpponentBase();
                    break;
                case "C":
                    break;
                case "D":
                    _consoleview.WriteLine($"{card.Name} draws a card!");
                    _player.DrawCard();
                    break;
                case "E":
                    _player.ExilesAvailable += kvp.Value;
                    _consoleview.WriteLine($"\nYou may exile {kvp.Value} cards from your hand or discard pile. Type exile [int] or exileDiscard [int] to action");
                    break;
                case "F": //TODO assign current player force alignment at start of turn.
                    int flipDarkSide = 1;
                    if (_player.Faction == Faction.Empire)
                    {
                        flipDarkSide = -1;
                    }
                    ForceChange(kvp.Value * flipDarkSide);
                    _consoleview.WriteLine($"{card.Name} has the force with them!");
                    ReportForce();
                    break;
                case "G":
					Card nextGalaxyCard = ShopDeck.First();
                    _consoleview.WriteLine($"The next card is a {nextGalaxyCard.Name} of the {nextGalaxyCard.Faction} faction");
                    if (nextGalaxyCard.Faction == Faction.Rebel) 
					{ 
						ShopDiscardPile.Add(ShopDeck[0]); ShopDeck.RemoveAt(0); 
						_consoleview.WriteLine($"The rebel scum card is discarded!"); 
					}
                    break;
                case "H":
                    PlayerBaseHealthChange(kvp.Value);
                    _consoleview.WriteLine($"{card.Name} Did repairs to the base!");
                    ReportBase();
                    break;
                case "I":
                    break;
                case "J":
                    break;
                case "K":
                    break;
                case "L":
                    break;
                case "M":
                    break;
                case "N":
                    break;
                case "O":
                    break;
                case "P":
                    break;
                default:
                    break;
                case "Q":
					_player.FishDiscard(card);
					_consoleview.WriteLine($"{card.Name} allows you to look through the discards, and finds these cards...");
                    break;
                case "R":
                    _player.ResourceAvailable += kvp.Value;
                    _consoleview.WriteLine($"{card.Name} contributed resource, you now have {_player.ResourceAvailable} resources to spend");
                    break;
                case "S":
                    _player.FreePurchasesAvailable += kvp.Value;
                    _consoleview.WriteLine($"{card.Name} gained a free purchase, you now have {_player.FreePurchasesAvailable} free purchases to make");
                    break;
                case "T":
					_player.NextPurchaseToTopOfDeck = true;
                    _consoleview.WriteLine($"{card.Name} gained a free purchase, you now have {_player.FreePurchasesAvailable} free purchases to make");
                    break;
                case "X":
                    _consoleview.WriteLine($"{card.Name} was exiled!");
					_player.Hand.Remove(card);
                    break;
            }
        }
    }

    private bool TestCondition(string condition)
    {
		bool output = true;

		switch (condition)
			{
            case "CC":

                break;
            case "CD":

				break;
            case "CE":

                break;
            case "CF":
                int flipDarkSide = 1;
                if (_player.Faction == Faction.Empire)
				{
                    flipDarkSide = -1;
				} 
                if (ForceBalance * flipDarkSide > 0 ? output = true : output = false);
                break;
            case "CG":
				Card nextGalaxyCard = ShopDeck.First();
				if (nextGalaxyCard.Faction == Faction.Empire ? output = true : output = false);
                break;
            case "CM":
				if (_player.Hand.Any(x => x.Name == "Milli Falcon") ? output = true: output = false);

                break;
            case "CO":

                break;
            case "CT":
				if (_player.Hand.Any(x => x.Category == "Trooper") || _player.Hand.Any(x => x.Category == "Vehicle")) 
				{
					output = true;
				}
                break;
            case "CU":
				if (_player.Hand.Any(x => x.IsUnique) ? output = true : output = false);
                break;
            default:
				output = true;
				break;
			}

		return output;
    }

    private void RewardFromBountyTarget(Card card)
    {
        _consoleview.WriteLine($"\nYou killed {card.Name}");
        _consoleview.WriteLine($"\nKilling {card.Name} yielded");
        Regex rewardPattern = new Regex(@"(\d+)([A-Z])");
        MatchCollection rewardMatches = rewardPattern.Matches(card.Reward);

        Dictionary<char, int> rewardDictionary = new Dictionary<char, int>();

        foreach (Match match in rewardMatches)
        {
            int count = int.Parse(match.Groups[1].Value);
            char rewardType = match.Groups[2].Value[0];
            rewardDictionary[rewardType] = count;
        }

        foreach (var kvp in rewardDictionary)
        {
            switch (kvp.Key.ToString())
            {
                case "E":
                    _player.ExilesAvailable += kvp.Value;
                    _consoleview.WriteLine($"\nYou may exile {kvp.Value} cards from your hand or discard pile. Type Exile [int] to action");
                    break;
                case "F":
                    int flipDarkSide = 1;
                    if (_player.Faction == Faction.Empire)
                    {
                        flipDarkSide = -1;
                    }
                    ForceChange(1 * flipDarkSide);
                    _consoleview.WriteLine($"\n A disturbance in the force. you gained {kvp.Value}");
                    ReportForce();
                    break;
                case "P":
                    _player.FreePurchasesAvailable += kvp.Value;
                    _consoleview.WriteLine($"\n You may purchase {kvp.Value} cards from the shop. Type FreePurchase [int] to action");
                    break;
                case "R":
                    _player.ResourceAvailable += kvp.Value;
                    _consoleview.WriteLine($"\n {kvp.Value} resource, you now have {_player.ResourceAvailable} resources to spend");
                    break;
			}
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

	private void BuyCard(int indexToBuy)
	{
        //user error handling
        string userErrorPrompt;
		if (indexToBuy < 0 || indexToBuy > ShopHand.Count)
		{
			userErrorPrompt = $"\nYou must choose a card between 1 and {ShopHand.Count}";
			_consoleview.WriteLine(userErrorPrompt);
			return;
		}
        Card cardtoBuy = ShopHand[indexToBuy];

		if ( cardtoBuy.Faction == OpponentFaction)
		{
			userErrorPrompt = $"This card is from the opponents faction! You can only purchase Neutral or {_player.Faction} cards.";
            _consoleview.WriteLine(userErrorPrompt);
			return;
		}

		if (cardtoBuy.CardCost > _player.ResourceAvailable)
		{
			userErrorPrompt = $"You can't afford this card! You have {_player.ResourceAvailable} resource, " +
				$"" + $"this card costs {cardtoBuy.CardCost} resource";
            _consoleview.WriteLine(userErrorPrompt);
            return;
        }

		//users command is legal in the game rules
		//take card from shop to discard, replace shop & reduce player resource by cost.
		List<Card> destination = _player.DiscardPile;


		//cards with special purchase actions
		if (cardtoBuy.Name == "Fang Fighter") { destination = _player.Hand; if (ForceBalance > 0) { _player.DrawCard(); }; }
		if (cardtoBuy.Name == "Quarren Merc") {if (ForceBalance > 0) {_player.ExilesAvailable += 1;} _player.ExilesAvailable+= 1; }
        
		ShopHand.RemoveAt(indexToBuy);
		if (_player.NextPurchaseToTopOfDeck == true)
		{ 
			_player.Deck.Insert(0,cardtoBuy);
            _consoleview.WriteLine($"You bought {cardtoBuy.Name} for {cardtoBuy.CardCost}. The card is on top of your player deck. \n");
			_player.NextPurchaseToTopOfDeck = false;
        }
        else
		{
            destination.Add(cardtoBuy);
            _consoleview.WriteLine($"You bought {cardtoBuy.Name} for {cardtoBuy.CardCost}. \n");
        }
        _player.ResourceAvailable -= cardtoBuy.CardCost;
		_consoleview.WriteLine($"You have {_player.ResourceAvailable} resource remaining.\n");

		ShopHand.Insert(indexToBuy - 1, ShopDeck.First());
		_consoleview.WriteLine($"{ShopDeck.First().Name} was added to the shop!\n");
		ShopDeck.Remove(ShopDeck.First());
		return;
	}

    private void BuyCardFree(int indexToBuy)
    {
		//user error handling
        string userErrorPrompt;

		if (indexToBuy! >= 0 && indexToBuy < ShopHand.Count)
		{
            userErrorPrompt = $"\nYou must choose a card between 1 and {ShopHand.Count}";
            _consoleview.WriteLine(userErrorPrompt);
        }
		if (_player.FreePurchasesAvailable < 1)
		{
			userErrorPrompt = "\nYou do not have a Free Purchase token available";
			_consoleview.WriteLine(userErrorPrompt);
		}
		Card cardToBuy = _player.Hand[indexToBuy];
		if (cardToBuy.Faction != _player.Faction)
		{
			userErrorPrompt = "\nYou must buy a card of your faction";
			_consoleview.WriteLine(userErrorPrompt);
		}

        //users command is legal in the game rules
		//take card from shop to players discard, replace shop & reduce player's free purchase count.
        ShopHand.RemoveAt(indexToBuy - 1);
        _player.DiscardPile.Add(cardToBuy);
        _consoleview.WriteLine($"You bought {cardToBuy.Name} for free! The card is in your discard pile. \n");
        _player.FreePurchasesAvailable -= 1;
        _consoleview.WriteLine($"You have {_player.FreePurchasesAvailable} free purchases remaining.\n");

        ShopHand.Insert(indexToBuy - 1, ShopDeck.First());
        _consoleview.WriteLine($"{ShopDeck.First().Name} was added to the shop!\n");
        ShopDeck.Remove(ShopDeck.First());
        return;
    }

    private void ExileHand(int index)
    {
        //user error handling
        string userErrorPrompt;
        if (index < 0 || index >= _player.Hand.Count)
        {
            userErrorPrompt = $"\nYou must choose a card between 1 and {_player.Hand.Count}";
            _consoleview.WriteLine(userErrorPrompt);
            return;
        }
        if (_player.ExilesAvailable <= 0)
        {
            userErrorPrompt = $"You do not have any exile tokens to exile this card!";
            _consoleview.WriteLine(userErrorPrompt);
            return;
        }
        Card card = _player.Hand[index];

        //users command is legal in the game rules
        //remove the card from the game

        _consoleview.WriteLine($"You exiled {card.Name} from your hand");
        _player.Hand.RemoveAt(index);
		_player.ExilesAvailable -= 1;

        return;
    }
    private void ExileDiscard(int index)
    {
        //user error handling
        string userErrorPrompt;
        if (index < 0 || index >= _player.DiscardPile.Count)
        {
            userErrorPrompt = $"\nYou must choose a card between 1 and {_player.Hand.Count}";
            _consoleview.WriteLine(userErrorPrompt);
            return;
        }
        if (_player.ExilesAvailable <= 0)
        {
            userErrorPrompt = $"You do not have any exile tokens to exile this card!";
            _consoleview.WriteLine(userErrorPrompt);
            return;
        }
        Card card = _player.DiscardPile[index];

        //users command is legal in the game rules
        //remove the card from the game

        _consoleview.WriteLine($"You exiled {card.Name} from your discard pile");
        _player.DiscardPile.RemoveAt(index);
        _player.ExilesAvailable -= 1;

        return;
    }

    private void AttackBase(int cardIndex)
	{
		Card card = _player.Hand[cardIndex];

		if (card.IsShown == false)
		{
			_consoleview.WriteLine($"You must first show card {cardIndex + 1} to use their attack ability!");
			return;
		}

        if (card.HasAttacked == true)
        {
            _consoleview.WriteLine($"You cannot attack with {card.Name}. This card has already attacked this turn!");
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

    private void AttackCard(int targetIndex, List<int> attackerIndexes)
    {
        Card targetCard = ShopHand[targetIndex - 1];
        List<Card> attackerCards = new List<Card>();
        foreach (int index in attackerIndexes)
        {
            attackerCards.Add(_player.Hand[index - 1]);
        }

        int attackStrength = attackerCards.Sum(x => x.AttackValue) + attackerCards.Sum(x => x.BonusAttackValue);

        if (attackStrength < targetCard.CardCost)
        {
            _consoleview.WriteLine(
                "\nYour attackers have insufficient strength to beat this target" +
               $"\nYou attacked with {attackerCards.Sum(x => x.AttackValue)}, you need {targetCard.CardCost} strength!");
            return;
        }

        if (targetCard.Faction != OpponentFaction)
        {
            _consoleview.WriteLine("You cannot attack a Neutral card or a card of your own faction!");
            return;
        }

        // attack is go

        foreach (Card card in attackerCards)
        {
            card.HasAttacked = true;
        }

        ShopHand.RemoveAt(targetIndex - 1);
        ShopDiscardPile.Add(targetCard);
        _consoleview.WriteLine($"You killed {targetCard.Name}. The card has been moved to the shop discard pile. \n");
        _consoleview.WriteLine($"You have {_player.ResourceAvailable} resource remaining.\n");

        ShopHand.Insert(targetIndex - 1, ShopDeck.First());
        _consoleview.WriteLine($"{ShopDeck.First().Name} was added to the shop!\n");
        ShopDeck.Remove(ShopDeck.First());

        //give bountyhunter rewards

        //give target rewards
        RewardFromBountyTarget(targetCard);
    }
    private void UseAbility(int cardIndex)
    {
		// check input
        if (!(cardIndex >= 0 && cardIndex < _player.Hand.Count))
        {
            _consoleview.WriteLine($"\n You don't have that card. You have {_player.Hand.Count} cards");
            return;
        }
        Card card = _player.Hand[cardIndex];
        if (card.IsShown == false)
        {
            _consoleview.WriteLine($"You must first show card {cardIndex + 1} to use their ability!");
            return;
        }

        //review conditional statements on card
        string condition = card.ConditionForAbilityBoon;
        string awardForConditionMet = card.AbilityBoonConditionMet;
        string awardForConditionNotMet = card.AbilityBoonConditionNotMet;

		//where card is not conditional, the award text is placed in awardForConditionMet, and condition == true
        if (TestCondition(condition))
        {
            ActionBoon(awardForConditionMet, card);
        }
        else
        {
            ActionBoon(awardForConditionNotMet, card);
        }
        return;
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

    void DisplayCardsShort(List<Card> hand)
    {
        _consoleview.WriteLine($" | Index | Cost | {PadBoth("Card Name", 14)}|\n");

        for (int i = 0; i < hand.Count; i++ )
        {
			_consoleview.WriteLine($" |{PadBoth(Convert.ToString(i+1),7)}|{PadBoth(Convert.ToString(hand[i].CardCost), 6)}| {PadBoth(hand[i].Name, 14)}|");
        }
    }

    private string PadBoth(string source, int length)
    {
        int spaces = length - source.Length;
        if (spaces > 0)
        {
            int padLeft = spaces / 2 + source.Length;
            return source.PadLeft(padLeft).PadRight(length);
        }
        return source;
    }
    private void ReportForce()
	{
		if (ForceBalance > 0)
		{
			_consoleview.WriteLine($"The force is with the Rebels with +{Math.Abs(ForceBalance)}");
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

	private void ReportOpponentBase()
	{
		_consoleview.WriteLine($"The opponents base now has {OpponenetBaseHitPoints} HP");
	}

	private void ReportBase()
	{
		//TODO capital ships
		_consoleview.WriteLine($"Your base now has {_player.BaseHitPoints} HP");
	}

    private void ReportResource(Player player)
    {
        _consoleview.WriteLine($"You have {_player.ResourceAvailable} resources available to buy with");
    }



    #endregion
    #region gameStateManagementHelpers
    void ForceChange(int delta)
	{
		ForceBalance += delta;
		ForceBalance = Math.Clamp(ForceBalance, -4, 4);
	}
	void PlayerBaseHealthChange(int delta)
	{
		_player.BaseHitPoints += delta;
		_player.BaseHitPoints = Math.Clamp(_player.BaseHitPoints, 0, 8);
	}
	void OpponentBaseHealthChange(int delta)
	{
		OpponenetBaseHitPoints+= delta;
		OpponenetBaseHitPoints = Math.Clamp(OpponenetBaseHitPoints, 0, 8);
	}


	void GameOver()
	{
		throw new NotImplementedException(message: "game over not implemented; how did you even get here!");
	}
	#endregion
}