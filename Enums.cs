using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceContest
{
	public enum PlayerCommand
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

	public enum Faction
	{
		Empire,
		Rebel,
		Neutral
	}

	public enum Category
	{
		Transport,
		Vehicle,
		Trooper,
		Scoundrel,
		Officer,
		Fighter,
		Sith,
		CapitalShip,
		Jedi,
		BountyHunter,
	}
}
