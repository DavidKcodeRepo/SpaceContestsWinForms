using SpaceContestsWinForms;
using System.Linq;
namespace SpaceContest;

// The player class contains the logic for the players cards, stores their choices and commits their actions.
public class Player
{
	private ConsoleView _consoleview;
	
	//the players faction limits them to which cards they can buy and attack.
	public Faction Faction { get; set; }

	//public BaseCard CurrentBase { get; set; }
	//public List<Card> CaptialShips { get; set; }
	//public List<BaseCard> RemainingBaseCards { get; set; }

	//TODO - implement bases
	public int BaseHitPoints { get; set; }
	public BaseCard Base { get;set; }
	public bool IsBaseAbilityAvailable { get; set; }
	public List<bool> IsHandShown { get; set; }
	public int ResourceAvailable { get; set; }
	public int AttackAvailable { get; set; }
	public int ExilesAvailable { get; set; }
    public int FreePurchasesAvailable { get; set; }
	public bool NextPurchaseToTopOfDeck { get; set; }

    public List<Card> Deck { get; set; } = new List<Card>();
	public List<Card> Hand { get; set; } = new List<Card>();
	public List<Card> DiscardPile { get; set; } = new List<Card>();

	public Player(ConsoleView consoleview)
	{
		_consoleview = consoleview;

		this.Faction = Faction.Rebel;
		this.BaseHitPoints = 8;
		this.IsBaseAbilityAvailable = false;
		this.IsHandShown = new List<bool>();
		this.ResourceAvailable = 0;
		this.AttackAvailable = 0;
		this.ExilesAvailable = 0;
		this.FreePurchasesAvailable = 0;
        this.NextPurchaseToTopOfDeck = false;

    }

	#region cardHandling
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

    public void DrawCard()
    {
        _consoleview.WriteLine("Drawing a card hand from player deck");

        if (Deck.Count == 0)
        {
            _consoleview.WriteLine("Player deck is empty...");
            ResetPlayerDeck();
        }

        Card card = Deck.First();
        card.IsShown = false;
        card.HasAttacked = false;
        card.BonusAttackValue = 0;
        Hand.Add(Deck.First());
        Deck.Remove(Deck.First());

        _consoleview.WriteLine($"You drew the {card.Name}");
    }
    public void DiscardHand()
	{
		for (int i = 0; i < Hand.Count; i++)
		{
			DiscardPile.AddRange(Hand);
			Hand.RemoveAll(item => true);
		}
	}

	public void FishDiscard(Card card)
	{
		// default predicate ensures that all cards are returned for discard fishing (e.g. tatooine, jawa), and is overwritten for cards that fish specifically
		Predicate<Card> predicate = x => x.Name is string;

        if (card.Name == "Milli Falcon") { predicate = x => x.IsUnique == true; }
        if (card.Name == "AT-AT") { predicate = x => x.Category == Category.Trooper.ToString(); }
        if (card.Name == "Jabba's Barge") { predicate = x => x.Category == Category.BountyHunter.ToString(); }

		List<Card> cardsOfCategory = DiscardPile.FindAll(predicate).ToList(); 

		_consoleview.WriteLine("please select a card. Write \"[int]\" to chose a card");
		for(int i = 0;i < cardsOfCategory.Count;i++ )
		{
			_consoleview.WriteLine($"|{i}|{cardsOfCategory[i].Name}|{cardsOfCategory[i].CardCost}|");
			_consoleview.RequestUserInput(FishDiscardUserchoice, "please select a card. Write 1, 2, 3",card);
        }
    }

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
