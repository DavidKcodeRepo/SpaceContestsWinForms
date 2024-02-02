using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SpaceContest;

/// <summary>
/// The basic cardModel of the game. 
/// Cards contain resources, designations, and abilities and represent characters in the universe who fight on the players' teams.
/// The class contains these properties, and methods for constructing card properties into a multiline template for display by the ConsoleView.
/// /// </summary>
public class Card
{
	#region Properties
	// TODO - lots of properties, move these to a dictionary?
	// TODO - ID unused? Maybe dictionary index?
	public int Id { get; set; }

	// game: Name of the card.
	public string Name { get; set; }
	// game: The faction to which the card is associated.
	public Faction Faction { get; set; }
	// game: The cost to buy the card, or HP to kill with attack, from the shop.
	public int CardCost { get; set; }
	// game: The amount of resource, that upon showing the card, is given to the player.
	public int ResourceValue { get; set; }
	// game: Cards can have an attackStrength to attack opponent / shop.
	public int AttackValue { get; set; }

	// game: abilities can grant bonus attack strength to the card.
	public int BonusAttackValue { get; set; } = 0;

	// game: Cards can have a resource to swayForce to players favour. Granted upon cardShown.  
	public int ForceValue { get; set; }
	// game: Cards have a category, which restricts how other cards interact with them when using abilities.  
	public string Category { get; set; }
	// game: Cards have an IsUniqueCharacter property, which restricts how other cards interact with them when using abilities.  
	public bool IsUnique { get; set; }
	// game: cards have abilities.
	// this string holds the ability text as written on the card.
	public string AbilityText { get; set; }

	// game: card abilities vary upon conditions (commonly the force balance or faction of next-revealed card in shop deck)
	// note: these abilities are programmed in a code of [char][int]^n, where char is ability affect, and int is the number of times
	//  		e.g. ability code X1D2 grants one exile and 2 draw a cards. review documentation for more advice.
	// note: some cards are non-conditional. Where this is the case,  singular award text is defined in awardForConditionMet, and condition == true
	// TODO- define a type for the ability to make typesafe?
	public string ConditionForAbilityBoon { get; set; }
	public string AbilityBoonConditionMet { get; set; }
	public string AbilityBoonConditionNotMet { get; set; }

	// game: Card rewards are given (e.g. resources, forceSway) upon killing an opposing faction card in the shop.
	// note: reward is coded as per abilityBoons
	public string Reward { get; set; }
	// note: Cards are represented in console with multiline text. See below for construction from card template.
	// note: only difference between hidden vs shown is dashed vs solid outline
	public string ShownCardConsoleText { get; set; }
	public string HiddenCardConsoleText { get; set; }

	// note: A round-tracker stat to check if card has been shown (showCard game action).
	// game: cards must be shown in gameround to grant resources/attack, but must not be shown to be used as fodder for other abilities.
	public bool IsShown { get; set; } = false;
	// note: A round-tracker stat to check if card has attacked (attackBase / attackShopCard game actions).
	// game: cards cannot attack twice per round.
	public bool HasAttacked { get; set; } = false; 
	/// <summary>
	/// game: cards of category "capital ships" can take damage in each round, and so must track this stat through rounds
	/// </summary>
	public int HPRemaining { get; set; }
	#endregion

	/// <summary>
	/// Constructor populates card properties from an csv file
	/// TODO - csv is very oldschool, upgrade gameContent to a better table data format, sml?
	/// </summary>
	/// <param name="line"></param>
	public Card(string line)
	{
        //read line data into class.
        List<string> lineData = line.Split(",").ToList();

        Name = lineData[8];

		Faction _faction = new Faction();
		_ = Enum.TryParse<Faction>(lineData[2], true, out _faction);
		Faction = _faction;

		//Faction = PadBoth((lineData[2]),7); //TODO - why isn't this needed?
		
		CardCost = Convert.ToInt32(lineData[0]);
		ResourceValue = Convert.ToInt32(lineData[1]);
		AttackValue = Convert.ToInt32(lineData[3]);
		ForceValue = Convert.ToInt32(lineData[4]);
		Category = PadBoth(lineData[5],22);
		IsUnique = Convert.ToBoolean(lineData[10]);
		
		AbilityText = lineData[6];

		//construct Ability text (requires line breaks and padding to fit template).
		List<string> abilityLines = new List<string>();
		//null indicates no text to be shown- so we pad spaces
		if (AbilityText.Contains("NULL"))
		{
			for (int i = 0; i < 7; i++) { string row = PadBoth("", 22); abilityLines.Add(row); }
		}
		else
		{
			abilityLines = ConvertToMultiLineText(lineData[6], 22, 7);

			for (int i = 0; i < abilityLines.Count; i++)
			{
				abilityLines[i] = PadBoth(abilityLines[i], 22);
			}
			for (int i = abilityLines.Count; i < 7; i++)
			{
				abilityLines.Add( PadBoth("", 22));
			}
		}

        //construct Rewardtext (requires line breaks and padding to fit template).

        Reward = lineData[11];
		List<string> rewardLines = new List<string>();

		if (Reward.Contains("NULL"))
		{
			for (int i = 0; i < 4; i++) { string row = PadBoth("", 22); rewardLines.Add(row); }
		}
		else
		{
			rewardLines = ConvertToMultiLineText(lineData[7], 20, 4);

			for (int i = 0; i < rewardLines.Count; i++)
			{
				rewardLines[i] = PadBoth(rewardLines[i], 22);
			}
			for (int i = rewardLines.Count; i < 4; i++)
			{
				rewardLines.Add(PadBoth(" ", 22));
			}
		}

		ShownCardConsoleText = 
			 $"  _________________________  \r\n"+
			 $" /                         \\ \r\n"+
			 $"/    {PadBoth(Name,14)}    H{CardCost}   \\\r\n"+
			 $"|                           |\r\n"+
			 $"|    R  [{ResourceValue}]     [{PadBoth((Faction.ToString()),7)}]   |\r\n"+
			 $"|    χ  [{AttackValue}]                 |\r\n"+
			 $"|    ϴ  [{ForceValue}]                 |\r\n"+
			 $"|                           |\r\n"+
			 $"|   {Category}  |\r\n"+
			 $"|                           |\r\n"+
			 $"|   {abilityLines[0]}  |\r\n"+
			 $"|   {abilityLines[1]}  |\r\n"+
			 $"|   {abilityLines[2]}  |\r\n"+
			 $"|   {abilityLines[3]}  |\r\n"+
			 $"|   {abilityLines[4]}  |\r\n"+
			 $"|   {abilityLines[5]}  |\r\n"+
			 $"|   {abilityLines[6]}  |\r\n"+
			 $"|     _  _  _  _  _  _  _   |\r\n"+
			 $"|                           |\r\n"+
			 $"|   {rewardLines[0]}  |\r\n"+
			 $"|   {rewardLines[1]}  |\r\n"+
			 $"|   {rewardLines[2]}  |\r\n"+
			 $"|   {rewardLines[3]}  |\r\n"+
			 $"\\                           /\r\n"+
			 $" \\_________________________/ ";

				HiddenCardConsoleText =
			 $"  _ _ _ _ _ _ _ _ _ _ _ _ _  \r\n" +
			 $"                             \r\n" +
			 $"/    {PadBoth(Name, 14)}    H{CardCost}   \\\r\n" +
			 $"                             \r\n" +
			 $"|    R  [{ResourceValue}]     [{Faction}]     |\r\n" +
			 $"     χ  [{AttackValue}]                  \r\n" +
			 $"|    ϴ  [{ForceValue}]                 |\r\n" +
			 $"                             \r\n" +
			 $"|   {Category}  |\r\n" +
			 $"                             \r\n" +
			 $"|   {abilityLines[0]}  |\r\n" +
			 $"    {abilityLines[1]}   \r\n" +
			 $"|   {abilityLines[2]}  |\r\n" +
			 $"    {abilityLines[3]}   \r\n" +
			 $"|   {abilityLines[4]}  |\r\n" +
			 $"    {abilityLines[5]}   \r\n" +
			 $"|   {abilityLines[6]}  |\r\n" +
			 $"      _  _  _  _  _  _  _    \r\n" +
			 $"|                           |\r\n" +
			 $"    {rewardLines[0]}   \r\n" +
			 $"|   {rewardLines[1]}  |\r\n" +
			 $"    {rewardLines[2]}   \r\n" +
			 $"|   {rewardLines[3]}  |\r\n" +
			 $"                             \r\n" +
			 $" \\ _ _ _ _ _ _ _ _ _ _ _ _ / ";

		ConditionForAbilityBoon = lineData[14]; // 14
		AbilityBoonConditionMet = lineData[15]; // 15
		AbilityBoonConditionNotMet = lineData[13]; // 13
	}

	/// <summary>
	/// A method for the console to call text to display cards.
	/// </summary>
	/// <param name="IsShown"></param>
	/// <returns></returns>
	public string DisplayText(bool IsShown)
	{
		if (IsShown) 
		{
			return ShownCardConsoleText;
		}
		else
		{ 
			return HiddenCardConsoleText; 
		}
	}

	#region TemplateBuilders
	/// <summary>
	/// A helper method used in constructing cards from properties & card template.
	/// </summary>
	/// <param name="source"></param>
	/// <param name="length"></param>
	/// <returns></returns>
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

	/// <summary>
	/// A helper method used in constructing cards from properties & card template.
	/// </summary>
	/// <param name="inputString"></param>
	/// <param name="colWidth"></param>
	/// <param name="rowNum"></param>
	/// <returns></returns>
	private List<string> ConvertToMultiLineText(string inputString, int colWidth, int rowNum)
	{
		//inputString = "Your opponent discards 1 card from their hand(at Random if the force is with you)"; //test string
		List<string> output = new List<string>();

		List<int> wordbreaks = FindCharIndexes(inputString, ' ');
		int nextLineBreak = colWidth;
		List<string> words = inputString.Split(' ').ToList();
		string line = "";

		//go through every space
		for (int i = 0; i <= wordbreaks.Count; i++)
		{
			//Special condition: are you on the very last space?
			if (i == wordbreaks.Count)
			{
				//check this is going to fit
				if ((line + " " + words[i]).Length < colWidth)
				{
					output.Add(line + " " + words[i]);
				}
				//else cut the last word onto new line.
				else
				{
					output.Add(line);
					output.Add(words[i]);
				}
				break;
			}
			//if not over the next linebreak char, we're good to add the word we just passed
			if (wordbreaks[i] + 1 < nextLineBreak)
			{
				line = line + " " + (words[i]);

			}
			//otherwise, its a newline			
			else
			{
				//calc start and max end character
				output.Add(line);
				nextLineBreak = wordbreaks[i] + colWidth - words[i].Length;
				line = words[i];
			}
		}
		int linesAdded = output.Count;
		for (int i = 0; i < rowNum - linesAdded; i++) { output.Add(""); }

		return output;
	}

	/// <summary>
	/// A helper method used in constructing cards from properties & card template.
	/// </summary>
	/// <param name="inputString"></param>
	/// <param name="charToFind"></param>
	/// <returns></returns>
	private List<int> FindCharIndexes(string inputString, char charToFind)
	{
		List<int> indexes = new List<int>();

		for (int i = 0; i < inputString.Length; i++)
		{
			if (inputString[i] == charToFind)
			{
				indexes.Add(i);
			}
		}
		return indexes;
	} 
	#endregion
}


