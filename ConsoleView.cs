using SpaceContest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpaceContestsWinForms
{

	public partial class ConsoleView : Form
	{
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
			// Check if Enter key is pressed
			if (e.KeyCode == Keys.Enter)
			{
				// Get the last line from the RichTextBox
				string lastLine = GetLastLine(rtbConsole.Text);

				// Trigger your custom event or perform any other action
				_game.DoGameLoop(lastLine);

				// Suppress the Enter key so that a new line is not added to the RichTextBox
				e.SuppressKeyPress = false;
			}
		}

		private string GetLastLine(string text)
		{
			string[] lines = text.Split('\n');
			return lines.Length > 0 ? lines[lines.Length - 2].Trim() : "";
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
	}
}
