using SpaceContestsWinForms;
using System.Linq;
namespace SpaceContest;

/// <summary>
/// Contains the players decks (discard pile, current hand and remaining deck), tracks their choices and actions affects relating to their decks.
/// Holds tracker stats about the player that persist through the rounds.
/// </summary>
public class Player
{
	/// <summary>
	/// reference to console view is held to provide user with feedback (via console) when player actions 
	/// </summary>
	private ConsoleView _consoleview;
	
	/// <summary>
	/// game: players belong to a faction. The players faction limits them to which cards they can buy and attack.
	/// </summary>
	public Faction Faction { get; set; }

	//TODO - implement multiple bases, at current build if you lose your first base you lose the game!
	//public BaseCard CurrentBase { get; set; }
	//public List<BaseCard> RemainingBaseCards { get; set; }
	//public BaseCard Base { get;set; }
	//public bool IsBaseAbilityAvailable { get; set; } = true;

	#region TrackerStats

	/// <summary>
	/// The hit points of the current base. When this reaches zero the player loses their current base.
	/// </summary>
	public int BaseHitPoints { get; set; }

	/// <summary>
	/// A tracker stat. 
	/// game: Some abilities are dependent on presence of player having capital ship. 
	/// </summary>
	public bool HasCapitalShipShown { get; set; }
	/// <summary>
	/// A tracker stat. 
	/// game: Some abilities grant new purchases to go to top of deck, meaning card is available to player earlier.
	/// </summary>
	public bool NextPurchaseToTopOfDeck { get; set; }

	/// <summary>
	/// game: Players use resource to purchase cards from the shop.
	/// </summary>
	public int ResourceAvailable { get; set; }
	/// <summary>
	/// game: Players use attack strength from cards to attack the enemy base. 
	/// note: specific cards are used to do attacks, but this total is a useful for the player to consider options.
	/// </summary>
	public int AttackAvailable { get; set; }
	/// <summary>
	/// game: Players can gain the ability to exile cards from the game from certain cards. Exiling weak cards strengthens the players deck.
	/// </summary>
	public int ExilesAvailable { get; set; }
	/// <summary>
	/// game: Players can gain the ability to purchase cards for free from the shop, saving resources.
	/// note: this stat is be used to purchase cards of same faction. 
	/// </summary>
	public int FreePurchasesOfFactionAvailable { get; set; }
	/// <summary>
	/// Game: The players deck is of yet to be used cards. They draw from this deck.
	/// Note: The First card in the list is the top of the deck.
	/// </summary> 
	#endregion
	// TODO - might be interesting to compare performance with a stack. Lists are re-programmed when removing from first item.

	#region PlayerCards
	public List<Card> Deck { get; set; } = new List<Card>();
	/// <summary>
	/// game: the player has a hand of cards. Usually 5.
	/// </summary>
	public List<Card> Hand { get; set; } = new List<Card>();
	/// <summary>
	/// game: After a round, the player discards cards. 
	/// game: The discard pile can be accessed to gain specific cards, but will be normally recycled to remake the deck when empty.
	/// </summary>
	public List<Card> DiscardPile { get; set; } = new List<Card>(); 
	#endregion

	public Player(ConsoleView consoleview)
	{
		_consoleview = consoleview;

		// assign default starting values
		// TODO - extend player choice to include Empire
		this.Faction = Faction.Rebel;
		this.BaseHitPoints = 8;
		//this.IsBaseAbilityAvailable = true;
		this.ResourceAvailable = 0;
		this.AttackAvailable = 0;
		this.ExilesAvailable = 0;
		this.FreePurchasesOfFactionAvailable = 0;
        this.NextPurchaseToTopOfDeck = false;

    }

	#region cardHandling
	/// <summary>
	/// game: at end of turn, player draws a new hand.
	/// </summary>
	public void DrawNewHand()
	{
		_consoleview.WriteLine("Drawing a new hand from player deck");

		for (int i = 0; i < 5; i++)
		{
			if (Deck.Count == 0)
			{
				_consoleview.WriteLine("Player deck is empty...");
				ResetPlayerDeck();
			}
			Card card = Deck.First();
			card.IsShown = false;
			card.HasAttacked = false;
			Hand.Add(Deck.First());
			Deck.Remove(Deck.First());
		}

		_consoleview.WriteLine($"You have {Hand.Count} cards in Hand");
	}

	/// <summary>
	/// game: when the players deck is empty and they next need to draw a new card, they create a new player deck by shuffling their discard pile.
	/// </summary>
	public void ResetPlayerDeck()
	{
		_consoleview.WriteLine("Shuffling discard pile to build the next player deck...");
		DiscardPile = DiscardPile.OrderBy(x => Random.Shared.Next()).ToList();

		for (int i = 0; i < DiscardPile.Count(); i++)
		{
			Deck.Add(DiscardPile.First());
			DiscardPile.Remove(DiscardPile.First());
		}
	}

	/// <summary>
	/// game: draw card is used in draw hand, but can be also be triggered by other game conditions.
	/// </summary>
    public void DrawCard()
    {
        _consoleview.WriteLine("Drawing a card hand from player deck");

		//game: when no cards to draw in playerDeck, discard deck is reset from discards 
        if (Deck.Count == 0)
        {
            _consoleview.WriteLine("Player deck is empty...");
            ResetPlayerDeck();
        }

        Card card = Deck.First();
		//as card is drawn, reset default values on trackedstats 
        card.IsShown = false;
        card.HasAttacked = false;
        card.BonusAttackValue = 0;
		//move card
        Hand.Add(Deck.First());
        Deck.Remove(Deck.First());

        _consoleview.WriteLine($"You drew the {card.Name}");
    }

	/// <summary>
	/// game: at the end of turn, the players discards their hand to discardPile
	/// </summary>
    public void DiscardHand()
	{
		for (int i = 0; i < Hand.Count; i++)
		{
			Card card = Hand[i];
			if (card.Category == Category.CapitalShip.ToString())
			{
				continue;
			}
			card.BonusAttackValue = 0;
			DiscardPile.AddRange(Hand);
			Hand.RemoveAll(item => true);
		}
	}

	/// <summary>
	/// game: some abilities on card allow either all cards or filtered subset of cards to be "fished" (search deck, retrieve a card)
	/// </summary>
	/// <param name="card"></param>
	public void FishDiscard(Card card)
	{
		// predicate is used to filter cards for certain cards that trigger this method, hard coded for ease.
		// Initial predicate value is chosen to be a fully inclusive filter (it removes none).
		Predicate<Card> filter = x => x.Name is string;

        if (card.Name == "Milli Falcon") { filter = x => x.IsUnique == true; }
        if (card.Name == "AT-AT") { filter = x => x.Category == Category.Trooper.ToString(); }
        if (card.Name == "Jabba's Barge") { filter = x => x.Category == Category.BountyHunter.ToString(); }

		List<Card> cardsOfCategory = DiscardPile.FindAll(filter).ToList(); 

		_consoleview.WriteLine("please select a card. Write \"[int]\" to chose a card");
		for(int i = 0;i < cardsOfCategory.Count;i++ )
		{
			_consoleview.WriteLine($"|{i}|{cardsOfCategory[i].Name}|{cardsOfCategory[i].CardCost}|");
			_consoleview.RequestUserInput(FishDiscardUserchoice, "please select a card. Write 1, 2, 3",card);
        }
    }

	/// <summary>
	/// note: some fishDiscard cards require a player choice to be made. To support this, this method can be called after choice is made.
	/// </summary>
	/// <param name="userChoice"></param>
	/// <param name="card"></param>
    public void FishDiscardUserchoice(string userChoice, Card card)
    {
		//check user reponse
		int.TryParse(userChoice, out int index);

        Predicate<Card> predicate = x => x.Name is string;

        if (card.Name == "Milli Falcon") { predicate = x => x.IsUnique == true; }
        if (card.Name == "AT-AT") { predicate = x => x.Category == Category.Trooper.ToString(); }
        if (card.Name == "Jabba's Barge") { predicate = x => x.Category == Category.BountyHunter.ToString(); }

        List<Card> cardsOfCategory = DiscardPile.FindAll(predicate).ToList();

		Card userCard = cardsOfCategory[index];

		Hand.Add(userCard);
		DiscardPile.Remove(userCard);
		return;
    }

    #endregion
}
