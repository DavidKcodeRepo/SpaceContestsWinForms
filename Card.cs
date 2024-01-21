using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SpaceContest;

public class Card
{
	// TODO - lots of properties, move these to a dictionary?
	public int Id { get; set; }
	public string Name { get; set; }
	public Faction Faction {get;set;}
	public int CardCost { get; set; }
	public int ResourceValue { get; set; }
	public int AttackValue { get; set; }
	public int BonusAttackValue { get; set; } = 0;

	public int ForceValue { get; set; }
	public string Category { get; set; }
	public string Ability { get; set; }
	public string ConditionForAbilityBoon { get; set; }
	public string AbilityBoonConditionMet { get; set; }
	public string AbilityBoonConditionNotMet { get; set; }

	public string Reward { get; set; }
	public string ShownCardText { get; set; }
	public string HiddenCardText { get; set; }

	public bool IsShown { get; set; }

public Card(string line)
	{
		List<string> lineData = line.Split(",").ToList();

		IsShown = true;
		Name = PadBoth(lineData[8],14);

		Faction _faction = new Faction();
		_ = Enum.TryParse<Faction>(lineData[2], true, out _faction);
		Faction = _faction;

		//Faction = PadBoth((lineData[2]),7);
		
		CardCost = Convert.ToInt32(lineData[0]);
		ResourceValue = Convert.ToInt32(lineData[1]);
		AttackValue = Convert.ToInt32(lineData[3]);
		ForceValue = Convert.ToInt32(lineData[4]);
		Category = PadBoth(lineData[5],22);
		
		Ability = lineData[6];
		List<string> abilityLines = new List<string>();


		if (Ability.Contains("NULL"))
		{
			for (int i = 0; i < 7; i++) { string row = PadBoth("", 22); abilityLines.Add(row); }
		}
		else
		{
			abilityLines = PutStringInColumns(lineData[6], 22, 7);

			for (int i = 0; i < abilityLines.Count; i++)
			{
				abilityLines[i] = PadBoth(abilityLines[i], 22);
			}
			for (int i = abilityLines.Count; i < 7; i++)
			{
				abilityLines.Add( PadBoth("", 22));
			}
		}

		Reward = lineData[7];
		List<string> rewardLines = new List<string>();

		if (Reward.Contains("NULL"))
		{
			for (int i = 0; i < 4; i++) { string row = PadBoth("", 22); rewardLines.Add(row); }
		}
		else
		{
			rewardLines = PutStringInColumns(lineData[7], 20, 4);

			for (int i = 0; i < rewardLines.Count; i++)
			{
				rewardLines[i] = PadBoth(rewardLines[i], 22);
			}
			for (int i = rewardLines.Count; i < 4; i++)
			{
				rewardLines.Add(PadBoth(" ", 22));
			}
		}

		ShownCardText = 
			 $"  _________________________  \r\n"+
			 $" /                         \\ \r\n"+
			 $"/    {Name}    H{CardCost}   \\\r\n"+
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

				HiddenCardText =
			 $"  _ _ _ _ _ _ _ _ _ _ _ _ _  \r\n" +
			 $"                             \r\n" +
			 $"/    {Name}    H{CardCost}   \\\r\n" +
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

	public string DisplayText(bool IsShown)
	{
		if (IsShown) 
		{
			return ShownCardText;
		}
		else
		{ 
			return HiddenCardText; 
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

	private List<string> PutStringInColumns(string inputString, int colWidth, int rowNum)
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
				if((line + " " + words[i]).Length < colWidth)
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
			if (wordbreaks[i] +1 < nextLineBreak)
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
		for(int i = 0; i < rowNum - linesAdded; i++) { output.Add(""); }

		return output;
	}
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
}


