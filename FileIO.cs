using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace SpaceContest;
public static class FileIO
{
	public static string FullFilePath(this string fileName)
	{
		return $"{ConfigurationManager.AppSettings["filePath"]}\\{fileName}";
	}

	public static List<string> LoadFile(this string fileName)
	{
		if (!File.Exists(fileName))
		{
			return new List<string>();
		}
		return File.ReadAllLines(fileName).ToList();
	}
}
public static class GlobalConfig
{
	public const string CardData = "CardTables.csv";
    public const string BaseCardData = "BaseCardTables.csv";

}
