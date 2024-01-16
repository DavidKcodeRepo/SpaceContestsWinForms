using SpaceContest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceContest;
using SpaceContestsWinForms;

namespace SpaceContest;

// The player class contains the logic for doing the players actions and stores their choices in the game.
public class Player
{
	/// <summary>
	/// consoleview is needed for player to send instructions to console
	/// </summary>
	private ConsoleView consoleview;
	private Game game;
	public Faction PlayerFaction { get; set; }
	//public BaseCard CurrentBase { get; set; }
	//public List<Card> CaptialShips { get; set; }

	public int BaseHitPoints { get; set; }

	public BaseCard Base { get;set; }
	public bool IsBaseAbilityAvailable { get; set; }
	public List<bool> IsHandShown { get; set; }

	//public List<BaseCard> RemainingBaseCards { get; set; }

	public Player(ConsoleView consoleView, Game game)
	{
		this.consoleview = consoleView;
		this.PlayerFaction = Faction.Rebel;
		this.game = game;
		this.BaseHitPoints = 8;
		this.IsBaseAbilityAvailable = false;
		this.IsHandShown = new List<bool>();
		//this.CurrentBase = // TODO get the starter base from deck
	}


}
