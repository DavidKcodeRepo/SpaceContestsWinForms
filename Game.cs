using SpaceContestsWinForms;
using System;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SpaceContest;

//TODO this class is uncomfortably long. Maybe create a rewarder class to handle perks?

/// <summary>
/// The game class is the primary class that calls action from the player and updates the state of the game to the console.
/// The game class also sets up the game by reading the cards from the gameContent.csv and dealing starting hands to the players.
/// The game class keeps track of the gameState, parses actions and checks legality of moves of current player.
/// The game class informs the viewconsole class of updates to inform the player.
/// </summary>
public class Game
{
    /// <summary>
    /// reference to console view for writing gamestate messages and prompting user interactions
    /// </summary>
    private ConsoleView _consoleview;
    /// <summary>
    /// reference to player, to action player to handle player-cards.
    /// </summary>
    private Player _player;


    #region Properties

    /// <summary>
    /// Game: The deck of unrevealed cards. These cards are drawn to replace cards taken from the shop. 
    /// Game: Their order is unknown and is the prime source of variation in the game (apart from the player choices).
    /// </summary>
    List<Card> ShopDeck { get; set; } = new List<Card>();
    /// <summary>
    /// Game: the discard pile is kept as some cards can fish cards from these discards. 
    /// The discard pile is also used to make a new ShopDeck when the shopDeck is empty and a new card is needed.
    /// </summary>
    List<Card> ShopDiscardPile { get; set; } = new List<Card>();
    /// <summary>
    /// Game: The ShopHand is the currently available cards to purchase or attack the opponents faction's cards for reward.
    /// </summary>
    List<Card> ShopHand { get; set; } = new List<Card>();
    /// <summary>
    /// game: The OuterRimPilotDeck is an alternate shop. It contains 10 of the same weak card (the outerRimPilot) but its an alternate option.
    /// </summary>
    List<Card> OuterRimPilotDeck { get; set; } = new List<Card>();

    /// <summary>
    /// Game: the forcebalance is a representation of the force. +ve is with the rebels. -ve is with the Empire. 
    /// Game: having the forcebalance towards your side is beneficial for many conditions on card abiltiies.
    /// Game: The forcebalance starts with the rebels.
    /// </summary>
    int ForceBalance = 4;
    int PlayerForceSideModifier = 1; //TODO - expand empire choice to player

    /// <summary>
    /// Game: Base hit points represent the health of the opponents base. Reduce to zero to destroy bases.
    /// Game: Loosing all bases results in victory for the other player.
    /// TODO- this implementation is a temporary workaround since baseCards are not yet implemented.
    /// </summary>
    bool OpponentHasCaptialShip = false;

    /// <summary>
    /// Game: Base hit points represent the health of the opponents base. Reduce to zero to destroy bases.
    /// Game: Loosing all bases results in victory for the other player.
    /// </summary>
    BaseCard OpponentCurrentBase;
    /// <summary>
    /// The available bases an opponent has to select from after their current base is destroyed.
    /// </summary>
    List<BaseCard> OpponentRemainingBases;
    /// <summary>
    /// The collection of Bases destroyed by the player.
    /// </summary>
    List<BaseCard> OpponentVictoryPile;

    /// <summary>
    /// The faction of the opposite player. Updated each alternating round, many game effects are dependent on which faction is active.
    /// TODO- this implementation is a temporary workaround until playerFaction choice of Empire is supported.
    /// </summary>
    public Faction OpponentFaction;

    /// <summary>
    /// A boolean that tracks the current player. Currently, the game is planned to be player vs automated opponent.
    /// TODO - refactor opponent into a class that extends player.cs
    /// </summary>
    public bool IsPlayerTurn = true;

    /// <summary>
    /// The opponents cards remaining.
    /// </summary>
    List<Card> OpponentDeck { get; set; } = new List<Card>();
    /// <summary>
    /// The opponents current hand.
    /// </summary>
    List<Card> OpponentHand { get; set; } = new List<Card>();
    List<Card> OpponentCapitalShipHand { get; set; } = new List<Card>();
    /// <summary>
    /// The opponents discard pile.
    /// </summary>
    List<Card> OpponentDiscardPile { get; set; } = new List<Card>();

    /// <summary>
    /// A list of all the possible CommandStrings in the game, used for the help command.
    /// </summary>
    List<string> CommandStrings = new List<string>(Enum.GetNames(typeof(PlayerCommand)));
    #endregion

    /// <summary>
    /// settings
    /// </summary>
    int GameHandCount = 5;
    int ShopHandCount = 6;
    #region setup
    /// <summary>
    /// Constructor to build the game.
    /// </summary>
    public Game(ConsoleView consoleView)
    {
        this._consoleview = consoleView;
        _player = new Player(_consoleview);
    }

    /// <summary>
    /// The inital setup method that reads the card data, deals starting hands and instructs the game to start.
    /// </summary>
    public void GameStartup()
    {
        //Creates the decks of cards to GameDeck, PlayerDeck & OpponentDeck
        // & Deals first hand to ShopHand, playerHand, and OpponentHand

        //opening console text
        List<string> TitleArt = new List<string> {

            "  ____                              ____               _              _        ",
            " / ___|  _ __    __ _   ___  ___   / ___| ___   _ __  | |_  ___  ___ | |_  ___ ",
            " \\___ \\ | '_ \\  / _` | / __|/ _ \\ | |    / _ \\ | '_ \\ | __|/ _ \\/ __|| __|/ __|",
            "  ___) || |_) || (_| || (__|  __/ | |___| (_) || | | || |_|  __/\\__ \\| |_ \\__ \\",
            " |____/ | .__/  \\__,_| \\___|\\___|  \\____|\\___/ |_| |_| \\__|\\___||___/ \\__||___/",
            "        |_|                                                                    "
            };
        foreach (string title in TitleArt)
        {
            _consoleview.WriteLine(title);
        }
        _consoleview.WriteLine("______________\n");
        _consoleview.WriteLine("Getting the deck out the box...");

        //read card data and put cards in the shop
        string cardSourceFile = GlobalConfig.CardData.FullFilePath();
        List<string> cardSourceData = cardSourceFile.LoadFile();
        for (int i = 0; i < cardSourceData.Count; i++)
        {
            string line = cardSourceData[i];
            Card newCard = new Card(line);
            ShopDeck.Add(newCard);
        }

        //read bases to a temp stack of bases to distribute
        string baseCardSourceFile = GlobalConfig.BaseCardData.FullFilePath();
        List<string> baseCardSourceData = baseCardSourceFile.LoadFile();

        List<BaseCard> baseCards = new List<BaseCard>();
        foreach (string line in baseCardSourceData)
        {
            BaseCard newBaseCard = new BaseCard(line);
            baseCards.Add(newBaseCard);
        }

        //deal starter cards
        _player.CurrentBase = baseCards[0];
        _player.RemainingBaseCards = baseCards.GetRange(1, 9);
        OpponentCurrentBase = baseCards[10];
        OpponentRemainingBases = baseCards.GetRange(11, 9);

        _player.CurrentBase.IsAbilityAvailable = false;

        //note: the gameContent.csv is arranged so the first 30 cards are the cards with specific starting positions.
        _consoleview.WriteLine("Dealing cards to starting positions...");
        OpponentDeck.AddRange((ShopDeck.GetRange(0, 10)));
        _player.Deck.AddRange((ShopDeck.GetRange(10, 10)));
        OuterRimPilotDeck.AddRange((ShopDeck.GetRange(20, 10)));

        ShopDeck.RemoveRange(0, 30);

        //assign factions
        _player.Faction = Faction.Rebel;
        if (_player.Faction == Faction.Rebel) { OpponentFaction = Faction.Empire; }
        if (_player.Faction == Faction.Empire) { OpponentFaction = Faction.Rebel; }

        //shuffle the starting decks
        _consoleview.WriteLine("Shuffling the galaxy, player and opponent decks...");
        OpponentDeck = OpponentDeck.OrderBy(x => Random.Shared.Next()).ToList();
        _player.Deck = _player.Deck.OrderBy(x => Random.Shared.Next()).ToList();
        ShopDeck = ShopDeck.OrderBy(x => Random.Shared.Next()).ToList();
        _consoleview.WriteLine("Dealing hand to player and opponent...");

        //give out starter hands
        for (int i = 0; i < GameHandCount; i++)
        {
            _consoleview.WriteLine("Dealing cards to the player!...");
            _player.Deck.First().IsShown = false;
            _player.Hand.Add(_player.Deck.First());
            _player.Deck.Remove(_player.Deck.First());

            _consoleview.WriteLine("Dealing cards to the opponent!...");
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
    int VictoryPileCountToWin = 4;


    public void DoGameRound()
    {
        //reset non-persistant player stats, non persistant cards effects are reset at drawNewHand

        if (_player.CurrentBase.RemainingHitPoints <= 0)
        {
            OpponentRemainingBases.Add(_player.CurrentBase);
            _player.CurrentBase = null;
            _consoleview.WriteLine($"\nYour base has been destroyed. Your opponent has destroyed {OpponentVictoryPile.Count} bases so far");
            if (OpponentVictoryPile.Count >= VictoryPileCountToWin)
            {
                IsGameOver = true;
            }
        }
        _player.ExilesAvailable = 0;
        _player.FreePurchasesOfFactionAvailable = 0;
        _player.ResourceAvailable = 0;
        PlayerForceSideModifier = (_player.Faction == Faction.Empire) ? -1 : 1;
        _player.DiscardHand();
        _player.DrawNewHand();
        _consoleview.WriteLine("\nHere is your hand:\n");
        DisplayCards(_player.Hand);
        _consoleview.WriteLine("\nHere is the shop hand:\n");
        DisplayCards(ShopHand);
        if (_player.CurrentBase == null)
        {
            _player.ChooseNewBase();
            if (_player.CurrentBase.IsAbilityRevealDependent)
            {
                UseBaseAbility(_player);
                _player.CurrentBase.IsAbilityAvailable = false;
            }
        }
        else
        {
            if (!_player.CurrentBase.IsAbilityRevealDependent) { _player.CurrentBase.IsAbilityAvailable = true; }
        }
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
            case "peekBase":
                List<BaseCard> baseList = new List<BaseCard> { _player.CurrentBase };
                DisplayBaseCards(baseList);
                break;
            case "reportForce":
                ReportForce();
                break;
            case "reportBases":
                ReportBases();
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
                        attackerIndexes.Add(attackerIndex - 1);
                    }
                    else { _consoleview.WriteLine(userErrorPrompt); }
                }
                AttackShopCard(targetIndex - 1, attackerIndexes);

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
            for (int i = 0; i < optionCount; i++) { optionText.Concat($"'{i}',"); }

            string prompt = $"That ability comes with choices. {card.Name} has ability: {card.AbilityText}. Which choice would you like? {optionText}";
            _consoleview.RequestUserInput(PerkOption, prompt, card);
            return;
        }
        //special condition for jabba
        if (card.Name == "Jabba the Hutt")
        {
            _consoleview.WriteLine("You must exile a card from hand to resolve this ability.");
            _consoleview.RequestUserInput(ExileHand, "Pick a card. Write '1', '2', '3' etc. ", card);
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
                    OpponentCurrentBase.HPChange(kvp.Value * -1);
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
                    ForceChange(kvp.Value * PlayerForceSideModifier);
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
                    _player.CurrentBase.HPChange(kvp.Value);
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
                    _player.FreePurchasesOfFactionAvailable += kvp.Value;
                    _consoleview.WriteLine($"{card.Name} gained a free purchase, you now have {_player.FreePurchasesOfFactionAvailable} free purchases to make");
                    break;
                case "T":
                    _player.NextPurchaseToTopOfDeck = true;
                    _consoleview.WriteLine($"{card.Name} gained a free purchase, you now have {_player.FreePurchasesOfFactionAvailable} free purchases to make");
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
                if (ForceBalance * PlayerForceSideModifier > 0 ? output = true : output = false) ;
                break;
            case "CG":
                Card nextGalaxyCard = ShopDeck.First();
                if (nextGalaxyCard.Faction == Faction.Empire ? output = true : output = false) ;
                break;
            case "CM":
                if (_player.Hand.Any(x => x.Name == "Milli Falcon") ? output = true : output = false) ;

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
                if (_player.Hand.Any(x => x.IsUnique) ? output = true : output = false) ;
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
                    ForceChange(1 * PlayerForceSideModifier);
                    _consoleview.WriteLine($"\n A disturbance in the force. you gained {kvp.Value}");
                    ReportForce();
                    break;
                case "P":
                    _player.FreePurchasesOfFactionAvailable += kvp.Value;
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

    /// <summary>
    /// Showing cards is necessary to use a cards resource/attack value, but some player options require cards not shown.
    /// </summary>
    /// <param name="cardIndex"></param>
    private void ShowCard(int cardIndex)
    {
        Card card = _player.Hand[cardIndex];
        //user input check
        if (!(cardIndex >= 0 && cardIndex < _player.Hand.Count))
        {
            _consoleview.WriteLine($"\n You don't have that card. You have {_player.Hand.Count} cards");
            return;
        }

        if (card.IsShown == true)
        {
            _consoleview.WriteLine($"The {card.Name} is already shown!");
            return;
        }


        //update player resources of the card. If statements suppress redundant console outputs.
        if (card.ResourceValue > 0)
        {
            _player.ResourceAvailable += card.ResourceValue;
            _consoleview.WriteLine($"{card.Name} gained {card.ResourceValue} for the {_player.Faction} team. "
            + $"\nYou now have {_player.ResourceAvailable} resources!");
        }
        if (card.ForceValue > 0)
        {
            ForceChange(card.ForceValue);
            _consoleview.WriteLine($"{card.Name} gained {card.ForceValue} for the {_player.Faction} team. ");
            ReportForce();
        }
        if (card.AttackValue > 0)
        {
            _player.AttackAvailable += card.AttackValue;
            _consoleview.WriteLine($"{card.Name} is ready to fight! They can attack with {card.AttackValue} strength."
                + $"\nYour team has {_player.AttackAvailable} total attack strength.");
        }
        if (card.Category == Category.CapitalShip.ToString())
        {
            _player.HasCapitalShipShown = true;
            _consoleview.WriteLine($"{card.Name} warps into orbit to protect the base!");
            _player.CapitalShipHand.Add(card);
            _player.Hand.Remove(card);
        }

        card.IsShown = true;
        return;
    }
    /// <summary>
    /// This is the normal way of using resources to strengthen player deck.
    /// </summary>
    /// <param name="indexToBuy"></param>
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

        if (cardtoBuy.Faction == OpponentFaction)
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
        if (cardtoBuy.Name == "Quarren Merc") { if (ForceBalance > 0) { _player.ExilesAvailable += 1; } _player.ExilesAvailable += 1; }

        ShopHand.RemoveAt(indexToBuy);
        if (_player.NextPurchaseToTopOfDeck == true)
        {
            _player.Deck.Insert(0, cardtoBuy);
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

    /// <summary>
    /// Some abilities grant free purchases.
    /// </summary>
    /// <param name="indexToBuy"></param>
    private void BuyCardFree(int indexToBuy)
    {
        //user error handling
        string userErrorPrompt;

        if (indexToBuy! >= 0 && indexToBuy < ShopHand.Count)
        {
            userErrorPrompt = $"\nYou must choose a card between 1 and {ShopHand.Count}";
            _consoleview.WriteLine(userErrorPrompt);
        }
        if (_player.FreePurchasesOfFactionAvailable < 1)
        {
            userErrorPrompt = "\nYou do not have a Free Purchase token available";
            _consoleview.WriteLine(userErrorPrompt);
        }
        Card cardToBuy = _player.Hand[indexToBuy];
        if (_player.Faction != cardToBuy.Faction)
        {
            userErrorPrompt = "\nYou must buy a card of your faction";
            _consoleview.WriteLine(userErrorPrompt);
        }

        //users command is legal in the game rules
        //take card from shop to players discard, replace shop & reduce player's free purchase count.
        ShopHand.RemoveAt(indexToBuy - 1);
        _player.DiscardPile.Add(cardToBuy);
        _consoleview.WriteLine($"You bought {cardToBuy.Name} for free! The card is in your discard pile. \n");
        _player.FreePurchasesOfFactionAvailable -= 1;
        _consoleview.WriteLine($"You have {_player.FreePurchasesOfFactionAvailable} free purchases remaining.\n");

        ShopHand.Insert(indexToBuy - 1, ShopDeck.First());
        _consoleview.WriteLine($"{ShopDeck.First().Name} was added to the shop!\n");
        ShopDeck.Remove(ShopDeck.First());
        return;
    }

    private void BaseBuyCard(string indexToBuy, Card card)
    {
        //user error handling
        string userErrorPrompt;
        int index = int.Parse(indexToBuy) - 1;

        if (index! >= 0 && index < ShopHand.Count)
        {
            userErrorPrompt = $"\nYou must choose a card between 1 and {ShopHand.Count}";
            _consoleview.WriteLine(userErrorPrompt);
        }
        Card cardToBuy = ShopHand[index];
        if (OpponentFaction != cardToBuy.Faction)
        {
            userErrorPrompt = "\nYou must buy a card of your faction or neutral";
            _consoleview.WriteLine(userErrorPrompt);
        }

        //users command is legal in the game rules
        //take card from shop to players hand, replace shop.
        ShopHand.RemoveAt(index);
        _player.Hand.Add(cardToBuy);
        _consoleview.WriteLine($"You bought {cardToBuy.Name} for free! The card is in your hand. \n");

        ShopHand.Insert(index, ShopDeck.First());
        _consoleview.WriteLine($"{ShopDeck.First().Name} was added to the shop!\n");
        ShopDeck.Remove(ShopDeck.First());
        return;
    }

    /// <summary>
    /// Players can exile from hand or discard depending on ability used. Exiling weak cards strenghtens the player's deck. 
	/// Sometimes this is a free option, but sometimes a card requires it as part of ability, (Jabba). 
	/// In second case an overload with (string, card) input is needed to conform to "_player.RequestUserInput" delegate.
    /// </summary>
    /// <param name="index"></param>
    private void ExileHand(int index)
    {
        //check user input
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
    private void ExileHand(string index, Card exilingCard)
    {
        //user error handling
        string userErrorPrompt;
        int.TryParse(index, out int result);

        if (result < 0 || result >= _player.Hand.Count)
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
        Card card = _player.Hand[result];

        //users command is legal in the game rules
        //remove the card from the game

        _consoleview.WriteLine($"You exiled {card.Name} from your hand");
        _player.Hand.RemoveAt(result);
        _player.ExilesAvailable -= 1;

        return;
    }
    private void ExileDiscard(int index)
    {
        //check user input
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

    /// <summary>
    /// Attacking the opponents base contributes to the players final objective.
    /// </summary>
    /// <param name="cardIndex"></param>
    private void AttackBase(int cardIndex)
    {
        //check user input
        if (!(cardIndex >= 0 && cardIndex < _player.Hand.Count))
        {
            _consoleview.WriteLine($"\n You don't have that card. You have {_player.Hand.Count} cards");
            return;
        }
        Card card = _player.Hand[cardIndex];
        //rule: card must be shown in order to attack
        if (card.IsShown == false)
        {
            _consoleview.WriteLine($"You must first show card {cardIndex + 1} to use their attack ability!");
            return;
        }
        //rule: card cannot attack twice per round
        if (card.HasAttacked == true)
        {
            _consoleview.WriteLine($"You cannot attack with {card.Name}. This card has already attacked this turn!");
            return;
        }
        _consoleview.WriteLine("You attack the enemy base!");
        //attack the base

        //TODO - capital ship logic
        if (OpponentHasCaptialShip)
        {
            OpponentHand.Where(x => x.Category == Category.CapitalShip.ToString());
            // console opponent has captial ships blocking the base. Would you like to attack a ship.
            DisplayCardsShort(OpponentCapitalShipHand);
            _consoleview.WriteLine("Opponent has captial ships blocking the base. Would you like to attack a ship?");
            _consoleview.RequestUserInput(AttackCapitalShip, "Opponent has captial ships blocking the base. Would you like to attack a ship?", card);
        }
        else
        //TODO - implement capitalShip defence logic
        if (card.AttackValue > 0)
        {

            OpponentCurrentBase.RemainingHitPoints -= card.AttackValue;
            _consoleview.WriteLine($"{card.Name} attacked the Empire base with {card.AttackValue} strength.");
            if (OpponentCurrentBase.RemainingHitPoints < 0)
            {
                _consoleview.WriteLine($"You have destroyed the enemy base.");
                IsGameOver = true;
            }
            else
            {
                _consoleview.WriteLine($"The Empire base was weakened and has {OpponentCurrentBase.RemainingHitPoints} health left");
            }
        }
    }

    public void AttackCapitalShip(string userInput, Card card)
    {
        //TODO - add an optout
        int index = int.Parse(userInput);
        int totalattack = (card.AttackValue + card.BonusAttackValue);

        if (OpponentCapitalShipHand[index].HPRemaining < totalattack)
        {
            int overkill = totalattack - OpponentCapitalShipHand[index].HPRemaining;
        }
    }

    /// <summary>
    /// Attacking a card in the shop allows the player access to rewards, and denies the other player buying the card.
    /// </summary>
    /// <param name="targetIndex"></param>
    /// <param name="attackerIndexes"></param>
    private void AttackShopCard(int targetIndex, List<int> attackerIndexes)
    {
        //check user input
        string userErrorPrompt;
        if (targetIndex < 0 || targetIndex >= ShopHand.Count)
        {
            userErrorPrompt = $"\nInvalid Order, sir! The first number in attackShop command is the target, and must be between 1 and 6";
            _consoleview.WriteLine(userErrorPrompt);
            return;
        }

        foreach (int attInd in attackerIndexes)
        {
            userErrorPrompt = $"\nInvalid Order, sir! The second and onward numbers in attackShop command are our troops, so must be within 1 to your hand Count";
            if (attInd < 0 || attInd >= _player.Hand.Count)
            {
                _consoleview.WriteLine(userErrorPrompt);
                return;
            }
        }
        Card targetCard = ShopHand[targetIndex];
        //rule: target must be opposite faction
        if (targetCard.Faction != OpponentFaction)
        {
            userErrorPrompt = $"Not possible, sir! We won't attack a Neutral card or a friendly {_player.Faction} card!";
            _consoleview.WriteLine(userErrorPrompt);
            return;
        }
        //rule: target of a bountyhunt cannot be a capital ship
        if (targetCard.Category == Category.CapitalShip.ToString())
        {
            userErrorPrompt = $"Not possible, sir! We won't go after a capital ship in a bounty hunt- that's just suicide!";
            _consoleview.WriteLine(userErrorPrompt);
            return;
        }

        //build the attack force
        List<Card> attackerCards = new List<Card>();

        foreach (int index in attackerIndexes)
        {
            attackerCards.Add(_player.Hand[index]);
        }

        //rule: captal ships cannot attack a bounty
        if (attackerCards.Any(x => x.Category == Category.CapitalShip.ToString()))
        {
            userErrorPrompt = $"Not possible, sir! We cannot steer our capital ship to chase that target!";

            _consoleview.WriteLine(userErrorPrompt);
            return;
        }
        int attackStrength = attackerCards.Sum(x => x.AttackValue) + attackerCards.Sum(x => x.BonusAttackValue);

        //rule: rodian gunslingers gain attack strength in a bountyhunt prior to strength check
        attackerCards.Where(x => x.Name == "Rodi Gunslingr").ToList().ForEach(x => x.BonusAttackValue += 2);

        //rule: attacker must provide cardCost value worth of attack strength
        if (attackStrength < targetCard.CardCost)
        {
            userErrorPrompt = $"Not possible, sir! We need more forces to beat that target." +
                    $"\n That order sends {attackerCards.Sum(x => x.AttackValue)} attack strength, we need {targetCard.CardCost} strength!";
            _consoleview.WriteLine(userErrorPrompt);
            return;
        }
        //rules ok, the attack order commences
        _consoleview.WriteLine($"\nOk, we're going in!");

        foreach (Card card in attackerCards)
        {
            card.HasAttacked = true;
        }

        ShopHand.RemoveAt(targetIndex);
        ShopDiscardPile.Add(targetCard);
        _consoleview.WriteLine($"You killed {targetCard.Name}. The card has been moved to the shop discard pile. \n");
        _consoleview.WriteLine($"You have {_player.ResourceAvailable} resource remaining.\n");

        ShopHand.Insert(targetIndex, ShopDeck.First());
        _consoleview.WriteLine($"{ShopDeck.First().Name} was added to the shop!\n");
        ShopDeck.Remove(ShopDeck.First());

        //Give bountyhunter rewards
        attackerCards.ForEach(x => RewardBountyHunters(x));

        //give bounty rewards
        RewardFromBountyTarget(targetCard);
    }

    private void RewardBountyHunters(Card card)
    {
        if (card.Name == "IG-88")
        {
            _player.ExilesAvailable += 1;
            _consoleview.WriteLine("IG-88 got his bounty! You gain an exile. Write exileHand or exileDiscard to action");
            return;
        }
        if (card.Name == "Dengar")
        {
            _player.ResourceAvailable += 2;
            _consoleview.WriteLine("Dengar got his bounty! You gain 2 resource. Write buyCard to action!");
            return;
        }
        if (card.Name == "Boba Fett")
        {
            _consoleview.WriteLine("Boba Fett got his bounty!");
            Perk("1D", card);
            return;
        }
        if (card.Name == "Bossk")
        {
            ForceChange(1 * PlayerForceSideModifier);
            _consoleview.WriteLine("Bosk got his bounty!");
            ReportForce();
            return;
        }
        if (card.Name == "Cassian Andor")
        {
            _consoleview.WriteLine("Cassian got his bounty! Sucks to be you, opponent discards aren't implemented yet");
            //todo - that.
        }
        return;
    }

    /// <summary>
    /// Primary entry point for triggering card abilities.
    /// </summary>
    /// <param name="cardIndex"></param>
    private void UseAbility(int cardIndex)
    {
        string userErrorPrompt;
        // check user input
        if (!(cardIndex >= 0 && cardIndex < _player.Hand.Count))
        {
            userErrorPrompt = $"\nInvalid Order, sir! You must give us a card reference from your hand. Choose from 1 to {_player.Hand.Count}";
            _consoleview.WriteLine(userErrorPrompt);
            return;
        }
        Card card = _player.Hand[cardIndex];
        if (card.IsShown == false)
        {
            userErrorPrompt = $"\nInvalid Order, sir! You must show card {cardIndex + 1} first to use it's ability.";
            _consoleview.WriteLine(userErrorPrompt);
            return;
        }

        //review conditional statements on card and action

        string condition = card.ConditionForAbilityBoon;
        string awardForConditionMet = card.AbilityBoonConditionMet;
        string awardForConditionNotMet = card.AbilityBoonConditionNotMet;

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
    private void UseBaseAbility(Player player)
    {
        if (_player.CurrentBase.Name == "Mon Cala" || _player.CurrentBase.Name == "Corellia")
        {
            DisplayCards(ShopHand);
            _player.FreePurchasesOfFactionAvailable += 1;
            _consoleview.RequestUserInput(BaseBuyCard, "which card would you like to buy? Write: \"[int]\"");
            return;
        }

        if(_player.CurrentBase.Name == "Mustafar" || _player.CurrentBase.Name == "Alderaan")
        {
            ForceChange(4 * PlayerForceSideModifier);
            ReportForce();
            return;
        }
        if (_player.CurrentBase.Name == "Kessel" || _player.CurrentBase.Name == "Dagobah")
        {
            _player.ExilesAvailable += 3; //TODO enforce this happening at start of turn
            _consoleview.WriteLine("The programe doesn't have the ability to enforce you doing this right now, but pretty please do it...");
            ReportForce();
            return;
        }
        if (_player.CurrentBase.Name == "Tatooine")
        {
            DisplayCardsShort(ShopDiscardPile);
            _consoleview.RequestUserInput(TatooineSwapRequestShopIndex, "which card would you like to put in the galaxy row? Write: \"[int]\"");
            return;
        }
        if (_player.CurrentBase.Name == "Rodia")
        {
            List<Card> Rebels = ShopHand.FindAll(x => x.Faction == Faction.Rebel);

            Rebels.ForEach(x =>
            {
                OpponentCurrentBase.HPChange(-1);
                ShopDiscardPile.Add(x);
                ShopHand.Remove(x);
            });
            return;
        }
    }

    public void TatooineSwapRequestShopIndex(string userInput, Card nullC)
    {
        int index = int.Parse(userInput);
        Card cardToFish = ShopDiscardPile[index];
        DisplayCards(ShopHand);
        _consoleview.RequestUserInput(TatooineSwap, "Which card would you like to put in galaxy discard pile? Write \"[int]\"", cardToFish);
    }
    public void TatooineSwap(string userInput, Card card)
    {
        int index = int.Parse(userInput);
        _player.ResourceAvailable += 1;
        ReportResource(_player);
        Card cardOut = ShopHand[index];
        ShopHand.Remove(cardOut);
        ShopHand.Insert(index, card);

        ShopDiscardPile.Add(cardOut);
        ShopDiscardPile.Remove(card);
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
    void DisplayBaseCards(List<BaseCard> hand)
    {
        _consoleview.WriteLine($"\n");
        int cardNo = 0;
        int lineNo = 0;
        List<string> lines = new List<string>();

        foreach (BaseCard card in hand)
        {
            using (StringReader reader = new StringReader(card.DisplayText()))
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

        for (int i = 0; i < hand.Count; i++)
        {
            _consoleview.WriteLine($" |{PadBoth(Convert.ToString(i + 1), 7)}|{PadBoth(Convert.ToString(hand[i].CardCost), 6)}| {PadBoth(hand[i].Name, 14)}|");
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
            _consoleview.WriteLine($"The force is with the Empire with + {Math.Abs(ForceBalance)}");
        }
    }

    private void ReportBases()
    {
        ReportBase();
        ReportOpponentBase();
    }

    private void ReportOpponentBase()
    {
        _consoleview.WriteLine($"The opponents base at {OpponentCurrentBase.Name} now has {OpponentCurrentBase.RemainingHitPoints} HP");
    }

    private void ReportBase()
    {
        //TODO capital ships
        _consoleview.WriteLine($"Your base at {_player.CurrentBase.Name} now has {_player.CurrentBase.RemainingHitPoints} HP");
        _consoleview.WriteLine($"Your base ability is {(_player.CurrentBase.IsAbilityAvailable ? "" : "not")} available!");
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

    void GameOver()
    {
        throw new NotImplementedException(message: "game over not implemented; how did you even get here!");
    }
    #endregion
}