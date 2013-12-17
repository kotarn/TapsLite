using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Telnet
{
	/// <summary>
	/// Interaction logic for CharacterPage.xaml
	/// </summary>
	public partial class CharacterPage : UserControl, IDisposable
	{
		Client client;
		DispatcherTimer checkMessageTimer = new DispatcherTimer();
		DispatcherTimer keepAliveTimer = new DispatcherTimer();
		List<string> previousMessages = new List<string>();
		int previousIndex = -1;
		string currentMessage;
		bool ignoreNextHuh = false;
		readonly Regex privateMessageRegex = new Regex(@"^[pagewhispr]+ ([\w_\-]+)=", RegexOptions.IgnoreCase);

		public event Action Quit;
		public event Action Whisper;
		public event Action Page;
		public event Action Notify;

		public CharacterPage(string name, string password)
		{
			InitializeComponent();

			client = new Client();
			client.Connect();

			checkMessageTimer.Interval = TimeSpan.FromMilliseconds(50);
			checkMessageTimer.Tick += CheckMessages;
			checkMessageTimer.Start();

			keepAliveTimer.Interval = TimeSpan.FromMinutes(1);
			keepAliveTimer.Tick += KeepAlive;
			keepAliveTimer.Start();

			Task.Run(async delegate { await Task.Delay(200); client.Write(string.Format("connect {0} {1}", name, password)); });

			Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
		}

		private void KeepAlive(object s, EventArgs args)
		{
			ignoreNextHuh = true;
			client.Write("keepalivejunk");
		}

		private void CheckMessages(object s, EventArgs args)
		{
			List<string> messages = client.GetPendingMessages();

			bool isAtBottom = OutputIsAtBottom();

			foreach (var message in messages)
			{
				if (ignoreNextHuh && message.IndexOf("Huh?") == 0)
				{
					ignoreNextHuh = false;
					continue;
				}

				Paragraph paragraph = new Paragraph();

				TextFormatter formatter = new TextFormatter(message);

				if (formatter.IsPage && Page != null)
				{
					Page();
				}

				if (formatter.IsWhisper && Whisper != null)
				{
					Whisper();
				}

				if (formatter.IsNotify && Notify != null)
				{
					Notify();
				}

				paragraph.Inlines.Add(formatter.FormattedRun);

				OutputBox.Document.Blocks.Add(paragraph);
			}

			if (messages.Any() && isAtBottom)
			{
				OutputBox.ScrollToEnd();
			}
		}

		private void EntryBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				if (InputBox.Text.Length < 1024)
				{
					client.Write(InputBox.Text);

					if (InputBox.Text == "QUIT")
					{
						if (Quit != null)
						{
							Quit();
						}
					}
				}
				else
				{
					string current = InputBox.Text;
					List<string> messages = new List<string>();

					string overflowFormat;

					var pmMatch = privateMessageRegex.Match(current);

					if (pmMatch.Success)
					{
						var match = pmMatch.Value;
						overflowFormat = match + "{0}";
					}
					else if (current[0] == ':' || current[0] == '\"')
					{
						overflowFormat = "spoof {0}";
					}
					else
					{
						overflowFormat = "{0}";
					}

					while (current.Length > 1016)
					{
						int i;
						int limit = 1016;

						if (messages.Count > 0)
							limit = 1000;

						for (i = limit; i >= 0 && current[i] != ' '; --i) { }

						if (i == 0)
							i = limit;

						if (messages.Count == 0)
						{
							messages.Add(current.Substring(0, i).Trim() + " -");
						}
						else
						{
							messages.Add(string.Format(overflowFormat, current.Trim()));
						}

						current = current.Substring(i).Trim();
					}

					messages.Add(string.Format(overflowFormat, current));

					foreach (var message in messages)
					{
						client.Write(message);
					}
				}

				previousMessages.Add(InputBox.Text);

				InputBox.Text = "";
			}
			else
			{
				previousIndex = -1;
			}
		}

		private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			LineFill.Value = InputBox.Text.Length;
		}

		private bool OutputIsAtBottom()
		{
			// get the vertical scroll position
			double dVer = OutputBox.VerticalOffset;

			//get the vertical size of the scrollable content area
			double dViewport = OutputBox.ViewportHeight;

			//get the vertical size of the visible content area
			double dExtent = OutputBox.ExtentHeight;

			return dExtent < dViewport || (Math.Abs(dVer + dViewport - dExtent) < 4);
		}

		private void InputBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Up)
			{
				if (previousMessages.Any())
				{
					if (previousIndex == -1)
					{
						currentMessage = InputBox.Text;
						previousIndex = previousMessages.Count - 1;
						InputBox.Text = previousMessages[previousIndex];
					}
					else
					{
						--previousIndex;

						if (previousIndex < 0)
						{
							previousIndex = 0;
						}

						InputBox.Text = previousMessages[previousIndex];
					}

					InputBox.CaretIndex = int.MaxValue;
				}
			}
			else if (e.Key == Key.Down)
			{
				if (previousMessages.Any())
				{
					if (previousIndex != -1)
					{
						++previousIndex;

						if (previousIndex >= previousMessages.Count)
						{
							previousIndex = -1;
							InputBox.Text = currentMessage;
						}
						else
						{
							InputBox.Text = previousMessages[previousIndex];
						}
					}

					InputBox.CaretIndex = int.MaxValue;
				}
			}
		}

		public void Dispose()
		{
			checkMessageTimer.Stop();
			keepAliveTimer.Stop();
			client.Close();
		}

		public void ScrollToEnd()
		{
			OutputBox.ScrollToEnd();
		}
	}
}
