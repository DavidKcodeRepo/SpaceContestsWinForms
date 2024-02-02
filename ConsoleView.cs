using SpaceContest;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.LinkLabel;

namespace SpaceContestsWinForms
{

	public partial class ConsoleView : Form
	{
		//normally, the user's console inputs goes to DoPlayerMove method, but some abilities in the game need to also request user input,
		//this delegate is updated when game abilities require user input so user input can be routed to the calling method

		public delegate void MethodRequestingInput(string lastLine, Card card);
		private MethodRequestingInput currentPendingMethod;
		private Card currentCard;

	
		private Game _game;

		public ConsoleView()
		{
			InitializeComponent();
			_game = new Game(this);
			this.Shown += ConsoleView_Shown;
			rtbConsole.KeyDown += RtbConsole_KeyDown;
		}

		private void RtbConsole_KeyDown(object sender, KeyEventArgs e)
		{
			if (_game.IsPlayerTurn == true)
			{
				if (e.KeyCode == Keys.Enter)
				{
					string lastLine = GetLastLine(rtbConsole.Text);

					if (currentPendingMethod != null)
					{
						currentPendingMethod?.Invoke(lastLine, currentCard);
						currentPendingMethod = null;
						return;
					}
					_game.DoPlayerGameMove(lastLine);
					e.SuppressKeyPress = false;
					return;
				}
			}
		}
		public void RequestUserInput(MethodRequestingInput method, string prompt, Card card)
		{
			rtbConsole.AppendText(prompt);
			rtbConsole.AppendText(Environment.NewLine);
			currentPendingMethod = method;
			currentCard = card;
			return;
		}

		private string GetLastLine(string text)
		{
			string[] lines = text.Split('\n');
			return lines.Length > 0 ? lines[lines.Length - 1].Trim() : "";
		}

		private void ConsoleView_Shown(object? sender, EventArgs e)
		{
			_game.GameStartup();
		}

		public void WriteLine(string text)
		{
			if (rtbConsole != null)
			{
				rtbConsole.AppendText(text + "\n");
			}
		}

		public void PromptPlayerForMove()
		{
			if (rtbConsole != null)
			{
				this.WriteLine("\rWhat is your next move?");
				this.WriteLine("--> ");
			}
		}
	}
}
