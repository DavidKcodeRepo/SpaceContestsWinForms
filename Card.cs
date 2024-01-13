using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceContest;

public class Card
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string Faction { get; set; }
	public int CardCost { get; set; }
	public int ResourceValue { get; set; }
	public int AttackValue { get; set; }
	public int ForceValue { get; set; }
	public string Category { get; set; }
	public string Ability { get; set; }
	public string Reward { get; set; }
	public string CardText { get; set; }

public Card(string line)
	{
		List<string> lineData = line.Split(",").ToList();

		Name = PadBoth(lineData[8],14);
		Faction = PadBoth(lineData[2],7);
		CardCost = Convert.ToInt32(lineData[0]);
		ResourceValue = Convert.ToInt32(lineData[1]);
		AttackValue = Convert.ToInt32(lineData[3]);
		ForceValue = Convert.ToInt32(lineData[4]);
		Category = PadBoth(lineData[5],22);
		Ability = lineData[6];
		Reward = lineData[7];

		CardText = 
			 $"  _________________________  \r\n"+
			 $" /                         \\ \r\n"+
			 $"/    {Name}    H{CardCost}   \\\r\n"+
			 $"|                           |\r\n"+
			 $"|    R  [{ResourceValue}]     [{Faction}]   |\r\n"+
			 $"|    χ  [{AttackValue}]                 |\r\n"+
			 $"|    ϴ  [{ForceValue}]                 |\r\n"+
			 $"|                           |\r\n"+
			 $"|   {Category}  |\r\n"+
			 $"|                           |\r\n"+
			 $"|   6666666666666666666666  |\r\n"+
			 $"|   6666666666666666666666  |\r\n"+
			 $"|   6666666666666666666666  |\r\n"+
			 $"|   6666666666666666666666  |\r\n"+
			 $"|   6666666666666666666666  |\r\n"+
			 $"|   6666666666666666666666  |\r\n"+
			 $"|   6666666666666666666666  |\r\n"+
			 $"|     _  _  _  _  _  _  _   |\r\n"+
			 $"|                           |\r\n"+
			 $"|   7777777777777777777777  |\r\n"+
			 $"|   7777777777777777777777  |\r\n"+
			 $"|   7777777777777777777777  |\r\n"+
			 $"|   7777777777777777777777  |\r\n"+
			 $"\\                           /\r\n"+
			 $" \\_________________________/ ";
	}

	public override string ToString()
	{
		return CardText;
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
}


