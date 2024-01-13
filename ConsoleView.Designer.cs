namespace SpaceContestsWinForms
{
	partial class ConsoleView
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			rtbConsole = new RichTextBox();
			SuspendLayout();
			// 
			// rtbConsole
			// 
			rtbConsole.BackColor = SystemColors.WindowFrame;
			rtbConsole.Location = new Point(12, 12);
			rtbConsole.Name = "rtbConsole";
			rtbConsole.Size = new Size(1740, 811);
			rtbConsole.TabIndex = 0;
			rtbConsole.Text = "";
			// 
			// ConsoleView
			// 
			AutoScaleDimensions = new SizeF(7F, 14F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = SystemColors.ControlDarkDark;
			ClientSize = new Size(1764, 835);
			Controls.Add(rtbConsole);
			Font = new Font("Courier New", 6.75F, FontStyle.Regular, GraphicsUnit.Point);
			Margin = new Padding(2);
			Name = "ConsoleView";
			Text = "Star Cards";
			ResumeLayout(false);
		}

		#endregion

		private RichTextBox rtbConsole;
	}
}