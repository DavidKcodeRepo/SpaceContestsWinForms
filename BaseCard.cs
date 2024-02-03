using SpaceContest;
using System.Xml.Linq;

public class BaseCard
{
    #region Properties

    // Name of the Base. 
    public string Name { get; }
    // A bit of flavour text that loosely describes the lore of the cards of ability.
    public string FlavourString { get; }
    // The faction to which the card is associated.
    public Faction Faction { get; }
    // The amount of HP the base provides the player.
    public int StartingHitPoints { get; }
    public string AbilityText { get; }
    /// <summary>
    /// Multilinetext of the card written for console view
    /// </summary>
    public string ConsoleText { get;}

    // game: Some abilities are only used upon revealing the base, other abilities may be activated once or many per turn.
    public bool IsAbilityRevealDependent { get; }

    // game: A gamestate stat for determining game progress & triggers. 
    public int RemainingHitPoints { get; set; }

    // game: A gamestate stat. Base Abilities may only be used once per turn.
    public bool IsAbilityAvailable { get; set; }

    public int HothDefence { get; set; } = 2;

    #endregion

    /// <summary>
    /// Constructor populates card properties from an csv file
    /// TODO - csv is very oldschool, upgrade gameContent to a better table data format, sml?
    /// </summary>
    /// <param name="line"></param>
    public BaseCard(string line)
    {
        //read line data into class.
        List<string> lineData = line.Split(",").ToList();

        Faction _faction = new Faction();
        _ = Enum.TryParse<Faction>(lineData[0], true, out _faction);
        
        //set readonly values
        Faction = _faction;
        StartingHitPoints = int.Parse(lineData[1]);
        Name = lineData[2];
        FlavourString = lineData[3];
        AbilityText = lineData[4];

        //set gamestate values
        RemainingHitPoints = StartingHitPoints;
        IsAbilityAvailable = true;

        //construct Ability text (requires line breaks and padding to fit template).
        List<string> abilityLines = new List<string>();
        //null indicates no text to be shown- so we pad spaces
        if (AbilityText.Contains("NULL"))
        {
            for (int i = 0; i < 7; i++) { string row = PadBoth("", 44); abilityLines.Add(row); }
        }
        else
        {
            abilityLines = ConvertToMultiLineText(AbilityText, 44, 5);

            for (int i = 0; i < abilityLines.Count; i++)
            {
                abilityLines[i] = PadBoth(abilityLines[i], 44);
            }
            for (int i = abilityLines.Count; i < 7; i++)
            {
                abilityLines.Add(PadBoth("", 44));
            }
        }

        ConsoleText =

$"\n  ____________________________________________   " +
$"\n /                                            \\  " +
$"\n/                  {PadBoth(lineData[2],12)}                \\ " +
$"\n|                     {PadBoth(lineData[0],6)}                   | " +
$"\n|              {PadBoth(lineData[3],20)}            | " +
$"\n|                                              |" +
$"\n| {abilityLines[0]} |" +
$"\n| {abilityLines[1]} |" +
$"\n| {abilityLines[2]} |" +
$"\n| {abilityLines[3]} |" +
$"\n| {abilityLines[4]} |" +
$"\n|                                              |" +
$"\n\\                      {PadBoth(lineData[1],2)}                      /" +
$"\n \\____________________________________________/\n";

    }

    /// <summary>
    /// A method for the console to call text to display cards.
    /// </summary>
    public string DisplayText()
    {
        return ConsoleText;
    }

    //TODO - move this and Card template builder (samecode) to a consoleViewHelper class?
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

    public void HPChange(int delta)
    {
        RemainingHitPoints += delta;
        RemainingHitPoints = Math.Clamp(RemainingHitPoints, 0, StartingHitPoints);
    }
}