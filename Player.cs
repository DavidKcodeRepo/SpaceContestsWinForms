using SpaceContest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceContest;
using SpaceContestsWinForms;

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

	public int BaseHitPoints { get; set; }
	public BaseCard Base { get;set; }
	public bool IsBaseAbilityAvailable { get; set; }
	public List<bool> IsHandShown { get; set; }
	public int ResourceAvailable { get; set; }
	public int AttackAvailable { get; set; }

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
	}

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
	public void DiscardHand()
	{
		for (int i = 0; i < Hand.Count; i++)
		{
			DiscardPile.AddRange(Hand);
			Hand.RemoveAll(item => true);
		}
	}
}
