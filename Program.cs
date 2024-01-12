using SpaceContest;

namespace SpaceContestsWinForms;

internal static class Program
{
	private static ConsoleView _consoleView;
	private static Game _gameInstance;
	
	/// <summary>
	///  The main entry point for the application.
	/// </summary>
	[STAThread]
	static void Main()
	{
		ApplicationConfiguration.Initialize();
		_consoleView = new ConsoleView();
		_gameInstance = new Game(_consoleView);
		Application.Run(new ConsoleView());
	}
}